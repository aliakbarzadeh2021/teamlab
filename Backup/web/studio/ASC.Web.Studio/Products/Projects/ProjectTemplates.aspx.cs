using System;
using System.Collections.Generic;
using AjaxPro;
using ASC.Core;
using ASC.Projects.Core.Domain;
using ASC.Web.Controls;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Controls.ProjectTemplates;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Projects.Controls;
using ASC.Projects.Engine;

namespace ASC.Web.Projects
{
    [AjaxNamespace("ProjectTemplates")]
    public partial class ProjectTemplates : BasePage
    {

        protected override void PageLoad()
        {
            Utility.RegisterTypeForAjax(this.GetType());
            Utility.RegisterTypeForAjax(typeof(ASC.Web.Projects.TProject.Overview));

            Title = ProjectResource.ProjectTemplates;

            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = ProjectResource.Projects,
                NavigationUrl = "projects.aspx"
            });

            EmptyScreenControl emptyScreenControl = new EmptyScreenControl();
            emptyScreenControl.HeaderContent = String.Format("<a href='{1}'>{0}</a>", ProjectResource.EmptyScreenProjectTemplates, ProjectTemplatesConst.CreateProjectTemplateUrl);
            emptyScreenControl.HeaderDescribe = String.Format(ProjectResource.ProjectTemplateSetUpDescription, "<br/>");
            EmpryScreenContainer.Controls.Add(emptyScreenControl);

            var action = UrlParameters.ActionType;

            switch (action)
            {
                case "create":
                    if (ProjectTemplatesUtil.CheckCreatePermission())
                    {
                        CreateProjectTemplateHolder.Visible = true;
                        SideActionsPanel.Visible = false;
                        Master.BreadCrumbs.Add(new BreadCrumb
                        {
                            Caption = ProjectResource.ProjectTemplates,
                            NavigationUrl = ProjectTemplatesConst.ProjectTemplatesPageTitle
                        });
                        Master.BreadCrumbs.Add(new BreadCrumb
                        {
                            Caption = ProjectResource.CreateProjectTemplate,
                            NavigationUrl = string.Empty
                        });

                        CreateProjectTemplateButton.ButtonText = ProjectResource.AddThisTemplate;
                        CreateProjectTemplateButton.AjaxRequestText = ProjectResource.TemplateCreationIsInProgress;

                        Page.ClientScript.RegisterClientScriptBlock(typeof(string), "set_input_focus", "; jq(function(){jq('#CreateProjectTemplateInput').focus();}); ", true);
                    }
                    break;
                default:
                    Master.BreadCrumbs.Add(new BreadCrumb
                    {
                        Caption = ProjectResource.ProjectTemplates,
                        NavigationUrl = string.Empty
                    });

                    if (ProjectTemplatesUtil.CheckCreatePermission())
                    {
                        SideActionsPanel.Controls.Add(new NavigationItem(ProjectResource.CreateProjectTemplate, ProjectTemplatesConst.CreateProjectTemplateUrl));
                    }
                    else
                    {
                        SideActionsPanel.Visible = false;
                    }
                    var templates = GetProjectTemplates();
                    if (templates == null || templates.Count == 0)
                    {
                        Page.ClientScript.RegisterClientScriptBlock(typeof(string), "emptyScreenContainer", "; jq(function(){ProjectTemplatesController.ShowProjectTemplatesEmptyScreen();}); ", true);
                    }
                    else
                    {
                        foreach (var t in templates)
                        {
                            ProjectTemplateItem templateItem = (ProjectTemplateItem)LoadControl(PathProvider.GetControlVirtualPath("ProjectTemplateItem.ascx"));
                            templateItem.Template = t;
                            ProjectTemplatesListContainer.Controls.Add(templateItem);
                            Page.ClientScript.RegisterClientScriptBlock(typeof(string), "highlightItems", "; jq(function(){ProjectTemplatesController.HighlightItems('ProjectTemplateHolder_');}); ", true);
                        }
                        ProjectTemplatesContainer.Visible = true;
                    }
                    break;
            }


        }

        protected List<TemplateProject> GetProjectTemplates()
        {
            try
            {
                var engine = Global.EngineFactory.GetTemplateEngine();
                var projects = engine.GetTemplateProjects();
                return projects;
            }
            catch
            {
                var projects = new List<TemplateProject>();
                projects.Add(new TemplateProject("aaaaaa"));
                projects.Add(new TemplateProject("bbbbbbb"));
                projects.Add(new TemplateProject("cccccccc"));
                return projects;
            }
        }


        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object CreateProjectTemplate(string title)
        {
            ProjectSecurity.DemandAuthentication();

            if (string.IsNullOrEmpty(title))
            {
                return new { ErrorMessage = ProjectResource.InvalidProjectTemplateTitle };
            }
            try
            {
                var engine = Global.EngineFactory.GetTemplateEngine();
                var template = engine.SaveTemplateProject(new TemplateProject(title));
                if (template != null)
                {
                    return new { Url = string.Format(ProjectTemplatesConst.TemplatePath, "overview", template.Id) };
                }
                throw new Exception("template is null");
            }
            catch (Exception e)
            {
                return new { ErrorMessage = e.Message };
            }

        }
    }
}
