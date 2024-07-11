using RhDev.Common.Web.Core.Configuration;
using RhDev.Common.Web.Core.DataAccess.SQL;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Configuration;
using RhDev.Common.Web.Core.Utils;
using System.Linq.Expressions;
using RhDev.Common.Web.Core.Extensions;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Stores.Configuration;

namespace RhDev.Common.Web.Core.Impl.Configuration
{
    public class UserConfigurationProvider : IUserConfigurationProvider
    {
        private readonly ICentralClockProvider centralClockProvider;
        private readonly IDataStoreAcessRepositoryFactory dataStoreAcessRepositoryFactory;

        public const string SafeStringPrefix = "???___";

        bool IsArrayIndexerKey(string key) => Regex.IsMatch(key, @":\d+$");
        bool KeyContainIndexer(string key) => Regex.IsMatch(key, @":\d+");

        public UserConfigurationProvider(
            ICentralClockProvider centralClockProvider,
            IDataStoreAcessRepositoryFactory dataStoreAcessRepositoryFactory)
        {
            this.centralClockProvider = centralClockProvider;
            this.dataStoreAcessRepositoryFactory = dataStoreAcessRepositoryFactory;
        }

        public async Task WriteConfigurationAsync<TConfiguration>(TConfiguration configuration, string userId, string configurationContainerPrefix = default!) where TConfiguration : IApplicationConfigurationSection
        {
            Guard.NotNull(configuration, nameof(configuration));
            Guard.StringNotNullOrWhiteSpace(userId, nameof(userId));

            var userSettingsRepository = dataStoreAcessRepositoryFactory.GetDomainQueryableStoreRepository<IUserConfigurationDataStore>();

            var clockOffsetLocal = centralClockProvider.Now().ExportDateTimeOffset;

            var settings = configuration;

            await ApplicationConfigurationWalker.WalkConfiguration(settings, async (pi, piObject, key) =>
            {
                await WriteProperty(userId, userSettingsRepository, clockOffsetLocal, pi, piObject, key, configurationContainerPrefix);
            });
        }

        public async Task WriteConfigurationPropertyAsync<TConfiguration>(
            TConfiguration configuration,
            Expression<Func<TConfiguration, object>> configurationProperty,
            string userId
            ) where TConfiguration : IApplicationConfigurationSection
        {
            Guard.NotNull(configuration, nameof(configuration));
            Guard.NotNull(configurationProperty, nameof(configurationProperty));
            Guard.StringNotNullOrWhiteSpace(userId, nameof(userId));

            var userSettingsRepository = dataStoreAcessRepositoryFactory.GetDomainQueryableStoreRepository<IUserConfigurationDataStore>();

            var clockOffsetLocal = centralClockProvider.Now().ExportDateTimeOffset;

            await WriteProperty(configuration, userId, userSettingsRepository, clockOffsetLocal, configurationProperty);
        }

        private async Task WriteProperty(
            string userId,
            IUserConfigurationDataStore userSettingsRepository,
            DateTimeOffset clockOffsetLocal,
            PropertyInfo pi,
            IApplicationConfigurationSection piObject,
            string piKey,
            string configurationContainerPrefix = default!)
        {
            var propertyValueType = pi.PropertyType;

            var value = pi.GetValue(piObject);

            await WriteConfigurationItemAsync(pi, userId, value, propertyValueType, piKey, userSettingsRepository, clockOffsetLocal, configurationContainerPrefix);
        }

        private async Task WriteProperty<TConfiguration>(
            TConfiguration configuration,
            string userId,
            IUserConfigurationDataStore userSettingsRepository,
            DateTimeOffset clockOffsetLocal,
            Expression<Func<TConfiguration, object>> propertyExpression) where TConfiguration : IApplicationConfigurationSection
        {
            var propertyGetter = propertyExpression;
                        
            var property = ConfigurationUtils.GetPathConfigurationPropertyUsingExpressionWalker(propertyGetter);

            var propertyKey = property.path;
            var propertyValueType = property.propertyType;
            var mi = property.mi;

            if (propertyKey is not null && propertyKey.StartsWith(":")) throw new InvalidOperationException("Unsupported expression");

            var value = propertyGetter.Compile()(configuration);           

            await WriteConfigurationItemAsync(mi, userId, value, propertyValueType, propertyKey, userSettingsRepository, clockOffsetLocal);
        }

