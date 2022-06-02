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
using ASC.Web.Core.Helpers;
using System.Collections;
using ASC.Web.Projects.Controls.Reports;
using System.Linq;
using ASC.Projects.Core.Domain.Reports;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Configuration;
#endregion

namespace ASC.Web.Projects.Controls.Dashboard
{
    [Serializable]
    public class MyProjectsWidgetSettings : ISettings
    {
        public int ProjectsCount { get; set; }

        #region ISettings Members

        public Guid ID
        {
            get { return new Guid("{EE3889A4-590B-43f6-B557-8B6B3733465E}"); }
        }

        public ISettings GetDefault()
        {
            return new MyProjectsWidgetSettings() { ProjectsCount = 5 };
        }

        #endregion
    }

    public partial class MyProjectsWidget : BaseUserControl
    {
        #region Properties

        public static Guid WidgetID { get { return new Guid("{89962785-3618-4b53-824E-F7D0CC6B73C1}"); } }

        public string CurrentUserID { get { return SecurityContext.CurrentAccount.ID.ToString(); } }

        public bool HasData { get; set; }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            var myProjects = Global.EngineFactory.GetProjectEngine().GetByParticipant(Page.Participant.ID);

            var widgetSettings = SettingsManager.Instance.LoadSettingsFor<MyProjectsWidgetSettings>(SecurityContext.CurrentAccount.ID);

            if (myProjects.Count > widgetSettings.ProjectsCount)
                myProjects = myProjects.GetRange(0, widgetSettings.ProjectsCount);

            var projectsStats = Global.EngineFactory.GetProjectEngine().GetTaskCount(myProjects.ConvertAll(p => p.ID), TaskStatus.Open, TaskStatus.NotAccept, TaskStatus.Unclassified);

            HasData = myProjects.Count > 0 ? true : false;

            MyProjectsRepeater.DataSource = myProjects.ConvertAll(p => new ProjectVM(p) { TasksCount = projectsStats[myProjects.IndexOf(p)] });
            MyProjectsRepeater.DataBind();
        }

        #endregion

        #region Methods

        public string ActiveTasksInProject(ProjectVM prj)
        {

            string str = prj.TasksCount + " " + GrammaticalHelper.ChooseNumeralCase(prj.TasksCount, GrammaticalResource.TaskNominative, GrammaticalResource.TaskGenitiveSingular, GrammaticalResource.TaskGenitivePlural);

            return string.Format("<a href='tasks.aspx?prjID={0}&action=2'>{1}</a>", prj.Project.ID, str);
        }

        public string GetReportUri()
        {
            var filter = new ReportFilter();
            filter.TimeInterval = ReportTimeInterval.Absolute;
            filter.UserId = new Guid(CurrentUserID);
            filter.ProjectStatuses.Add(ProjectStatus.Open);
            return string.Format("reports.aspx?action=generate&reportType=7&{0}", filter.ToUri());
        }

        public string IsPrivateProject(bool isPrivate)
        {
            if (isPrivate)
                return string.Format("<img title='{0}' src='{1}' alt='{0}' align='absmiddle'>",
                ProjectResource.HiddenProject,
                WebImageSupplier.GetAbsoluteWebPath("lock.png", ProductEntryPoint.ID)
                );
            else return string.Empty;
        }

        #endregion

        public class ProjectVM
        {
            public ProjectVM(Project project)
            {
                Project = project;
            }

            public Project Project;
            public int TasksCount;
        }
    }
}