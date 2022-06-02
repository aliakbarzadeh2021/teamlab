﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASC.Common.Security;
using ASC.Web.UserControls.Wiki;
using ASC.Common.Security.Authorizing;

namespace ASC.Web.Community.Wiki.Common
{
    public class WikiObjectsSecurityObject : ISecurityObject
    {
        public IWikiObjectOwner Object { get; private set; }
        public WikiObjectsSecurityObject(IWikiObjectOwner obj)
        {
            Object = obj;
        }

        #region ISecurityObjectId Members

        public Type ObjectType
        {
            get { return this.GetType(); }
        }

        public object SecurityId
        {
            get { return Object.GetObjectId(); }
        }

        #endregion

        #region ISecurityObjectProvider Members

        public IEnumerable<ASC.Common.Security.Authorizing.IRole> GetObjectRoles(ASC.Common.Security.Authorizing.ISubject account, ISecurityObjectId objectId, SecurityCallContext callContext)
        {
            var roles = new List<IRole>();

            if (!Guid.Empty.Equals(Object.OwnerID) && Object.OwnerID.Equals(account.ID))
            {
                roles.Add(ASC.Common.Security.Authorizing.Constants.Owner);
            }

            return roles;
        }

        public ISecurityObjectId InheritFrom(ISecurityObjectId objectId)
        {
            throw new NotImplementedException();
        }

        public bool InheritSupported
        {
            get { return false; }
        }

        public bool ObjectRolesSupported
        {
            get { return true; }
        }

        #endregion
    }
}
