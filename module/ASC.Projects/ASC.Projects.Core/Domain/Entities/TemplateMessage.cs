using System;

namespace ASC.Projects.Core.Domain
{
    [Serializable]
    public class TemplateMessage : TemplateBase
    {
        public int ProjectId
        {
            get;
            set;
        }

        public string Text
        {
            get;
            set;
        }


        public TemplateMessage(int projectId, string title)
        {
            if (projectId <= 0) throw new ArgumentException("ProjectId can not be less or equal to zero.");
            if (string.IsNullOrEmpty(title)) throw new ArgumentNullException("title");

            ProjectId = projectId;
            Title = title;
        }
    }
}
