using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASC.Web.Projects.Classes;

namespace ASC.Web.Projects.Controls.ProjectTemplates
{
	public class ProjectTemplatesConst
	{
		public const string ProjectTemplatesPageTitle = "projecttemplates.aspx";
		
		public static readonly string ProjectTemplatesBaseAbsolute = PathProvider.BaseAbsolutePath + ProjectTemplatesPageTitle;

		public const string ActionCreateProjectTemplate = "create";

		public static readonly string TemplateBaseAbsolutePath = PathProvider.BaseAbsolutePath + "tproject/";

		public static readonly string TemplatePath = string.Concat(TemplateBaseAbsolutePath, "{0}.aspx", "?", UrlConstant.ProjectID, "=", "{1}");

		public static readonly string CreateProjectTemplateUrl = string.Format("{0}?{1}={2}", ProjectTemplatesConst.ProjectTemplatesBaseAbsolute, UrlConstant.Action, ProjectTemplatesConst.ActionCreateProjectTemplate);

		public static readonly string PlanNewMilestonePath = string.Concat(ProjectTemplatesConst.TemplatePath, "&action=add");

		public static readonly string ItemInfoPath = string.Concat(TemplatePath, "&id={2}");

		public static readonly string ItemEditPath = string.Concat(ItemInfoPath, "&action=edit");

		public static readonly string CreateProjectFromTemplatePath = PathProvider.BaseAbsolutePath + "projects.aspx?&action=add&templateID={0}";
	}
}
