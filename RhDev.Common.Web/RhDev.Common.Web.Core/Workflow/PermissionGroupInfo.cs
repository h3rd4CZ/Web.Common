using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Workflow
{
    [Serializable]
    public class PermissionGroupInfo : IPrincipalInfo
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string DisplayName { get; private set; }
        public bool IsValid => !string.IsNullOrWhiteSpace(Id);
        public bool IsPermissionGroup => true;

        public SectionDesignation SectionDesignation { get; private set; }

        public PermissionGroupInfo(SectionDesignation sectionDesignation, string id) : this(sectionDesignation, id, string.Empty, string.Empty, string.Empty) { }

        public PermissionGroupInfo(SectionDesignation sectionDesignation, string id, string name, string displayName, string description)
        {
            SectionDesignation = sectionDesignation;
            Id = id;
            Name = name;
            DisplayName = displayName;
            Description = description;
        }

        protected bool Equals(PermissionGroupInfo other)
        {
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PermissionGroupInfo)obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} (ID {1})", Name, Id);
        }
    }
}
