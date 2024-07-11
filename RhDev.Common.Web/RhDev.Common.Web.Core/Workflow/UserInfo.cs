using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RhDev.Common.Web.Core.Workflow
{
    [Serializable]
    public class UserInfo : IPrincipalInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public bool IsPermissionGroup => false;
        public bool IsExternal { get; set; }
        public static UserInfo UnknownUser => new UserInfo(default!, string.Empty, string.Empty, "?", string.Empty);
        public bool IsValid => !string.IsNullOrWhiteSpace(Id);

        public bool HasEmail => !string.IsNullOrEmpty(Email);

        public SectionDesignation SectionDesignation { get; set; }

        [XmlIgnore]
        public CultureInfo Culture => new CultureInfo(CultureLcid);
        public int CultureLcid { get; set; }

        public UserInfo() { }

        public static UserInfo CreateExternalUser(SectionDesignation sectionDesignation, string email)
        {
            var ui =
                new UserInfo(sectionDesignation, string.Empty, "ExternalUser", "External User", email) { IsExternal = true };

            return ui;
        }

        public UserInfo(SectionDesignation sectionDesignation, string id, string name, string displayName, string email)
        {
            SectionDesignation = sectionDesignation;
            Id = id;
            Name = name;
            DisplayName = displayName;
            Email = email;
            CultureLcid = 1029;
        }

        public override string ToString() => $"{Name} (ID {Id})";

        protected bool Equals(UserInfo other)
        {
            return Id == other.Id && SectionDesignation.Equals(other.SectionDesignation);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((UserInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Id.GetHashCode() ^ (Equals(null, SectionDesignation) ? 0 : SectionDesignation.GetHashCode());
            }
        }
    }

    public class UserInfoNameEqualityComparer : IEqualityComparer<IPrincipalInfo>
    {
        public bool Equals(IPrincipalInfo x, IPrincipalInfo y)
        {
            return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(IPrincipalInfo obj)
        {
            return obj.Name != null ? obj.Name.GetHashCode() : 0;
        }
    }


    public class UserInfoIdEqualityComparer : IEqualityComparer<UserInfo>
    {
        public bool Equals(UserInfo x, UserInfo y)
        {
            return Equals(x.Id, y.Id);
        }

        public int GetHashCode(UserInfo obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
