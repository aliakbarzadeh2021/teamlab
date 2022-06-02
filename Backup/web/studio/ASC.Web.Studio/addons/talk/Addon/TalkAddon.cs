using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Core;
using ASC.Web.Core;
using ASC.Web.Core.WebZones;
using ASC.Xmpp.Common;

namespace ASC.Web.Talk.Addon
{
    [WebZoneAttribute(WebZoneType.StartProductList | WebZoneType.CustomProductList)]
    public class TalkAddon : AbstractAddon, IRenderCustomNavigation, IRenderControlNavigation
    {
        public static Guid AddonID { get { return new Guid("{BF88953E-3C43-4850-A3FB-B1E43AD53A3E}"); } }

        private AddonContext _context;

        public override AddonContext Context
        {
            get { return _context; }
        }

        public override string Description
        {
            get { return Resources.TalkResource.TalkDescription; }
        }

        public override Guid ID
        {
            get { return AddonID; }
        }

        public override void Init(AddonContext context)
        {
            _context = context;
            _context.ThemesFolderVirtualPath = "~/addons/talk/App_Themes";
            _context.ImageFolder = "images";
            _context.IconFileName = "product_logo.png";
            _context.LargeIconFileName = "product_logolarge.png";
            _context.DefaultSortOrder = 40;
        }

        public override string Name
        {
            get { return Resources.TalkResource.ProductName; }
        }

        public override void Shutdown()
        {

        }

        public override string StartURL
        {
            get { return BaseVirtualPath + "/default.aspx"; }
        }

        public const string BaseVirtualPath = "~/addons/talk";

        public static string GetClientUrl()
        {
            return VirtualPathUtility.ToAbsolute("~/addons/talk/jabberclient.aspx");
        }

        public static string GetTalkClientURL()
        {
            return "javascript:window.ASC.Controls.JabberClient.open('" + VirtualPathUtility.ToAbsolute("~/addons/talk/jabberclient.aspx") + "')";
        }

        #region IRenderCustomNavigation Members

        public string RenderCustomNavigation()
        {
            //            var msgStr = GetMessageStr();
            //            var sb = new StringBuilder();

            //            sb.Append("<style>");
            //            sb.Append(
            //                @".studioTopNavigationPanel .systemSection .itemBox.talk a
            //                        {
            //                          background-image:url(" +
            //                WebImageSupplier.GetAbsoluteWebPath("messages_user.png", TalkAddon.AddonID) +
            //                ");}" +
            //                @".studioTopNavigationPanel .systemSection .itemBox.talk a.newmessage
            //                        {
            //                          background-image:url(" +
            //                WebImageSupplier.GetAbsoluteWebPath("messages_user.png", TalkAddon.AddonID) +
            //                ");}");//Replace with gif

            //            sb.Append("</style>");
            //            sb.Append("<li class=\"itemBox talk\" style=\"float:right;\">");
            //            sb.Append("<a href=\"" + GetTalkClientURL() + "\">");
            //            sb.Append("<span id=\"talk_msg_count\">"+msgStr+"</span>");
            //            sb.Append("</a>");
            //            sb.Append("</li>");

            //            return sb.ToString();
            return string.Empty;
        }

        public static int GetMessageCount(string username)
        {
            try
            {
                return new JabberServiceClient().GetNewMessagesCount(username, CoreContext.TenantManager.GetCurrentTenant().TenantId);
            }
            catch { }
            return 0;
        }

        public static string GetMessageStr()
        {
            return Resources.TalkResource.Chat;
        }

        public System.Web.UI.Control LoadCustomNavigationControl(System.Web.UI.Page page)
        {
            return null;
        }

        #endregion

        public string RenderCustomNavigation(Page page)
        {
            var sb = new StringBuilder();
            using (var tw = new StringWriter(sb))
            {
                using (var hw = new HtmlTextWriter(tw))
                {
                    var ctrl = page.LoadControl(UserControls.TalkNavigationItem.Location);
                    ctrl.RenderControl(hw);
                    return sb.ToString();
                }
            }
        }
    }
}
