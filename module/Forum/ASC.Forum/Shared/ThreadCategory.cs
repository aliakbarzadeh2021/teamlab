﻿using System;
using System.Collections.Generic;
using System.Reflection;
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;
using ASC.Core;
using ASC.Core.Users;

namespace ASC.Forum
{
	public class ThreadCategory : ISecurityObject
    {
        public virtual int ID { get; set; }

        public virtual string Title { get; set;}

        public virtual string Description { get; set;}

        public virtual int SortOrder { get; set; }

        public virtual DateTime CreateDate { get; set; }

        public virtual Guid PosterID { get; set; }

     

        public virtual int TenantID { get; set; }

        public virtual UserInfo Poster
        {
            get
            {
                return ASC.Core.CoreContext.UserManager.GetUsers(PosterID);
            }
        }

        public ThreadCategory()
        {   
        }

        public virtual bool Visible
        {
            get
            {
                return SecurityContext.CheckPermissions(this, Module.Constants.ReadPostsAction);

            }
        }


        #region ISecurityObjectId Members

		/// <inheritdoc/>
        public object SecurityId
        {
            get { return this.ID; }
        }

		/// <inheritdoc/>
        public Type ObjectType
        {
            get { return this.GetType(); }
        }

        #endregion

        #region ISecurityObjectProvider Members

		/// <inheritdoc/>
        public IEnumerable<IRole> GetObjectRoles(ISubject account, ISecurityObjectId objectId, SecurityCallContext callContext)
        {
            return new IRole[0];
        }

		/// <inheritdoc/>
        public ISecurityObjectId InheritFrom(ISecurityObjectId objectId)
        {
            throw new NotImplementedException();
        }

		/// <inheritdoc/>
        public bool InheritSupported
        {
            get { return false; }
        }

		/// <inheritdoc/>
        public bool ObjectRolesSupported
        {
            get { return true; }
        }

        #endregion
    }
}
