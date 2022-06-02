using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Common.Security;
using ASC.Core;
using ASC.Projects.Core.Domain;
using ASC.Web.Controls;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Masters;
using ASC.Web.Studio.UserControls.Common;
using ASC.Web.Studio.UserControls.Users;
using ASC.Web.Studio.Utility;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core;
using ASC.Web.Projects.Controls.ProjectTemplates;
using System.Web.Configuration;
using ASC.Projects.Engine;
using ASC.Web.Studio.Core.Users;

namespace ASC.Web.Projects.Masters
{
    public partial class BasicTemplate : MasterPage, IStudioMaster
    {
        public List<BreadCrumb> BreadCrumbs
        {
            get
            {
                if (_commonContainer.BreadCrumbs == null) _commonContainer.BreadCrumbs = new List<BreadCrumb>();
                return _commonContainer.BreadCrumbs;
            }
        }

        public String InfoMessageText
        {
            set { _commonContainer.Options.InfoMessageText = value; }
        }

        public String InfoPanelClientID
        {
            get { return _commonContainer.GetInfoPanelClientID(); }

        }

        public String CommonContainerHeader
        {
            set { _commonContainer.Options.HeaderBreadCrumbCaption = value; }
        }

        public SideContainer AboutContainer
        {
            get { return _aboutContainer; }
        }

        public SideContainer RecentActivity
        {
            get { return _recentActivityContainer; }
        }

        private string GetUserInterestedProjects()
        {
            var interes = RequestContext.GetCurrentUserProjects();
            interes.AddRange(RequestContext.GetCurrentUserFollowingProjects());
            return string.Join(",", interes.ConvertAll(p => p.ID.ToString()).ToArray());
        }

        protected void InitControls()
        {
            var searchHandler = (BaseSearchHandlerEx)(SearchHandlerManager.GetHandlersExForProduct(ProductEntryPoint.ID)).Find(sh => sh is SearchHandler);

            if (String.IsNullOrEmpty(Request[UrlConstant.ProjectID]))
                searchHandler.AbsoluteSearchURL = (VirtualPathUtility.ToAbsolute(PathProvider.BaseVirtualPath + "/search.aspx"));
            else
                searchHandler.AbsoluteSearchURL = (VirtualPathUtility.ToAbsolute(PathProvider.BaseVirtualPath + "/search.aspx") + "?prjID=" + Request[UrlConstant.ProjectID]);

            _recentActivityContainer.ImageURL = WebImageSupplier.GetAbsoluteWebPath("what_to_read.png", ProductEntryPoint.ID);
            _recentActivityContainer.Title = ProjectsCommonResource.RecentActivityContainerTitle;
            _recentActivityContainer.HeaderCSSClass = "studioSideBoxWhatToRead";
            _recentActivityContainer.Controls.Add(LoadControl(PathProvider.GetControlVirtualPath("RecentActivitySideBoxBody.ascx")));

            _aboutContainer.ImageURL = WebImageSupplier.GetAbsoluteWebPath("navigation.png");
            _aboutContainer.Title = ProjectsCommonResource.About;
            _aboutContainer.BodyCSSClass = "studioSideBoxBodyAbout";
            _aboutContainer.Visible = false;

            RenderHeader();

            BottomNavigator bottomNavigator = new BottomNavigator();

            _bottomNavigatorPlaceHolder.Controls.Add(bottomNavigator);

            OnlineUsers onlineUsersControl = (OnlineUsers)LoadControl(OnlineUsers.Location);
            onlineUsersControl.ProductId = ProductEntryPoint.ID;
            phOnlineUsers.Controls.Add(onlineUsersControl);

            //Visited Users
            if (this.Page is ASC.Web.Projects.Dashboard)
            {
                VisitedUsers VisitedUsersControl = (VisitedUsers)LoadControl(VisitedUsers.Location);
                VisitedUsersControl.ProductId = ProductEntryPoint.ID;

                phVisitedUsers.Controls.Add(VisitedUsersControl);
            }



            if (Page.GetType() == typeof(Dashboard) && HaveProjects()) _commonContainer.Visible = false;

            //RSS
            //all interested projects
            //
            InterestedProjectsFeedControl.ContainerId = GetUserInterestedProjects();
            if (RequestContext.IsInConcreteProject() && !TemplateDipslayMode)
            {
                //this project feed
                //
                ConcreteProjectFeedControl.Visible = true;
                ConcreteProjectFeedControl.ContainerId = RequestContext.GetCurrentProjectId().ToString();
                ConcreteProjectFeedControl.Title = RequestContext.GetCurrentProject().HtmlTitle.HtmlEncode();
            }

            if (TemplateDipslayMode)
            {
                _recentActivityContainer.Visible = false;
                DisplayBelowNavigationPanel();
            }
        }

