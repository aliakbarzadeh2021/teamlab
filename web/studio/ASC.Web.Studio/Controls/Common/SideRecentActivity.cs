using System;
using System.ComponentModel;
using System.Web.UI;
using ASC.Web.Core.Users.Activity;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.UserControls.Common;

namespace ASC.Web.Studio.Controls.Common
{
    /// <summary>
    /// Side recent activity
    /// </summary>
    [ToolboxData("<{0}:SideRecentActivity runat=server></{0}:SideRecentActivity>")]
    [PersistChildren(false)]
    public class SideRecentActivity : SideContainer
    {
        [Category("Style"), PersistenceMode(PersistenceMode.Attribute)]
        public string ItemCSSClass { get; set; }

        public int TenantId { get; set; }
        public int MaxItems { get; set; }

        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public Guid ModuleId { get; set; }

        public SideRecentActivity()
        {
            this.UserId = Guid.Empty;
            this.ProductId = Guid.Empty;
            this.ModuleId = Guid.Empty;
            this.MaxItems = 5;
            this.Title = Resources.Resource.RecentActivity;
            this.ImageURL = WebImageSupplier.GetAbsoluteWebPath("recent_activity.png");
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.Title = (!String.IsNullOrEmpty(Title) ? this.Title.HtmlEncode() : Resources.Resource.RecentActivity);

            RecentActivityBox activityBox = (RecentActivityBox)TemplateControl.LoadControl(RecentActivityBox.Location);
            activityBox.userActivityList = UserActivityManager.GetUserActivities(
                TenantId, 
                UserId, 
                ProductId, 
                new[] { ModuleId }, 
                UserActivityConstants.AllActionType, 
                null, 
                0, MaxItems);
            activityBox.MaxLengthTitle = 20;
            activityBox.ItemCSSClass = ItemCSSClass;
            this.Controls.Add(activityBox);

            this.Controls.Add(new LiteralControl()
                                    {
                                        Text =
                                        string.Format("<div style='margin:10px 20px 0 20px;'><a href='{0}'>{1}</a></div>",
                                                   WhatsNew.GetUrlForModule(ProductId, ModuleId),
                                                   Resources.Resource.ToWhatsNewPage)
                                    });
        }
    }
}