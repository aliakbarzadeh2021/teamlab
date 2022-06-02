#region Import

using System;
using System.Text;
using ASC.Core;
using ASC.Projects.Core;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Controls;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Controls;
using ASC.Web.Projects.Controls.Projects;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;
using ASC.Projects.Core.Domain.Reports;
using System.Web;

#endregion

namespace ASC.Web.Projects
{
    public partial class Projects : BasePage
    {

        #region Events

        protected override void PageLoad()
        {

            if (RequestContext.IsInConcreteProject())
            {
                var project = RequestContext.EnsureCurrentProduct();

                if (String.Compare(UrlParameters.ActionType, "edit", true) == 0)
                {
                    if (ProjectSecurity.CanEdit(project))
                    {
                        Response.Redirect("projectsettings.aspx?prjID=" + project.ID);
                    }
                    else
                    {
                        Response.Redirect("projects.aspx");
                    }
                }
                else
                    ProjectDetailsView(RequestContext.GetCurrentProjectFat());
            }
            else
            {

                if (String.Compare(UrlParameters.ActionType, "add", true) == 0)
                {
                    if (SecurityContext.IsAuthenticated)
                    {
                        ProjectActionView();
                    }
                    else
                    {
                        Response.Redirect("~/auth.aspx");
                    }
                }
                else
                {
                    var enumValue = ASC.Web.Projects.Controls.Projects.ListProjectView.ProjectFilter.Default;
                    if (!String.IsNullOrEmpty(UrlParameters.ProjectsFilter))
                        try
                        {
                            enumValue = (ListProjectView.ProjectFilter)Enum.Parse(typeof(ListProjectView.ProjectFilter), UrlParameters.ProjectsFilter, true);
                        }
                        catch { }

                    ListProjectView(enumValue);
                }
            }


            if (SideActionsPanel.Controls.Count == 0)
                SideActionsPanel.Visible = false;

            if (_requestContainer.Controls.Count == 0)
                _requestContainer.Visible = false;

            Title = HeaderStringHelper.GetPageTitle(ProjectResource.Projects, Master.BreadCrumbs);
        }

        #endregion

        #region Methods

