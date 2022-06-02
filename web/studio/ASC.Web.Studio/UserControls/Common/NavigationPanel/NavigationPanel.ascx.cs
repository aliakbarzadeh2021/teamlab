using System;
using System.Collections.Generic;
using System.Web.UI;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Users;
using ASC.Web.Studio.Utility;


namespace ASC.Web.Studio.UserControls.Common
{
    public partial class NavigationPanel : System.Web.UI.UserControl
    {
        protected override void OnInit(EventArgs e)
        {
            Page.ClientScript.RegisterClientScriptInclude(typeof(string), "ajaxupload_script", WebPath.GetPath("js/ajaxupload.3.5.js"));
            Page.ClientScript.RegisterClientScriptInclude(typeof(string), "userimporter_script", WebPath.GetPath("usercontrols/users/userimporter/js/userimporter.js"));
            var impEmpControl = this.LoadControl(UserImporter.Location);
            ImportEmploeeysNavigationPanel.Controls.Add(impEmpControl);
        }

        private sealed class WidgetButton
        {
            public string Link { get; set; }
            public string Icon { get; set; }
            public string Label { get; set; }
        }

        private TenantInfoSettings _tenantInfoSettings = null;
        private List<WidgetButton> _buttons = null;

        protected TenantInfoSettings TenantInfoSettings
        {
            get
            {
                if (_tenantInfoSettings == null)
                    _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);
                return _tenantInfoSettings;
            }
        }

        private List<WidgetButton> buttons
        {
            get
            {
                if (_buttons == null)
                {
                    _buttons = new List<WidgetButton>();

                    //check permissions 
                    //
                    Boolean showInvite = SecurityContext.CheckPermissions(ASC.Core.Users.Constants.Action_AddRemoveUser);
                    Boolean ShowSettings = SecurityContext.CheckPermissions(ASC.Core.Users.Constants.Action_AddRemoveUser,
                                                                            ASC.Core.Users.Constants.Action_EditUser,
                                                                            ASC.Core.Users.Constants.Action_EditGroups);

                    //adding buttons on panel
                    //
                    if (ShowSettings)
                        addButton(
                            Resources.UserControlsCommonResource.BtnChangeSettings,
                            WebImageSupplier.GetAbsoluteWebPath("btn_settings.png"),
                            CommonLinkUtility.GetAdministration());

                    if (SecurityContext.IsAuthenticated)
                        addButton(
                            Resources.UserControlsCommonResource.BtnChangeProfile,
                            WebImageSupplier.GetAbsoluteWebPath("btn_changeprofile.png"),
                            CommonLinkUtility.GetMyStaff());

                    if (showInvite)
                    {
                        if (SetupInfo.IsMassAddEnabled())
                            addButton(
                                CustomNamingPeople.Substitute<Resources.Resource>("AddEmployeesButton"),
                                WebImageSupplier.GetAbsoluteWebPath("btn_invitepeople.png"),
                                "javascript:getContactsPopupWindowDisplay('" + SetupInfo.GetImportServiceUrl() + "','" + CustomNamingPeople.Substitute<Resources.UserControlsCommonResource>("AddEmployees") + "')");
                        else
                            addButton(
                                CustomNamingPeople.Substitute<Resources.Resource>("InviteEmloyeesButton"),
                                WebImageSupplier.GetAbsoluteWebPath("btn_invitepeople.png"),
                                "javascript:AuthManager.ShowInviteEmployeeDialog()");

                    }
                }
                return _buttons;
            }
        }

        public static string Location { get { return "~/UserControls/Common/NavigationPanel/NavigationPanel.ascx"; } }

        protected string RenderGreetingTitle()
        {
            return TenantInfoSettings.CompanyName.HtmlEncode();
        }

        public void addButton(String name, String icon, String link)
        {
            buttons.Add(new WidgetButton { Link = link, Icon = icon, Label = name });
        }

        public void addButton(String name, String icon, String link, Int32 position)
        {
            if (position > 0 && position <= buttons.Count)
                buttons.Insert(buttons.Count - position + 1, new WidgetButton { Link = link, Icon = icon, Label = name });
            else
                buttons.Add(new WidgetButton { Link = link, Icon = icon, Label = name });
        }

        public void removeButtons()
        {
            buttons.Clear();
        }

        private void InitScripts()
        {
            Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "navpanel_script", WebPath.GetPath("usercontrols/common/navigationpanel/js/navigator.js"));
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "navpanel_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebSkin.GetUserSkin().GetAbsoluteWebPath("usercontrols/common/navigationpanel/css/<theme_folder>/navigationpanel.css") + "\">", false);
        }

        protected Boolean isMinimized()
        {
            return !String.IsNullOrEmpty(CookiesManager.GetCookies(CookiesType.MinimizedNavpanel));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitScripts();

            if (SecurityContext.CheckPermissions(ASC.Core.Users.Constants.Action_AddRemoveUser))
            {
                InviteEmployeeControl invEmpControl = (InviteEmployeeControl)this.LoadControl(InviteEmployeeControl.Location);
                InvitesMailer.Controls.Add(invEmpControl);
            }

            buttonRepeater.DataSource = buttons;
            buttonRepeater.DataBind();
        }
    }
}