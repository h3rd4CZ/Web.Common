using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Workflow
{
    [Serializable]
    public class SectionDesignation
    {
        public string Location { get; set; }
        public SectionDesignation() { }
        public static SectionDesignation From(string location) => new SectionDesignation { Location = location };
        public static SectionDesignation Empty => new SectionDesignation { Location = string.Empty };

        public override string ToString() => Location.ToString();

        public override int GetHashCode() => Location.GetHashCode();

        public bool Equals(SectionDesignation sectionDesignation) => Location == sectionDesignation.Location;
    }

}