        #region Below Navigation Panel
        public void DisplayBelowNavigationPanel(string text)
        {
            BelowNavigationPanel.Visible = true;
            BelowNavigationPanelText.InnerHtml = text;
        }
        private void DisplayBelowNavigationPanel()
        {
            if (ProjectTemplatesUtil.CheckCreatePermission())
            {
                var templateId = UrlParameters.ProjectID;
                var createProjectFromTemplateLink = string.Format(@"<a href='{0}' style='margin-left:24px;' class='grayLinkButton'>{1}</a>", string.Format(ASC.Web.Projects.Controls.ProjectTemplates.ProjectTemplatesConst.CreateProjectFromTemplatePath, templateId), ProjectResource.CreateProjectFromTemplate);
                DisplayBelowNavigationPanel(ProjectResource.TemplateModeAttention + createProjectFromTemplateLink);
            }
            else
            {
                DisplayBelowNavigationPanel(ProjectResource.TemplateModeAttention);
            }
        }
        #endregion

        protected bool HaveProjects()
        {
            return RequestContext.HasAnyProjects();
        }

        protected void BuildProjectsList(ref StringBuilder innerHTML, String selectedPageString)
        {
            var verticalLinePositions = new ArrayList();
            int count = 0;

            innerHTML.AppendFormat("<div id='projects_dropdown' class='pm-dropdown' style='display:none;' >");

            var projects = RequestContext.GetCurrentUserProjects();
            verticalLinePositions.Add(projects.Count);

            int verticalLinePosition = projects.Count;

            if (verticalLinePosition < 10)
            {
                projects.AddRange(RequestContext.GetCurrentUserFollowingProjects());
                verticalLinePositions.Add(projects.Count);
            }

            if (verticalLinePosition < 10)
            {

                var otherProjects = Global.EngineFactory.GetProjectEngine().GetAll(ProjectStatus.Open, (10 - verticalLinePosition) + 1);
                otherProjects.Sort((x, y) => string.Compare(x.Title, y.Title));
                projects.AddRange(otherProjects.FindAll(item => !projects.Contains(item)));
            }

            foreach (var project in projects)
            {
                if (count + 1 > 10)
                {
                    innerHTML.AppendFormat(@"<a  class='linkSmall' style='margin:5px 10px; display:block;padding:2px;' href='{0}'>{1}</a>",
                         String.Concat(PathProvider.BaseAbsolutePath, String.Format("projects.aspx?{0}={1}", UrlConstant.ProjectsFilter, ASC.Web.Projects.Controls.Projects.ListProjectView.ProjectFilter.All)),
                         ProjectResource.SeeMore
                        );
                    break;
                }
                innerHTML.AppendFormat(@"<a  class='pm-dropdown-item'  href='{0}'>{1}</a>", String.Concat(PathProvider.BaseAbsolutePath, selectedPageString + ".aspx?prjID=" + project.ID), project.HtmlTitle.HtmlEncode());
                count++;

                if (verticalLinePositions.Contains(count))
                    innerHTML.Append("<div style='margin-top: 0px; margin-bottom: 0px; font-size: 1px; border-top: 1px solid rgb(209, 209, 209);' class='pm-dropdown-item'><!--– –--></div>");
            }

            innerHTML.AppendFormat("</div>");
        }

