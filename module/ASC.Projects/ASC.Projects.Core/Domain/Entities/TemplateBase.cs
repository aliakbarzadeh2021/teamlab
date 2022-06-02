using System;

namespace ASC.Projects.Core.Domain
{
    [Serializable]
    public class TemplateBase
    {
        public int Id
        {
            get;
            internal set;
        }

        public string Title
        {
            get;
            set;
        }

        public Guid CreateBy
        {
            get;
            set;
        }

        public DateTime CreateOn
        {
            get;
            set;
        }

        public string HtmlTitle
        {
            get
            {
                if (Title == null) return string.Empty;
                return Title.Length <= 40 ? Title : Title.First(37) + "...";
            }
        }

        public override string ToString()
        {
            return Title;
        }

        public override bool Equals(object obj)
        {
            var t = obj as TemplateBase;
            return t != null && t.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
