using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using ASC.Core;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Projects.Controls.IssueTracker
{
    public partial class AddIssueDialogView : BaseUserControl
    {
        #region Property

        public List<Participant> Participants { get; set; }

        public String Title { get; set; }

        public String DetectedInVersion { get; set; }

        public String Description { get; set; }

        public String CorrectedInVersion { get; set; }

        public IssuePriority Priority { get; set; }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            InitView();

            RemoveDisabledParticipants();

            Participants.Sort((oX, oY) => String.Compare(oX.UserInfo.DisplayUserName(), oY.UserInfo.DisplayUserName()));

            bool bIsInList = false;
            foreach (var participant in Participants)
            {

                ListItem listItem = new ListItem(participant.UserInfo.DisplayUserName(), participant.UserInfo.ID.ToString());

                if (participant.UserInfo.ID == ASC.Core.SecurityContext.CurrentAccount.ID)
                {
                    listItem.Selected = true;
                    bIsInList = true;
                }

                ddlParticipants.Items.Add(listItem);
            }

            ListItem lItem = new ListItem("------", new Guid().ToString());

            if (bIsInList == false)
                lItem.Selected = true;

            ddlParticipants.Items.Add(lItem);
        }

        private void RemoveDisabledParticipants()
        {
            List<Participant> users = new List<Participant>();
            foreach (Participant prt in Participants)
            {
                if (ASC.Core.CoreContext.UserManager.UserExists(prt.ID) && (CoreContext.UserManager.GetUsers(prt.ID).Status != EmployeeStatus.Terminated))
                {
                    users.Add(prt);
                }
            }
            Participants = users;
        }

        protected void InitView()
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(ListIssueTrackerView));

            tbTitle.Text             = Title;
            tbDetectedInVersion.Text = DetectedInVersion;
            tbDescription.Value      = Description;

            switch (Priority)
            {
                case IssuePriority.Immediate:
                    ddlPriorityImmediate.Checked = true;
                    break;
                case IssuePriority.Urgent:
                    ddlPriorityUrgent.Checked = true;
                    break;
                case IssuePriority.High:
                    ddlPriorityHigh.Checked = true;
                    break;
                case IssuePriority.Normal:
                    ddlPriorityNormal.Checked = true;
                    break;
                case IssuePriority.Low:
                    ddlPriorityLow.Checked = true;
                    break;
                default:
                    ddlPriorityNormal.Checked = true;
                    break;
            }

            tbDescription.BasePath      = CommonControlsConfigurer.FCKEditorBasePath;
            tbDescription.ToolbarSet    = "BlogToolbar";
            tbDescription.EditorAreaCSS = WebSkin.GetUserSkin().BaseCSSFileAbsoluteWebPath;
        }

        #endregion
    }
}