using System;
using System.Collections.Generic;
using System.Diagnostics;
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;

namespace ASC.Web.Studio.Core
{
    class TcpIpFilterActions
    {
        public static readonly IAction TcpIpFilterAction = new ASC.Common.Security.Authorizing.Action(new Guid("0000ffff-ae36-4d2e-818d-726cb650aeb7"), "Tcp/Ip filter", false, false);
    }


    [DebuggerDisplay("IP = {SecurityId}")]
    class TcpIpFilterSecurityObject : ISecurityObject
    {
        private readonly string ip;


        public TcpIpFilterSecurityObject(string ip)
        {
            if (string.IsNullOrEmpty(ip)) throw new ArgumentNullException("ip");
            this.ip = ip;
        }


        public Type ObjectType
        {
            get { return GetType(); }
        }

        public object SecurityId
        {
            get { return ip; }
        }


        public bool InheritSupported
        {
            get { return true; }
        }

        public bool ObjectRolesSupported
        {
            get { return false; }
        }


        public ISecurityObjectId InheritFrom(ISecurityObjectId objectId)
        {
            var parts = ip.Split('.');
            if (parts.Length != 4 || parts[0] == "*") return null;

            var format = "{0}.{1}.{2}.*";
            if (parts[1] == "*") format = "*.*.*.*";
            else if (parts[2] == "*") format = "{0}.*.*.*";
            else if (parts[3] == "*") format = "{0}.{1}.*.*";

            return new TcpIpFilterSecurityObject(string.Format(format, parts[0], parts[1], parts[2]));
        }

        public IEnumerable<IRole> GetObjectRoles(ISubject account, ISecurityObjectId objectId, SecurityCallContext callContext)
        {
            throw new NotImplementedException();
        }
    }
}
