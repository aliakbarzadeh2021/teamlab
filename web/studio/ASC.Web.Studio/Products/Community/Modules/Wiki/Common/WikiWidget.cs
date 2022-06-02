using System;
using System.Collections.Generic;
using System.Web;
using AjaxPro;
using System.Web.UI.WebControls;
using ASC.Web.Core.Utility.Settings;
using ASC.Core;
using System.Text;
using ASC.Web.UserControls.Wiki.Data;
using ASC.Web.UserControls.Wiki;
using ASC.Web.Community.Wiki.Resources;
using ASC.Web.UserControls.Wiki.Handlers;
using System.Web.Configuration;
using System.Web.Hosting;
using ASC.Core.Users;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Controls.Dashboard;


namespace ASC.Web.Community.Wiki.Common
{
    
        [AjaxNamespace("WikiWidget")]
        [WidgetPosition(0, 3)]
        public class WikiWidget : WebControl, IContextInitializer
        {
			private static string connectionStringName;
            private static HttpContext _context = null;
            protected static HttpContext m_Context
            {
                get
                {
                    return _context;
                }
                set
                {
                    if(_context == null && value != null)
                    {
                        _context = value;
                    }
                }
            }

            protected override void OnLoad(EventArgs e)
            {
                m_Context = HttpContext.Current;
                base.OnLoad(e);
                Utility.RegisterTypeForAjax(typeof(WikiWidget));
				if (string.IsNullOrEmpty(connectionStringName))
				{
					WikiSection section = (WikiSection)WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath + WikiManager.WikiSectionConfig).GetSection("wikiSettings");
					connectionStringName = section.DB.ConnectionStringName;
				}
				PagesProvider.SetConnectionStringName(connectionStringName);
            }

            protected override void RenderContents(System.Web.UI.HtmlTextWriter writer)
            {
                base.RenderContents(writer);
                writer.Write(RenderWidget());
            }

            private static int TenantId
            {
                get
                {
                    return TenantProvider.CurrentTenantID;
                }
            }

            private static string RenderWidget()
            {
                WikiWidgetSettings widgetSettings = SettingsManager.Instance.LoadSettingsFor<WikiWidgetSettings>(SecurityContext.CurrentAccount.ID);

                StringBuilder widget = new StringBuilder();
                List<Pages> pages = PagesProvider.PagesGetTopNewAndUserCreated(widgetSettings.NewPageCount, TenantId);

                widget.Append("<div id=\"Feed_DataContent\">");
                
                var isEmpty = true;
                foreach (Pages page in pages)
                {
                    if (!Guid.Empty.Equals(page.UserID))
                    {
                        isEmpty = false;
                        widget.Append(@"<div style=""padding-bottom: 15px;"">");
                        widget.Append("<table cellspacing='0' cellpadding='0' border='0'><tr valign='top'><td width='25'>");
                        widget.AppendFormat(@"<span class=""textMediumDescribe"">{0}<br/>{1}</span>", page.Date.ToShortDayMonth(), page.Date.ToShortTimeString());
                        widget.Append("</td>");
                        widget.Append("<td>");
                        widget.Append("<div style=\"padding-left: 10px;\">");
                        widget.AppendFormat(@"<a href=""{0}"">{1}</a>", ActionHelper.GetViewPagePath(VirtualPathUtility.ToAbsolute(WikiManager.ViewVirtualPath), page.PageName), string.IsNullOrEmpty(page.PageName) ? WikiResource.menu_MainPage : HttpUtility.HtmlEncode(page.PageName));
                        widget.AppendFormat("<div style=\"margin-top:5px;\">{0}</div>", CoreContext.UserManager.GetUsers(page.UserID).RenderProfileLink(ASC.Web.Community.Product.CommunityProduct.ID));
                        widget.Append("</div>");
                        widget.Append("</td>");
                        widget.Append("</tr>");
                        widget.Append("</table>");
                        widget.Append(@"</div>");
                    }
                }

                if (isEmpty)
                {
                    widget.Append("<div class=\"empty-widget\" style=\"padding:40px; text-align: center;\">" + 
                        String.Format(Resources.WikiResource.NoPagesWidgetMessage, 
                        "<div style=\"padding-top:3px;\"><a class=\"promoAction\" href=\""+WikiShortcutProvider.GetCreateContentPageUrl()+"\">",
                        "</a></div>") + "</div>");
                }
                else
                {
                    widget.Append(@"<div style=""margin-top: 10px;"">");
                    widget.AppendFormat(@"<a href=""{1}"">{0}</a>", WikiResource.wikiWidgetAllPages, VirtualPathUtility.ToAbsolute(WikiManager.BaseVirtualPath + "/ListPages.aspx").ToLower());
                    widget.Append(@"</div>");
                }

                widget.Append("</div>");
                return widget.ToString();
            }

            [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
            public static string UpdateContent()
            {
                return RenderWidget();
            }


            #region IContextInitializer Members

            public void InitializeContext(HttpContext context)
            {
                m_Context = context;
            }

            #endregion
        }
    
}
