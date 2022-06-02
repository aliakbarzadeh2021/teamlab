using System;
using System.Web;
using ASC.Web.Core.Security.Ajax;
using AjaxPro;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Utility;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Users;
using ASC.Core;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("MySettings")]
    public partial class PwdTool : System.Web.UI.UserControl
    {
        public static string Location { get { return "~/UserControls/Management/PwdTool.ascx"; } }

        public Guid UserID { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            _userPwdContainer.Options.IsPopup = true;
            _pwdRemainderContainer.Options.IsPopup = true;
            _userPwdContainer.Options.InfoMessageText = "";
            _userPwdContainer.Options.InfoType = ASC.Web.Controls.InfoType.Info;
            _pwdRemainderContainer.Options.InfoMessageText = "";
            _pwdRemainderContainer.Options.InfoType = ASC.Web.Controls.InfoType.Info;

            AjaxPro.Utility.RegisterTypeForAjax(this.GetType());
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse ChangePwd(Guid userID, string newPwd, string repwd)
        {
            AjaxResponse resp = new AjaxResponse();
            newPwd = newPwd == null ? String.Empty : newPwd;
            newPwd = newPwd.Trim();
            if (newPwd == String.Empty)
            {
                resp.rs1 = "0";
                resp.rs2 = "<div>" + Resources.Resource.ErrorPasswordEmpty + "</div>";
                return resp;
            }

            if (String.Compare(newPwd, repwd) != 0)
            {
                resp.rs1 = "0";
                resp.rs2 = "<div>" + Resources.Resource.ErrorMissMatchPwd + "</div>";
                return resp;
            }

            try
            {
                UserManagerWrapper.SetUserPassword(userID, newPwd);

                if (SecurityContext.CurrentAccount.ID.Equals(userID))
                {
                    var userSettings = SettingsManager.Instance.LoadSettingsFor<DisplayUserSettings>(SecurityContext.CurrentAccount.ID);
                    userSettings.IsChangedDefaultPwd = true;
                    SettingsManager.Instance.SaveSettingsFor<DisplayUserSettings>(userSettings, SecurityContext.CurrentAccount.ID);
                }
                
                resp.rs1 = "1";
                resp.rs2 = Resources.Resource.SuccessfullyChangePasswordMessage;
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs2 = "<div>" + HttpUtility.HtmlEncode(e.Message) + "</div>";
            }
            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        [AjaxSecurityPassthroughAttribute]
        public AjaxResponse RemaindPwd(string email)
        {
            AjaxResponse responce = new AjaxResponse();
            responce.rs1 = "0";

            if (!email.TestEmailRegex())
            {
                responce.rs2 = "<div>" + Resources.Resource.ErrorNotCorrectEmail + "</div>";
                return responce;
            }

            try
            {
                UserManagerWrapper.SendUserPassword(email);

                responce.rs1 = "1";
                responce.rs2 = String.Format(Resources.Resource.MessageYourPasswordSuccessfullySendedToEmail, email);
            }
            catch (Exception exc)
            {
                responce.rs2 = "<div>" + HttpUtility.HtmlEncode(exc.Message) + "</div>";
            }

            return responce;
        }
    }
}