using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxPro;
using ASC.Core;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Common;
using ASC.Web.Studio.Utility;
using ASC.Web.Core.Users;

namespace ASC.Web.Studio.Masters
{
    [AjaxNamespace("WebStudio")]
    public partial class BaseTemplate : System.Web.UI.MasterPage, IStudioMaster
    {
        protected bool _visibleSidePanel { get; set; }

        protected StudioViewSettings _panelViewSettings;

        protected bool _leftSidePanel;

        protected void Page_Load(object sender, EventArgs e)
        {
            _visibleSidePanel = true;
            _panelViewSettings = SettingsManager.Instance.LoadSettings<StudioViewSettings>(TenantProvider.CurrentTenantID);

            _leftSidePanel = LeftSidePanel.HasValue ? LeftSidePanel.Value : _panelViewSettings.LeftSidePanel;

            if (SecurityContext.IsAuthenticated)
            {
                AjaxPro.Utility.RegisterTypeForAjax(this.GetType());
                _visibleSidePanel = _panelViewSettings.VisibleSidePanel;
            }
            else
            {
                //DisabledSidePanel = true;
            }

            RegisterClientSideScript();

        }

        private void RegisterClientSideScript()
        {
            string script = string.Format(@"
<script language=""javascript"" type=""text/javascript"">
    function GetMaxImageWidth() {{
        return {0};
    }}   

    jq(document).ready(function() {{
            jq('a.fancyzoom').fancyZoom({{scaleImg: true, closeOnClick: true, directory : '{1}'}});  
    }});

</script>", System.Configuration.ConfigurationManager.AppSettings["MaxImageFCKWidth"] ?? "620", WebSkin.GetUserSkin().GetCSSFileAbsoluteWebPath("fancyzoom_img"));


            StringBuilder sb = new StringBuilder();


            //Skin images
            sb.Append("<script language=\"javascript\"  type=\"text/javascript\">");
            sb.Append(JsSkinHash.GetJs());
            sb.Append("</script>");

            sb.Append("<script language=\"javascript\" src=\"" + WebPath.GetPath("js/auto/jquery_full.js") + "\" type=\"text/javascript\"></script>");
            sb.Append("<script language=\"javascript\" src=\"" + WebPath.GetPath("js/auto/jquery.cookies.js") + "\" type=\"text/javascript\"></script>");
            sb.Append("<script language=\"javascript\" src=\"" + WebPath.GetPath("js/auto/fancyzoom.min.js") + "\" type=\"text/javascript\"></script>");

            sb.Append("<script language=\"javascript\"  type=\"text/javascript\"> var jq = jQuery.noConflict(); </script>");
            sb.Append("<script language=\"javascript\" src=\"" + WebPath.GetPath("js/auto/common.js") + "\" type=\"text/javascript\"></script>");
            sb.Append("<script language=\"javascript\" src=\"" + WebPath.GetPath("js/auto/profile_info_tooltip.js") + "\" type=\"text/javascript\"></script>");
            sb.Append("<script language=\"javascript\" src=\"" + WebPath.GetPath("js/auto/management.js") + "\" type=\"text/javascript\"></script>");
            sb.Append("<script language=\"javascript\" src=\"" + WebPath.GetPath("js/auto/asc.anchorcontroller.js") + "\" type=\"text/javascript\"></script>");
            sb.Append("<script language=\"javascript\" src=\"" + WebPath.GetPath("js/auto/asc.xsltmanager.js") + "\" type=\"text/javascript\"></script>");
            sb.Append("<script language=\"javascript\" src=\"" + WebPath.GetPath("js/auto/jquery.datepick.lang.js") + "\" type=\"text/javascript\"></script>");


            sb.Append("<script language=\"javascript\"  type=\"text/javascript\">");

            sb.Append(" jq.datepick.setDefaults(jq.datepick.regional['" + CoreContext.TenantManager.GetCurrentTenant().GetCulture().Name + "']); ");

            sb.Append(" StudioManager.RemoveMessage=\"" + Resources.Resource.DeleteButton + "\"; ");
            sb.Append(" StudioManager.ErrorFileSizeLimit=\"" + Resources.Resource.ErrorFileSizeLimitText + "\"; ");
            sb.Append(" StudioManager.ErrorFileEmpty=\"" + Resources.Resource.ErrorFileEmptyText + "\"; ");

            sb.Append(" CommonSubscriptionManager.ConfirmMessage=\"" + Resources.Resource.ConfirmMessage + "\"; ");
            sb.Append(" AuthManager.ConfirmMessage=\"" + Resources.Resource.ConfirmMessage + "\"; ");


            if (SetupInfo.WorkMode == WorkMode.Promo)
            {
                sb.Append(" PromoMode = true; PromoActionURL='" + SetupInfo.PromoActionURL + "'; ");
            }

            sb.Append(" var StudioUserProfileInfo = new PopupBox('pb_StudioUserProfileInfo',320,140,'tintMedium','borderBase', 'WebStudio.GetUserInfo'); ");


            sb.Append("</script>");

            sb.Append(script);

            mainScript.Text = sb.ToString();
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string SaveSidePanelState(bool visible)
        {
            StudioViewSettings personalView = SettingsManager.Instance.LoadSettingsFor<StudioViewSettings>(SecurityContext.CurrentAccount.ID);
            personalView.VisibleSidePanel = visible;
            if (SettingsManager.Instance.SaveSettingsFor<StudioViewSettings>(personalView, SecurityContext.CurrentAccount.ID))
            {
                return "1";
            }
            return "0";
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public JavaScriptObject GetUsersInfo(Guid userID, Guid productID, Object userValue)
        {
            const String host = "teamlab";

            List<UserInfo> users = new List<UserInfo>(CoreContext.UserManager.GetUsers());
            users = users.SortByUserName();

            JavaScriptArray jsUsers = new JavaScriptArray();
            for (int i = 0, n = users.Count; i < n; i++)
            {
                JavaScriptObject jsUser = new JavaScriptObject();
                jsUser.Add("id", new JavaScriptString(users[i].ID.ToString()));
                jsUser.Add("departmentId", new JavaScriptString(users[i].GetUserDepartment().ID.ToString()));
                jsUser.Add("jid", new JavaScriptString(users[i].UserName + "@" + host));
                jsUser.Add("userName", new JavaScriptString(users[i].UserName));
                jsUser.Add("displayName", new JavaScriptString(users[i].DisplayUserName(true)));
                jsUser.Add("profileURL", new JavaScriptString(users[i].GetUserProfilePageURL(productID)));
                //            jsUser.Add("email", new JavaScriptString(users[i].Email));
                jsUser.Add("photo", new JavaScriptString(users[i].GetPhotoURL()));
                jsUser.Add("bigPhoto", new JavaScriptString(users[i].GetBigPhotoURL()));
                jsUser.Add("mediumPhoto", new JavaScriptString(users[i].GetMediumPhotoURL()));
                jsUser.Add("smallPhoto", new JavaScriptString(users[i].GetSmallPhotoURL()));
                jsUser.Add("title", new JavaScriptString(HttpUtility.HtmlEncode(users[i].Title)));
                jsUser.Add("departmentTitle", new JavaScriptString(HttpUtility.HtmlEncode(users[i].Department)));
                //            jsUser.Add("departmentURL", new JavaScriptString(CommonLinkUtility.GetDepartment(productID, users[i].GetUserDepartment().ID)));
                //            jsUser.Add("workFromDate", new JavaScriptString(users[i].WorkFromDate == null ? String.Empty : users[i].WorkFromDate.Value.ToShortDateString()));

                jsUsers.Add(jsUser);
            }
            JavaScriptObject jsResult = new JavaScriptObject();
            jsResult.Add("users", jsUsers);
            jsResult.Add("userValue", new JavaScriptString(userValue.ToString()));

            return jsResult;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public JavaScriptObject GetFullUserInfo(Guid userID, Guid productID, Object userValue)
        {
            UserInfo userInfo = CoreContext.UserManager.GetUsers(userID);

            JavaScriptObject jsUserInfo = new JavaScriptObject();
            jsUserInfo.Add("id", new JavaScriptString(userInfo.ID.ToString()));
            jsUserInfo.Add("departmentId", new JavaScriptString(userInfo.GetUserDepartment().ID.ToString()));
            jsUserInfo.Add("userName", new JavaScriptString(userInfo.UserName));
            jsUserInfo.Add("displayName", new JavaScriptString(userInfo.DisplayUserName(true)));
            jsUserInfo.Add("profileURL", new JavaScriptString(userInfo.GetUserProfilePageURL(productID)));
            jsUserInfo.Add("email", new JavaScriptString(userInfo.Email));
            jsUserInfo.Add("photo", new JavaScriptString(userInfo.GetPhotoURL()));
            jsUserInfo.Add("bigPhoto", new JavaScriptString(userInfo.GetBigPhotoURL()));
            jsUserInfo.Add("mediumPhoto", new JavaScriptString(userInfo.GetMediumPhotoURL()));
            jsUserInfo.Add("smallPhoto", new JavaScriptString(userInfo.GetSmallPhotoURL()));
            jsUserInfo.Add("title", new JavaScriptString(HttpUtility.HtmlEncode(userInfo.Title)));
            jsUserInfo.Add("departmentTitle", new JavaScriptString(HttpUtility.HtmlEncode(userInfo.Department)));
            jsUserInfo.Add("departmentURL", new JavaScriptString(CommonLinkUtility.GetDepartment(productID, userInfo.GetUserDepartment().ID)));
            jsUserInfo.Add("workFromDate", new JavaScriptString(userInfo.WorkFromDate == null ? String.Empty : userInfo.WorkFromDate.Value.ToShortDateString()));
            jsUserInfo.Add("userValue", new JavaScriptString(userValue.ToString()));

            return jsUserInfo;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public void SaveGettingStartedState(bool disableGettingStarted)
        {
            var settings = SettingsManager.Instance.LoadSettingsFor<DisplayUserSettings>(SecurityContext.CurrentAccount.ID);
            settings.IsDisableGettingStarted = disableGettingStarted;
            SettingsManager.Instance.SaveSettingsFor<DisplayUserSettings>(settings, SecurityContext.CurrentAccount.ID);

        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse GetUserInfo(Guid userID, Guid productID, string popupBoxID)
        {
            AjaxResponse resp = new AjaxResponse();
            resp.rs1 = popupBoxID;

            UserInfo userInfo = CoreContext.UserManager.GetUsers(userID);
            StringBuilder sb = new StringBuilder();
            sb.Append("<div style=\"overflow: hidden; margin-right: 10px; padding: 10px 0pt 10px 10px;\"><table cellspacing='0' cellpadding='0' style='width:100%;'><tr valign=\"top\">");

            //avatar
            sb.Append("<td style=\"width:90px;\">");
            sb.Append("<img alt=\"\" class='userPhoto' src=\"" + userInfo.GetBigPhotoURL() + "\"/>");
            sb.Append("</td>");

            sb.Append("<td style='padding-left:10px;'>");

            //name
            sb.Append("<div>");
            sb.Append("<a class='linkHeaderLight' href='" + userInfo.GetUserProfilePageURL(productID) + "'>" + userInfo.DisplayUserName(true) + "</a>");
            sb.Append("</div>");

            //department
            GroupInfo dep = userInfo.GetUserDepartment();
            if (dep != null)
            {
                sb.Append("<div style=\"margin-top:6px;\">");
                sb.Append("<a href='" + CommonLinkUtility.GetDepartment(productID, dep.ID) + "'>" + dep.Name.HtmlEncode() + "</a>");
                sb.Append("</div>");
            }

            //title
            sb.Append("<div style=\"margin-top:6px;\">");
            sb.Append(HttpUtility.HtmlEncode(userInfo.Title));
            sb.Append("</div>");

            //communications
            sb.Append("<div style=\"margin-top:6px;\">");
            sb.Append(userInfo.RenderUserCommunication());
            sb.Append("</div>");

            sb.Append("</td>");
            sb.Append("</tr></table></div>");

            resp.rs2 = sb.ToString();
            return resp;
        }

        protected string GetCurrentLanguage()
        {
            return System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        }

        protected string RenderStatRequest()
        {
            if (!SetupInfo.StatisticEnable)
                return "";

            var page = HttpUtility.UrlEncode(this.Page.AppRelativeVirtualPath.Replace("~", ""));
            return String.Format("<img style=\"display:none;\" src=\"{0}\"/>", SetupInfo.StatisticTrackURL + "?ver=3.1&page=" + page);
        }

        protected string RenderHTMLInjections()
        {
            if (!SecurityContext.IsAuthenticated)
                return "";

            StringBuilder sb = new StringBuilder();
            foreach (var product in ProductManager.Instance.Products)
            {
                var injectionProvider = product.GetInjectionProvider();
                if (injectionProvider != null)
                    sb.Append(injectionProvider.GetInjection());

                if (product.Modules == null)
                    continue;
                foreach (var module in product.Modules)
                {
                    injectionProvider = module.GetInjectionProvider();
                    if (injectionProvider != null)
                        sb.Append(injectionProvider.GetInjection());
                }

            }
            return sb.ToString();
        }

        protected string RenderCustomScript()
        {
            var sb = new StringBuilder();
            //custom scripts
            foreach (var script in SetupInfo.CustomScripts)
            {
                if (!String.IsNullOrEmpty(script))
                    sb.Append("<script language=\"javascript\" src=\"" + script + "\" type=\"text/javascript\"></script>");
            }

            return sb.ToString();
        }


        #region Keep session alive during FCKEditor is opened
        /// <summary>
        /// This method is called from common.js file when document is loaded and the page contains FCKEditor.
        /// It is used to keep session alive while FCKEditor is opened and not empty.
        /// </summary>
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public void KeepSessionAlive()
        {
            try
            {
                HttpContext.Current.Session["KeepSessionAlive"] = DateTime.Now;
            }
            catch { }
        }
        #endregion


        #region IStudioMaster Members

        /// <summary>
        /// Block side panel
        /// </summary>
        public bool DisabledSidePanel { get; set; }

        public bool? LeftSidePanel { get; set; }

        public PlaceHolder ContentHolder
        {
            get { return _contentHolder; }
        }

        public PlaceHolder SideHolder
        {
            get { return _sideHolder; }
        }

        public PlaceHolder TitleHolder
        {
            get { return _titleHolder; }
        }

        public PlaceHolder FooterHolder
        {
            get { return _footerHolder; }
        }

        public ScriptManager ScriptManager
        {

            get { return ScriptManager1; }

        }

        #endregion

        protected string RenderNotifyBar()
        {
            var sb = new StringBuilder();
            var script = SetupInfo.NotifyAddress;
            if (!string.IsNullOrEmpty(script) && SecurityContext.IsAuthenticated)
            {
                sb.AppendFormat("<script>jq(function(){{jq.getScript('{0}');}});</script>",
                                string.Format(script, SecurityContext.CurrentAccount.ID, CoreContext.TenantManager.GetCurrentTenant().Language.ToLowerInvariant()));
            }
            return sb.ToString();
        }
    }
}