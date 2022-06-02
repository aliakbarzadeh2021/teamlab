using System;
using System.Reflection;
using ASC.Core;
using ASC.Core.Users;

namespace ASC.Web.Studio.Core.Statistic
{
	public class UserVisit
    {
        public virtual int ID { get; set; }

        public virtual int TenantID { get; set; }

        public virtual DateTime VisitDate { get; set; }

        public virtual DateTime? FirstVisitTime { get; set; }

        public virtual DateTime? LastVisitTime { get; set; }

        public virtual Guid UserID{ get; set; }

        public virtual UserInfo User
        {
            get
            {               
                return CoreContext.UserManager.GetUsers(UserID);
            }
        } 

        public virtual Guid ModuleID { get; set; }

        public virtual Guid ProductID { get; set; }

        public virtual int VisitCount { get; set; }
        
    }

  public class Visit
  {
    public DateTime VisitDate { get; set; }
  }

  public class Host : Visit
  {
    public Guid UserId { get; set; }
  }

  public class Hit : Visit
  {
    public Int32 Count { get; set; }
  }
}