        protected void BuildTemplateProjectsList(ref StringBuilder innerHTML, String selectedPageString)
        {
            var verticalLinePositions = new ArrayList();
            int count = 0;

            innerHTML.AppendFormat("<div id='projects_dropdown' class='pm-dropdown' style='display:none;' >");

            var engine = Global.EngineFactory.GetTemplateEngine();
            var curUser = SecurityContext.CurrentAccount.ID;

            var projects = engine.GetTemplateProjects();
            var projectsToDisplay = projects.FindAll(p => p.Responsible == curUser);
            verticalLinePositions.Add(projects.Count);

            int verticalLinePosition = projects.Count;

            if (verticalLinePosition < 10)
            {
                var otherProjects = projects.FindAll(p => p.Responsible != curUser);
                otherProjects.Sort((x, y) => string.Compare(x.Title, y.Title));
                projectsToDisplay.AddRange(otherProjects);
                verticalLinePositions.Add(projects.Count);
            }

            foreach (var project in projects)
            {
                if (count + 1 > 10)
                {
                    innerHTML.AppendFormat(@"<a  class='linkSmall' style='margin:5px 10px; display:block;padding:2px;' href='{0}'>{1}</a>",
                         ProjectTemplatesConst.ProjectTemplatesBaseAbsolute,
                         ProjectResource.SeeMore
                        );
                    break;
                }
                innerHTML.AppendFormat(@"<a  class='pm-dropdown-item'  href='{0}'>{1}</a>", String.Concat(ProjectTemplatesConst.TemplateBaseAbsolutePath, selectedPageString + ".aspx?prjID=" + project.Id), project.Title.HtmlEncode());
                count++;

                if (verticalLinePositions.Contains(count))
                    innerHTML.Append("<div style='margin-top: 0px; margin-bottom: 0px; font-size: 1px; border-top: 1px solid rgb(209, 209, 209);' class='pm-dropdown-item'><!--– –--></div>");
            }

            innerHTML.AppendFormat("</div>");
        }

        protected String RenderAllTemplateProjectsBlock(int projectID, String selectedPageString)
        {
            var innerHTML = new StringBuilder();
            innerHTML.AppendLine("<div   style='padding-top: 2px;' >");
            innerHTML.AppendLine("<table>");
            innerHTML.AppendLine("<tr>");
            innerHTML.AppendFormat(@"<td  style='padding-left: 30px; height: 30px;'> 
                                            <a id='linkAllProjectsCombobox' href='{2}'>
                                                {0}
                                            </a>
                                             <a href='javascript:void(0)' id='projects_dropdown_switch' onclick='javascript:{3}'>
                                               <img style='border: 0px none ;' src='{1}'/>
                                             </a>
                                     </td>",
                                     ProjectResource.AllProjectTemplates,
                                     WebImageSupplier.GetAbsoluteWebPath("top_comb_arrow.gif", ProductEntryPoint.ID),
                                     ProjectTemplatesConst.ProjectTemplatesBaseAbsolute,
                                     "ASC.Projects.Common.dropdownToggle(jq(this).prev(), \"projects_dropdown\")"
                                  );

            innerHTML.AppendFormat(@"<td style='padding-left: 15px; padding-right: 15px;'>
                                            <img src='{1}' />
                                         </td>
                                         <td>
                                            <span id='projTitle'>{0}</span>  
                                         </td>",
                                           Global.EngineFactory.GetTemplateEngine().GetTemplateProject(int.Parse(UrlParameters.ProjectID)).HtmlTitle.HtmlEncode(),
                                           WebImageSupplier.GetAbsoluteWebPath("top_split.gif", ProductEntryPoint.ID));
            BuildTemplateProjectsList(ref innerHTML, selectedPageString);

            innerHTML.AppendLine("</table>");
            innerHTML.AppendLine("</div>");
            return innerHTML.ToString();
        }