        protected void ProjectActionView()
        {
            SideActionsPanel.Visible = false;
            _requestContainer.Visible = false;
            ProjectActionView cntrlProjectActionView = (ProjectActionView)LoadControl(PathProvider.GetControlVirtualPath("ProjectActionView.ascx"));
            _content.Controls.Add(cntrlProjectActionView);

            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = ProjectResource.Projects,
                NavigationUrl = "projects.aspx"

            });

            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = ProjectResource.CreateNewProject,
                NavigationUrl = ""

            });

            if (!ProjectSecurity.CanCreateProject()) Master.InfoMessageText = ProjectResource.InfoMessageText;
        }

        protected void ListProjectView(ListProjectView.ProjectFilter filter)
        {
            if (RequestContext.HasAnyProjects())
            {
                ListProjectView cntrlListProjectView = (ListProjectView)LoadControl(PathProvider.GetControlVirtualPath("ListProjectView.ascx"));
                cntrlListProjectView.CurrentFilter = filter;
                cntrlListProjectView.Tag = UrlParameters.ProjectsTag;
                _content.Controls.Add(cntrlListProjectView);
            }
            else
            {
                EmptyScreenControl emptyScreenControl = new EmptyScreenControl();
                if (SecurityContext.IsAuthenticated)
                {
                    emptyScreenControl.HeaderContent = String.Format("<a href='{1}'>{0}</a>", ProjectResource.EmptyScreenHeaderContent, "projects.aspx?action=add");
                }
                emptyScreenControl.HeaderDescribe = ProjectResource.EmptyScreenHeaderDescribe;
                _content.Controls.Add(emptyScreenControl);
            }

            if (SecurityContext.IsAuthenticated)
            {
                SideActionsPanel.Controls.Add(new NavigationItem
                                                  {
                                                      Name = ProjectResource.CreateNewProject,
                                                      URL = "projects.aspx?action=add"

                                                  });
            }
            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = ProjectResource.Projects,
                NavigationUrl = "projects.aspx"

            });

            _requestContainer.ImageURL = WebImageSupplier.GetAbsoluteWebPath("request.gif", ProductEntryPoint.ID);
            _requestContainer.Title = RequestResource.Requests;
            _requestContainer.HeaderCSSClass = "studioSideBoxWhatToRead";

            _requestContainer.Controls.Add(LoadControl(PathProvider.GetControlVirtualPath("Request.ascx")));

            if (!ProjectSecurity.CanCreateProject() || Global.EngineFactory.GetProjectEngine().GetRequests().Count == 0)
                _requestContainer.Visible = false;
        }

        protected void ProjectDetailsView(ProjectFat projectFat)
        {
            InitActionPanel(projectFat);
            InitBreadCrumbs(projectFat.Project);
            _requestContainer.Visible = false;

            ProjectDetailsView cntrlProjectDetailsView = (ProjectDetailsView)LoadControl(PathProvider.GetControlVirtualPath("ProjectDetailsView.ascx"));
            cntrlProjectDetailsView.ProjectFat = projectFat;
            _content.Controls.Add(cntrlProjectDetailsView);

        }

        protected void InitActionPanel(ProjectFat projectFat)
        {
            if (SecurityContext.IsAuthenticated)
            {
                SideActionsPanel.Controls.Add(new NavigationItem
                {
                    Name = ProjectResource.CreateNewProject,
                    URL = "projects.aspx?action=add"

                });
            }
            bool isFollowing = RequestContext.GetCurrentUserFollowingProjects().Contains(projectFat.Project);

            StringBuilder innerHTML = new StringBuilder();

            String followingLinkTitle = !isFollowing ? ProjectResource.FollowingLinkTitle : String.Empty;

            if (SecurityContext.IsAuthenticated)
            {
                innerHTML.Append("<div id='project_following'>");
                innerHTML.AppendFormat("<a  title=\"{2}\" href='javascript:void(0)' onclick='javascript:ASC.Projects.Projects.checkAsFollowing({1})' class=\"linkAction\">{0}</a>",
                      isFollowing ? ProjectResource.UnFollowingProjects : ProjectResource.FollowingProjects,
                      projectFat.Project.ID,
                      followingLinkTitle
                    );
                innerHTML.Append("</div>");
            }
            if (!Global.EngineFactory.GetProjectEngine().IsInTeam(projectFat.Project.ID, SecurityContext.CurrentAccount.ID))
                SideActionsPanel.Controls.Add(new HtmlMenuItem(innerHTML.ToString()));

            if (ProjectSecurity.CanCreateMilestone(projectFat.Project))
                SideActionsPanel.Controls.Add(new NavigationItem
                {
                    Name = MilestoneResource.CreateNewMilestone,
                    URL = "milestones.aspx?prjID=" + projectFat.Project.ID + "&action=add"

                });

            if (ProjectSecurity.CanCreateTask(projectFat.Project, false))
                SideActionsPanel.Controls.Add(new NavigationItem
                {
                    Name = TaskResource.AddTask,
                    URL = "javascript:ASC.Projects.TaskActionPage.init(-1,null, null); ASC.Projects.TaskActionPage.show();"
                });

            SideActionsPanel.Controls.Add(new NavigationItem
            {
                Name = ReportResource.TaskReport,
                URL = string.Format("javascript:ASC.Projects.Reports.generateReportByUrl('{0}')", GetReportUri(projectFat.Project.ID))

            });

        }

        protected void InitBreadCrumbs(Project project)
        {
            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = ProjectResource.Projects,
                NavigationUrl = "projects.aspx"

            });
            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = project.HtmlTitle.HtmlEncode()

            });
            if (!project.Private)
                Master.CommonContainerHeader = Global.RenderCommonContainerHeader(project.HtmlTitle.HtmlEncode(), EntityType.Project);
            else
                Master.CommonContainerHeader = Global.RenderPrivateProjectHeader(project.HtmlTitle.HtmlEncode());

        }

        public string GetReportUri(int prjID)
        {
            var filter = new ReportFilter();
            filter.TimeInterval = ReportTimeInterval.Absolute;
            filter.ProjectIds.Add(prjID);
            filter.TaskStatuses.Add(TaskStatus.Unclassified);
            filter.TaskStatuses.Add(TaskStatus.Open);
            filter.TaskStatuses.Add(TaskStatus.Closed);
            return string.Format("reports.aspx?action=generate&reportType=9&{0}", filter.ToUri());
        }

        #endregion

    }
}