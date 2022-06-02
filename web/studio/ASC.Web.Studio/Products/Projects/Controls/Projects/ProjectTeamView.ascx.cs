#region Import

using System;
using System.Collections;
using  System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using ASC.Projects.Core.Domain;
using ASC.Web.Core.Users;
using System.Text;
using ASC.Projects.Core.Domain.Reports;

#endregion

namespace ASC.Web.Projects.Controls.Projects
{
    public partial class ProjectTeamView : BaseUserControl
    {
        #region Property

        public IList<Participant> Team { get; set; }
        public List<Participant> OldTeam { get; set; }
        public Participant TeamLeader { get; set; }

		public bool TemplateMode { get; set; }
        
        #endregion

        #region Events
        
        protected void Page_Load(object sender, EventArgs e)
        {
            var prjTeam = new List<Participant>(OldTeam);
            var IsNew = true;
            
            foreach (var newParticipant in Team)
            {
                IsNew = true;
                foreach (Participant oldParticipant in OldTeam)
                {
                    if (newParticipant.UserInfo.ID == oldParticipant.UserInfo.ID)
                    {
                        IsNew = false;
                    }
                }
                if (IsNew) prjTeam.Add(newParticipant);
            }

            prjTeam.Sort((x, y) => UserInfoComparer.Default.Compare(x.UserInfo, y.UserInfo));

            _rptContent.DataSource = prjTeam;
            _rptContent.DataBind();
        }

        public string IsNewUser(Guid id)
        {
            bool isNew = true;
                foreach (Participant oldParticipant in OldTeam)
                {
                    if (id == oldParticipant.UserInfo.ID)
                    {
                        isNew = false;
                        break;
                    }

                }
                if (isNew) return "NewUserInTeam";
                else return string.Empty;
        }

        public string IsOldUser(Guid id)
        {
            bool isOld = true;
            foreach (Participant newParticipant in Team)
            {
                if (id == newParticipant.UserInfo.ID)
                {
                    isOld = false;
                    break;
                }
                if (id == TeamLeader.UserInfo.ID)
                {
                    isOld = false;
                    break;
                }

            }
            if (isOld) return "OldUserInTeam";
            else return string.Empty;
        }

        public string GetReportUri(bool onlyOpenTasks, Guid userID)
        {
            var filter = new ReportFilter();
            filter.TimeInterval = ReportTimeInterval.Absolute;
            filter.UserId = userID;
            filter.MilestoneStatuses.Add(MilestoneStatus.Open);
            filter.MilestoneStatuses.Add(MilestoneStatus.Closed);
            filter.MilestoneStatuses.Add(MilestoneStatus.Late);
            if (onlyOpenTasks)
            {
                filter.TaskStatuses.Add(TaskStatus.NotAccept);
                filter.TaskStatuses.Add(TaskStatus.Open);
            }
            else
            {
                filter.TaskStatuses.Add(TaskStatus.Closed);
            }
            return string.Format("reports.aspx?action=generate&reportType=10&{0}", filter.ToUri());
        }

        #endregion
    }
}