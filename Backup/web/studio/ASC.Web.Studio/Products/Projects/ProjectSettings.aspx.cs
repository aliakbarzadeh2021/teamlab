#region Import

using System;
using System.Data;
using AjaxPro;
using ASC.Core;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Web.Controls;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;
using System.Text;
using System.Collections.Generic;
using ASC.Projects.Core;
using System.Web;
using System.Threading;
using ASC.Projects.Engine;
using ASC.Web.Studio.UserControls.Common;

#endregion

namespace ASC.Web.Projects
{
    [AjaxNamespace("AjaxPro.ProjectSettings")]
    public partial class ProjectSettings : BasePage
    {
        protected bool _securityEnable;

        #region Properies

        public Project Project { get; set; }

        #endregion

        #region Events

        protected override void PageLoad()
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(ProjectSettings));

            HiddenFieldForTbxTagsID.Value = tbxTags.ClientID;

            int projectID;
            if (int.TryParse(UrlParameters.ProjectID, out projectID))
            {
                Project = Global.EngineFactory.GetProjectEngine().GetByID(projectID);
                if (Project != null)
                {
                    _securityEnable = ProjectSecurity.SecurityEnabled(projectID);
                    if (!_securityEnable)
                    {
                        var stub = LoadControl(PremiumStub.Location) as PremiumStub;
                        stub.Type = PremiumFeatureType.PrivateProjects;
                        _premiumStubHolder.Controls.Add(stub);
                    }
                    
                    tbxTitle.Text = HttpUtility.HtmlDecode(Project.Title);
                    tbxDescription.Text = HttpUtility.HtmlDecode(Project.Description);

                    var tags = Global.EngineFactory.GetTagEngine().GetProjectTags(projectID);
                    tbxTags.Text = string.Join(", ", tags);

                    InitActionPanel();

                    InitBreadCrumbs(projectID);

                    Title = HeaderStringHelper.GetPageTitle(ProjectResource.Settings, Master.BreadCrumbs);

                    userSelector.SelectedUserId = Project.Responsible;
                }
                else
                    Response.Redirect("projects.aspx");
            }
            else
                Response.Redirect("projects.aspx");
        }

        #endregion

        #region Methods

        protected void InitActionPanel()
        {

            SideActionsPanel.Visible = false;

        }

        protected void InitBreadCrumbs(int PrjID)
        {
            var project = Global.EngineFactory.GetProjectEngine().GetByID(PrjID);
            if (project != null)
            {
                Master.BreadCrumbs.Add(new BreadCrumb
                {
                    Caption = ProjectResource.Projects,
                    NavigationUrl = "projects.aspx"

                });
                Master.BreadCrumbs.Add(new BreadCrumb
                {
                    Caption = project.HtmlTitle.HtmlEncode(),
                    NavigationUrl = "projects.aspx?PrjID=" + PrjID.ToString()

                });
                Master.BreadCrumbs.Add(new BreadCrumb
                {
                    Caption = ProjectResource.Settings,
                    NavigationUrl = ""

                });

                if (ProjectSecurity.CanEdit(project))
                    Master.InfoMessageText = ProjectsCommonResource.ChangesSaved;
                else
                    Master.InfoMessageText = ProjectResource.InfoMessageText2;
            }
        }

        public int GetProjectStatus()
        {
            if (Project.Status == ProjectStatus.Open) return 0;
            else return 1;
        }

        public bool IsHidden()
        {
            return Project.Private;
        }

        [AjaxMethod]
        public string SaveProject(int prjID, string title, string description, string leaderID, string tags, bool isOpen, bool isHidden)
        {
            Project project = Global.EngineFactory.GetProjectEngine().GetByID(prjID);
            if (ProjectSecurity.CanEdit(project))
            {
                var projectLeader = new Guid(leaderID);
                project.Title = title;
                project.Status = isOpen ? ProjectStatus.Open : ProjectStatus.Closed;
                project.Private = isHidden;
                project.Responsible = projectLeader;
                project.Description = description;

                Global.EngineFactory.GetProjectEngine().SaveOrUpdate(project,false);
                if(!Global.EngineFactory.GetProjectEngine().IsInTeam(project.ID, projectLeader))
                    Global.EngineFactory.GetProjectEngine().AddToTeam(project, Global.EngineFactory.GetParticipantEngine().GetByID(projectLeader), true);
                Global.EngineFactory.GetTagEngine().SetProjectTags(project.ID, tags);

                return "?prjID=" + project.ID;
            }

            if (SecurityContext.CurrentAccount.ID == project.Responsible)
            {
                var projectLeader = new Guid(leaderID);
                var request = new ProjectChangeRequest
                {
                    Title = title,
                    Status = isOpen ? ProjectStatus.Open : ProjectStatus.Closed,
                    Private = isHidden,
                    Responsible = projectLeader,
                    Description = description,
                    RequestType = ProjectRequestType.Edit,
                    ProjectID = prjID
                };

                Global.EngineFactory.GetProjectEngine().SendRequest(request);
                Global.EngineFactory.GetTagEngine().SetProjectRequestTags(request.ID, tags);

                return String.Empty;
            }

            return String.Empty;
        }

        [AjaxMethod]
        public string DeleteProject(int prjID)
        {
            var project = Global.EngineFactory.GetProjectEngine().GetByID(prjID);
            if (ProjectSecurity.CanEdit(project))
            {
                Global.EngineFactory.GetProjectEngine().Delete(prjID);
                return "projects.aspx";
            }
            if (SecurityContext.CurrentAccount.ID == project.Responsible)
            {
                var request = new ProjectChangeRequest
                {
                    Description = project.Description,
                    RequestType = ProjectRequestType.Remove,
                    ProjectID = prjID,
                    Title = project.Title,
                    Responsible = project.Responsible,
                    Status = project.Status
                };

                var tags = string.Empty;
                foreach (var t in Global.EngineFactory.GetTagEngine().GetProjectTags(prjID))
                {
                    tags += t + ",";
                }

                Global.EngineFactory.GetProjectEngine().SendRequest(request);
                Global.EngineFactory.GetTagEngine().SetProjectRequestTags(request.ID, tags);
            }
            return string.Empty;
        }

        [AjaxMethod]
        public string[] TagsAutocomplete(string tagName)
        {
            ProjectSecurity.DemandAuthentication();

            return !string.IsNullOrEmpty(tagName) && tagName.Trim() != string.Empty ?
                Global.EngineFactory.GetTagEngine().GetTags(tagName.Trim()) :
                new string[0];
        }

        #endregion

    }
}
