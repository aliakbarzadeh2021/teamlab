using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.Web.Core;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.SearchHandlers;
using ASC.Web.Studio.Utility;
using ASC.Web.Core.Users.Activity;
using AjaxPro;
using Resources;
using System.Web.UI.WebControls;

namespace ASC.Web.Studio.UserControls.Common
{
    [AjaxNamespace("AjaxPro.WhatsNewBody")]
    public partial class WhatsNewBody : System.Web.UI.UserControl
    {
        #region Property
        public static string Location { get { return "~/UserControls/Common/WhatsNew/WhatsNewBody.ascx"; } }

        public Guid ProductId { get; set; }

        #endregion
        #region Member
        protected class ActivityContainer
        {
            public string UserProfileLink;
            public string ActionText;
            public string URL;
            public string Title;
            public string ModuleName;
            public string ModuleIconUrl;
            public string Date;
            public string AgoSentence;
        }
        #endregion
        #region Event
        protected void Page_Load(object sender, EventArgs e)
        {
            //ajax
            AjaxPro.Utility.RegisterTypeForAjax(this.GetType());
            //js
            Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "whatsnew_script", WebPath.GetPath("usercontrols/common/whatsnew/js/whatsnew.js"));

            //jTemplate
            const string jTemplateJavaScript = "jTemplateJavaScript";
            if (!Page.ClientScript.IsClientScriptBlockRegistered(jTemplateJavaScript))
            {
                Page.ClientScript.RegisterClientScriptInclude(jTemplateJavaScript, ASC.Data.Storage.WebPath.GetPath("js/jquery-jtemplates.js"));
            }

            const string WhatsNewCssStyle = "studio_whatsnew_style";
            if (!Page.ClientScript.IsClientScriptBlockRegistered(this.GetType(), WhatsNewCssStyle))
            {
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), WhatsNewCssStyle, "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebSkin.GetUserSkin().GetAbsoluteWebPath("usercontrols/common/whatsnew/css/<theme_folder>/whatsnew.css") + "\">", false);
            }
        }
        #endregion
        #region Method
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse ShowRecentActivity(Guid productId, List<Guid> moduleIds, string strFromDate, string strToDate, int type, int currentPage, Guid userOrDeptID)
        {
            AjaxResponse resp = new AjaxResponse();

            DateTime fromDate = Convert.ToDateTime(strFromDate);
            DateTime toDate = Convert.ToDateTime(strToDate);
            var actionType = UserActivityConstants.ActivityActionType;
            if (type == 0) actionType = UserActivityConstants.AllActionType;
            if (type == 1) actionType = UserActivityConstants.ContentActionType;

            var userActivity = UserActivityManager.GetUserActivities(
                    TenantProvider.CurrentTenantID,
                    userOrDeptID,
                    productId,
                    moduleIds,
                    actionType,
                    null,
                    fromDate,
                    toDate.AddDays(1));

            var activityContainer = userActivity.ConvertAll<ActivityContainer>(rec =>
                {
                    return new ActivityContainer()
                    {
                        UserProfileLink = ASC.Core.Users.StudioUserInfoExtension.RenderProfileLink(ASC.Core.CoreContext.UserManager.GetUsers(rec.UserID), rec.ProductID),
                        ActionText = rec.ActionText.ToLower(),
                        URL = CommonLinkUtility.GetFullAbsolutePath(rec.URL),
                        Title = rec.Title.HtmlEncode(),
                        ModuleName = GetModuleName(rec),
                        ModuleIconUrl = GetModuleIconUrl(rec),
                        Date = rec.Date.ToString(DateTimeExtension.DateFormatPattern),
                        AgoSentence = GetAgoSentence(rec.Date)
                    };
                });


            int CountShowOnPage = 15;

            int countTotal = activityContainer.Count;
            int amountPage = Convert.ToInt32(Math.Ceiling(countTotal / (CountShowOnPage * 1.0)));

            currentPage = currentPage > 0 ? currentPage : 1;
            if (amountPage != 0)
            {
                currentPage = currentPage <= amountPage ? currentPage : amountPage;
            }

            resp.rs10 = "5";					//CountVisiblePage 
            resp.rs11 = amountPage.ToString();
            resp.rs12 = currentPage.ToString();
            resp.rs13 = global::Resources.Resource.BackButton;
            resp.rs14 = global::Resources.Resource.NextButton;

            List<ActivityContainer> result = new List<ActivityContainer>();
            for (int i = (currentPage - 1) * CountShowOnPage; i < currentPage * CountShowOnPage && i < activityContainer.Count; i++)
                result.Add(activityContainer[i]);

            resp.rs1 = JavaScriptSerializer.Serialize(result);
            return resp;
        }

        private string GetAgoSentence(DateTime dateTime)
        {
            string result = dateTime.ToString("dd MMM yyyy");
            DateTime now = ASC.Core.Tenants.TenantUtil.DateTimeNow();
            if (result.Equals(now.ToString("dd MMM yyyy")))
                result = Resource.DrnToday;
            else
                if (result.Equals(now.AddDays(-1).ToString("dd MMM yyyy")))
                    result = Resource.DrnYesterday;
            return result;
        }

        private string GetModuleName(UserActivity userActivity)
        {
            IModule module = ProductManager.Instance.GetModuleByID(userActivity.ModuleID);
            return module == null ? "Unknown module" : module.ModuleName;
        }

        private string GetModuleIconUrl(UserActivity userActivity)
        {
            if (userActivity.ImageOptions != null && !string.IsNullOrEmpty(userActivity.ImageOptions.ImageFileName))
            {
                return WebImageSupplier.GetAbsoluteWebPath(userActivity.ImageOptions.ImageFileName, userActivity.ModuleID);
            }
            IModule module = ProductManager.Instance.GetModuleByID(userActivity.ModuleID);
            return module == null ? "Unknown module" : module.GetIconAbsoluteURL();
        }

        protected string InitModuleList()
        {
            StringBuilder sb = new StringBuilder();
            var product = ProductManager.Instance[ProductId];
            foreach (var item in WebItemManager.Instance.GetSubItems(product.ProductID))
            {
                if ((item is IModule) == false)
                    continue;

                var module = item as IModule;
                sb.Append("<div class='whatsNewFilterModule'>");
                sb.AppendFormat(@"<input id='whatsnew_modulefilter_{0}' class='whatsNewFilterModuleCheck' checked='checked' type='checkbox' onclick='javascript:WhatsNew.CheckboxToAnchor();' ", module.ModuleID);
                sb.AppendFormat("<label class='whatsNewFilterModuleItem' for='whatsnew_modulefilter_{1}' >{0}</label>", module.ModuleName.HtmlEncode(), module.ModuleID);
                sb.Append("</div>");
            }

            return sb.ToString();

        }

        #endregion
    }
}