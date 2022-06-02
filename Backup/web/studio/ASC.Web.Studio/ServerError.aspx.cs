using System;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using ASC.Web.Controls;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Masters;

namespace ASC.Web.Studio
{
    public partial class ServerError : MainPage
    {
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            base.SetProductMasterPage();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            IStudioMaster master = this.Master as IStudioMaster;
            if (this.Master is IStudioMaster)
            {
                Container container = new Container() { Body = new PlaceHolder(), Header = new PlaceHolder() };
                master.ContentHolder.Controls.Add(container);
                container.BreadCrumbs.Add(new BreadCrumb() { Caption = Resources.Resource.ServerErrorTitle });

                master.DisabledSidePanel = true;

                StringBuilder sb = new StringBuilder();
                sb.Append(Resources.Resource.ServerErrorText);
                sb.Append("<div style=\"margin-top:20px;\"><a href=\""+VirtualPathUtility.ToAbsolute("~/")+"\">"+Resources.Resource.BackToHomeLink+"</a></div>");

                container.Body.Controls.Add(new Literal() { Text = sb.ToString() });

                Title = HeaderStringHelper.GetPageTitle(Resources.Resource.ServerErrorTitle, container.BreadCrumbs);
            }
        }
    }
}
