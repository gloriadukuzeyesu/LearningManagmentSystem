using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class AssignmentCategory
    {
        public AssignmentCategory()
        {
            Assignments = new HashSet<Assignment>();
        }

        public uint CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public uint Weight { get; set; }
        public uint InClass { get; set; }

        public virtual Class InClassNavigation { get; set; } = null!;
        public virtual ICollection<Assignment> Assignments { get; set; }
    }
}