        protected String RenderAllProjectsBlock(int projectID, String selectedPageString)
        {
            var innerHTML = new StringBuilder();
            innerHTML.AppendLine("<div   style='padding-top: 2px;' >");
            innerHTML.AppendLine("<table>");
            innerHTML.AppendLine("<tr>");
            innerHTML.AppendFormat(@"<td  style='padding-left: 30px; height: 30px;'> 
                                            <a id='linkAllProjectsCombobox' href='{2}'>
                                                {0}
                                            </a>
                                             <a href='javascript:void(0)' id='projects_dropdown_switch' onclick='javascript:{3}'>
                                               <img style='border: 0px none ;' src='{1}'/>
                                             </a>
                                     </td>",
                                     ProjectResource.AllProjects,
                                     WebImageSupplier.GetAbsoluteWebPath("top_comb_arrow.gif", ProductEntryPoint.ID),
                                     String.Concat(PathProvider.BaseAbsolutePath, "projects.aspx?filter=all"),
                                     "ASC.Projects.Common.dropdownToggle(jq(this).prev(), \"projects_dropdown\")"
                                  );

            if (RequestContext.IsInConcreteProject())
            {
                innerHTML.AppendFormat(@"<td style='padding-left: 15px; padding-right: 15px;'>
                                            <img src='{1}' />
                                         </td>
                                         <td>
                                            <span id='projTitle'>{0}</span>  
                                         </td>",
                                               RequestContext.GetCurrentProject().HtmlTitle.HtmlEncode(),
                                               WebImageSupplier.GetAbsoluteWebPath("top_split.gif", ProductEntryPoint.ID));
            }

            BuildProjectsList(ref innerHTML, selectedPageString);
            innerHTML.AppendLine("</table>");
            innerHTML.AppendLine("</div>");
            return innerHTML.ToString();
        }

        private static NavigationItem GetTemplateNavigationItem(string pageName, string pageTitle, string sysName)
        {
            return new NavigationItem
                {
                    URL = ProjectTemplatesUtil.GetTemplatePageUrl(pageName),
                    Name = pageTitle,
                    Description = "",
                    Selected = String.Compare(sysName, pageName, true) == 0
                };
        }

