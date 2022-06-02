using System;
using System.Collections.Generic;

namespace ASC.Projects.Core.Domain
{
    [Serializable]
    public class TemplateProject : TemplateBase
    {
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

        public string Tags
        {
            get;
            set;
        }

        public List<Guid> Team
        {
            get;
            private set;
        }

        public int MilestonesCount
        {
            get;
            internal set;
        }

        public int TasksCount
        {
            get;
            internal set;
        }

        public int MessagesCount
        {
            get;
            internal set;
        }
        
        
        public TemplateProject(string title)
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentNullException("title");

            Title = title;
            Team = new List<Guid>();
        }
    }
}
