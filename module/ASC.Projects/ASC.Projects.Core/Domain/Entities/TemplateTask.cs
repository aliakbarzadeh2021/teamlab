using System;

namespace ASC.Projects.Core.Domain
{
    [Serializable]
    public class TemplateTask : TemplateBase
    {
        public int ProjectId
        {
            get;
            set;
        }

        public int MilestoneId
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public Guid Responsible
        {
            get;
            set;
        }

        public TaskPriority Priority
        {
            get;
            set;
        }

        public int SortOrder
        {
            get;
            set;
        }


        public TemplateTask(int projectId, string title)
            : this(projectId, title, 0)
        {
        }

        public TemplateTask(int projectId, string title, int milestoneId)
        {
            if (projectId <= 0) throw new ArgumentException("ProjectId can not be less or equal to zero.");
            if (string.IsNullOrEmpty(title)) throw new ArgumentNullException("title");

            ProjectId = projectId;
            Title = title;
            MilestoneId = milestoneId;
        }
    }
}
