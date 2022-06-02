using System;
using System.Web;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Security.Cryptography;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Users;

namespace ASC.Web.Studio.UserControls.Management
{
    public partial class ConfirmInvite : System.Web.UI.UserControl
    {
        public static string Location { get { return "~/UserControls/Management/ConfirmInvite.ascx"; } }

        protected string _firstName
        {
            get
            {
                return (Request["firstname"] ?? "").Trim();
            }
        }

        protected string _lastName
        {
            get
            {
                return (Request["lastname"] ?? "").Trim();
            }
        }

        protected string _pwd
        {
            get
            {
                return (Request["pwd"] ?? "").Trim();
            }
        }

        protected string _rePwd
        {
            get
            {
                return (Request["repwd"] ?? "").Trim();
            }
        }

        protected string _loginEmailView
        {
            get
            {
                return (String.IsNullOrEmpty(Request["loginEmail"])?(Request["email"] ?? "").Trim(): Request["loginEmail"]).Trim();
            }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            
            var email = (Request["email"] ?? "").Trim();
            var key = Request["key"] ?? "";
            var fap = Request["fap"] ?? "";

            this.Page.Title = HeaderStringHelper.GetPageTitle(Resources.Resource.Authorization, null, null);

            var user = CoreContext.UserManager.GetUserByEmail(email);
            if (!user.ID.Equals(ASC.Core.Users.Constants.LostUser.ID))
            {
                (this.Page as confirm).ErrorMessage =CustomNamingPeople.Substitute<Resources.Resource>("ErrorEmailAlreadyExists");
                if (SecurityContext.IsAuthenticated == false)
                    (this.Page as confirm).ErrorMessage += ". " + String.Format(Resources.Resource.ForSignInFollowMessage,
                        string.Format("<a href=\"{0}\">", VirtualPathUtility.ToAbsolute("~/auth.aspx")), "</a>");
                
                _confirmHolder.Visible = false;
                return;
            }

            if (IsPostBack)
            {
                var loginEmail = (Request["loginEmailInput"] ?? "").Trim();
                var firstName = (Request["firstnameInput"] ?? "").Trim();
                var lastName = (Request["lastnameInput"] ?? "").Trim();
                var pwd = (Request["pwdInput"] ?? "").Trim();
                var repwd = (Request["repwdInput"] ?? "").Trim();

                if (!loginEmail.TestEmailRegex())
                {
                    (this.Page as confirm).ErrorMessage = Resources.Resource.ErrorNotCorrectEmail;
                    return;
                }

                if (String.IsNullOrEmpty(firstName))
                {
                    (this.Page as confirm).ErrorMessage = Resources.Resource.ErrorEmptyUserFirstName;
                    return;
                }
                else if (String.IsNullOrEmpty(lastName))
                {
                    (this.Page as confirm).ErrorMessage = Resources.Resource.ErrorEmptyUserLastName;
                    return;
                }
                else if (String.IsNullOrEmpty(pwd))
                {
                    (this.Page as confirm).ErrorMessage = Resources.Resource.ErrorPasswordEmpty;
                    return;
                }
                else if (!String.Equals(pwd, repwd))
                {
                    (this.Page as confirm).ErrorMessage = Resources.Resource.ErrorMissMatchPwd;
                    return;
                }

                Guid userID;
                try
                {
                    SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                    var newUser = UserManagerWrapper.AddUser(new ASC.Core.Users.UserInfo()
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Email = loginEmail,
                        WorkFromDate = TenantUtil.DateTimeNow()
                    }, pwd, true);

                    userID = newUser.ID;

                    var userSettings = SettingsManager.Instance.LoadSettingsFor<DisplayUserSettings>(userID);
                    userSettings.IsChangedDefaultPwd = true;
                    SettingsManager.Instance.SaveSettingsFor<DisplayUserSettings>(userSettings, userID);


					#region Department
					try
					{
						var deptID = new Guid((Request["deptID"] ?? "").Trim());
						CoreContext.UserManager.AddUserIntoGroup(userID, deptID);
					}
					catch { }
					#endregion

                    
                    if(String.Equals(fap,"1"))                    
                        CoreContext.UserManager.AddUserIntoGroup(userID, ASC.Core.Users.Constants.GroupAdmin.ID);    
                


                }
                catch (Exception exception)
                {
                    (this.Page as confirm).ErrorMessage = HttpUtility.HtmlEncode(exception.Message);
                    return;
                }
                finally
                {
                    SecurityContext.Logout();
                }


                try
                {
                    string cookiesKey = SecurityContext.AuthenticateMe(userID.ToString(), pwd);

                    
                    CookiesManager.SetCookies(CookiesType.UserID, userID.ToString());
                    CookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey);

                    StudioNotifyService.Instance.UserHasJoin();

                }
                catch (Exception exception)
                {
                    (this.Page as confirm).ErrorMessage = HttpUtility.HtmlEncode(exception.Message);
                    return;
                }

                UserOnlineManager.Instance.RegistryOnlineUser(SecurityContext.CurrentAccount.ID);

                ProductManager.Instance.ProductGlobalHandlers.Login(SecurityContext.CurrentAccount.ID);

                Response.Redirect("~/");

            } 
        }
    }
}