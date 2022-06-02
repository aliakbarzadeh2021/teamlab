#region Import

using System;
using System.Web;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using ASC.Core.Users;
using ASC.Projects.Core;
using ASC.Projects.Core.Domain;
using ASC.Web.Studio.Utility;
using ASC.Web.Controls;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Projects.Classes;
using ASC.Core;
using ASC.Web.Projects.Resources;
using ASC.Data.Storage;
using AjaxPro;
using ASC.Web.Core.Users;
using System.Linq;

#endregion

namespace ASC.Web.Projects.Controls.ProjectTemplates.Tasks
{
	public partial class MoveTaskTemplateView : BaseUserControl
    {
		public TemplateProject Template { get; set; }
		public List<TemplateMilestone> Milestones { get; private set; }
		public string CssClassForEmptyMilestone { get { return Milestones.Count % 2 == 0 ? "tintMedium" : "tintLight"; } }

		protected void Page_Load(object sender, EventArgs e)
		{
			Milestones = Global.EngineFactory.GetTemplateEngine().GetTemplateMilestones(Template.Id);

			moveTaskContainer.Options.IsPopup = true;

			rptMilestoneSelector.DataSource = Milestones;
			rptMilestoneSelector.DataBind();
		}

		protected string GetMilestoneTitle(TemplateMilestone milestone) { return String.Format("[{0}] {1}", ProjectTemplatesUtil.GetMilestoneDeadLine(milestone), HtmlUtility.GetText(milestone.Title, 150)); }
    }
}