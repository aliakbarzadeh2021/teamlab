#region Import

using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using ASC.Projects.Core.Domain;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Resources;
using System.Collections.Generic;
using ASC.Web.Studio.Utility;
using ASC.Web.Projects.Classes;

#endregion

namespace ASC.Web.Projects.Controls.Trash
{
    public partial class TrashActionPanel : BaseUserControl
    {
        public Project Project { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            CommonContainer.Options.IsPopup = true;
        }

        public string MilestoneSelect()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<select id='milestones' class='comboBox'>");
            List<Milestone> milestones = Global.EngineFactory.GetMilestoneEngine().GetByProject(Project.ID)
                .FindAll(item => item.Status == MilestoneStatus.Open);
            foreach (Milestone milestone in milestones)
            {
                sb.AppendFormat("<option value='{0}'>{1}</option>", milestone.ID, milestone.Title);
            }
            sb.AppendLine("</select>");
            return sb.ToString();
        }
    }
}