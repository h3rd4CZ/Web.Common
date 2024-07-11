using JasperFx.CodeGeneration.Model;
using RhDev.Common.Web.Core.Composition.Factory;
using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;
using RhDev.Common.Workflow.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RhDev.Common.Workflow.PropertyModel.Properties
{
    [DataContract]
    [KnownType(typeof(UserInfo))]
    [KnownType(typeof(PermissionGroupInfo))]
    public class StateManagementUserValue : StateManagementValue
    {
        private bool _wasBuild = false;
        protected override IList<(Regex regex, Func<string, string> evaluator)> Formatters => new List<(Regex regex, Func<string, string> evaluator)>
        {
            (new Regex(@"^[iI]d$"), s => id.ToString()),
            (new Regex(@"^[lL]ogin[nN]ame$"), s => name),
            (new Regex(@"^[eE]mail$"), s => email),
            (new Regex(@"^[dD]isplay[nN]ame"), s => displayName),
            (new Regex(@"^[cC]omposite[nN]ame"), s => string.IsNullOrWhiteSpace(displayName) ? name : $"{displayName} ({name})"),
        };

        [DataMember]
        private string id;
        [DataMember]
        private string name;
        [DataMember]
        private string email;
        [DataMember]
        private string displayName;
        [DataMember]
        private bool isPermissionGroup;
        [DataMember]
        private SectionDesignation section;

        public string Id => id;

        public string Name => name;
        public string Email => email;
        public string DisplayName => displayName;
        public bool IsPermissionGroup =>isPermissionGroup;
        public SectionDesignation Section => section;
                                        
        public StateManagementUserValue(IPrincipalInfo value, SectionDesignation designation) : base(value)
        {
            Guard.NotNull(value, nameof(value));

            section = designation;

            isPermissionGroup = value is PermissionGroupInfo;
            id = value.Id;
            name = value.Name;
            displayName = IsPermissionGroup ? value.Name : value.DisplayName;
            email = IsPermissionGroup ? string.Empty : ((UserInfo)value).Email;
        }

        public IPrincipalInfo GetPrincipalInfo()
        {
            return IsPermissionGroup
                ? new PermissionGroupInfo(Section, Id, Name, string.Empty, string.Empty) :
                (IPrincipalInfo)new UserInfo(Section, Id, Name, DisplayName, Email);
        }

        public override bool Equals(object obj)
        {
            return obj is StateManagementUserValue value && Id == value.Id;
        }

        public override int GetHashCode()
        {
            int hashCode = -1249983435;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Email);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DisplayName);
            hashCode = hashCode * -1521134295 + IsPermissionGroup.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<SectionDesignation>.Default.GetHashCode(Section);
            return hashCode;
        }

        public async override Task<bool> IsPermissionGroupEmptyAsync()
        {
            Build();

            if (IsPermissionGroup)
            {
                throw new NotImplementedException();
            }

            else throw new InvalidOperationException("Value is not permission group");
        }

        public override async Task<bool> IsGroupNotEmptyAsync() => !await IsPermissionGroupEmptyAsync();

        public override bool CurrentUserInGroup(IList<string> currentUsersGroup)
        {
            Guard.NotNull(currentUsersGroup, nameof(currentUsersGroup));

            if (!IsPermissionGroup) throw new InvalidOperationException($"Value is not Permission group");

            return currentUsersGroup.Any(g => !Equals(null, g) && g.Equals(Name));
        }

        public override async Task<bool> UserInGroupAsync(StateManagementValue value)
        {
            var rightGroup = Cast<StateManagementUserValue>(value);

            Build();

            var isUser = !isPermissionGroup;

            if (!isUser) throw new InvalidOperationException("Value is not a user");

            if (rightGroup.isPermissionGroup)
            {
                throw new NotImplementedException();
            }
            else throw new InvalidOperationException("UserInfo to check is not permission group");
        }

        public override bool ValueEquals(StateManagementValue right)
        {
            if (right.IsNullValue) return false;

            var rightUser = Cast<StateManagementUserValue>(right);
                                    
            return Equals(rightUser);
        }
        
        public override bool ValueNotEquals(StateManagementValue right) => right.IsNullValue || !ValueEquals(right);

        public override string ToString() => ToStringFormatter(displayName);
                
        private void Build()
        {
            lock (this)
            {
                if (!_wasBuild)
                {                                                            
                    _wasBuild = true;
                }
            }
        }
    }
}
