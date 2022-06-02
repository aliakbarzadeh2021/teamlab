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
using ASC.Web.Projects.Configuration;
using ASC.Web.Core.Utility.Skins;
#endregion

namespace ASC.Web.Projects.Controls.Dashboard
{
    [Serializable]
    public class NewProjectsWidgetSettings : ISettings
    {
        public int ProjectsCount { get; set; }

        #region ISettings Members

        public Guid ID
        {
            get { return new Guid("{BAC19E83-EAA3-4b30-9628-0DE39DE762A7}"); }
        }

        public ISettings GetDefault()
        {
            return new NewProjectsWidgetSettings() { ProjectsCount = 5 };
        }

        #endregion
    }

    public partial class NewProjectsWidget : BaseUserControl
    {
        #region Properties

        public static Guid WidgetID { get { return new Guid("{A9DDD449-EB17-46ed-9F7B-98725DA3A1ED}"); } }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {

            var widgetSettings = SettingsManager.Instance.LoadSettingsFor<NewProjectsWidgetSettings>(SecurityContext.CurrentAccount.ID);

            var projects = Global.EngineFactory.GetProjectEngine().GetLast(ProjectStatus.Open, 30);

            if (projects.Count > widgetSettings.ProjectsCount)
                projects = projects.GetRange(0, widgetSettings.ProjectsCount);

            NewProjectsRepeater.DataSource = projects;
            NewProjectsRepeater.DataBind();

        }

        #endregion

        #region Methods

        public string GetProjectLeader(Project prj)
        {
            return Global.EngineFactory.GetParticipantEngine().GetByID(prj.Responsible).UserInfo.RenderProfileLink(ProductEntryPoint.ID);
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
    }
}