using System;
using AjaxPro;
using ASC.Forum;
using ASC.Web.Controls;
using ASC.Web.Studio;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Forum.Common;
using System.Web;

namespace ASC.Web.Community.Forum
{    
    public partial class Topics : MainPage                                           
    {  
        protected void Page_Load(object sender, EventArgs e)
        {
            ForumManager.Instance.SetCurrentPage(ForumPage.TopicList);           
           
            int idThread = 0;
            if (!String.IsNullOrEmpty(Request["f"]))
            {
                try
                {
                    idThread = Convert.ToInt32(Request["f"]);
                }
                catch { idThread = 0; }
            }
          

            if (idThread == 0)
                Response.Redirect("default.aspx");

            var thread = ForumDataProvider.GetThreadByID(TenantProvider.CurrentTenantID, idThread);

            if (thread==null)
                Response.Redirect("default.aspx");

            var topicsControl = LoadControl(ForumManager.Settings.UserControlsVirtualPath + "/TopicListControl.ascx") as ASC.Web.UserControls.Forum.TopicListControl;
            topicsControl.SettingsID = ForumManager.Settings.ID;
            topicsControl.ThreadID = thread.ID;

            if (thread.TopicCount > 0)
                topicsHolder.Controls.Add(topicsControl);
            else
            {
                topicsHolder.Controls.Add(new NotFoundControl() {Text = Resources.ForumResource.NoTopicsMessage,
                                                                 HasLink = ForumManager.Instance.ValidateAccessSecurityAction(ForumAction.TopicCreate, thread),
                                                                 LinkFormattedText = string.Format(Resources.ForumResource.CreateTopicMessage,"<a href=\"{0}\">","</a>"),
                                                                 LinkURL = VirtualPathUtility.ToAbsolute(ForumManager.BaseVirtualPath + "/newpost.aspx") + "?f=" + thread.ID.ToString() + "&m=0"});
            }

            Utility.RegisterTypeForAjax(typeof(Subscriber));
            var subscriber = new Subscriber();

            var isThreadSubscribe = subscriber.IsThreadSubscribe(thread.ID);

            var master = this.Master as ForumMasterPage;
            master.ActionsPlaceHolder.Controls.Add(new HtmlMenuItem(subscriber.RenderThreadSubscription(!isThreadSubscribe, thread.ID)));

         
            //bread crumbs
            var breadCrumbs = (this.Master as ForumMasterPage).BreadCrumbs;
            breadCrumbs.Add(new BreadCrumb() { Caption = Resources.ForumResource.ForumsBreadCrumbs, NavigationUrl = "default.aspx" });
            breadCrumbs.Add(new BreadCrumb() { Caption = HttpUtility.HtmlEncode(thread.Title) });

            this.Title = HeaderStringHelper.GetPageTitle(Resources.ForumResource.AddonName, breadCrumbs);
        }

    }
}
