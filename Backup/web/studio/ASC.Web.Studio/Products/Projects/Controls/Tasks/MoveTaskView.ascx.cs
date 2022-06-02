#region Import

using System;
using System.Web;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using ASC.Core.Users;
using ASC.Projects.Core;
using ASC.Projects.Core.Domain;
using ASC.Web.Studio.Utility;
using ASC.Web.Controls;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Projects.Classes;
using ASC.Core;
using ASC.Web.Projects.Resources;
using ASC.Data.Storage;
using AjaxPro;
using ASC.Web.Core.Users;
using System.Linq;

#endregion

namespace ASC.Web.Projects.Controls.Tasks
{
    public partial class MoveTaskView : BaseUserControl
    {
        public ProjectFat ProjectFat { get; set; }
        public List<Milestone> Milestones { get { return ProjectFat.GetMilestones(MilestoneStatus.Open); } }
        public string CssClassForEmptyMilestone { get { return Milestones.Count % 2 == 0 ? "tintMedium" : "tintLight"; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            moveTaskContainer.Options.IsPopup = true;
            
            rptMilestoneSelector.DataSource = Milestones;
            rptMilestoneSelector.DataBind();
        }

        protected string GetMilestoneTitle(Milestone milestone) { return String.Format("[{0}] {1}", milestone.DeadLine.ToString(DateTimeExtension.DateFormatPattern), HtmlUtility.GetText(milestone.Title, 150)); }
    }
}