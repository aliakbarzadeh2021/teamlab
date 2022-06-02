using System;
using ASC.Common.Security.Authentication;
using ASC.Core.Users;

namespace ASC.Core.Security.Authentication
{
    [Serializable]
    class UserAccount : MarshalByRefObject, IUserAccount
    {
        private Credential credential;


        public Guid ID { get; private set; }

        public string Name { get; private set; }

        public string AuthenticationType { get { return "ASC"; } }

        public virtual bool IsAuthenticated { get { return true; } }

        public string FirstName { get; private set; }

        public string LastName { get; private set; }

        public string Title { get; private set; }

        public string Department { get; private set; }

        public int Tenant { get; private set; }

        public string Email { get; private set; }


        public UserAccount(Credential credential)
        {
            if (credential == null) throw new ArgumentNullException("credential");
            this.credential = credential;
        }

        public UserAccount(UserInfo info, int tenant)
        {
            ID = info.ID;
            Name = UserFormatter.GetUserName(info);
            FirstName = info.FirstName;
            LastName = info.LastName;
            Title = info.Title;
            Department = info.Department;
            Tenant = tenant;
            Email = info.Email;
        }


        public Credential GetCredential()
        {
            return credential;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            var a = obj as IUserAccount;
            return a != null && ID.Equals(a.ID);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}