#region Import

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using AjaxPro;
using ASC.Core;
using ASC.Core.Users;
using ASC.Notify;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Projects.Core;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Web.Controls;
using ASC.Web.Controls.CommentInfoHelper;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;
using System.Web;
using ASC.Core.Tenants;
using ASC.Data.Storage;
using ASC.Web.Studio.Helpers;
using ASC.Projects.Core.Domain.Reports;
using ASC.Projects.Engine;

#endregion

namespace ASC.Web.Projects.Controls.ProjectTemplates.Tasks
{

    [AjaxNamespace("AjaxPro.TaskDetailsView")]
	public partial class TaskDetailsTemplateView : BaseUserControl
    {
        #region Members

        #endregion

        #region Property

        public TemplateTask Task { get; set; }
        public int AttachedImagesCount { get; set; }

		public TemplateProject Template { get; set; }

		public string MilestoneTitle
		{
			get
			{
				try
				{
					return Global.EngineFactory.GetTemplateEngine().GetTemplateMilestone(Task.MilestoneId).Title.HtmlEncode();
				}
				catch { return string.Empty; }
			}
		}

        #endregion

        #region Methods

        protected String RenderActionBlock(TemplateTask task)
        {
            var innerHTML = new StringBuilder();
            innerHTML.AppendLine("<dl class='pm-flexible'>");
            innerHTML.AppendLine("<dt class='textBigDescribe'>&nbsp;</dt>");
            innerHTML.AppendLine("<dd>");
            innerHTML.AppendLine("</dd>");
            innerHTML.AppendLine("</dl>");

            return innerHTML.ToString();
        }

        public string GetTimeTrackingImagePath()
        {
            if (Global.EngineFactory.GetTimeTrackingEngine().HasTime(Task.Id))
                return WebImageSupplier.GetAbsoluteWebPath("clock_active.png", ProductEntryPoint.ID);
            else
                return WebImageSupplier.GetAbsoluteWebPath("clock_noactive.png", ProductEntryPoint.ID);
        }

        #region AjaxPro

        [AjaxMethod]
        public int SaveOrUpdateTask(int projectID, int taskID, int milestoneID, String title, String description, Guid responsible)
        {
            ProjectSecurity.DemandAuthentication();
            
            Task = taskID != -1 ? Global.EngineFactory.GetTemplateEngine().GetTemplateTask(taskID) : new TemplateTask(projectID, title);

            Task.MilestoneId = milestoneID;
            Task.Title = title;
            Task.Description = description;
            Task.Responsible = responsible;
			//Task.Priority = (TaskPriority)Enum.Parse(typeof(TaskPriority), priority.ToString());
            Task.ProjectId = projectID;

            Global.EngineFactory.GetTemplateEngine().SaveTemplateTask(Task);

			//if (responsible.Equals(Guid.Empty)) Global.EngineFactory.GetTaskEngine().ChangeStatus(Task,TaskStatus.Unclassified);

            if (taskID == -1) return Task.Id;

            return -1;


        }

        public String RenderPriorityBlock(TaskPriority priority)
        {


            var innerHTML = new StringBuilder();


            String imagePath;
            String priorityTitle;

            switch (priority)
            {

                case TaskPriority.High:
                    priorityTitle = ResourceEnumConverter.ConvertToString(TaskPriority.High);
                    imagePath = WebImageSupplier.GetAbsoluteWebPath("prior_hi.png",
                                                                                               ProductEntryPoint.ID);

                    innerHTML.AppendFormat(@"
                       <img align='absmiddle' src='{0}' style='border: 0px;' />
                       <span style='padding-left:3px'>{1}</span>",
                       imagePath, priorityTitle);

                    break;

                case TaskPriority.Normal:
                    priorityTitle = ResourceEnumConverter.ConvertToString(TaskPriority.Normal);

                    innerHTML.AppendFormat(@"<span>{0}</span>", priorityTitle);

                    break;
                case TaskPriority.Low:
                    priorityTitle = ResourceEnumConverter.ConvertToString(TaskPriority.Low);
                    imagePath = WebImageSupplier.GetAbsoluteWebPath("prior_lo.png",
                                                                           ProductEntryPoint.ID);

                    innerHTML.AppendFormat(@"
                       <img align='absmiddle' src='{0}' style='border: 0px;' />
                       <span style='padding-left:3px'>{1}</span>",
                       imagePath, priorityTitle);

                    break;
            }

            return innerHTML.ToString();

        }


		//[AjaxMethod]
		//public String ChangeSubscribe(int taskID)
		//{

		//    Task task = Global.EngineFactory.GetTaskEngine().GetByID(taskID);

		//    String objectID = String.Format("{0}_{1}", task.UniqID, task.Project.ID);

		//    var objects = new List<String>(NotifySource.Instance.GetSubscriptionProvider().GetSubscriptions(
		//      NotifyConstants.Event_NewCommentForTask,
		//      NotifySource.Instance.GetRecipientsProvider().GetRecipient(
		//          SecurityContext.CurrentAccount.ID.ToString())
		//      ));

		//    bool subscribed = !String.IsNullOrEmpty(objects.Find(item => String.Compare(item, objectID, true) == 0));

