using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Web.Controls;
using ASC.Web.Projects.Controls.ProjectTemplates;
using ASC.Web.Projects.Resources;
using ASC.Web.Projects.Classes;
using ASC.Projects.Core.Domain;
using ASC.Web.Studio.Controls.Common;
using AjaxPro;
using ASC.Web.Studio.Utility;
using ASC.Web.Projects.Controls;
using ASC.Projects.Engine;

namespace ASC.Web.Projects.TProject
{

	[AjaxNamespace("TemplateOverview")]
	public partial class Overview : BasePage
	{
		public TemplateProject Template { get; set; }

		private void InitTemplate()
		{
			Template = ProjectTemplatesUtil.GetProjectTemplate();
		}

		protected string GetProjectTemplateUrl(string pageName)
		{
			return string.Format(ProjectTemplatesConst.TemplatePath, pageName, UrlParameters.ProjectID);
		}


		protected override void PageLoad()
		{
			Utility.RegisterTypeForAjax(this.GetType());
			InitTemplate();			

			//ProjectTemplatesUtil.InitProjectTemplatesBreadcrumbs(Master, Template, null, null);

			Title = HeaderStringHelper.GetPageTitle(Template.Title.HtmlEncode(), Master.BreadCrumbs);

			Page.ClientScript.RegisterClientScriptBlock(typeof(string), "setPageBackgroundColor", "; jq(function(){ProjectTemplatesController.InitOverviewPage();}); ", true);
		}

		[AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
		public string DeleteProjectTemplate(int projectID)
		{
            ProjectSecurity.DemandAuthentication();

			try
			{
				var engine = Global.EngineFactory.GetTemplateEngine();
				engine.RemoveTemplateProject(projectID);
			}
			catch { }
			return ProjectTemplatesConst.ProjectTemplatesBaseAbsolute;
		}
	}
}