        private async Task WriteConfigurationItemAsync(MemberInfo mi, string userId, object? value, Type propertyValueType, string propertyKey, IUserConfigurationDataStore userSettingsRepository, DateTimeOffset clockOffsetLocal, string configurationContainerPrefix = default!)
        {
            propertyKey = $"{(string.IsNullOrWhiteSpace(configurationContainerPrefix) ? string.Empty : $"{configurationContainerPrefix}:")}{propertyKey}";

            var isArrayIndexerKey = IsArrayIndexerKey(propertyKey);

            if (isArrayIndexerKey)
            {
                var isDefault = IsDefaultValue(value);

                if (!isDefault)
                {
                    await RemoveAllKeysStartWith(propertyKey, userSettingsRepository);
                }
                else
                {
                    await RemoveAllKeysStartWith(propertyKey, userSettingsRepository);

                    await RecalculateArrayIndices(userSettingsRepository, propertyKey);
                }
            }

            if (typeof(ICollection).IsAssignableFrom(propertyValueType))
            {
                var allCollectionKeys = await userSettingsRepository.ReadAsync(s => s.Key.StartsWith(propertyKey));

                foreach (var settings in allCollectionKeys) await userSettingsRepository.DeleteAsync(settings.Id);

                if (value is not null)
                {
                    var data = ((IEnumerable)value).Cast<object>().ToList().Distinct();

                    data = data.Where(d => d is not null);

                    if (data.Count() > 0)
                    {
                        var element = data.First();
                        var elementType = element.GetType();

                        var isConfigurationObject = element is IApplicationConfigurationSection;

                        if (!isConfigurationObject)
                        {
                            var typedData = data.Select(d => GetStringValue(d, mi)).Where(d => d is not null).ToList();

                            for (var i = 0; i < typedData.Count; i++)
                            {
                                var key = $"{propertyKey}:{i}";
                                var valueItem = typedData[i];

                                var newSettings = new ApplicationUserSettings
                                {
                                    Key = key,
                                    Value = valueItem,
                                    ChangedBy = userId,
                                    Changed = clockOffsetLocal
                                };

                                await userSettingsRepository.CreateAsync(newSettings);
                            }
                        }
                        else
                        {
                            int i = 0;
                            foreach (var configArrayElement in data)
                            {
                                var key = $"{propertyKey}:{i}";

                                await WriteConfigurationAsync((configArrayElement as IApplicationConfigurationSection)!, userId, key);

                                i++;
                            }
                        }
                    }
                }
            }
            else
            {
                if (IsDefaultValue(value))
                {
                    var settings = await userSettingsRepository.ReadAsync(s => s.Key == propertyKey);

                    if (settings is not null && settings.Count > 0) await userSettingsRepository.DeleteAsync(settings[0].Id);
                }
                else
                {
                    if (value is IApplicationConfigurationSection valueSection)
                    {
                        var keyContainsIndexer = KeyContainIndexer(propertyKey);
                        
                        if(keyContainsIndexer && !isArrayIndexerKey)
                        {
                            var pathItems = propertyKey.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                            var lastElementProperty = pathItems.LastOrDefault();
                            if(lastElementProperty is not null) propertyKey = Regex.Replace(propertyKey, $":{lastElementProperty}$", string.Empty);
                        }

                        await WriteConfigurationAsync(valueSection, userId, keyContainsIndexer ? propertyKey : string.Empty);
                    }
                    else
                    {
                        string stringValue = GetStringValue(value, mi);

                        var settings = await userSettingsRepository.ReadAsync(s => s.Key == propertyKey);

                        if (settings is not null && settings.Count > 0)
                        {
                            var first = settings[0];
                            first.Value = stringValue;
                            first.ChangedBy = userId;
                            first.Changed = clockOffsetLocal;
                            await userSettingsRepository.UpdateAsync(first);
                        }
                        if (settings is null || settings.Count == 0)
                        {
                            var newSettings = new ApplicationUserSettings
                            {
                                Key = propertyKey,
                                Value = stringValue,
                                ChangedBy = userId,
                                Changed = clockOffsetLocal
                            };

                            await userSettingsRepository.CreateAsync(newSettings);
                        }
                    }
                }
            }
        }
                
        private async Task RecalculateArrayIndices(IUserConfigurationDataStore userSettingsRepository, string propertyKey)
        {
            var propertyKeys = propertyKey.Split(':');
            
            Guard.CollectionNotNullAndNotEmpty(propertyKeys, nameof(propertyKey));

            Guard.NumberMin(propertyKeys.Length, 2);

            var arrayIndex = Convert.ToInt32(propertyKeys.Last());

            var arrayPrefix = string.Join(':', propertyKeys.Take(propertyKeys.Length - 1));

            var allIndexerKeys = await userSettingsRepository.ReadAsync(s => s.Key.StartsWith(arrayPrefix));

            foreach (var settings in allIndexerKeys)
            {
                var key = settings.Key;
                var replaced = Regex.Replace(key, $"{arrayPrefix}:\\d+", m =>
                {
                    var match = m.Value;
                    var matchKeys = match.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                    var matchIndex = Convert.ToInt32(matchKeys.Last());
                    if (matchIndex > arrayIndex)
                    {
                        var matchArrayPrefix = string.Join(':', matchKeys.Take(matchKeys.Length - 1));

                        var newMatchKey = $"{matchArrayPrefix}:{matchIndex - 1}";

                        return newMatchKey;
                    }

                    return match;
                });

                if (key != replaced)
                {
                    settings.Key = replaced;

                    await userSettingsRepository.UpdateAsync(settings);
                }
            }
        }

        private string GetStringValue(object value, MemberInfo mi)
        {
            if (value.GetType() == typeof(decimal)) return Convert.ToString(value.ToString().Replace(",", "."));
            else if (value.GetType() == typeof(float)) return Convert.ToString(value.ToString().Replace(",", "."));
            else if(value is string stringValue)
            {
                Guard.NotNull(mi, nameof(mi), $"MethodInfio missing for string property");

                if (mi.IsSecurityString()) return $"{SafeStringPrefix}{stringValue.EncryptPlainString()}";

                return stringValue;
            }
            else return value.ToString();
        }

        private bool IsDefaultValue(object? value)
        {
            if (value is string stringValue) return string.IsNullOrWhiteSpace(stringValue);
            else if (value is int valueInt) return valueInt == default;
            else if (value is double valueDouble) return valueDouble == default;
            else if (value is long) return EqualityComparer<long>.Equals(value, default(long));
            else if (value is float) return EqualityComparer<float>.Equals(value, default(float));
            else if (value is decimal) return EqualityComparer<decimal>.Equals(value, default(decimal));
            else return value == default;
        }

        static async Task RemoveAllKeysStartWith(string propertyKey, IUserConfigurationDataStore userSettingsRepository)
        {
            var allIndexerKeys = await userSettingsRepository.ReadAsync(s => s.Key.StartsWith(propertyKey));

            foreach (var settings in allIndexerKeys) await userSettingsRepository.DeleteAsync(settings.Id);
        }
    }
}
