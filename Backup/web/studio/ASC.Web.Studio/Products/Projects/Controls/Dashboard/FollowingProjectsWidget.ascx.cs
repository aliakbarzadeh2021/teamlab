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
    public class FollowingProjectsWidgetSettings : ISettings
    {
        public int ProjectsCount { get; set; }

        #region ISettings Members

        public Guid ID
        {
            get { return new Guid("{B3FDD014-5CB6-4991-B88D-9759789DCB04}"); }
        }

        public ISettings GetDefault()
        {
            return new FollowingProjectsWidgetSettings() { ProjectsCount = 5 };
        }

        #endregion
    }
    
    public partial class FollowingProjectsWidget : BaseUserControl
    {
        #region Properties

        public static Guid WidgetID { get { return new Guid("{EE382D05-3568-48c7-BF48-81A1B42F03F8}"); } }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {

            var widgetSettings = SettingsManager.Instance.LoadSettingsFor<FollowingProjectsWidgetSettings>(SecurityContext.CurrentAccount.ID);

            var prt = SecurityContext.CurrentAccount.ID;

            var follow_projects = Global.EngineFactory.GetProjectEngine().GetFollowing(prt);

            if (follow_projects.Count == 0) return;

            follow_projects.Reverse();

            if (widgetSettings.ProjectsCount < follow_projects.Count)
                follow_projects = follow_projects.GetRange(0, widgetSettings.ProjectsCount);

            FollowingProjectsRepeater.DataSource = follow_projects;
            FollowingProjectsRepeater.DataBind();

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