#region Import

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Web.Controls;
using ASC.Web.Core.Users.Activity;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Controls.Common;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;
using System.Text;
using ASC.Core;
using AjaxPro;
using ASC.Web.Projects.Controls.TimeSpends;
using System.Web.UI;
using ASC.Web.Studio.Controls.Common;
using System.Globalization;
using ASC.Projects.Core.Domain.Reports;
using ASC.Web.Projects.Controls;
using System.Linq;
using ASC.Projects.Engine;
using ASC.Core.Tenants;

#endregion


namespace ASC.Web.Projects
{
    [AjaxNamespace("AjaxPro.TimeTracking")]
    public partial class TimeTracking : BasePage
    {
        #region Properties

        public ProjectFat ProjectFat { get; set; }

        public double TotalHoursCount { get; set; }

        public double TotalHoursCountOnPage { get; set; }

        public string VisibleTotalHoursCountOnPage { get; set; }

        public static String StartDateTiks
        {
            get
            {
                string result = HttpContext.Current.Request["startDate"];
                return result == null ? "" : result;
            }
        }

        public static String FinishDateTiks
        {
            get
            {
                string result = HttpContext.Current.Request["finishDate"];
                return result == null ? "" : result;
            }
        }

        public static String TimeRange
        {
            get
            {
                string result = HttpContext.Current.Request["timeRange"];
                return result == null ? "" : result;
            }
        }

        public static String UID
        {
            get
            {
                string result = HttpContext.Current.Request["uid"];
                return result == null ? "" : result;
            }
        }

        public static int TaskID
        {
            get
            {
                int ID;
                if (Int32.TryParse(UrlParameters.EntityID, out ID))
                {
                    return ID;
                }
                return -1;
            }
        }

        protected bool IsTimer
        {
            get { return UrlParameters.ActionType == "timer"; }
        }

        protected bool HasTasksData { get; set; }

        #endregion

        #region Events