		//    if (subscribed)
		//    {
		//        NotifySource.Instance.GetSubscriptionProvider().UnSubscribe(NotifyConstants.Event_NewCommentForTask, objectID, NotifySource.Instance.GetRecipientsProvider().GetRecipient(ASC.Core.SecurityContext.CurrentAccount.ID.ToString()));

		//        return ProjectsCommonResource.SubscribeOnNewComment;


		//    }
		//    else
		//    {
		//        NotifySource.Instance.GetSubscriptionProvider().Subscribe(NotifyConstants.Event_NewCommentForTask, objectID, NotifySource.Instance.GetRecipientsProvider().GetRecipient(ASC.Core.SecurityContext.CurrentAccount.ID.ToString()));

		//        return ProjectsCommonResource.UnSubscribeOnNewComment;
		//    }
		//}

		//[AjaxMethod]
		//public void ExecChangeStatus(int taskID, int newStatus)
		//{

		//    Global.EngineFactory.GetTaskEngine().ChangeStatus(Global.EngineFactory.GetTaskEngine().GetByID(taskID), (TaskStatus)Enum.ToObject(typeof(TaskStatus), newStatus));

		//}

        [AjaxMethod]
        public void ExecBeResponsible(int taskID)
        {
            ProjectSecurity.DemandAuthentication();
			
            var engine = Global.EngineFactory.GetTemplateEngine();
            TemplateTask target = engine.GetTemplateTask(taskID);
            target.Responsible = SecurityContext.CurrentAccount.ID;
			engine.SaveTemplateTask(target);
        }

        [AjaxMethod]
        public void ExecDeleteTask(int taskID)
        {
            ProjectSecurity.DemandAuthentication();

            Global.EngineFactory.GetTemplateEngine().RemoveTemplateTask(taskID);
        }

        [AjaxMethod]
        public void DeleteFile(int id, int version)
        {
        }

        #endregion

        protected string GetTaskResponsible()
        {
            if (!Task.Responsible.Equals(Guid.Empty))
                return StudioUserInfoExtension.RenderProfileLink(ASC.Core.CoreContext.UserManager.GetUsers(Task.Responsible), ProductEntryPoint.ID);
            return TaskResource.WithoutResponsible;
        }

		protected void InitControls()
		{
			if (ProjectTemplatesUtil.CheckEditPermission(Template))
			{
				TaskActionTemplateView cntrlTaskActionView = (TaskActionTemplateView)LoadControl(PathProvider.GetControlVirtualPath("TaskActionTemplateView.ascx"));
				cntrlTaskActionView.Template = Template;
				cntrlTaskActionView.Target = Task;
				phAddTaskPanel.Controls.Add(cntrlTaskActionView);
			}
		}

        protected void WriteClientScripts()
        {

            if (Page.ClientScript.IsClientScriptBlockRegistered(typeof(TaskDetailsTemplateView), "7883251B-1149-4617-8490-45AF33F2631A"))
                return;

            if (Task.MilestoneId != 0)
				Page.ClientScript.RegisterClientScriptBlock(typeof(TaskDetailsTemplateView), "E58A5C45-ACB2-4bf3-8A2C-83BA25C7018B", "CurrTask = " +
                                                            JavaScriptSerializer.Serialize(
                                                                new
                                                                {
                                                                    Title = HttpUtility.HtmlDecode(Task.Title),
                                                                    Task.Id,
                                                                    ResponsibleID = Task.Responsible,
                                                                    Task.Priority,
                                                                    Description = Task.Description,
                                                                    MilestoneID = Task.MilestoneId
                                                                }) + "; ", true);

            else
				Page.ClientScript.RegisterClientScriptBlock(typeof(TaskDetailsTemplateView), "E58A5C45-ACB2-4bf3-8A2C-83BA25C7018B", "CurrTask = " +
                                                                          JavaScriptSerializer.Serialize(
                                                                              new
                                                                              {
                                                                                  Title = HttpUtility.HtmlDecode(Task.Title),
                                                                                  Task.Id,
                                                                                  ResponsibleID = Task.Responsible,
                                                                                  Task.Priority,
                                                                                  Description = Task.Description,
                                                                                  MilestoneID = -1
																			  }) + "; ", true);


        }

        protected double TaskHoursCount()
        {
            double count = 0;

            List<TimeSpend> list = Global.EngineFactory.GetTimeTrackingEngine().GetByTask(Task.Id);

            foreach (TimeSpend ts in list)
            {
                count += ts.Hours;
            }

            return Math.Round(count, 2);
        }

        public string GetFilesImgURL()
        {
            return WebImageSupplier.GetAbsoluteWebPath("skrepka.gif", ProductEntryPoint.ID);
        }

        public string GetReportUri()
        {
            var filter = new ReportFilter();
            filter.TimeInterval = ReportTimeInterval.Absolute;
            filter.ViewType = 1;
            filter.ProjectIds.Add(Task.ProjectId);
            return string.Format("reports.aspx?action=generate&reportType=8&{0}", filter.ToUri());
        }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {

			AjaxPro.Utility.RegisterTypeForAjax(typeof(TaskDetailsTemplateView));
            AjaxPro.Utility.RegisterTypeForAjax(typeof(TaskBlockTemplateView));


            InitControls();

            //WriteClientScripts();

            Global.EngineFactory.GetParticipantEngine().Read(Page.Participant.ID, Task.Id.ToString(), TenantUtil.DateTimeNow());
        }

        #endregion

    }
}