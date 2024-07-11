using System.Reflection;

namespace RhDev.Common.Web.Core.Configuration
{
    public static class ApplicationConfigurationWalker
    {
        public static async Task WalkConfiguration<T>(T configurationSection, Func<PropertyInfo, IApplicationConfigurationSection, string, Task> propertyVisitor, IList<string> treeLevel = default!) where T : IApplicationConfigurationSection?
        {
            treeLevel ??= new List<string>();

            var currentPath = configurationSection!.Path;

            if (!string.IsNullOrWhiteSpace(currentPath))
            {
                if (currentPath.StartsWith(":") && treeLevel.Any())
                    treeLevel.Add($"{treeLevel.Last()}{currentPath}");
                else if (currentPath.StartsWith(":"))
                    treeLevel.Add(currentPath.Substring(1));
                else
                    treeLevel.Add(currentPath);
            }

            var pt = configurationSection!.GetType();

            var props
                = pt.GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public)
                .Where(pi => pi.GetSetMethod() is not null);

            var propertyStack = new Stack<PropertyInfo>(props);

            foreach (var pi in propertyStack)
            {
                if (typeof(IApplicationConfigurationSection).IsAssignableFrom(pi.PropertyType))
                {
                    var propertyvalue = pi.GetValue(configurationSection);

                    if (propertyvalue is not null)
                    {
                        await WalkConfiguration(propertyvalue as IApplicationConfigurationSection, propertyVisitor, treeLevel);
                    }
                }
                else if (typeof(System.Collections.ICollection).IsAssignableFrom(pi.PropertyType))
                {
                    var propertyvalue = pi.GetValue(configurationSection);
                    var data = ((System.Collections.ICollection)propertyvalue).Cast<object>().ToList().Distinct();
                    data = data.Where(d => d is not null);

                    if (data is not null && data.Count() > 0)
                    {
                        var element = data.First();
                        var elementType = element.GetType();

                        var isConfigurationObject = element is IApplicationConfigurationSection;

                        if (isConfigurationObject)
                        {
                            var piName = pi.Name;
                            int i = 0;
                            foreach (var configArrayElement in data)
                            {
                                var prefix = treeLevel.LastOrDefault()?.ToString();

                                treeLevel.Add($"{(string.IsNullOrWhiteSpace(prefix) ? string.Empty : $"{prefix}:")}{piName}:{i}");
                                await WalkConfiguration(configArrayElement as IApplicationConfigurationSection, propertyVisitor,  treeLevel);
                                i++;
                            }
                        }
                        else await ProcessPi(pi);
                    }
                }
                else
                    await ProcessPi(pi);
            }

            async Task ProcessPi(PropertyInfo pi)
            {
                var key = string.Join(":", new List<string> { treeLevel.LastOrDefault()!, pi.Name }.Where(s => s is not null));

                await propertyVisitor(pi, configurationSection, key);
            }

            if (treeLevel.Count > 0) treeLevel.RemoveAt(treeLevel.Count - 1);
        }
    }
}
