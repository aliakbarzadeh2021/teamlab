#region Import

using System;
using System.Text;
using System.Web;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Resources;
using System.Collections.Generic;
using ASC.Web.Projects.Classes;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Projects.Engine;

#endregion

namespace ASC.Web.Projects.Controls.Tasks
{
    public partial class ClosedTaskBlockViewRow : BaseUserControl
    {
        #region Property

        public ProjectFat ProjectFat { get; set; }
        public Task Target { get; set; }
        public Participant TeamLeader { get; set; }
        public string Status { get; set; }
        public string MilestoneName { get; set; }
        public string ProjectName { get { return (Target.Project != null ? Target.Project.HtmlTitle : ""); } }
        public int RowIndex { get; set; }
        public bool HasTime { get; set; }
        protected bool HavePermission { get; set; }
        public bool IsAllMyTasks { get; set; }
        public bool OneList { get; set; }
        protected bool IsOpenMilestone { get; set; }
        public bool ExecuteHtmlEncode { get; set; }

        #endregion

        #region Methods

        protected string GetTitle()
        {
            StringBuilder innerHTML = new StringBuilder();

            switch (Target.Priority)
            {

                case TaskPriority.High:
                    innerHTML.AppendFormat("<img title='{0}' src='{1}' align='top' alt='{0}' style='margin-right:5px;margin-top:1px; ' />", TaskResource.HighPriority_Title, WebImageSupplier.GetAbsoluteWebPath("prior_hi.png", ProductEntryPoint.ID));
                    break;
                case TaskPriority.Low:
                    innerHTML.AppendFormat("<img title='{0}' src='{1}' align='top' alt='{0}' style='margin-right:5px;margin-top:1px; ' />", TaskResource.LowPriority_Title, WebImageSupplier.GetAbsoluteWebPath("prior_lo.png", ProductEntryPoint.ID));
                    break;
            }

            innerHTML.AppendFormat("<a id='taskTitle_{1}' class='pm-linkClosedTask' title=\"{3}\" href='tasks.aspx?prjID={0}&id={1}'>{2}</a>",
                Target.Project.ID,
                Target.ID,
                ExecuteHtmlEncode ? Target.Title.HtmlEncode() : Target.Title,
                Target.Description.ReplaceSingleQuote().Replace("\n", "<br/>").HtmlEncode().Replace("'", "-")
                );

            return innerHTML.ToString();
        }

        protected string GetUserName()
        {
            return Target.Responsible != Guid.Empty ? CoreContext.UserManager.GetUsers(Target.Responsible).DisplayUserName(true).ReplaceSingleQuote() : string.Empty;
        }

        protected string GetTitleContainerWidth()
        {
            int width = 490;

            if (IsAllMyTasks)
            {
                if (OneList)
                    width = 500;
                else width = 830;
            }
            return string.Format("style='width:{0}px;'", width);
        }

        public string GetLoaderImgURL()
        {
            return WebImageSupplier.GetAbsoluteWebPath("loader_12.gif", ProductEntryPoint.ID);
        }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            HavePermission = ProjectSecurity.CanEdit(Target);

            IsOpenMilestone = false;
            if (Target.Milestone == 0)
            {
                IsOpenMilestone = true;
            }
            else
            {
                var milestone = Global.EngineFactory.GetMilestoneEngine().GetByID(Target.Milestone);
                if (milestone != null && milestone.Status != MilestoneStatus.Closed) IsOpenMilestone = true;
            }
        }

        #endregion
    }
}