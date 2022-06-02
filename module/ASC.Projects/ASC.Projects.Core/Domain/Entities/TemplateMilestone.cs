using System;

namespace ASC.Projects.Core.Domain
{
    [Serializable]
    public class TemplateMilestone : TemplateBase
    {
        internal int Flags
        {
            get;
            set;
        }

        public int ProjectId
        {
            get;
            set;
        }

		/// <summary>
		/// Duration in days has the following format
		/// wwd
		/// where ww - week number (1 - 52), d - day of week (0 - 6, 0 - Sunday, 1 - Monday, etc.)
		/// </summary>
        public int DurationInDays
        {
            get;
            set;
        }

        public bool IsKey
        {
            get { return (Flags & 1) == 1; }
            set { Flags = value ? Flags | 1 : Flags & 1; }
        }

        public bool IsNotify
        {
            get { return (Flags & 2) == 2; }
            set { Flags = value ? Flags | 2 : Flags & 2; }
        }

        public int TasksCount
        {
            get;
            internal set;
        }


        public TemplateMilestone(int projectId, string title)
        {
            if (projectId <= 0) throw new ArgumentException("ProjectId can not be less or equal to zero.");
            if (string.IsNullOrEmpty(title)) throw new ArgumentNullException("title");

            ProjectId = projectId;
            Title = title;
        }
    }
}
