using System;
using System.Diagnostics;
using ASC.Core;
using ASC.Core.Users;
using System.Collections;

namespace ASC.Projects.Core.Domain
{
    [DebuggerDisplay("{UserInfo.ToString()}")]
    public class Participant : IComparable
    {
        public Guid ID
        {
            get;
            private set;
        }

        public UserInfo UserInfo
        {
            get { return CoreContext.UserManager.GetUsers(ID); }
        }


        public Participant(Guid userID)
        {
            ID = userID;
        }


        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var p = obj as Participant;
            return p != null && p.ID == ID;
        }

        public int CompareTo(object obj)
        {
            var other = obj as Participant;
            return other == null ?
                Comparer.Default.Compare(this, obj) :
                UserFormatter.Compare(UserInfo, other.UserInfo);
        }
    }
}

