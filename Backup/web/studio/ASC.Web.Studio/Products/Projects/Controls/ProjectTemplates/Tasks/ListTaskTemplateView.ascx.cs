using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Projects.Core;
using ASC.Projects.Core.Domain;
using ASC.Web.Controls;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Resources;

namespace ASC.Web.Projects.Controls.ProjectTemplates.Tasks
{
    public class MilestonWithTasksWrapper
    {
        public TemplateMilestone Milestone { get; set; }
        public List<TemplateTask> Tasks { get; set; }
    }

    public partial class ListTaskTemplateView : BaseUserControl
    {
        public enum TaskFilter
        {
            MyTask = 1,
            AllTasks = 2,
            ByUser = 3
        }


        protected bool _isUnsortedBlock;
        protected bool _viewMilestoneBlock;

        public TemplateProject Template { get; set; }

        public int Interval { get; set; }
        public TaskFilter CurrentFilter { get; set; }


        protected void rptContent_OnItemDataBound(Object Sender, RepeaterItemEventArgs e)
        {
            if (!(e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)) return;

            var milestonWithTasksWrapper = (MilestonWithTasksWrapper)e.Item.DataItem;

            if (milestonWithTasksWrapper.Milestone == null)
                _isUnsortedBlock = true;
            else
                _isUnsortedBlock = false;

			var cntrlTaskBlockView = (TaskBlockTemplateView)e.Item.FindControl("_taskBlockView");

            cntrlTaskBlockView.Template = Template;
            cntrlTaskBlockView.MakeChangeStatus = true;
            cntrlTaskBlockView.BlockMilestone = milestonWithTasksWrapper.Milestone;
			var milestones = Global.EngineFactory.GetTemplateEngine().GetTemplateMilestones(Template.Id);
			cntrlTaskBlockView.OtherMilestones = milestones.FindAll(m => m != milestonWithTasksWrapper.Milestone);

            cntrlTaskBlockView.Items = milestonWithTasksWrapper.Tasks;
        }

        protected String MilestoneTitleHTML(Milestone milestone)
        {
            if (milestone == null) return TaskResource.UnsortedTask;

            var milestoneLocation = String.Format("milestones.aspx?prjID={0}&id={1}", milestone.Project.ID, milestone.ID);
            return String.Format("<a class='linkHeader' href='{1}'>{0}</a>", milestone.Title, milestoneLocation);
        }

