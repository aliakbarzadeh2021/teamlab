using System;
using System.Diagnostics;

namespace ASC.Projects.Core.Domain
{
    [DebuggerDisplay("Project: ID = {ID}, Title = {Title}")]
    public class Project : DomainObject<Int32>
    {
        public string Title { get; set; }

        public string HtmlTitle
        {
            get
            {
                if (Title == null) return string.Empty;
                return Title.Length <= 40 ? Title : Title.First(37) + "...";
            }
        }

        public String Description { get; set; }

        public ProjectStatus Status { get; set; }

        public Guid Responsible { get; set; }

        public bool Private { get; set; }


        public Guid CreateBy { get; set; }

        public DateTime CreateOn { get; set; }

        public Guid LastModifiedBy { get; set; }

        public DateTime LastModifiedOn { get; set; }
    }
}