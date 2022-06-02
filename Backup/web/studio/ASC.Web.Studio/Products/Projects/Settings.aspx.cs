#region Import

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using AjaxPro;
using ASC.Common.Threading.Workers;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Web.Controls;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Controls.Common;
using ASC.Core;
using System.Text;
using ASC.Web.Projects.Controls.Reports;
using System.Web;
using ASC.Web.Projects.Controls;
using ASC.Web.Core.Users;
using System.Threading;
using BasecampRestAPI;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Notify.Patterns;
using ASC.Core.Tenants;
using ASC.Projects.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Controls.Dashboard.Settings;
using ASC.Projects.Engine;

#endregion

namespace ASC.Web.Projects
{
    [AjaxNamespace("AjaxPro.SettingsPage")]
    public partial class Settings : BasePage
    {
        #region Properties

        protected ModuleSettings ModuleSettings { get; set; }

        #endregion

        #region Events

        protected override void PageLoad()
        {
            if (!SecurityContext.IsAuthenticated)
            {
                Response.Redirect("~/auth.aspx");
            }

            AjaxPro.Utility.RegisterTypeForAjax(typeof(Settings));

            InitPage();
        }

        #endregion

        #region Methods

        public void InitPage()
        {
            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = SettingsResource.Settings
            });

            Title = HeaderStringHelper.GetPageTitle(SettingsResource.Settings, Master.BreadCrumbs);

            HiddenFieldForPermission.Value = Global.IsAdmin ? "1" : "0";

            ModuleSettings = SettingsManager.Instance.LoadSettings<ModuleSettings>(CoreContext.TenantManager.GetCurrentTenant().TenantId);

            import_info_container.Options.IsPopup = true;

            /*Old attention content
            import_info_attention_container.Options.IsPopup = true;
            */

            /*Default tab comment 
            var currTab = (int)SettingsManager.Instance.LoadSettingsFor<DefaultTabSettings>(SecurityContext.CurrentAccount.ID).Tab;
            hfCurrentTab.Value = currTab.ToString();
            */
        }

        public void EndProcessInformation(IAsyncResult result)
        {

        }

        public string GetDefaultTabURL(DefaultTab tab)
        {
            switch (tab)
            {
                case DefaultTab.Dashboard:
                    return String.Concat(PathProvider.BaseAbsolutePath, "default.aspx");
                case DefaultTab.Projects:
                    return String.Concat(PathProvider.BaseAbsolutePath, "projects.aspx");
                case DefaultTab.MyTasks:
                    return String.Concat(PathProvider.BaseAbsolutePath, "mytasks.aspx");
                case DefaultTab.Reports:
                    return String.Concat(PathProvider.BaseAbsolutePath, "reports.aspx");
            }
            return string.Empty;
        }

        [AjaxMethod]
        public ImportStatus StartImportFromBasecamp(string url, string token, bool processClosed, bool disableNotifications)
        {
            ProjectSecurity.DemandAuthentication();

            //Validate all data
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException(SettingsResource.EmptyURL);
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException(SettingsResource.EmptyToken);
            if (!Uri.IsWellFormedUriString(url,UriKind.Absolute))
                throw new ArgumentException(SettingsResource.MalformedUrl);
            if (!Regex.IsMatch(url.Trim(), @"^http[s]{0,1}://.+\.basecamphq\.com[/]{0,1}$"))
                throw new ArgumentException(SettingsResource.NotBasecampUrl);

            ImportQueue.Add(url, token, processClosed, disableNotifications);
            return ImportQueue.GetStatus();
        }

        [AjaxMethod]
        public ImportStatus GetImportFromBasecampStatus()
        {
            ProjectSecurity.DemandAuthentication();

            return ImportQueue.GetStatus();
        }

        [AjaxMethod]
        public void SaveModuleChanges(ModuleSettingsWrapper item)
        {
            ProjectSecurity.DemandAuthentication();

            var moduleSettings = SettingsManager.Instance.LoadSettings<ModuleSettings>(CoreContext.TenantManager.GetCurrentTenant().TenantId);

            moduleSettings.ShowCalendar = item.ViewCalendar;
            moduleSettings.ShowFiles = item.ViewFiles;
            moduleSettings.ShowTimeTracking = item.ViewTimeTracking;
            moduleSettings.ShowIssueTracker = item.ViewIssueTracker;

            SettingsManager.Instance.SaveSettings(moduleSettings, CoreContext.TenantManager.GetCurrentTenant().TenantId);
        }

        [AjaxMethod]
        public string SaveDefaultTabChanges(int tabID)
        {
            ProjectSecurity.DemandAuthentication();

            var tab = (DefaultTab)tabID;

            var tabSettings = SettingsManager.Instance.LoadSettingsFor<DefaultTabSettings>(SecurityContext.CurrentAccount.ID);

            tabSettings.URL = GetDefaultTabURL(tab);
            tabSettings.Tab = tab;

            SettingsManager.Instance.SaveSettingsFor(tabSettings, SecurityContext.CurrentAccount.ID);

            return ProjectsCommonResource.ChangesSaved;
        }

        #endregion
    }


    public class ModuleSettingsWrapper
    {
        public bool ViewCalendar { get; set; }
        public bool ViewFiles { get; set; }
        public bool ViewTimeTracking { get; set; }
        public bool ViewIssueTracker { get; set; }
    }

    public enum DefaultTab
    {
        Dashboard = 0,
        Projects = 1,
        MyTasks = 2,
        Reports = 3
    }

    [Serializable]
    public class ModuleSettings : ISettings
    {
        public bool ShowCalendar { get; set; }
        public bool ShowFiles { get; set; }
        public bool ShowTimeTracking { get; set; }
        public bool ShowIssueTracker { get; set; }

        public Guid ID
        {
            get { return new Guid("{649301E2-BA73-4a65-884D-12A0E8FDFCE4}"); }
        }
        public ISettings GetDefault()
        {
            return new ModuleSettings() { ShowCalendar = true, ShowFiles = true, ShowTimeTracking = true, ShowIssueTracker = true };
        }

    }

    [Serializable]
    public class DefaultTabSettings : ISettings
    {
        public string URL { get; set; }
        public DefaultTab Tab { get; set; }

        public Guid ID
        {
            get { return new Guid("{7A4798D0-03B1-4a33-93C3-CB6D7D67C5B4}"); }
        }
        public ISettings GetDefault()
        {
            return new DefaultTabSettings()
            {
                URL = String.Concat(PathProvider.BaseAbsolutePath, "default.aspx"),
                Tab = DefaultTab.Dashboard
            };
        }

    }

}
