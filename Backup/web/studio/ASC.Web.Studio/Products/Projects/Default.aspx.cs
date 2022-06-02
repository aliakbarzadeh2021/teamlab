#region Import

using System;
using System.Collections.Generic;
using System.Drawing;
using ASC.Core.Common.Publisher;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Controls.Dashboard;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Controls.Dashboard;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Publisher;
using ASC.Web.Studio.UserControls.Common;
using ASC.Web.Studio.Utility;
using ASC.Core;


#endregion

namespace ASC.Web.Projects
{
    public partial class Dashboard : BasePage
    {
        #region Members


        private WidgetTab _widgetTabControl;

        private readonly PublishZone _greetingZone = new PublishZone(new Zone(Constants.GreetingZoneID));
        private readonly List<PublishZone> _zoneCollection = new List<PublishZone>();
        protected TenantInfoSettings _tenantInfoSettings;

        #endregion

        protected void RenderWidgetPanel()
        {
            _widgetTabControl = new WidgetTab(new Guid("{698C5962-80FB-40fd-B345-1859D5116A40}"),
                                                            ColumnSchemaType.Schema_33_33_33,
                                                            "projectsDashboard"
                                                            );

            _widgetTabControl.Settings = true;

            LateMilestonesWidget cntrlLateMilestonesWidget = (LateMilestonesWidget)LoadControl(PathProvider.GetControlVirtualPath("LateMilestonesWidget.ascx"));
            _widgetTabControl.WidgetCollection.Add(new Widget(LateMilestonesWidget.WidgetID,
                                                            cntrlLateMilestonesWidget,
                                                            ReportResource.ReportLateMilestones_Title,
                                                            ProjectsCommonResource.MilestonesWidget_Overdue_Description)
            {
                ImageURL = WebImageSupplier.GetAbsoluteWebPath("overdue_milestones.png", ProductEntryPoint.ID),
                SettingsProviderType = typeof(ProjectsWidgetSettingsProvider),
                Position = new Point(0, 0)
            });

            UpcomingMilestonesWidget cntrlUpcomingMilestonesWidget = (UpcomingMilestonesWidget)LoadControl(PathProvider.GetControlVirtualPath("UpcomingMilestonesWidget.ascx"));
            _widgetTabControl.WidgetCollection.Add(new Widget(UpcomingMilestonesWidget.WidgetID,
                                                            cntrlUpcomingMilestonesWidget,
                                                            ReportResource.ReportUpcomingMilestones_Title,
                                                            ProjectsCommonResource.MilestonesWidget_Upcoming_Description)
            {
                ImageURL = WebImageSupplier.GetAbsoluteWebPath("upcoming_milestones.png", ProductEntryPoint.ID),
                SettingsProviderType = typeof(ProjectsWidgetSettingsProvider),
                Position = new Point(1, 0)
            });

            if (SecurityContext.IsAuthenticated)
            {
                MyNewTasksWidget cntrlMyNewTasksWidget = (MyNewTasksWidget)LoadControl(PathProvider.GetControlVirtualPath("MyNewTasksWidget.ascx"));
                _widgetTabControl.WidgetCollection.Add(new Widget(MyNewTasksWidget.WidgetID,
                                                                cntrlMyNewTasksWidget,
                                                                TaskResource.MyNewTasks,
                                                                ProjectsCommonResource.MyTasksWidget_Description)
                {
                    ImageURL = WebImageSupplier.GetAbsoluteWebPath("tasks_widget.png", ProductEntryPoint.ID),
                    SettingsProviderType = typeof(ProjectsWidgetSettingsProvider),
                    Position = new Point(2, 2)
                });
            }
            RecentActivityWidget cntrlRecentActivityWidget = (RecentActivityWidget)LoadControl(PathProvider.GetControlVirtualPath("RecentActivityWidget.ascx"));
            _widgetTabControl.WidgetCollection.Add(new Widget(RecentActivityWidget.WidgetID,
                                                            cntrlRecentActivityWidget,
                                                            ProjectsCommonResource.RecentActivity,
                                                            ProjectsCommonResource.RecentActivityWidget_Description)
            {
                ImageURL = WebImageSupplier.GetAbsoluteWebPath("what_to_read.png", ProductEntryPoint.ID),
                SettingsProviderType = typeof(ProjectsWidgetSettingsProvider),
                Position = new Point(0, 1)
            });

            MessagesWidget cntrlMessagesWidget = (MessagesWidget)LoadControl(PathProvider.GetControlVirtualPath("MessagesWidget.ascx"));
            _widgetTabControl.WidgetCollection.Add(new Widget(MessagesWidget.WidgetID,
                                                            cntrlMessagesWidget,
                                                            MessageResource.Messages,
                                                            ProjectsCommonResource.MessagesWidget_Description)
            {
                ImageURL = WebImageSupplier.GetAbsoluteWebPath("discussions.png", ProductEntryPoint.ID),
                SettingsProviderType = typeof(ProjectsWidgetSettingsProvider),
                Position = new Point(1, 1)
            });

            if (SecurityContext.IsAuthenticated)
            {
                MyProjectsWidget cntrlMyProjectsWidget = (MyProjectsWidget)LoadControl(PathProvider.GetControlVirtualPath("MyProjectsWidget.ascx"));
                _widgetTabControl.WidgetCollection.Add(new Widget(MyProjectsWidget.WidgetID,
                                                                cntrlMyProjectsWidget,
                                                                ProjectResource.MyProjects,
                                                                ProjectsCommonResource.MyProjectsWidget_Description)
                {
                    ImageURL = WebImageSupplier.GetAbsoluteWebPath("my_projects.png", ProductEntryPoint.ID),
                    SettingsProviderType = typeof(ProjectsWidgetSettingsProvider),
                    Position = new Point(2, 0)
                });
            }

            NewProjectsWidget cntrlNewProjectsWidget = (NewProjectsWidget)LoadControl(PathProvider.GetControlVirtualPath("NewProjectsWidget.ascx"));
            _widgetTabControl.WidgetCollection.Add(new Widget(NewProjectsWidget.WidgetID,
                                                            cntrlNewProjectsWidget,
                                                            ProjectResource.NewProjects,
                                                            ProjectsCommonResource.NewProjectsWidget_Description)
            {
                ImageURL = WebImageSupplier.GetAbsoluteWebPath("new_projects.png", ProductEntryPoint.ID),
                SettingsProviderType = typeof(ProjectsWidgetSettingsProvider),
                Position = new Point(2, 1)
            });

            if (SecurityContext.IsAuthenticated)
            {
                //following projects
                FollowingProjectsWidget cntrlFollowingProjectsWidget = (FollowingProjectsWidget)LoadControl(PathProvider.GetControlVirtualPath("FollowingProjectsWidget.ascx"));
                _widgetTabControl.WidgetCollection.Add(new Widget(FollowingProjectsWidget.WidgetID,
                                                                cntrlFollowingProjectsWidget,
                                                                ProjectResource.ProjectsKeepTrack,
                                                                ProjectsCommonResource.FollowingProjectsWidget_Description)
                {
                    ImageURL = WebImageSupplier.GetAbsoluteWebPath("product_logo.png", ProductEntryPoint.ID),
                    SettingsProviderType = typeof(ProjectsWidgetSettingsProvider),
                    Position = new Point(1, 2)
                });
            }

            _widgetContainer.Controls.Add(_widgetTabControl);
            Master.ContentHolder.Visible = false;
        }