        protected String MilestoneHighlightHTML(Milestone milestone)
        {
            if (milestone == null) return "style='display:none;'";

            switch (milestone.Status)
            {
                case MilestoneStatus.Open:
                    return ASC.Core.Tenants.TenantUtil.DateTimeNow().Date > milestone.DeadLine.Date ? "class='pm-redText'" : "class='pm-blueText'";
                case MilestoneStatus.Closed:
                    return "class='pm-greenText'";
                default:
                    return "style='display:none;'";
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitView();
            InitFilterBlock();
		}

        #region Methods

        private String MilestoneDeadlineInfo(TemplateMilestone milestone)
        {
            String highlightClass = String.Empty;
            String deadLineString;
			highlightClass = "pm-blueText";
			deadLineString = "<strong>" + ProjectTemplatesUtil.GetMilestoneDeadLine(milestone) + "</strong>";
            return String.Format("<span style='font-size:14px' class='{1}'>{0}</span>", deadLineString, highlightClass);
        }

        protected String RenderInfoBlock(TemplateMilestone milestone)
        {
            if (milestone != null)
            {
                String keyMilestoneImg = String.Format(@"<img align='absmiddle' title='{0}' src='{1}' alt='{0}' style='padding-right: 5px;'>",
                    MilestoneResource.RootMilestone,
                    WebImageSupplier.GetAbsoluteWebPath("key.png", ProductEntryPoint.ID));

                return String.Format(@"  <div style='float:left;  width:85px'>
                                           {0}
                                         </div>
                                        <div style='margin-left:100px; overflow: hidden; width: 420px'> 
                                          {4}
                                         <a class='linkHeader' href='{3}'>
                                            {1}
                                         </a>
                                         <div>
                                           {2}
                                         </div>                                    
                                        </div>                                     
                                   ", Global.RenderEntityPlate(EntityType.Milestone, true),
                                      milestone.Title.HtmlEncode(),
                                      MilestoneDeadlineInfo(milestone),
                                      String.Format(@"milestones.aspx?prjID={0}&id={1}", milestone.ProjectId, milestone.Id),
                                      milestone.IsKey ? keyMilestoneImg : String.Empty
                                    );
            }
            return String.Format(@"<div class='headerBase'>
                                        {0}
                                       </div>", TaskResource.UnsortedTask);
        }

        protected List<TemplateTask> RecieveData()
        {

			List<TemplateTask> result = new List<TemplateTask>();

			var engine = Global.EngineFactory.GetTemplateEngine();

			result = engine.GetTemplateTasks(Template.Id);

			switch (CurrentFilter)
			{
				case TaskFilter.MyTask:
					result = result.FindAll(item => item.Responsible == Page.Participant.ID);
					break;
				case TaskFilter.AllTasks:
					break;
				case TaskFilter.ByUser:
					result = result.FindAll(item => item.Responsible == UrlParameters.UserID);
					break;
			}
            return result;
        }

        protected bool ViewAddTaskPanel(Milestone milestone)
        {
			return true;
        }

        protected List<MilestonWithTasksWrapper> PreparationDataToView(List<TemplateTask> tasks)
        {
            var milestonWithTasksWrappers = new List<MilestonWithTasksWrapper>();
            if (tasks.Count == 0) return milestonWithTasksWrappers;

			var milestones = Global.EngineFactory.GetTemplateEngine().GetTemplateMilestones(Template.Id);

            foreach (var task in tasks)
            {
                var milestone = milestonWithTasksWrappers.Find(m => (m.Milestone != null && m.Milestone.Id == task.MilestoneId) || (m.Milestone == null && task.MilestoneId == 0));
                if (milestone == null)
                {
                    milestone = new MilestonWithTasksWrapper()
                    {
                        Milestone = milestones.Find(m => m.Id == task.MilestoneId),
                        Tasks = new List<TemplateTask>()
                    };
                    milestonWithTasksWrappers.Add(milestone);
                }
                milestone.Tasks.Add(task);
            }
            milestonWithTasksWrappers = SortByStatus(milestonWithTasksWrappers);
            return milestonWithTasksWrappers;
        }

        protected void InitControls()
        {
            group_manager_container.Options.IsPopup = true;

            EmptyScreenControl emptyScreenControl = new EmptyScreenControl();

            switch (CurrentFilter)
            {
                case TaskFilter.MyTask:
                    emptyScreenControl.HeaderContent = TaskResource.EmptyScreenMyTasksHeaderContent;
                    break;
                case TaskFilter.AllTasks:
                    emptyScreenControl.HeaderContent = TaskResource.EmptyScreenAllTasksHeaderContent;
                    break;
                case TaskFilter.ByUser:
                    emptyScreenControl.HeaderContent = TaskResource.EmptyScreenByUserTasksHeaderContent;
                    emptyScreenControl.HeaderContent = String.Format(emptyScreenControl.HeaderContent,
                                                                     ASC.Core.CoreContext.UserManager.GetUsers(
                                                                         UrlParameters.UserID).DisplayUserName());
                    break;
            }
            _taskActionView.Template = Template;
			_moveTaskView.Template = Template;
            _phEmptyScreen.Controls.Add(emptyScreenControl);
        }

        protected void InitView()
        {
            if (String.Compare(Request["view"], "all", true) == 0)
            {
                int interval;
                if (!int.TryParse(Request["interval"], out interval))
                {
                    interval = 3;
                }
                Interval = interval;
            }

            InitControls();

            _rptContent.DataSource = PreparationDataToView(RecieveData());
            _rptContent.DataBind();
        }

        protected String RenderDeadline(Milestone milestone)
        {
			if (milestone == null)
			{
				return String.Empty;
			}
            return milestone.DeadLine.ToShortDateString();
        }

        protected void InitFilterBlock()
        {
            var SortControl = new ViewSwitcher();			

            SortControl.SortItems.Add(new ViewSwitcherLinkItem
            {
                IsSelected = TaskFilter.MyTask == CurrentFilter ? true : false,
                SortUrl = string.Format("tasks.aspx?prjID={0}&action={1}", Template.Id, (int)TaskFilter.MyTask),
                SortLabel = TaskResource.MyTasks
            });

            SortControl.SortItems.Add(new ViewSwitcherLinkItem()
            {
                IsSelected = TaskFilter.AllTasks == CurrentFilter ? true : false,
                SortUrl = string.Format("tasks.aspx?prjID={0}&action={1}", Template.Id, (int)TaskFilter.AllTasks),
                SortLabel = TaskResource.AllTasks
            });

			if (Template.Team != null && Template.Team.Count > 0)
			{
				var combobox = new ViewSwitcherCombobox() { SortLabel = TaskResource.TasksByParticipant };
				combobox.ComboboxItems = new List<ViewSwitcherComboboxItem>();
				foreach (var prt in Template.Team)
				{
					var participant = new Participant(prt);
					combobox.ComboboxItems.Add(new ViewSwitcherComboboxItem
					{
						SortLabel = participant.UserInfo.DisplayUserName(true),
						SortUrl = string.Format("tasks.aspx?prjID={0}&action={1}&userID={2}", Template.Id, (int)TaskFilter.ByUser, participant.ID)
					});
				}
				SortControl.SortItems.Add(combobox);
			}            

            SortControl.SortItemsHeader = ProjectsCommonResource.Show + ":";

            _tabsContainer.Controls.Add(SortControl);
        }

        protected List<MilestonWithTasksWrapper> SortByStatus(List<MilestonWithTasksWrapper> list)
        {
            var upcomingMilestones = new List<MilestonWithTasksWrapper>();
            var unsortedTasks = new MilestonWithTasksWrapper();

            foreach (var item in list)
            {
                if (item.Milestone != null)
                {
					upcomingMilestones.Add(item);
                }
                else
                {
                    unsortedTasks = item;
                }
            }

			list = new List<MilestonWithTasksWrapper>();
            list.AddRange(upcomingMilestones);

            if (unsortedTasks.Tasks != null)
                if (unsortedTasks.Tasks.Count > 0)
                    list.Add(unsortedTasks);

            return list;
        }



		protected String RenderAddTaskButton(TemplateMilestone milestone)
		{
			var sb = new StringBuilder();

				if (Global.IsAdmin)
				{
						var milestoneID = milestone != null ? milestone.Id.ToString() : "0";
						var userID = UrlParameters.UserID != Guid.Empty ? String.Concat("'", UrlParameters.UserID.ToString(), "'") : "null";
						var onclick = string.Format("javascript:ASC.Projects.TaskActionPage.init(-1,{0},{1}); ASC.Projects.TaskActionPage.show()",
													milestoneID, userID);
						sb.AppendFormat("<a class='grayButton' href='javascript:void(0)' onclick='{0}'><div></div>{1}</a>",
										onclick, TaskResource.AddNewTask);
				}

			return sb.ToString();
		}

        #endregion

    }
}