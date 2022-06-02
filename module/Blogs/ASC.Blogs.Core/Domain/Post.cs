﻿using System;
using System.Collections.Generic;
using ASC.Blogs.Core.Security;
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;
using ASC.Core.Users;
using System.Reflection;

namespace ASC.Blogs.Core.Domain
{
    public enum BlogType
    {
        Personal,
        Corporate
    }

    public class Post
        : ISecurityObject
    {
        #region members

        //private UserInfo _User;
        private Guid _ID;
        private Guid _UserID;
        private string _Title;
        private string _Content;
        private DateTime _Datetime;
        private List<Tag> _TagList = new List<Tag>();


        #endregion

        #region Properties

        public virtual BlogType BlogType
        {
            get
            {
                return BlogType.Personal;
            }
        }

        public virtual Guid ID
        {
            get { return _ID; }
            set { _ID = value; }
        }


        public long BlogId { get; set; }

        public virtual Guid UserID
        {
            get { return _UserID; }
            set { _UserID = value; }
        }

        public virtual string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }
        public virtual string Content
        {
            get { return _Content; }
            set { _Content = value; }
        }

        public virtual DateTime Datetime
        {
            get { return _Datetime; }
            set { _Datetime = value; }
        }


        public virtual List<Tag> TagList
        {
            get { return _TagList; }
            set { _TagList = value; }
        }
        public virtual void ClearTags()
        {
            _TagList.Clear();
        }


        public virtual UserInfo Author
        {
            get
            {
                return ASC.Core.CoreContext.UserManager.GetUsers(this.UserID);
            }
        }


        #endregion

        #region Methods



        public override int GetHashCode()
        {
            return (GetType().FullName + "|" +
                    _ID.ToString()).GetHashCode();
        }

        public virtual string GetPreviewText(int maxCharCount)
        {
            string result = string.Empty;
            string content = this._Content.Replace("\r\n", "");

            IList<string> tagExcludeList = new List<string>();
            tagExcludeList.Add("img");
            tagExcludeList.Add("!--");
            tagExcludeList.Add("meta");
            tagExcludeList.Add("embed");
            tagExcludeList.Add("col");
            tagExcludeList.Add("input");
            tagExcludeList.Add("object");
            tagExcludeList.Add("hr");
            tagExcludeList.Add("br");
            tagExcludeList.Add("li");

            Stack<string> tagList = new Stack<string>();

            int charCount = 0;
            int index = 0;

            while (index < content.Length)
            {
                int posBeginOpenTag = content.IndexOf("<", index);
                int posEndOpenTag = content.IndexOf(">", index);

                int posBeginCloseTag = content.IndexOf("</", index);
                int posEndCloseTag = content.IndexOf("/>", index);

                if (posBeginOpenTag == -1)
                {
                    AddHTMLText(content, index, content.Length - index, ref result, ref charCount, maxCharCount);
                    break;
                }
                else if (index < posBeginOpenTag)
                {
                    if (AddHTMLText(content, index, posBeginOpenTag - index, ref result, ref charCount, maxCharCount) == 1)
                        break;
                    index = posBeginOpenTag;
                }
                else
                {
                    index = AddHTMLTag(tagExcludeList, content, posBeginOpenTag, posEndOpenTag, posBeginCloseTag, posEndCloseTag, ref result, ref tagList);
                    if (index == -1)
                        break;
                }
            }


            while (tagList.Count != 0)
            {
                string temp = tagList.Pop();
                if (!tagExcludeList.Contains(temp.ToLower()))
                    result += "</" + temp + ">";
            }


            return result;
        }

        private int AddHTMLText(string sourceStr, int startPos, int len, ref string outStr, ref int charCount, int maxCharCount)
        {

            string str = sourceStr.Substring(startPos, len);
            if (str.Replace("&nbsp;", " ").Length + charCount > maxCharCount)
            {
                int dif = maxCharCount - charCount;
                int sublen = str.Replace("&nbsp;", " ").IndexOf(" ", dif);
                if (len > str.Replace("&nbsp;", " ").Length)
                    len = str.Replace("&nbsp;", " ").Length;
                outStr += str.Replace("&nbsp;", " ").Substring(0, (sublen == -1 ? len : sublen)).Replace("  ", "&nbsp;&nbsp;");
                if (sourceStr.Length > startPos + len)
                {
                    outStr += " ...";
                } return 1;
            }
            else
            {
                outStr += str;
                charCount += str.Replace("&nbsp;", " ").Length;
                return 0;
            }
        }

        private int AddHTMLTag(IList<string> tagExcludeList, string sourceStr, int posBeginOpenTag, int posEndOpenTag, int posBeginCloseTag, int posEndCloseTag, ref string outStr, ref Stack<string> tagList)
        {
            if (posEndOpenTag == posEndCloseTag + 1)
            {
                if (sourceStr.Substring(posBeginOpenTag, posEndOpenTag - posBeginOpenTag + 1) == "<hr class=\"hidden_line\" />")
                {
                    return -1;
                }
                outStr += sourceStr.Substring(posBeginOpenTag, posEndOpenTag - posBeginOpenTag + 1);
                return posEndOpenTag + 1;

            }
            else if (posBeginOpenTag == posBeginCloseTag)
            {
                string closeTag = sourceStr.Substring(posBeginCloseTag + 2, posEndOpenTag - posBeginCloseTag - 2);

                if (tagList.Peek() != closeTag)
                {
                    while (tagExcludeList.Contains(tagList.Peek().ToLower()))
                        tagList.Pop();
                }
                if (tagList.Peek() == closeTag)
                    outStr += "</" + tagList.Pop() + ">";
                else
                    outStr += "</" + closeTag + ">";
                return posEndOpenTag + 1;
            }
            else
            {
                string tagName = sourceStr.Substring(posBeginOpenTag + 1, posEndOpenTag - posBeginOpenTag - 1).Split(' ')[0];
                tagList.Push(tagName);

                outStr += sourceStr.Substring(posBeginOpenTag, posEndOpenTag - posBeginOpenTag + 1);
                return posEndOpenTag + 1;
            }
        }

        #endregion

        #region ISecurityObjectId Members

        public Type ObjectType
        {
            get { return this.GetType(); }
        }

        public object SecurityId
        {
            get { return this.ID; }
        }

        #endregion

        #region ISecurityObjectProvider Members

        public IEnumerable<ASC.Common.Security.Authorizing.IRole> GetObjectRoles(ASC.Common.Security.Authorizing.ISubject account, ISecurityObjectId objectId, SecurityCallContext callContext)
        {
            var roles = new List<IRole>();
            if (Equals(account.ID, this.UserID))
            {
                roles.Add(ASC.Common.Security.Authorizing.Constants.Owner);
            }
            return roles;
        }

        public ISecurityObjectId InheritFrom(ISecurityObjectId objectId)
        {
            if (Guid.Equals(UserID, objectId.SecurityId))
                return new PersonalBlogSecObject(this.Author);
            else
                return null;

        }

        public bool InheritSupported
        {
            get { return true; }
        }

        public bool ObjectRolesSupported
        {
            get { return true; }
        }

        #endregion
    }
}
