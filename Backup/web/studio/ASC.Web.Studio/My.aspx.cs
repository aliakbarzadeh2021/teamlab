using System;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Core.WebZones;
using ASC.Web.Studio.Masters;
using ASC.Web.Studio.UserControls.Users;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio
{
	public partial class MyStaff : MainPage
	{
        protected void Page_Load(object sender, EventArgs e)
        {
            if (SecurityContext.DemoMode || (CoreContext.TenantManager.GetCurrentTenant().Public && !SecurityContext.CurrentAccount.IsAuthenticated))
                Response.Redirect("~/default.aspx");

            (this.Master as IStudioMaster).DisabledSidePanel = true;
            var _userInfo = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            var type = Request["type"] ?? "";

            var myToolsItems = WebItemManager.Instance.GetItems(WebZoneType.MyTools);
            IRenderMyTools myToolsRender = null;
            foreach (var item in myToolsItems)
            {
                myToolsRender = WebItemManager.Instance[item.ID] as IRenderMyTools;
                if (myToolsRender != null && String.Equals(myToolsRender.ParameterName, type, StringComparison.InvariantCultureIgnoreCase))
                    break;

                myToolsRender = null;
            }

            if (String.Equals(type, MyStaffType.Activity.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {

                _myStaffContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = Resources.Resource.RecentActivity });

                bool isFirst = true;
                foreach (var product in WebItemManager.Instance.GetItems(WebZoneType.All).OfType<IProduct>())
                {
                    if (product.Context != null && product.Context.UserActivityControlLoader != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("<div id='studio_product_activityBox_" + product.ProductID + "' class='borderBase tintMedium clearFix' style='border-left:none; border-right:none; margin-top:-1px; padding:10px;'>");
                        sb.Append("<div class='headerBase' style='float:left; cursor:pointer;' onclick=\"StudioManager.ToggleProductActivity('" + product.ProductID + "');\">");
                        string logoURL = product.GetIconAbsoluteURL();
                        if (!String.IsNullOrEmpty(logoURL))
                            sb.Append("<img alt='' style='margin-right:5px;' align='absmiddle' src='" + logoURL + "'/>");
                        sb.Append(product.ProductName.HtmlEncode());
                        sb.Append("<img alt='' align='absmiddle' id='studio_activityProductState_" + product.ProductID + "' style='margin-left:15px;'  src='" + WebImageSupplier.GetAbsoluteWebPath(isFirst ? "collapse_down_dark.png" : "collapse_right_dark.png") + "'/>");
                        sb.Append("</div>");
                        sb.Append("</div>");
                        sb.Append("<div id=\"studio_product_activity_" + product.ProductID + "\" style=\"padding-left:40px; " + (isFirst ? "" : "display:none;") + " padding-top:20px;\">");

                        _contentHolder.Controls.Add(new Literal() { Text = sb.ToString() });
                        var activityControl = product.Context.UserActivityControlLoader.LoadControl(_userInfo.ID);
                        _contentHolder.Controls.Add(activityControl);

                        sb = new StringBuilder();
                        sb.Append("</div>");
                        _contentHolder.Controls.Add(new Literal() { Text = sb.ToString() });

                        isFirst = false;
                    }
                }

            }
            else if (String.Equals(type, MyStaffType.Subscriptions.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
             
                _myStaffContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = Resources.Resource.Subscriptions });
                var userSubscriptions = LoadControl(UserSubscriptions.Location) as UserSubscriptions;
                _contentHolder.Controls.Add(userSubscriptions);

            }           

            else if (myToolsRender != null)
            {

                _myStaffContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = myToolsRender.TabName });
                var control = myToolsRender.LoadMyToolsControl(this);
                _contentHolder.Controls.Add(control);
            }
            else
            {
                _myStaffContainer.BreadCrumbs.Add(new ASC.Web.Controls.BreadCrumb() { Caption = _userInfo.DisplayUserName() });

                var userProfileControl = LoadControl(UserProfileControl.Location) as UserProfileControl;
                userProfileControl.UserInfo = _userInfo;
                userProfileControl.MyStaffMode = true;

                _contentHolder.Controls.Add(userProfileControl);

            }

            this.Title = HeaderStringHelper.GetPageTitle(Resources.Resource.MyStaff, null, null);

        }
	}
}