        protected override void PageLoad()
        {

            if (!Global.ModuleManager.IsVisible(ModuleType.TimeTracking))
                Response.Redirect(ProjectsCommonResource.StartURL);

            RequestContext.EnsureCurrentProduct();
            ProjectFat = RequestContext.GetCurrentProjectFat();

            AjaxPro.Utility.RegisterTypeForAjax(typeof(TimeTracking));

            if (!IsTimer)
            {
                InitPage(ProjectFat);

                var list = Global.EngineFactory.GetTimeTrackingEngine().GetByProject(ProjectFat.Project.ID);

                if (TaskID > 0)
                    list = list.FindAll(ts => ts.RelativeTask == TaskID);

                list.Sort((oX, oY) => (-1) * oX.ID.CompareTo(oY.ID));
                TotalHoursCount = 0;

                list.ForEach(ts => TotalHoursCount += ts.Hours);

                //pagging-----------------------------------------------
                int factor = 3;

                if (list.Count > Global.EntryCountOnPage * factor)
                    VisibleTotalHoursCountOnPage = "display:block;";
                else
                    VisibleTotalHoursCountOnPage = "display:none;";

                List<TimeSpend> finded = new List<TimeSpend>();
                int start = (UrlParameters.PageNumber - 1) * Global.EntryCountOnPage * factor;
                int finish = start + Global.EntryCountOnPage * factor;
                if (finish > list.Count) finish = list.Count;
                for (int index = start; index < finish; index++)
                    finded.Add(list[index]);

                TotalHoursCountOnPage = 0;
                finded.ForEach(ts => TotalHoursCountOnPage += ts.Hours);

                var taskids = finded.ConvertAll(ts => ts.RelativeTask);
                var selectedtaskids = taskids.FindAll(id => id > 0);

                var selectedTasks = new List<Task>();
                if (selectedtaskids.Count > 0)
                    selectedTasks = Global.EngineFactory.GetTaskEngine().GetByID(selectedtaskids);


                var tsvm = finded.ConvertAll(ts => new TimeSpendVM(ts, selectedTasks.Find(task => task.ID == ts.RelativeTask)));

                tsvm = tsvm.Where(item => ProjectSecurity.CanRead(item.RelatedTask)).ToList();

                timeSpendRpt.DataSource = tsvm;
                timeSpendRpt.DataBind();

                EmptyScreenContainer.Controls.Add(new NotFoundControl());

                if ((list.Count > 0) && (!IsPostBack))
                {
                    string pageUrl = String.Concat(PathProvider.BaseAbsolutePath, "timeTracking.aspx?prjID=", UrlParameters.ProjectID);

                    phContent.Controls.Add(new PageNavigator
                    {
                        CurrentPageNumber = UrlParameters.PageNumber,
                        EntryCountOnPage = Global.EntryCountOnPage * factor,
                        VisiblePageCount = Global.VisiblePageCount,
                        EntryCount = list.Count,
                        VisibleOnePage = false,
                        ParamName = UrlConstant.PageNumber,
                        PageUrl = pageUrl
                    });

                    EmptyScreenContainer.Style.Add("display", "none");
                }
                else
                {
                    MainPageContainer.Visible = false;
                    EmptyScreenContainer.Style.Add("display", "block");
                }

                var projects = Global.EngineFactory.GetProjectEngine().GetByParticipant(SecurityContext.CurrentAccount.ID);
                projects = projects.FindAll(p =>
                    Global.EngineFactory.GetTaskEngine().GetByProject(p.ID, TaskStatus.Open, SecurityContext.CurrentAccount.ID).Count > 0
                    );
                HasTasksData = projects.Count > 0;
            }
            else
            {
                int taskID;
                int task = -1;
                if (Int32.TryParse(UrlParameters.EntityID, out taskID))
                {
                    var t = Global.EngineFactory.GetTaskEngine().GetByID(taskID);
                    if (t != null && t.Status == TaskStatus.Open) task = taskID;
                }
                Master.DisabledSidePanel = true;
                var cntrlTimer = (TimeSpendTimer)LoadControl(PathProvider.GetControlVirtualPath("TimeSpendTimer.ascx"));
                cntrlTimer.ProjectFat = ProjectFat;
                cntrlTimer.Target = task;
                _phTimeSpendTimer.Controls.Add(cntrlTimer);
                Title = HeaderStringHelper.GetPageTitle(ProjectsCommonResource.AutoTimer, Master.BreadCrumbs);
            }

            if (!IsTimer && HasTasksData)
                SideActionsPanel.Controls.Add(new NavigationItem
                {
                    Name = ProjectsCommonResource.AutoTimer,
                    URL = String.Format("javascript:ASC.Projects.TimeSpendActionPage.showTimer('timetracking.aspx?prjID={0}&action=timer');", ProjectFat.Project.ID)
                });
            else
                SideActionsPanel.Visible = false;
        }

        #endregion

        #region Methods