        protected void RenderHeader()
        {
            TopNavigationPanel topNavigationPanel = (TopNavigationPanel)LoadControl(TopNavigationPanel.Location);
            topNavigationPanel.SingleSearchHandlerType = typeof(ASC.Web.Projects.Configuration.SearchHandler);

            string absolutePathWithoutQuery = Request.Url.AbsolutePath.Substring(0, Request.Url.AbsolutePath.IndexOf(".aspx"));
            String sysName = absolutePathWithoutQuery.Substring(absolutePathWithoutQuery.LastIndexOf('/') + 1);
            int projectID;

            if (TemplateDipslayMode)
            {
                projectID = RequestContext.GetCurrentProjectId();
                topNavigationPanel.NavigationItems.Add(GetTemplateNavigationItem("Milestones", Resources.MilestoneResource.Milestones, sysName));
                topNavigationPanel.NavigationItems.Add(GetTemplateNavigationItem("Messages", Resources.MessageResource.Messages, sysName));
                topNavigationPanel.NavigationItems.Add(GetTemplateNavigationItem("Tasks", Resources.TaskResource.Tasks, sysName));
                topNavigationPanel.NavigationItems.Add(GetTemplateNavigationItem("Team", Resources.ProjectResource.Team, sysName));

                topNavigationPanel.CustomInfoHTML = RenderAllTemplateProjectsBlock(projectID, sysName);
            }
            else
                if (RequestContext.IsInConcreteProject())
                {
                    projectID = RequestContext.GetCurrentProjectId();

                    var rigthItems = new List<NavigationItem>();
                    foreach (var webitem in Global.ModuleManager.GetSortedVisibleItems())
                    {
                        var module = webitem as NavigationWebItem;

                        if (String.Compare(module.ModuleSysName, "ProjectSettings", true) == 0 && !Global.IsAdmin && !RequestContext.GetCurrentProjectFat().IsResponsible())
                        {
                            continue;
                        }

                        var navigationItem = new NavigationItem()
                                                 {
                                                     URL = String.Format(webitem.StartURL, projectID),
                                                     Name = webitem.Name,
                                                     Description = webitem.Description,
                                                     Selected = String.Compare(sysName, module.ModuleSysName, true) == 0
                                                 };

                        var added = false;
                        if (String.Compare(module.ModuleSysName, "History", true) == 0 ||
                            String.Compare(module.ModuleSysName, "ProjectSettings", true) == 0 ||
                            String.Compare(module.ModuleSysName, "ProjectTeam", true) == 0)
                        {
                            navigationItem.RightAlign = true;
                            rigthItems.Add(navigationItem);
                            added = true;
                        }

                        //hide in private projects
                        if (String.Compare(module.ModuleSysName, "Milestones", true) == 0 && !ProjectSecurity.CanReadMilestones(RequestContext.GetCurrentProject()))
                        {
                            continue;
                        }
                        if (String.Compare(module.ModuleSysName, "Messages", true) == 0 && !ProjectSecurity.CanReadMessages(RequestContext.GetCurrentProject()))
                        {
                            continue;
                        }
                        if (String.Compare(module.ModuleSysName, "TMDocs", true) == 0 && !ProjectSecurity.CanReadFiles(RequestContext.GetCurrentProject()))
                        {
                            continue;
                        }

                        if (String.Compare(module.ModuleSysName, "TMDocs", true) == 0)
                        {
                            navigationItem.Selected = String.Compare(sysName, "tmdocs", true) == 0;
                            navigationItem.Name = ProjectsFileResource.Documents;
                            navigationItem.URL = PathProvider.BaseAbsolutePath + "tmdocs.aspx?prjID=" + projectID;
                        }

                        if (!added)
                            topNavigationPanel.NavigationItems.Add(navigationItem);
                    }

                    rigthItems.Reverse();
                    topNavigationPanel.NavigationItems.AddRange(rigthItems);


                    topNavigationPanel.CustomInfoHTML = RenderAllProjectsBlock(projectID, sysName);
                }
                else
                {
                    topNavigationPanel.NavigationItems.Add(new NavigationItem
                    {
                        URL = String.Concat(PathProvider.BaseAbsolutePath, "default.aspx"),
                        Name = ProjectsCommonResource.Dashboard,
                        Description = "",
                        Selected = String.Compare(sysName, "Default", true) == 0
                    });

                    topNavigationPanel.NavigationItems.Add(new NavigationItem
                    {
                        URL = String.Concat(PathProvider.BaseAbsolutePath, "projects.aspx"),
                        Name = ProjectResource.Projects,
                        Description = "",
                        Selected = String.Compare(sysName, "Projects", true) == 0
                    });

                    topNavigationPanel.NavigationItems.Add(new NavigationItem
                    {
                        URL = String.Concat(PathProvider.BaseAbsolutePath, "mytasks.aspx"),
                        Name = TaskResource.MyTasks,
                        Description = "",
                        Selected = String.Compare(sysName, "MyTasks", true) == 0
                    });


                    topNavigationPanel.NavigationItems.Add(new NavigationItem
                    {
                        URL = String.Concat(PathProvider.BaseAbsolutePath, "reports.aspx"),
                        Name = ReportResource.Reports,
                        Description = "",
                        Selected = String.Compare(sysName, "Reports", true) == 0 ? true : String.Compare(sysName, "Templates", true) == 0
                    });

                    if (SecurityContext.IsAuthenticated)
                    {
                        topNavigationPanel.NavigationItems.Add(new NavigationItem
                                                                   {
                                                                       URL =
                                                                           String.Concat(PathProvider.BaseAbsolutePath,
                                                                                         "settings.aspx"),
                                                                       Name = SettingsResource.Settings,
                                                                       Description = "",
                                                                       Selected = String.Compare(sysName, "Settings", true) == 0,
                                                                       RightAlign = true
                                                                   });
                    }
                    topNavigationPanel.NavigationItems.Add(new NavigationItem
                    {
                        URL = String.Concat(PathProvider.BaseAbsolutePath, ProjectTemplatesConst.ProjectTemplatesPageTitle),
                        Name = ProjectResource.ProjectTemplates,
                        Description = "",
                        Selected = String.Compare(sysName, "ProjectTemplates", true) == 0,
                        RightAlign = true
                    });

                    topNavigationPanel.NavigationItems.Add(new NavigationItem
                    {
                        URL = CommonLinkUtility.GetEmployees(ProductEntryPoint.ID),
                        Name = CustomNamingPeople.Substitute<ProjectsCommonResource>("Employees"),
                        Description = "",
                        Selected = UserOnlineManager.Instance.IsEmployeesPage() || UserOnlineManager.Instance.IsUserProfilePage(),
                        RightAlign = true

                    });


                    if (RequestContext.HasAnyProjects())
                        topNavigationPanel.CustomInfoHTML = RenderAllProjectsBlock(-1, "projects");
                }

            _topNavigationPanelPlaceHolder.Controls.Add(topNavigationPanel);
        }


