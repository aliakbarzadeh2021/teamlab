using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects.Classes;
using System.Text;
using ASC.Web.Projects.Resources;
using ASC.Web.Core.Utility.Skins;

namespace ASC.Web.Projects.Controls.ProjectTemplates
{
	public partial class ListMilestonesTemplate : System.Web.UI.UserControl
	{
		public List<TemplateMilestone> MilestoneTemplates { get; set; }

		public TemplateProject Template { get; set; }

		protected void Page_Load(object sender, EventArgs e)
		{
			MilestoneTemplatesRepeater.DataSource = MilestoneTemplates;
			MilestoneTemplatesRepeater.DataBind();
		}

		public String GetCaption(TemplateMilestone milestone)
		{
			StringBuilder innerHTML = new StringBuilder();

			if (milestone.IsKey)
				innerHTML.AppendFormat(@"<img align='absmiddle' style='margin-right: 5px;' alt='{0}' src='{1}' title='{0}'>",
									MilestoneResource.RootMilestone, WebImageSupplier.GetAbsoluteWebPath("key.png", ASC.Web.Projects.Configuration.ProductEntryPoint.ID));

			innerHTML.AppendFormat(@"<a class='linkHeaderLight' href='{0}'>{1}</a>",
									GenerateMilestoneLink(milestone), milestone.Title.HtmlEncode());

			return innerHTML.ToString();
		}		

		protected static string GenerateMilestoneLink(TemplateMilestone milestone)
		{
			return string.Format(ProjectTemplatesConst.ItemInfoPath, "milestones", milestone.ProjectId, milestone.Id);
		}

		protected string GetMilestoneDeadLine(TemplateMilestone milestone)
		{
			return ProjectTemplatesUtil.GetMilestoneDeadLine(milestone);
		}
	}
}