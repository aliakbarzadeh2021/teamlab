#region Import

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Projects.Core.Domain;
using ASC.Web.Controls;
using ASC.Web.Projects.Classes;
using ASC.Core.Users;
using ASC.Core;
using ASC.Web.Projects.Resources;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using ASC.Web.Projects.Controls.Reports;
using System.Collections;
using ASC.Projects.Core.Domain.Reports;
using ASC.Core.Tenants;
#endregion

namespace ASC.Web.Projects.Controls.Dashboard
{

    [Serializable]
    public class MyNewTasksWidgetSettings : ISettings
    {
        public int TasksCount { get; set; }

        #region ISettings Members

        public Guid ID
        {
            get { return new Guid("{F20594AE-E8A1-4bb7-87D0-7A36B5B4AC3A}"); }
        }

        public ISettings GetDefault()
        {
            return new MyNewTasksWidgetSettings() { TasksCount = 5 };
        }

        #endregion
    }

    public partial class MyNewTasksWidget : BaseUserControl
    {
        #region Properties

        public static Guid WidgetID { get { return new Guid("{6547B78F-CC99-4fbe-BEB0-F008D7EF8B87}"); } }

        public string CurrentUserID { get { return SecurityContext.CurrentAccount.ID.ToString(); } }

        public bool HasData { get; set; }

        #endregion

        #region Methods

        public string GetReportUri()
        {
            var filter = new ReportFilter();
            filter.TimeInterval = ReportTimeInterval.Absolute;
            filter.UserId = new Guid(CurrentUserID);
            filter.TaskStatuses.Add(TaskStatus.Open);
            return string.Format("reports.aspx?action=generate&reportType=10&{0}", filter.ToUri());
        }

        public string GetTaskDeadline(Task Target)
        {
            if (Target.Deadline == null || Target.Deadline == DateTime.MinValue)
                return string.Empty;

            int count = Target.Deadline.Date.Subtract(TenantUtil.DateTimeNow().Date).Days;
            //string days = GrammaticalHelper.ChooseNumeralCase(Math.Abs(count), GrammaticalResource.DayNominative, GrammaticalResource.DayGenitiveSingular, GrammaticalResource.DayGenitivePlural);

            if (count > 0)
                return string.Format("<div><span class='pm-grayText'>{0} {1}</span></div>", Math.Abs(count), TaskResource.DaysLeft, Target.ID);

            if (count < 0)
                return string.Format("<div><span class='pm-taskDeadlineLateInfoContainer'>{0} {1}</span></div>", TaskResource.TaskDeadlineLateInfo, Target.Deadline.ToString(DateTimeExtension.DateFormatPattern), Target.ID);

            return string.Format("<div><span class='pm-redText'>{0}</span></div>", ProjectsCommonResource.Today, Target.ID);
        }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {

            var widgetSettings = SettingsManager.Instance.LoadSettingsFor<MyNewTasksWidgetSettings>(SecurityContext.CurrentAccount.ID);

            List<Task> tasks = Global.EngineFactory.GetTaskEngine().GetLastTasks(SecurityContext.CurrentAccount.ID, widgetSettings.TasksCount);

            HasData = tasks.Count > 0 ? true : false;
            
            MyNewTasksRepeater.DataSource = tasks;
            MyNewTasksRepeater.DataBind();
        }

        #endregion

        
    }
}