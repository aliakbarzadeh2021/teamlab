using System.Collections.Generic;
using System.Linq;
using ASC.Core;
using ASC.Projects.Core.Domain;
using ASC.Web.Controls;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Controls;
using ASC.Web.Projects.Controls.Tasks;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;
using ASC.Web.Projects.Controls.TimeSpends;
using System;
using ASC.Projects.Core.Domain.Reports;
using System.Xml.Xsl;
using System.Web;
using System.IO;
using System.Xml;
using System.Text;
using ASC.Core.Users;
using ASC.Web.Projects.Configuration;
using ASC.Core.Tenants;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Utility.Skins;
using AjaxPro;

namespace ASC.Web.Projects
{
    [AjaxNamespace("AjaxPro.MyTasksPage")]
    public partial class MyTasks : BasePage
    {
        #region Properties

        protected bool OneList { get { return !string.IsNullOrEmpty(Request["show"]); } }

        protected bool IsToDoList { get { return !string.IsNullOrEmpty(Request["type"]) && Request["type"] == "todo"; } }

        protected string ToDoFilter { get { return string.IsNullOrEmpty(Request["show"]) ? "all" : Request["show"]; } }

        #endregion


        protected override void PageLoad()
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(MyTasks));
            AjaxPro.Utility.RegisterTypeForAjax(typeof(TimeSpendActionView));
            InitControls();
            RenderContent();
        }

        private void InitControls()
        {
            Title = HeaderStringHelper.GetPageTitle(TaskResource.MyTasks, Master.BreadCrumbs);
            Master.DisabledSidePanel = true;
            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = TaskResource.MyTasks,
                NavigationUrl = "mytasks.aspx"
            });
            if (Global.ModuleManager.IsVisible(ModuleType.TimeTracking))
            {
                TimeSpendActionView _timeSpendActionView = (TimeSpendActionView)LoadControl(PathProvider.GetControlVirtualPath("TimeSpendActionView.ascx"));
                timeSpendPlaceHolder.Controls.Add(_timeSpendActionView);
            }
            //InitTasks();
            _empty.Controls.Add(new EmptyScreenControl(string.Empty) { HeaderContent = ProjectsCommonResource.NothingHasBeenFound });

            InitFilterBlock();
        }

        private void InitTasks()
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(TaskBlockView));

            if (Global.ModuleManager.IsVisible(ModuleType.TimeTracking))
            {
                TimeSpendActionView _timeSpendActionView = (TimeSpendActionView)LoadControl(PathProvider.GetControlVirtualPath("TimeSpendActionView.ascx"));
                timeSpendPlaceHolder.Controls.Add(_timeSpendActionView);
            }

            var tasks = Global.EngineFactory.GetTaskEngine().GetByResponsible(SecurityContext.CurrentAccount.ID, TaskStatus.Open, TaskStatus.NotAccept);

            if (OneList)
            {
                ListTaskView view = null;
                tasks.Sort((x, y) => x.Priority != y.Priority ? y.Priority.CompareTo(x.Priority) : x.Project.ID.CompareTo(y.Project.ID));
                foreach (var t in tasks)
                {
                    if (view == null || view.ProjectFat.Project.ID != t.Project.ID)
                    {
                        view = (ListTaskView)LoadControl(PathProvider.GetControlVirtualPath("ListTaskView.ascx"));

                        view.ProjectFat = new ProjectFat(t.Project);
                        view.Tasks = new List<Task> { t };
                        view.CurrentFilter = ListTaskView.TaskFilter.MyTask;
                        view.IsAllMyTasks = true;
                        view.OneList = OneList;

                        _content.Controls.Add(view);
                    }
                    else
                    {
                        view.Tasks.Add(t);
                        view.Tasks.Sort((x, y) => x.SortOrder.CompareTo(y.SortOrder));
                    }
                }
            }
            else
            {
                foreach (var g in tasks.GroupBy(t => t.Project))
                {
                    var view = (ListTaskView)LoadControl(PathProvider.GetControlVirtualPath("ListTaskView.ascx"));

                    view.ProjectFat = new ProjectFat(g.Key);
                    view.Tasks = g.OrderBy(t => t.SortOrder).ToList();
                    view.CurrentFilter = ListTaskView.TaskFilter.MyTask;
                    view.IsAllMyTasks = true;
                    view.OneList = OneList;

                    _content.Controls.Add(view);
                }
            }
        }

        protected void InitFilterBlock()
        {
            var SortControl = new ViewSwitcher();

            SortControl.SortItems.Add(new ViewSwitcherLinkItem
            {
                IsSelected = !string.IsNullOrEmpty(Request["show"]),
                SortUrl = "mytasks.aspx?show=list",
                SortLabel = TaskResource.OneList
            });

            SortControl.SortItems.Add(new ViewSwitcherLinkItem()
            {
                IsSelected = string.IsNullOrEmpty(Request["show"]),
                SortUrl = "mytasks.aspx",
                SortLabel = TaskResource.ProjectsMilestones
            });

            SortControl.SortItemsHeader = ProjectsCommonResource.Show + ":";

            _tabsContainer.Controls.Add(SortControl);

        }

        protected string RenderTypeSwitcher()
        {
            if (IsToDoList)
            {
                return string.Format(@"<span class=""pm_taskIcon""></span><a class=""linkAction"" href=""{1}"">{0}</a>", TaskResource.SwitchToTaskList, "mytasks.aspx");
            }
            else
            {
                return string.Format(@"<span class=""pm_todoIcon""></span><a class=""linkAction"" href=""{1}"">{0}</a>", TaskResource.SwitchToToDoList, "mytasks.aspx?type=todo");
            }
        }

        private void RenderContent()
        {
            IList<object[]> result = null;

            var filter = new ReportFilter();
            filter.TaskStatuses.Add(TaskStatus.Open);
            filter.UserId = SecurityContext.CurrentAccount.ID;
            filter.TimeInterval = ReportTimeInterval.Absolute;

            result = Global.EngineFactory.GetReportEngine().BuildReport(ReportType.TasksByUsers, filter);
            
            result = AddMilestoneParams(result);
            result = AddTaskParams(result);

            if (result.Count != 0)
                myTasksContent.Text = Transform(result);
        }

        protected string Transform(IList<object[]> rows)
        {
            var xml = new StringBuilder()
                .Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>")
                .Append("<reportResult>");
            foreach (var row in rows)
            {
                xml.Append("<r ");
                for (int i = 0; i < row.Length; i++)
                {
                    xml.AppendFormat("c{0}=\"{1}\" ", i, i != 11 ? ToString(row[i]) : ToString(row[i]).ReplaceSingleQuote().Replace("\n", "<br/>").HtmlEncode().Replace("'", "-"));
                }
                xml.Append("/>");
            }
            xml.Append("</reportResult>");

            return Transform(xml.ToString());
        }

        protected string ToString(object value)
        {
            if (value == null) return null;
            if (value is Enum) return ((Enum)value).ToString("d");
            if (value is DateTime) return ((DateTime)value).ToString("o");
            return value.ToString().Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        protected string Transform(string xml)
        {
            var xslt = GetXslTransform("MyTasks.xsl");
            if (xslt == null) throw new InvalidOperationException("Xslt not found");

            using (var reader = XmlReader.Create(new StringReader(xml.ToString())))
            using (var writer = new StringWriter())
            using (var xmlwriter = XmlWriter.Create(writer, new XmlWriterSettings() { Encoding = Encoding.UTF8 }))
            {
                xslt.Transform(reader, GetXslParameters(), writer);
                return writer.ToString();
            }
        }

        protected XslCompiledTransform GetXslTransform(string fileName)
        {
            //use const products\projects\data\templates for async not in http request
            fileName = Path.Combine(HttpRuntime.AppDomainAppPath, "products\\projects\\templates\\" + fileName).ToLower();
            if (File.Exists(fileName))
            {
                var transform = new XslCompiledTransform();
                transform.Load(fileName);
                return transform;
            }
            return null;
        }

        protected XsltArgumentList GetXslParameters()
        {
            var parameters = new XsltArgumentList();

            parameters.AddParam("p0", string.Empty, ProjectResource.Project);
            parameters.AddParam("p1", string.Empty, TaskResource.TaskTitle);
            parameters.AddParam("p2", string.Empty, TaskResource.ActiveTaskCheckBoxTooltip);
            parameters.AddParam("p3", string.Empty, ProjectsCommonResource.TimeTracking);
            parameters.AddParam("p4", string.Empty, TaskResource.UnsortedTask);
            parameters.AddParam("p5", string.Empty, WebImageSupplier.GetAbsoluteWebPath("project_search_icon.png", ProductEntryPoint.ID));
            parameters.AddParam("p6", string.Empty, WebImageSupplier.GetAbsoluteWebPath("loader_12.gif", ProductEntryPoint.ID));
            parameters.AddParam("p7", string.Empty, OneList);
            parameters.AddParam("p8", string.Empty, WebImageSupplier.GetAbsoluteWebPath("start-track.png", ProductEntryPoint.ID));
            parameters.AddParam("p9", string.Empty, ProjectsCommonResource.AutoTimer);
            
            return parameters;
        }

        protected IList<object[]> AddTaskParams(IList<object[]> rows)
        {
            var result = new List<object[]>();
            var tmp = new List<object[]>();
            var tasks = new List<int>();

            foreach (var row in rows)
            {
                var list = new List<object>(row);
                var taskDeadline = new DateTime();
                if (DateTime.TryParse((string)row[10], out taskDeadline))
                {
                    list = AddTaskDeadlineParams(list, taskDeadline);
                }
                else
                {
                    list.Add(string.Empty);
                    list.Add(string.Empty);
                }
                tmp.Add(list.ToArray());
                tasks.Add(Convert.ToInt32(row[6]));
            }

            var dic = Global.EngineFactory.GetTimeTrackingEngine().HasTime(tasks.ToArray());
            
            foreach (var row in tmp)
            {
                var list = new List<object>(row);
                list = AddTaskTimeParams(list, dic[Convert.ToInt32(row[6])]);
                result.Add(list.ToArray());
            }
            return result;
        }

        protected List<object> AddTaskTimeParams(List<object> list, bool hasTime)
        {
            if (hasTime)
            {
                list.Add(WebImageSupplier.GetAbsoluteWebPath("clock_active.png", ProductEntryPoint.ID));
                list.Add(1);
            }
            else
            {
                list.Add(WebImageSupplier.GetAbsoluteWebPath("clock_noactive.png", ProductEntryPoint.ID));
                list.Add(0);
            }
            return list;
        }

        protected List<object> AddTaskDeadlineParams(List<object> list, DateTime deadline)
        {
            if (deadline == null || deadline == DateTime.MinValue)
            {
                list.Add(string.Empty);
                list.Add(string.Empty);
                return list;
            }

            int count = deadline.Date.Subtract(TenantUtil.DateTimeNow().Date).Days;

            if (count > 0)
            {
                list.Add("describeText");
                list.Add(string.Format("{0} {1}", Math.Abs(count), TaskResource.DaysLeft));
                return list;
            }

            if (count < 0)
            {
                list.Add("pm-taskDeadlineLateInfoContainer");
                list.Add(string.Format("{0} {1}", TaskResource.TaskDeadlineLateInfo, deadline.ToString(DateTimeExtension.DateFormatPattern)));
                return list;
            }

            list.Add("pm-redText");
            list.Add(ProjectsCommonResource.Today);
            return list;
        }

        protected IList<object[]> AddMilestoneParams(IList<object[]> rows)
        {
            var result = new List<object[]>();
            foreach (var row in rows)
            {
                var list = new List<object>(row);
                
                if ((int)row[5] != -1)
                {
                    var milestoneStatus = (MilestoneStatus)row[5];
                    if (milestoneStatus == MilestoneStatus.Open)
                    {
                        var milestoneDeadline = new DateTime();
                        if (DateTime.TryParse((string)row[4], out milestoneDeadline))
                        {
                            list = AddMilestoneImgParams(list, milestoneDeadline, milestoneStatus);
                            list = AddMilestoneDeadlineInfo(list, milestoneDeadline, milestoneStatus);
                        }
                        else
                        {
                            list.Add(string.Empty);
                            list.Add(string.Empty);
                            list.Add(string.Empty);
                            list.Add(string.Empty);
                            list.Add(string.Empty);
                        }
                    }
                    else
                    {
                        list.Add(string.Empty);
                        list.Add(string.Empty);
                        list.Add(string.Empty);
                        list.Add(string.Empty);
                        list.Add(string.Empty);
                    }
                }
                else
                {
                    list.Add(string.Empty);
                    list.Add(string.Empty);
                    list.Add(string.Empty);
                    list.Add(string.Empty);
                    list.Add(string.Empty);
                }
                result.Add(list.ToArray());
            }

            return result;
        }

        protected List<object> AddMilestoneDeadlineInfo(List<object> list, DateTime deadline, MilestoneStatus status)
        {
            if (deadline == null || deadline == DateTime.MinValue)
            {
                list.Add(string.Empty);
                list.Add(string.Empty);
                list.Add(string.Empty);
                return list;
            }

            switch (status)
            {
                case MilestoneStatus.Open:
                    if (TenantUtil.DateTimeNow().Date > deadline.Date)
                    {
                        list.Add("pm-redText");
                        list.Add(string.Format(MilestoneResource.MilestoneDeadlineLateInfo, string.Empty));
                        list.Add(deadline.ToString(DateTimeExtension.ShortDatePattern));
                    }
                    else
                    {
                        list.Add("pm-blueText");
                        list.Add(string.Empty);
                        list.Add(deadline.ToString(DateTimeExtension.ShortDatePattern));

                    }
                    break;

                case MilestoneStatus.Closed:
                    list.Add("pm-greenText");
                    list.Add(string.Empty);
                    list.Add(deadline.ToString(DateTimeExtension.ShortDatePattern));
                    break;
            }

            return list;
        }

        protected List<object> AddMilestoneImgParams(List<object> list, DateTime deadline, MilestoneStatus status)
        {
            if (deadline == null || deadline == DateTime.MinValue)
            {
                list.Add(string.Empty);
                list.Add(string.Empty);
                return list;
            }

            string image = string.Empty;

            if (status == MilestoneStatus.Closed)
            {
                list.Add(WebImageSupplier.GetAbsoluteWebPath("milestone_status_completed_16.png", ProductEntryPoint.ID));
                list.Add(MilestoneResource.ClosedMilestone);
            }

            if (deadline.AddDays(1) < TenantUtil.DateTimeNow())
            {
                list.Add(WebImageSupplier.GetAbsoluteWebPath("milestone_status_late_16.png", ProductEntryPoint.ID));
                list.Add(MilestoneResource.LateMilestone);
            }
            else
            {
                list.Add(WebImageSupplier.GetAbsoluteWebPath("milestone_status_active_16.png", ProductEntryPoint.ID));
                list.Add(MilestoneResource.OpenMilestone);
            }

            return list;
        }

        protected string GetTaskDeadlineInfo(Task Target)
        {
            if (Target.Deadline == null || Target.Deadline == DateTime.MinValue)
                return string.Empty;

            int count = Target.Deadline.Date.Subtract(TenantUtil.DateTimeNow().Date).Days;

            if (count > 0)
                return string.Format("<span class='splitter describeText'>{0} {1}</span>", Math.Abs(count), TaskResource.DaysLeft);

            if (count < 0)
                return string.Format("<span class='splitter pm-taskDeadlineLateInfoContainer'>{0} {1}</span>", TaskResource.TaskDeadlineLateInfo, Target.Deadline.ToString(DateTimeExtension.DateFormatPattern));

            return string.Format("<span class='splitter pm-redText'>{0}</span>", ProjectsCommonResource.Today);
        }

        [AjaxMethod]
        public void ChangeTaskStatus(int taskID, bool isClosed)
        {
            var task = Global.EngineFactory.GetTaskEngine().GetByID(taskID);
            
            if (isClosed)
            {
                Global.EngineFactory.GetTaskEngine().ChangeStatus(task, TaskStatus.Closed);
            }
            else
            {
                Global.EngineFactory.GetTaskEngine().ChangeStatus(task, TaskStatus.Open);
            }
        }
    }
}
