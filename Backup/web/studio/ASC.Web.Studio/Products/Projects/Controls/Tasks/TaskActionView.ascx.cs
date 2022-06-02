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
using ASC.Web.Files.Api;
using ASC.Projects.Engine;

#endregion

namespace ASC.Web.Projects.Controls.Tasks
{
    public partial class TaskActionView : BaseUserControl
    {
        #region Members

        #endregion

        #region Property

        public Task Target { get; set; }

        public ProjectFat ProjectFat { get; set; }
        public Project Project { get { return ProjectFat.Project; } }

        public List<Milestone> Milestones { get { return ProjectFat.GetMilestones(MilestoneStatus.Open); } }
        public List<Participant> GetParticipants { get { return ProjectFat.GetActiveTeam(); } }

        public Milestone SelectedMilestone { get; set; }

        public bool isMobileVersion { get { return ASC.Web.Core.Mobile.MobileDetector.IsRequestMatchesMobile(Context); } }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            hfTaskId.Value = "-1";

            _uploadSwitchHolder.Controls.Add(new FileUploaderModeSwitcher());

            AjaxPro.Utility.RegisterTypeForAjax(typeof(TaskActionView));

            _taskContainer.Options.IsPopup = true;

            RegisterClientScript();

            //init milestone selector
            if (isMobileVersion)
            {
                rptMilestoneSelectorMobile.DataSource = Milestones;
                rptMilestoneSelectorMobile.DataBind();
            }
            else
            {
                rptMilestoneSelector.DataSource = Milestones;
                rptMilestoneSelector.DataBind();
            }

            //init responsible selector
            var users = new List<UserInfo>();
            users.Add(new UserInfo());

            if (ProjectSecurity.CanCreateTask(Project,true))
                users.AddRange(ProjectFat.GetActiveTeam().Select(p => p.UserInfo).OrderBy(u => u, UserInfoComparer.Default).ToList());
            else if (ProjectSecurity.CanCreateTask(Project, false))
                users.Add(CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID));

            if (isMobileVersion)
            {
                rptUserSelectorMobile.DataSource = users;
                rptUserSelectorMobile.DataBind();
            }
            else
            {
                rptUserSelector.DataSource = users;
                rptUserSelector.DataBind();
            }

            //init task deadline
            taskDeadline.Text = string.Empty;

            if (Target != null)
            {
                hfTaskId.Value = Target.ID.ToString();

                var files = FileEngine2.GetTaskFiles(Target);
                if (0 < files.Count)
                {
                    var uploadedFiles = new List<Object>();

                    foreach (var fileInfo in files)
                    {
                        uploadedFiles.Add(new
                        {
                            Name = fileInfo.Title,
                            Size = fileInfo.ContentLength,
                            URL = fileInfo.FileUri,
                            TargerContainerID = "task_uploadedContainer",
                            RemoveHandler = "function (fileInfo) {return ASC.Projects.TaskActionPage.deleteFile(" + String.Format("{0},{1}, '{2}'", fileInfo.ID, fileInfo.Version, fileInfo.Title) + " );}"

                        });
                    }

                    Page.ClientScript.RegisterClientScriptBlock(typeof(TaskActionView),
                                                                "{5E6C09D3-A9EB-4ffd-9377-C97C52972E9E}",
                                                                "; var uploadedFiles  = " +
                                                                JavaScriptSerializer.Serialize(uploadedFiles).Replace("\"function", "function").Replace(";}\"", ";}"), true);

                }
            }
        }

        private void RegisterClientScript()
        {
            UserInfo userInfo = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            var havePermissions = true;
            if (!Global.IsAdmin && !ProjectFat.IsResponsible())
                havePermissions = false;

            Page.ClientScript.RegisterClientScriptBlock(typeof(TaskActionView), "D1F5CC2D-0B7C-41fd-809F-42A64EDB95F1", "CurrUser = " +
                                                                          JavaScriptSerializer.Serialize(
                                                                              new
                                                                              {
                                                                                  UserName = userInfo.DisplayUserName(true),
                                                                                  UserID = userInfo.ID.ToString(),
                                                                                  EmptyGuid = Guid.Empty.ToString(),
                                                                                  HavePermission = havePermissions
                                                                              }), true);
        }

        protected string GetUserAvatarUrl(UserInfo userInfo) { return userInfo.GetSmallPhotoURL(); }

        protected string GetUserName(UserInfo userInfo) { return userInfo.ID != Guid.Empty ? userInfo.DisplayUserName(true).ReplaceSingleQuote() : TaskResource.WithoutResponsible; }

        protected string GetUserTitle(UserInfo userInfo) { return userInfo.ID != Guid.Empty ? userInfo.Title.HtmlEncode() : string.Empty; }

        protected string GetMilestoneTitle(Milestone milestone) { return String.Format("[{0}] {1}", milestone.DeadLine.ToString(DateTimeExtension.DateFormatPattern), HtmlUtility.GetText(milestone.Title, 150)); }

        #endregion

    }
}