        public void InitPage(ProjectFat projectFat)
        {
            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = ProjectResource.Projects,
                NavigationUrl = "projects.aspx"
            });

            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = projectFat.Project.HtmlTitle.HtmlEncode(),
                NavigationUrl = "projects.aspx?prjID=" + projectFat.Project.ID
            });

            if (TaskID > 0)
            {
                Master.BreadCrumbs.Add(new BreadCrumb
                {
                    Caption = ProjectsCommonResource.TimeTracking,
                    NavigationUrl = "timetracking.aspx?prjID=" + projectFat.Project.ID
                });

                Master.BreadCrumbs.Add(new BreadCrumb
                {
                    Caption = Global.EngineFactory.GetTaskEngine().GetByID(TaskID).Title
                });
            }
            else
            {
                Master.BreadCrumbs.Add(new BreadCrumb
                {
                    Caption = ProjectsCommonResource.TimeTracking
                });
            }

            Title = HeaderStringHelper.GetPageTitle(ProjectsCommonResource.TimeTracking, Master.BreadCrumbs);

            SideNavigatorPanel.Visible = TaskID > 0 ? false : true;

            var filter = new ReportFilter();
            filter.TimeInterval = ReportTimeInterval.Absolute;
            filter.ProjectIds.Add(projectFat.Project.ID);
            filter.ViewType = 1;
            SideNavigatorPanel.Controls.Add(new NavigationItem
                                {
                                    Name = ReportResource.GenerateReport,
                                    URL = "javascript:ASC.Projects.Reports.generateReportByUrl('reports.aspx?action=generate&reportType=8&" + filter.ToUri() + "')"
                                });
        }

        public string GetAction(TimeSpend TimeSpend)
        {
            bool permission = Global.IsAdmin || ProjectFat.Responsible.ID == SecurityContext.CurrentAccount.ID || TimeSpend.Person == SecurityContext.CurrentAccount.ID;
            if (permission)
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat("<img style='border:0px; cursor:pointer;' align='absmiddle' title='{0}' alt='{0}' src='{1}' onclick='ASC.Projects.TimeSpendActionPage.ViewEditTimeLogPanel({2})'/>", ProjectsCommonResource.Edit, WebImageSupplier.GetAbsoluteWebPath("edit_small.png", ProductEntryPoint.ID), TimeSpend.ID);
                sb.AppendFormat("<img style='border:0px; cursor:pointer; margin-left:4px' align='absmiddle' title='{0}' alt='{0}' src='{1}' onclick='ASC.Projects.TimeSpendActionPage.deleteTimeSpend({2})'/>", ProjectsCommonResource.Delete, WebImageSupplier.GetAbsoluteWebPath("delete_small.png", ProductEntryPoint.ID), TimeSpend.ID);

                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public string GetNote(TimeSpendVM TimeSpendVm)
        {
            if (TimeSpendVm.RelatedTask != null)
            {
                var task = TimeSpendVm.RelatedTask;
                if (TimeSpendVm.TimeSpend.Note != string.Empty)
                    return string.Format("<a href=\"tasks.aspx?prjID={0}&id={1}\">{2}</a><br/>{3}",
                        ProjectFat.Project.ID, TimeSpendVm.RelatedTask.ID, HtmlUtility.GetText(TimeSpendVm.RelatedTask.Title, 45), TimeSpendVm.TimeSpend.Note.HtmlEncode());
                else
                    return string.Format("<a href=\"tasks.aspx?prjID={0}&id={1}\">{2}</a>",
                        ProjectFat.Project.ID, TimeSpendVm.RelatedTask.ID, HtmlUtility.GetText(TimeSpendVm.RelatedTask.Title, 45));
            }
            else
                return TimeSpendVm.TimeSpend.Note.HtmlEncode();
        }

        public string GetTitle(TimeSpendVM TimeSpendVm)
        {
            return string.Format("{0} {1}",
                TimeSpendVm.RelatedTask != null ? TaskResource.Task + ": " + TimeSpendVm.RelatedTask.Title + "; " : "",
                TimeSpendVm.TimeSpend.Note == string.Empty ? "" : ProjectResource.ProjectDescription + ": " + TimeSpendVm.TimeSpend.Note.HtmlEncode());
        }

        #endregion

        #region Ajax Methods

        [AjaxMethod]
        public string QuickAdd(string date, string personID, string hours, string note, string prjID, string cssClass)
        {
            Project prj = Global.EngineFactory.GetProjectEngine().GetByID(Convert.ToInt32(prjID));

            float res;
            if (!float.TryParse(hours, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out res)) res = (float)Convert.ToDouble(hours);


            TimeSpend ts = new TimeSpend
            {
                Date = DateTime.Parse(date),
                Person = new Guid(personID),
                Hours = res,
                Note = note,
                Project = prj.ID
            };

            Global.EngineFactory.GetTimeTrackingEngine().SaveOrUpdate(ts);

            System.Web.UI.Page page = new System.Web.UI.Page();
            TimeSpendRecord cntrl = (TimeSpendRecord)LoadControl(PathProvider.GetControlVirtualPath("TimeSpendRecord.ascx"));

            cntrl.TimeSpend = ts;
            cntrl.Project = prj;
            cntrl.CssClass = cssClass;

            page.Controls.Add(cntrl);
            var writer = new System.IO.StringWriter();
            HttpContext.Current.Server.Execute(page, writer, false);
            string output = writer.ToString();
            writer.Close();

            return output;
        }

        [AjaxMethod]
        public string DeleteTimeSpend(int id)
        {
            ProjectSecurity.DemandAuthentication();

            var ts = Global.EngineFactory.GetTimeTrackingEngine().GetByID(id);
            var prj = ts.Project;

            Global.EngineFactory.GetTimeTrackingEngine().Delete(ts.ID);

            var list = Global.EngineFactory.GetTimeTrackingEngine().GetByProject(prj);

            double totalHoursCount = 0;

            foreach (TimeSpend timeSpend in list)
            {
                totalHoursCount += timeSpend.Hours;
            }

            return totalHoursCount.ToString("N2");
        }

        [AjaxMethod]
        public string GetTotalHoursCount(string prjID)
        {
            ProjectSecurity.DemandAuthentication();

            var list = Global.EngineFactory.GetTimeTrackingEngine().GetByProject(Convert.ToInt32(prjID));

            double totalHoursCount = 0;

            foreach (TimeSpend timeSpend in list)
            {
                totalHoursCount += timeSpend.Hours;
            }

            return totalHoursCount.ToString("N2");
        }

        [AjaxMethod]
        public string GetTotalHoursCountOnPage(string oldValue, string timeSpendItemOldValue, string timeSpendItemNewValue)
        {
            ProjectSecurity.DemandAuthentication();

            oldValue = oldValue.Trim().Replace("&nbsp;", "");
            timeSpendItemOldValue = timeSpendItemOldValue.Trim().Replace("&nbsp;", "");
            timeSpendItemNewValue = timeSpendItemNewValue.Trim().Replace("&nbsp;", "");

            float totalHoursCountOnPage;
            if (!float.TryParse(oldValue, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out totalHoursCountOnPage))
                totalHoursCountOnPage = (float)Convert.ToDouble(oldValue);

            float itemOldValue;
            if (!float.TryParse(timeSpendItemOldValue, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out itemOldValue))
                itemOldValue = (float)Convert.ToDouble(timeSpendItemOldValue);

            float itemNewValue;
            if (!float.TryParse(timeSpendItemNewValue, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out itemNewValue))
                itemNewValue = (float)Convert.ToDouble(timeSpendItemNewValue);

            return (totalHoursCountOnPage + itemNewValue - itemOldValue).ToString("N2");
        }

        [AjaxMethod]
        public void SaveTaskTimeByTimer(int projectID, int taskID, long seconds, string description, string startValue)
        {
            ProjectSecurity.DemandAuthentication();

            if (seconds == 0)
            {
                seconds = (long)GetSeconds(startValue);
            }

            if (seconds == 0)
                throw new Exception(ProjectsCommonResource.AutoTimerNullTimeError);

            double hours = seconds / 3600.0;
            var item = new TimeSpend
            {
                Hours = (float)hours,
                Date = TenantUtil.DateTimeNow(),
                Note = description,
                Project = projectID,
                RelativeTask = taskID,
                Person = SecurityContext.CurrentAccount.ID
            };

            Global.EngineFactory.GetTimeTrackingEngine().SaveOrUpdate(item);
        }

        [AjaxMethod]
        public string InitSelectUserTasksByProject(int projectID, int taskID)
        {
            ProjectSecurity.DemandAuthentication();

            var sb = new StringBuilder();
            var currentUser = SecurityContext.CurrentAccount.ID;
            var tasks = Global.EngineFactory.GetTaskEngine().GetByProject(projectID, TaskStatus.Open, currentUser);
            tasks.Sort((x, y) => String.Compare(x.Title, y.Title));
            foreach (var task in tasks)
            {
                if (taskID != -1 && task.ID == taskID)
                    sb.AppendFormat("<option selected='selected' value='{0}' id='optionUserTask_{0}'>{1}</option>", task.ID, task.Title);
                else
                    sb.AppendFormat("<option value='{0}' id='optionUserTask_{0}'>{1}</option>", task.ID, task.Title);
            }

            return sb.ToString();
        }

        [AjaxMethod]
        public float GetSeconds(string hours)
        {
            ProjectSecurity.DemandAuthentication();

            float res;
            if (!float.TryParse(hours, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out res)) res = (float)Convert.ToDouble(hours);
            return res * 3600;
        }

        #endregion

        public class TimeSpendVM
        {
            public TimeSpendVM(TimeSpend timeSpend, Task relatedTask)
            {
                TimeSpend = timeSpend;
                RelatedTask = relatedTask;
            }

            public TimeSpend TimeSpend;
            public Task RelatedTask;
        }

    }
}