        protected void InitControls()
        {
            _greetingZone.Load += GreetingZoneLoadHandler;
            _zoneCollection.Add(_greetingZone);

            (this.Master).DisabledSidePanel = true;

            NavigationPanel cntrlNavigationPanel = (NavigationPanel)LoadControl(NavigationPanel.Location);

            if (SecurityContext.IsAuthenticated && HaveProjects())
            {
                cntrlNavigationPanel.addButton(ProjectResource.NewProject, WebImageSupplier.GetAbsoluteWebPath("product_logo.png", ProductEntryPoint.ID), "projects.aspx?action=add", 2);
                cntrlNavigationPanel.addButton(ProjectsCommonResource.CustomizeWidgets, WebImageSupplier.GetAbsoluteWebPath("btn_managewidgets.png"), "javascript:projectsDashboard.ShowSettings()", 3);
            }
            
            _navigationPanelContent.Controls.Add(cntrlNavigationPanel);

            Title = HeaderStringHelper.GetPageTitle(ProjectsCommonResource.Dashboard, Master.BreadCrumbs);

        }

        protected bool HaveProjects()
        {
            return ASC.Web.Projects.Classes.RequestContext.HasAnyProjects();
        }

        protected void RenderGreetingPanel()
        {

            var dashboardEmptyScreen = (DashboardEmptyScreen)Page.LoadControl(PathProvider.GetControlVirtualPath("DashboardEmptyScreen.ascx"));
            
            _content.Controls.Add(dashboardEmptyScreen);
        }

        protected override void PageLoad()
        {
            InitControls();

            if (HaveProjects())
                RenderWidgetPanel();
            else
                RenderGreetingPanel();


        }

        private void GreetingZoneLoadHandler(object sender, ZoneLoadEventArgs e)
        {


        }

        protected string RenderGreetingTitle()
        {
            return String.Format("{0}", _tenantInfoSettings.CompanyName);
        }



        //IPublishZoneCollection Members
        public override List<PublishZone> PublishZones
        {
            get { return _zoneCollection; }
        }


    }
}
