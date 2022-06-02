﻿using System;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Controls;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Masters;
using ASC.Web.Studio.UserControls.Company;
using ASC.Web.Studio.UserControls.Users;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio
{
    public partial class UserProfile : MainPage
    {
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            base.SetProductMasterPage();
        }

        private UserProfileType _userProfileType = UserProfileType.General;
        private Guid _productID = Guid.Empty;
        private UserInfo _userInfo;
        private Guid _userID;

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(Employee));
        }

        protected void Page_PreLoad(object sender, EventArgs e)
        {
            if (this.Master is IStudioMaster)
            {
                var master = this.Master as IStudioMaster;

                #region define profile type
                if (!String.IsNullOrEmpty(Request["type"]))
                {
                    try
                    {
                        _userProfileType = (UserProfileType)Convert.ToInt32(Request["type"]);
                    }
                    catch
                    {
                        _userProfileType = UserProfileType.General;
                    }
                }
                #endregion

                _userID = SecurityContext.CurrentAccount.ID;
                _productID = GetProductID();

                #region find request user
                _userInfo = CoreContext.UserManager.GetUserByUserName(Request[CommonLinkUtility.ParamName_UserUserName]);
                if (_userInfo == null || _userInfo == ASC.Core.Users.Constants.LostUser)
                {
                    if (!String.IsNullOrEmpty(Request["uid"]))
                    {
                        try
                        {
                            _userID = new Guid(Request["uid"]);
                        }
                        catch
                        {
                            _userID = SecurityContext.CurrentAccount.ID;
                        }
                    }

                    if (!CoreContext.UserManager.UserExists(_userID))
                    {
                        //user not found
                        Response.Redirect(CommonLinkUtility.GetEmployees(_productID));
                        return;
                    }
                    else
                        _userInfo = CoreContext.UserManager.GetUsers(_userID);

                }
                else
                    _userID = _userInfo.ID;

                #endregion

                bool self = SecurityContext.CurrentAccount.ID.Equals(_userID);

                Container container = new Container() { Body = new PlaceHolder(), Header = new PlaceHolder() };
                master.ContentHolder.Controls.Add(container);

                container.BreadCrumbs.Add(new BreadCrumb() { Caption = ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<Resources.Resource>("Employees"), NavigationUrl = CommonLinkUtility.GetEmployees(_productID) });
                container.BreadCrumbs.Add(new BreadCrumb() { Caption = (self ? Resources.Resource.MyProfile : (_userInfo.DisplayUserName(true))), NavigationUrl = CommonLinkUtility.GetEmployees(_productID) });

                Title = HeaderStringHelper.GetPageTitle(ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<Resources.Resource>("Employees"), container.BreadCrumbs);

                //user card
                UserProfileControl userCard = (UserProfileControl)LoadControl(UserProfileControl.Location);
                userCard.UserInfo = _userInfo;
                container.Body.Controls.Add(new Literal()
                {
                    Text = "<div class=\"headerBase borderBase\" style=\"padding: 0px 0px 5px 15px; border-top:none; border-right:none; border-left:none;\">" + Resources.Resource.PersonalInfo +
                           "</div><div style=\"padding:15px 0px 0px 0px\">"
                });
                container.Body.Controls.Add(userCard);
                container.Body.Controls.Add(new Literal() { Text = "</div><div style=height:20px;>&nbsp;</div>" });


                var product = ProductManager.Instance[_productID];
                if (product != null && product.Context != null && product.Context.UserActivityControlLoader != null)
                {
                    container.Body.Controls.Add(product.Context.UserActivityControlLoader.LoadControl(_userID));
                    container.Body.Controls.Add(new Literal() { Text = "<div style=height:20px;>&nbsp;</div>" });
                }
                else
                {
                    bool isFirst = true;
                    foreach (var prod in WebItemManager.Instance.GetItems(ASC.Web.Core.WebZones.WebZoneType.All).OfType<IProduct>())
                    {
                        if (prod.Context != null && prod.Context.UserActivityControlLoader != null)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append("<div id='studio_product_activityBox_" + prod.ProductID + "' class='borderBase tintMedium clearFix' style='border-left:none; border-right:none; margin-top:-1px; padding:10px;'>");
                            sb.Append("<div class='headerBase' style='float:left; cursor:pointer;' onclick=\"StudioManager.ToggleProductActivity('" + prod.ProductID + "');\">");
                            string logoURL = prod.GetIconAbsoluteURL();
                            if (!String.IsNullOrEmpty(logoURL))
                                sb.Append("<img alt='' style='margin-right:5px;' align='absmiddle' src='" + logoURL + "'/>");
                            sb.Append(prod.ProductName.HtmlEncode());
                            sb.Append("<img alt='' align='absmiddle' id='studio_activityProductState_" + prod.ProductID + "' style='margin-left:15px;'  src='" + WebImageSupplier.GetAbsoluteWebPath(isFirst ? "collapse_down_dark.png" : "collapse_right_dark.png") + "'/>");
                            sb.Append("</div>");
                            sb.Append("</div>");
                            sb.Append("<div id=\"studio_product_activity_" + prod.ProductID + "\" style=\"padding-left:40px; " + (isFirst ? "" : "display:none;") + " padding-top:20px;\">");

                            container.Body.Controls.Add(new Literal() { Text = sb.ToString() });
                            var activityControl = prod.Context.UserActivityControlLoader.LoadControl(_userID);
                            container.Body.Controls.Add(activityControl);

                            sb = new StringBuilder();
                            sb.Append("</div>");
                            container.Body.Controls.Add(new Literal() { Text = sb.ToString() });

                            isFirst = false;
                        }
                    }

                }


                Employee.WriteEmployeeActions(this);


                if (SecurityContext.CheckPermissions(ASC.Core.Users.Constants.Action_AddRemoveUser))
                    master.SideHolder.Controls.Add(Employee.GetEmployeeNavigation());

                CompanyNavigation sideControl = (CompanyNavigation)LoadControl(CompanyNavigation.Location);
                master.SideHolder.Controls.Add(sideControl);

            }
        }

    }
}
