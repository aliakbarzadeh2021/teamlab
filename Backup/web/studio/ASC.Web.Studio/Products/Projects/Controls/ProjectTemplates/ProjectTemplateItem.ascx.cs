using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Projects.Core.Domain;
using ASC.Web.Core.Helpers;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Core.Users;

namespace ASC.Web.Projects.Controls.ProjectTemplates
{
	public partial class ProjectTemplateItem : BaseUserControl
	{

		public TemplateProject Template { get; set; }

		protected void Page_Load(object sender, EventArgs e)
		{

		}

		protected string TemplateDefaultUrl()
		{
			return string.Format(ProjectTemplatesConst.TemplatePath, "overview", Template.Id);
		}

		protected string TemplateMilestoneUrl()
		{
			return string.Format(ProjectTemplatesConst.TemplatePath, "milestones", Template.Id);
		}

		protected string TemplateTasksUrl()
		{
			return string.Format(ProjectTemplatesConst.TemplatePath, "tasks", Template.Id);
		}

		protected string TemplateMessagesUrl()
		{
			return string.Format(ProjectTemplatesConst.TemplatePath, "messages", Template.Id);
		}


		public string MilestonesCount
		{
			get
			{
				var count = Template.MilestonesCount;
				var s = GrammaticalHelper.ChooseNumeralCase(Convert.ToInt32(count), GrammaticalResource.MilestoneLowerCaseNominative, GrammaticalResource.MilestoneLowerCaseGenitiveSingular, GrammaticalResource.MilestoneLowerCaseGenitivePlural);
				return String.Format("{0} {1}", count, s);
			}
		}

		public string TasksCount
		{
			get
			{
				var count = Template.TasksCount;
				var s = GrammaticalHelper.ChooseNumeralCase(Convert.ToInt32(count), GrammaticalResource.TaskNominative, GrammaticalResource.TaskGenitiveSingular, GrammaticalResource.TaskGenitivePlural);
				return String.Format("{0} {1}", count, s);
			}
		}

		public string MessagesCount
		{
			get
			{
				var count = Template.MessagesCount;
				var s = GrammaticalHelper.ChooseNumeralCase(Convert.ToInt32(count), GrammaticalResource.DiscussionsNominative, GrammaticalResource.DiscussionsGenitiveSingular, GrammaticalResource.DiscussionsGenitivePlural);
				return String.Format("{0} {1}", count, s);
			}
		}

		public string EmployeesCount
		{
			get
			{
				var count = Template.Team.Count;
				var s = GrammaticalHelper.ChooseNumeralCase(Convert.ToInt32(count), GrammaticalResource.EmployeesNominative, GrammaticalResource.EmployeesGenitiveSingular, GrammaticalResource.EmployeesGenitivePlural);
				return String.Format("{0} {1}", count, s);
			}
		}

        public string GetContentWidth
        {
            get
            {
                if (ASC.Web.Projects.Controls.ProjectTemplates.ProjectTemplatesUtil.CheckEditPermission(Template))
                    return "width:500px;";
                else
                    return "width:690px;";
            }
        }


	}
}