#region Import

using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;
using AjaxPro;
using ASC.Core;
using ASC.Core.Users;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Studio.Controls.Users;
using System.Text;
using ASC.Projects.Core;
using ASC.Web.Projects.Resources;
using ASC.Web.Controls;
using System.Web;
using ASC.Web.Projects.Controls.ProjectTemplates;
using ASC.Projects.Engine;
using ASC.Web.Studio.UserControls.Common;

#endregion

namespace ASC.Web.Projects.Controls.Projects
{
    [AjaxNamespace("AjaxPro.ProjectActionView")]
    public partial class ProjectActionView : BaseUserControl
    {
        protected bool _securityEnable;

		#region Project templates properties
		protected bool HasTemplates { get; set; }
        protected string TemplatesContainerStyle { get; set; }
		protected List<TemplateProject> Templates { get; set; } 
		#endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(ProjectActionView));
            AjaxPro.Utility.RegisterTypeForAjax(typeof(ProjectSettings));
            HiddenFieldForTbxTagsID.Value = tbxTags.ClientID;

            _securityEnable = ProjectSecurity.SecurityEnabled(0);
            if (!_securityEnable)
            {
                var stub = LoadControl(PremiumStub.Location) as PremiumStub;
                stub.Type = PremiumFeatureType.PrivateProjects;
                _premiumStubHolder.Controls.Add(stub);
            }

			#region Project Templates
			Templates = Global.EngineFactory.GetTemplateEngine().GetTemplateProjects();
            
            if (!ProjectSecurity.CanCreateProject())
                TemplatesContainerStyle = "margin-top: -90px; margin-bottom: 90px;";
            else TemplatesContainerStyle = "margin-top: -54px; margin-bottom: 50px;";

			if (Templates == null || Templates.Count == 0)
			{
				HasTemplates = false;
				Templates = new List<TemplateProject>();
			}
			else
			{
				HasTemplates = true;

				var SortControl = new ViewSwitcher();

				var combobox = new ViewSwitcherCombobox() { SortLabel = ProjectResource.CreateFromTemplate };
				combobox.ComboboxClass = "projectTemplatesDottedLink";
				combobox.ComboboxItems = new List<ViewSwitcherComboboxItem>();
				foreach (var t in Templates)
				{
                    var title = HtmlUtility.GetText(t.Title, 50).HtmlEncode();
					combobox.ComboboxItems.Add(new ViewSwitcherComboboxItem
					{
						SortLabel = title,
						SortUrl = string.Format(@"javascript:ProjectTemplatesController.ChangeDropdownItem({0});", t.Id)
					});
				}

				combobox.ComboboxItems.Add(new ViewSwitcherComboboxItem
				{
					SortLabel = ProjectResource.CreateProjectTemplate,
					SortUrl = ProjectTemplatesConst.CreateProjectTemplateUrl,
					CssClass = "projectTemplatesCreateProjectComboLink"
				});

				SortControl.SortItems.Add(combobox);

				TemplatesDropdownContainer.Controls.Add(SortControl);
				Page.ClientScript.RegisterClientScriptBlock(typeof(string), "moveTemplatesCombo", "jq(function(){ProjectTemplatesController.MoveProjectTemplatesCombo();});", true);
				int templateID;
				if (int.TryParse(Request["templateID"] ?? string.Empty, out templateID))
				{
					var template = Global.EngineFactory.GetTemplateEngine().GetTemplateProject(templateID);
					if (template != null)
					{
						var title = HttpUtility.HtmlEncode(template.Title);
						Page.ClientScript.RegisterClientScriptBlock(typeof(string), "selectTemplate", string.Format(@"jq(function(){{ProjectTemplatesController.ChangeDropdownItem({0});}});", template.Id), true);
					}
				}				
			} 
			#endregion
        }

        #endregion

        #region Methods

        [AjaxMethod]
		public string AddNewProject(string title, string description, string leader, string tags, bool notifyManager, bool isHidden, int templateProjectId)
        {
            _securityEnable = ProjectSecurity.SecurityEnabled(0);

            if (ProjectSecurity.CanCreateProject())
            {
				Project project = null;
				var projectLeader = new Guid(leader);

				if (templateProjectId > 0)
				{
					try
					{
						project = Global.EngineFactory.GetTemplateEngine().CreateProjectFromTemplate(templateProjectId, title, projectLeader, description, tags, isHidden);
					}
					catch { }
				}
				if (project == null)
				{
					project = new Project();
					project.Title = title;
					project.Status = ProjectStatus.Open;
                    project.Private = _securityEnable? isHidden: false;
					project.Responsible = projectLeader;
					project.Description = description;

					Global.EngineFactory.GetProjectEngine().SaveOrUpdate(project,notifyManager);
					Global.EngineFactory.GetProjectEngine().AddToTeam(project, Global.EngineFactory.GetParticipantEngine().GetByID(projectLeader), true);
					Global.EngineFactory.GetTagEngine().SetProjectTags(project.ID, tags);
				}

                return "?prjID=" + project.ID;
            }
            else
            {
                ProjectSecurity.DemandAuthentication();

                var projectLeader = new Guid(leader);
                var request = new ProjectChangeRequest
                {
                    Title = title,
                    Status = ProjectStatus.Open,
                    Private = _securityEnable? isHidden: false,
                    Responsible = projectLeader,
                    Description = description,
                    RequestType = ProjectRequestType.Create,
                    TemplateId = templateProjectId
                };

                Global.EngineFactory.GetProjectEngine().SendRequest(request);
                Global.EngineFactory.GetTagEngine().SetProjectRequestTags(request.ID, tags);

                return string.Empty;
            }
        }

        [AjaxMethod]
        public string GetProjectTemplateTitle(int id)
        {
            ProjectSecurity.DemandAuthentication();

            var template = Global.EngineFactory.GetTemplateEngine().GetTemplateProject(id);
            if(template != null)
            {
                return HtmlUtility.GetText(template.Title, 30).HtmlEncode();
            }
            return string.Empty;
        }

        #endregion
    }
}