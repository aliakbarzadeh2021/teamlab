using System;
using System.Collections.Generic;
using System.Text;
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
using ASC.Projects.Engine;

namespace ASC.Web.Projects.Controls.Tasks
{
    public class MilestonWithTasksWrapper
    {
        public Milestone Milestone { get; set; }
        public List<Task> Tasks { get; set; }
    }

    public partial class ListTaskView : BaseUserControl
    {
        public enum TaskFilter
        {
            MyTask = 1,
            AllTasks = 2,
            ByUser = 3
        }


        protected bool _isUnsortedBlock;
        protected bool _viewMilestoneBlock;
        protected Dictionary<Int32, bool> _taskHasTimeSpend;
        protected List<Milestone> _activeMilestones;

        protected string ProjectName { get { return Project.HtmlTitle.HtmlEncode(); } }
        protected int ProjectId { get { return Project.ID; } }

        public List<Task> Tasks { get; set; }

        public bool IsAllMyTasks { get; set; }

        public bool OneList { get; set; }

        public ProjectFat ProjectFat { get; set; }

        public Project Project { get { return this.ProjectFat.Project; } }

        public TaskFilter CurrentFilter { get; set; }


        protected void rptContent_OnItemDataBound(Object Sender, RepeaterItemEventArgs e)
        {
            if (!(e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)) return;

            var milestonWithTasksWrapper = (MilestonWithTasksWrapper)e.Item.DataItem;

            if (milestonWithTasksWrapper.Milestone == null)
                _isUnsortedBlock = true;
            else
                _isUnsortedBlock = false;

            var cntrlTaskBlockView = (TaskBlockView)e.Item.FindControl("_taskBlockView");

            cntrlTaskBlockView.ProjectFat = ProjectFat;
            cntrlTaskBlockView.IsAllMyTasks = this.IsAllMyTasks;
            cntrlTaskBlockView.OneList = this.OneList;

            cntrlTaskBlockView.TaskHasTimeSpend = _taskHasTimeSpend;
            cntrlTaskBlockView.BlockMilestone = milestonWithTasksWrapper.Milestone;
            cntrlTaskBlockView.OtherMilestones = ProjectFat.GetMilestones(MilestoneStatus.Open)
                                                            .FindAll(m => m != milestonWithTasksWrapper.Milestone);

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
                    return TenantUtil.DateTimeNow().Date > milestone.DeadLine.Date ? "class='pm-redText'" : "class='pm-blueText'";
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

        private String MilestoneDeadlineInfo(Milestone milestone)
        {
            String highlightClass = String.Empty;
            String deadLineString;
            switch (milestone.Status)
            {
                case MilestoneStatus.Open:
                    if (TenantUtil.DateTimeNow().Date > milestone.DeadLine.Date)
                    {
                        highlightClass = "pm-redText";
                        deadLineString = String.Format(MilestoneResource.MilestoneDeadlineLateInfo, "<strong>" + milestone.DeadLine.ToString(DateTimeExtension.ShortDatePattern) + "</strong>");
                    }
                    else
                    {

                        highlightClass = "pm-blueText";
                        deadLineString = "<strong>" + milestone.DeadLine.ToString(DateTimeExtension.ShortDatePattern) + "</strong>";

                    }
                    break;

                case MilestoneStatus.Closed:
                    highlightClass = "pm-greenText";
                    deadLineString = "<strong>" + milestone.DeadLine.ToString(DateTimeExtension.ShortDatePattern) + "</strong>";
                    break;

                default:
                    return String.Empty;
            }

            return String.Format("<span style='font-size:14px' class='{1}'>{0}</span>", deadLineString, highlightClass);
        }

        protected String RenderInfoBlock(Milestone milestone)
        {
            if (milestone != null)
            {
                String keyMilestoneImg = String.Format(@"<img align='absmiddle' title='{0}' src='{1}' alt='{0}' style='padding-right: 5px;'>",
                    MilestoneResource.RootMilestone,
                    WebImageSupplier.GetAbsoluteWebPath("key.png", ProductEntryPoint.ID));

                if (IsAllMyTasks)
                {
                    return String.Format(@"<div style='overflow-x:hidden;'> 
                                          {3}
                                         <a class='linkHeader' href='{2}'>
                                            {0}
                                         </a>
                                         <span style='margin-left:7px;'>
                                           {1}
                                         </span>                                    
                                         </div>",
                                            milestone.Title.HtmlEncode(),
                                            MilestoneDeadlineInfo(milestone),
                                            String.Format(@"milestones.aspx?prjID={0}&id={1}", milestone.Project.ID, milestone.ID),
                                            GetImageByDeadline(milestone)
                                         );
                }
                else
                {

                    return String.Format(@"<div style='float:left;  width:85px'>
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
                                        </div>", Global.RenderEntityPlate(EntityType.Milestone, true),
                                          milestone.Title.HtmlEncode(),
                                          MilestoneDeadlineInfo(milestone),
                                          String.Format(@"milestones.aspx?prjID={0}&id={1}", milestone.Project.ID, milestone.ID),
                                          milestone.IsKey ? keyMilestoneImg : String.Empty
                                        );
                }
            }
            return String.Format(@"<div class='headerBase'>
                                        {0}
                                       </div>", TaskResource.UnsortedTask);
        }

        protected String RenderAddTaskButton(Milestone milestone)
        {
            var sb = new StringBuilder();

            if (!IsAllMyTasks)
            {
                if (ProjectSecurity.CanCreateTask(ProjectFat.Project,false))
                {
                    if (milestone == null || (milestone != null && milestone.Status != MilestoneStatus.Closed))
                    {
                        var milestoneID = milestone != null ? milestone.ID.ToString() : "0";
                        var userID = UrlParameters.UserID != Guid.Empty ? String.Concat("'", UrlParameters.UserID.ToString(), "'") : "null";
                        var onclick = string.Format("javascript:ASC.Projects.TaskActionPage.init(-1,{0},{1}); ASC.Projects.TaskActionPage.show()", milestoneID, userID);
                        sb.AppendFormat("<a class='grayButton' href='javascript:void(0)' onclick=\"{0}\"><div></div>{1}</a>", onclick, TaskResource.AddNewTask);
                    }
                }
            }

            return sb.ToString();
        }

        protected List<Task> RecieveData()
        {

            List<Task> result = new List<Task>();

            switch (CurrentFilter)
            {
                case TaskFilter.MyTask:
                    if (IsAllMyTasks)
                        result = Tasks;
                    else
                        result = new List<Task>(Global.EngineFactory.GetTaskEngine().GetByProject(Project.ID, null, Page.Participant.ID)).FindAll(item => item.Status != TaskStatus.Disable);
                    break;
                case TaskFilter.AllTasks:
                    result = new List<Task>(Global.EngineFactory.GetTaskEngine().GetByProject(Project.ID, null, Guid.Empty)).FindAll(item => item.Status != TaskStatus.Disable);
                    break;
                case TaskFilter.ByUser:
                    result = new List<Task>(Global.EngineFactory.GetTaskEngine().GetByProject(Project.ID, null, UrlParameters.UserID)).FindAll(item => item.Status != TaskStatus.Disable);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("CurrentFilter");

            }

            if (result.Count == 0 && IsAllMyTasks)
            {
                this.Visible = false;
            }

            return result;

        }

        protected bool ViewAddTaskPanel(Milestone milestone)
        {
            var canAddTask = ProjectSecurity.CanCreateTask(ProjectFat.Project,false);
            if (milestone == null) return canAddTask;

            return milestone.Status != MilestoneStatus.Closed && canAddTask;
        }

        protected List<MilestonWithTasksWrapper> PreparationDataToView(List<Task> tasks)
        {
            _taskHasTimeSpend = Global.EngineFactory.GetTimeTrackingEngine().HasTime(tasks.ConvertAll(t => t.ID).ToArray());

            var milestonWithTasksWrappers = new List<MilestonWithTasksWrapper>();
            if (tasks.Count == 0) return milestonWithTasksWrappers;

            foreach (var task in tasks)
            {
                var milestone = milestonWithTasksWrappers.Find(m => (m.Milestone != null && m.Milestone.ID == task.Milestone) || (m.Milestone == null && task.Milestone == 0));
                if (milestone == null)
                {
                    milestone = new MilestonWithTasksWrapper()
                    {
                        Milestone = ProjectFat.GetMilestones().Find(m => m.ID == task.Milestone),
                        Tasks = new List<Task>()
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

            if (IsAllMyTasks)
                emptyScreenControl.Visible = false;

            _taskActionView.ProjectFat = ProjectFat;
            _moveTaskView.ProjectFat = ProjectFat;
            _phEmptyScreen.Controls.Add(emptyScreenControl);

        }

        protected void InitView()
        {
            InitControls();

            _rptContent.DataSource = PreparationDataToView(RecieveData());
            _rptContent.DataBind();

        }

        protected String RenderDeadline(Milestone milestone)
        {

            if (milestone == null) return String.Empty;

            return milestone.DeadLine.ToShortDateString();

        }

        protected void InitFilterBlock()
        {
            var SortControl = new ViewSwitcher();

            var combobox = new ViewSwitcherCombobox() {
                SortLabel = TaskResource.TasksByParticipant,
                HintText = UrlParameters.UserID == Guid.Empty
                        ? String.Empty: CoreContext.UserManager.GetUsers(UrlParameters.UserID).DisplayUserName(true)
        };

            combobox.ComboboxItems = new List<ViewSwitcherComboboxItem>();

            foreach (var participant in ProjectFat.GetTeam())
                combobox.ComboboxItems.Add(new ViewSwitcherComboboxItem
                {
                    SortLabel = participant.UserInfo.DisplayUserName(true),
                    SortUrl = string.Format("tasks.aspx?prjID={0}&action={1}&userID={2}", Project.ID, (int)TaskFilter.ByUser, participant.ID)

                });

            SortControl.SortItems.Add(new ViewSwitcherLinkItem
            {
                IsSelected = TaskFilter.MyTask == CurrentFilter ? true : false,
                SortUrl = string.Format("tasks.aspx?prjID={0}&action={1}", Project.ID, (int)TaskFilter.MyTask),
                SortLabel = TaskResource.MyTasks
            });

            SortControl.SortItems.Add(new ViewSwitcherLinkItem()
            {
                IsSelected = TaskFilter.AllTasks == CurrentFilter ? true : false,
                SortUrl = string.Format("tasks.aspx?prjID={0}&action={1}", Project.ID, (int)TaskFilter.AllTasks),
                SortLabel = TaskResource.AllTasks
            });

            SortControl.SortItems.Add(combobox);

            SortControl.SortItemsHeader = ProjectsCommonResource.Show + ":";

            _tabsContainer.Controls.Add(SortControl);

        }

        protected List<MilestonWithTasksWrapper> SortByStatus(List<MilestonWithTasksWrapper> list)
        {
            var lateMilestones = new List<MilestonWithTasksWrapper>();
            var upcomingMilestones = new List<MilestonWithTasksWrapper>();
            //var closedMilestones = new List<MilestonWithTasksWrapper>();
            var unsortedTasks = new MilestonWithTasksWrapper();

            foreach (var item in list)
            {
                //if (HaveOpenTasks(item))
                //{
                    if (item.Milestone != null)
                    {
                        switch (item.Milestone.Status)
                        {
                            case MilestoneStatus.Open:
                                if (TenantUtil.DateTimeNow().Date > item.Milestone.DeadLine.Date)
                                    lateMilestones.Add(item);
                                else
                                    upcomingMilestones.Add(item);
                                break;
                            //case MilestoneStatus.Closed:
                            //    closedMilestones.Add(item);
                            //    break;
                        }
                    }
                    else
                    {
                        unsortedTasks = item;
                    }
                //}
            }

            lateMilestones.Sort((x, y) => DateTime.Compare(x.Milestone.DeadLine, y.Milestone.DeadLine));
            upcomingMilestones.Sort((x, y) => DateTime.Compare(x.Milestone.DeadLine, y.Milestone.DeadLine));
            //closedMilestones.Sort((x, y) => DateTime.Compare(x.Milestone.DeadLine, y.Milestone.DeadLine));

            list = new List<MilestonWithTasksWrapper>();

            list.AddRange(lateMilestones);
            list.AddRange(upcomingMilestones);
            //list.AddRange(closedMilestones);

            if (unsortedTasks.Tasks != null)
                if (unsortedTasks.Tasks.Count > 0)
                    list.Add(unsortedTasks);

            return list;
        }

        protected bool HaveOpenTasks(MilestonWithTasksWrapper item)
        {
            var openTasks = item.Tasks.FindAll(t => t.Status != TaskStatus.Closed);
            return openTasks.Count > 0;
        }

        protected string GetImageByDeadline(Milestone milestone)
        {
            if (milestone == null)
                return "";

            string image = string.Empty;

            if (milestone.Status == MilestoneStatus.Closed)
                image = string.Format("<img align=\"absmiddle\" src=\"{0}\" alt=\"{1}\" title=\"{2}\">",
                    WebImageSupplier.GetAbsoluteWebPath("milestone_status_completed_16.png", ProductEntryPoint.ID),
                    MilestoneResource.ClosedMilestone, MilestoneResource.ClosedMilestone);

            if (milestone.Status == MilestoneStatus.Open)
                if (milestone.DeadLine.AddDays(1) < TenantUtil.DateTimeNow())
                    image = string.Format("<img align=\"absmiddle\" src=\"{0}\" alt=\"{1}\" title=\"{2}\">",
                        WebImageSupplier.GetAbsoluteWebPath("milestone_status_late_16.png", ProductEntryPoint.ID),
                        MilestoneResource.LateMilestone, MilestoneResource.LateMilestone);
                else image = string.Format("<img align=\"absmiddle\" src=\"{0}\" alt=\"{1}\" title=\"{2}\">",
                        WebImageSupplier.GetAbsoluteWebPath("milestone_status_active_16.png", ProductEntryPoint.ID),
                        MilestoneResource.OpenMilestone, MilestoneResource.OpenMilestone);

            return image;
        }

        #endregion

    }
}