        protected void WriteClientScripts()
        {
            String script;
            if (RequestContext.IsInConcreteProject() && !TemplateDipslayMode)
            {
                var project = RequestContext.GetCurrentProject();
                script = String.Format("ASC.Projects.Common.IAmIsManager = {0};", (project.Responsible == ASC.Core.SecurityContext.CurrentAccount.ID).ToString().ToLower());

                if (!Page.ClientScript.IsClientScriptBlockRegistered(typeof(BasicTemplate), "{00F020A4-49F9-4d47-AE99-C8A44EB8FEB8}"))
                    Page.ClientScript.RegisterClientScriptBlock(typeof(BasicTemplate), "{00F020A4-49F9-4d47-AE99-C8A44EB8FEB8}", script, true);
            }

            String imagesFolderProductPath = WebSkin.GetUserSkin().GetAbsoluteWebPath(String.Format(@"{0}/App_Themes/<theme_folder>/Images/", PathProvider.BaseVirtualPath.Replace("~", "")));

            String imagesFolderCommonPath =
                VirtualPathUtility.ToAbsolute(String.Format(@"~/common/{0}/Images/", PathProvider.BaseVirtualPath));

            script = String.Format(@"ASC.Projects.Constants.IMAGES_FOLDER_PRODUCT_PATH = '{0}';
                                    ASC.Projects.Constants.IMAGES_FOLDER_COMMON_PATH = '{1}';",
                                    imagesFolderProductPath,
                                    imagesFolderCommonPath
                                 );
            if (!Page.ClientScript.IsClientScriptBlockRegistered(typeof(BasicTemplate), "{72EF1EC7-1B31-4d71-A5AE-BE90A8ABDEE2}"))
                Page.ClientScript.RegisterClientScriptBlock(typeof(BasicTemplate), "{72EF1EC7-1B31-4d71-A5AE-BE90A8ABDEE2}", script, true);
        }

        protected String GetResourcesPath()
        {
            return PathProvider.GetFileStaticRelativePath(String.Format("resources.{0}.js", CultureInfo.CurrentCulture.Name.ToLower()));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitControls();

            WriteClientScripts();

            Page.EnableViewState = false;
        }


        public PlaceHolder ContentHolder
        {
            get
            {
                _commonContainer.Visible = false;
                return _contentHolder;
            }
        }

        public PlaceHolder SideHolder
        {
            get { return (Master as IStudioMaster).SideHolder; }
        }

        public PlaceHolder TitleHolder
        {
            get { return (Master as IStudioMaster).TitleHolder; }
        }

        public PlaceHolder FooterHolder
        {
            get { return (Master as IStudioMaster).FooterHolder; }
        }

        public bool DisabledSidePanel
        {
            get { return (this.Master as IStudioMaster).DisabledSidePanel; }
            set { (this.Master as IStudioMaster).DisabledSidePanel = value; }
        }

        public bool? LeftSidePanel
        {
            get { return (this.Master as IStudioMaster).LeftSidePanel; }
            set { (this.Master as IStudioMaster).LeftSidePanel = value; }
        }

        public bool TemplateDipslayMode
        {
            get
            {
                return Request.Url.AbsolutePath.ToLower().Contains("/tproject");
            }
        }
    }
}
