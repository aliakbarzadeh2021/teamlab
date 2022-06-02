using System;

namespace ASC.Projects.Core.Domain
{
    public class ProjectChangeRequest : DomainObject<Int32>
    {
        public int ProjectID { get; set; }

        public int TemplateId { get; set; }

        public String Title { get; set; }

        public String Description { get; set; }

        public ProjectStatus Status { get; set; }

        public Guid Responsible { get; set; }

        public ProjectRequestType RequestType { get; set; }

        public bool Private { get; set; }


        public Guid CreateBy { get; set; }

        public DateTime CreateOn { get; set; }
    }
}
