using ASC.Web.Controls;
using ASC.Web.Projects.Masters;
using ASC.Web.Projects.Resources;
using ASC.Web.Projects.Classes;
using ASC.Projects.Core.Domain;
using ASC.Web.Studio.Controls.Common;
using AjaxPro;
using System;
using System.Web;
using ASC.Core;

namespace ASC.Web.Projects.Controls.ProjectTemplates
{
    [AjaxNamespace("TemplatesUtil")]
    public static class ProjectTemplatesUtil
    {

        public static void InitProjectTemplatesBreadcrumbs(BasicTemplate master, TemplateProject template, string pageTitle, string pageName)
        {
            if (template == null)
            {
                if (HttpContext.Current == null) throw new ApplicationException("Not in http request");
                RedirectToProjectTemplatesBase();
                return;
            }
            master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = ProjectResource.ProjectTemplates,
                NavigationUrl = ProjectTemplatesConst.ProjectTemplatesBaseAbsolute
            });
            //master.BreadCrumbs.Add(new BreadCrumb
            //{
            //    Caption = template.Title,
            //    NavigationUrl = GetTemplatePageUrl("overview")
            //});
            if (!string.IsNullOrEmpty(pageTitle) && !string.IsNullOrEmpty(pageName))
            {
                master.BreadCrumbs.Add(new BreadCrumb
                {
                    Caption = pageTitle,
                    NavigationUrl = GetTemplatePageUrl(pageName.ToLower())
                });
            }
        }

        private static void RedirectToProjectTemplatesBase()
        {
            HttpContext.Current.Response.Redirect(ProjectTemplatesConst.ProjectTemplatesBaseAbsolute, true);
        }

        public static int ProjectId()
        {
            int projectId;
            if (!int.TryParse(UrlParameters.ProjectID, out projectId))
            {
                RedirectToProjectTemplatesBase();
            }
            return projectId;
        }

        public static TemplateProject GetProjectTemplate()
        {
            int projectID = ProjectTemplatesUtil.ProjectId();
            var template = Global.EngineFactory.GetTemplateEngine().GetTemplateProject(projectID);
            if (template == null)
            {
                RedirectToProjectTemplatesBase();
            }
            return template;
        }

        public static string GetTemplatePageUrl(string pageName)
        {
            var projectID = RequestContext.GetCurrentProjectId();
            return string.Format(ProjectTemplatesConst.TemplatePath, pageName.ToLower(), projectID);
        }

        public static void AddCreateProjectFromTemplateActions(SideActions SideActionsPanel, TemplateProject template)
		{
			AjaxPro.Utility.RegisterTypeForAjax(typeof(ProjectTemplatesUtil));

			if (!CheckEditPermission(template))
			{
				SideActionsPanel.Controls.Clear();
			}
            if (ProjectTemplatesUtil.CheckCreatePermission())
            {
                SideActionsPanel.Controls.Add(new NavigationItem(ProjectResource.CreateProjectFromTemplate,string.Format(ProjectTemplatesConst.CreateProjectFromTemplatePath, template.Id)));
            }
		}

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public static string CreateProjectFromTemplate(int projectID)
        {
            try
            {
                var project = Global.EngineFactory.GetTemplateEngine().CreateProjectFromTemplate(projectID);
                if (project != null)
                {
                    return PathProvider.BaseAbsolutePath + "projects.aspx?prjID=" + project.ID;
                }
            }
            catch { }
            return string.Empty;
        }

        public static void GetMilestoneDeadLine(int milestoneDays, out int week, out int dayOfWeek)
        {
            week = milestoneDays / 10;
            dayOfWeek = milestoneDays - week * 10;
            week = week < 1 ? 1 : week;
        }

        public static string GetMilestoneDeadLine(TemplateMilestone milestone)
        {
            int week, day;
            ProjectTemplatesUtil.GetMilestoneDeadLine(milestone.DurationInDays, out week, out day);
            return string.Format("{0} {1}, {2}", ReportResource.Week, week, System.Globalization.DateTimeFormatInfo.CurrentInfo.GetDayName((DayOfWeek)day));
        }


        public static bool CheckEditPermission(TemplateProject template)
        {
            return Global.IsAdmin || (template != null && template.CreateBy == SecurityContext.CurrentAccount.ID);
        }

        public static bool CheckEditPermission()
        {
            int templateId = 0;
            int.TryParse(UrlParameters.ProjectID, out templateId);
            return CheckEditPermission(Global.EngineFactory.GetTemplateEngine().GetTemplateProject(templateId));
        }

        public static bool CheckCreatePermission()
        {
            return SecurityContext.IsAuthenticated;
        }
    }
}
