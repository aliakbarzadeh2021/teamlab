﻿using System;

namespace ASC.Projects.Core.Domain
{
    public abstract class ProjectEntity : DomainObject<int>
    {
        public string Title
        {
            get;
            set;
        }

        public Project Project
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

        public DateTime LastModifiedOn
        {
            get;
            set;
        }

        public Guid LastModifiedBy
        {
            get;
            set;
        }

        public string NotifyId
        {
            get { return string.Format("{0}_{1}", UniqID, Project.ID); }
        }


        public static string BuildUniqId<T>(int id)
        {
            return DomainObject<int>.DoUniqId(typeof(T), id);
        }

        public override int GetHashCode()
        {
            return string.Format("{0}|{1}|{2}", GetType().FullName, Title, Project.GetHashCode()).GetHashCode();
        }
    }
}
