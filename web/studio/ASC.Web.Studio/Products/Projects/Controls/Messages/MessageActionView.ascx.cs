#region Import

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using AjaxPro;
using ASC.Core;
using ASC.Core.Users;
using ASC.Notify.Recipients;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Projects.Engine;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.UserControls.Users;
using ASC.Web.Studio.Utility;

#endregion

namespace ASC.Web.Projects.Controls.Messages
{
    [AjaxNamespace("AjaxPro.MessageActionView")]
    public partial class MessageActionView : BaseUserControl
    {

        #region Members

        private bool _isEdit;
        protected bool _mobileVer = false;
        protected string _text = "";

        #endregion

        #region Property

        public Message Message { get; set; }
        public ProjectFat ProjectFat { get; set; }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            _uploadSwitchHolder.Controls.Add(new FileUploaderModeSwitcher());
            AjaxPro.Utility.RegisterTypeForAjax(typeof(MessageActionView));

            _mobileVer = ASC.Web.Core.Mobile.MobileDetector.IsRequestMatchesMobile(this.Context);
            if (_mobileVer && Request["mobiletext"] != null)
                _text = Request["mobiletext"];

            var target = Request["__EVENTTARGET"] ?? "";
            if (IsPostBack && !String.Equals(target, _cancelButton.UniqueID, StringComparison.InvariantCultureIgnoreCase))
                SaveOrUpdateMessage();
            else
                InitView();

            _cancelButton.Attributes["name"] = _fcKeditor.ClientID;
        }

        protected void lbCancel_Click(object sender, EventArgs e)
        {
            if (!_isEdit)
                CommonControlsConfigurer.FCKEditingCancel("projects");
            else
                CommonControlsConfigurer.FCKEditingCancel("projects", Message.ID.ToString());

            Response.Redirect("messages.aspx?prjID=" + ProjectFat.Project.ID.ToString());
        }


        #endregion

        #region Methods

        protected void InitView()
        {
            InitUserSelector();

            if (Message == null)
            {

                _isEdit = false;
                tbxTitle.Text = String.Empty;
                _fcKeditor.Value = String.Empty;

            }
            else
            {
                _isEdit = true;
                tbxTitle.Text = HttpUtility.HtmlDecode(Message.Title);

                _fcKeditor.Value = Message.Content;
                _text = Message.Content;

                _panelEditNotify.Visible = true;


            }

            _fcKeditor.BasePath = CommonControlsConfigurer.FCKEditorBasePath;
            _fcKeditor.ToolbarSet = "BlogToolbar";
            _fcKeditor.EditorAreaCSS = WebSkin.GetUserSkin().BaseCSSFileAbsoluteWebPath;
            _fcKeditor.Visible = !_mobileVer;

            tbxTitle.Focus();

            if (Message != null)
            {
                var files = FileEngine2.GetMessageFiles(Message);
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
                            TargerContainerID = "message_uploadedContainer",
                            RemoveHandler = "function (fileInfo) {return ASC.Projects.Messages.deleteFile(" + String.Format("{0},{1}, '{2}'", fileInfo.ID, fileInfo.Version, fileInfo.Title) + " );}"

                        });
                    }

                    Page.ClientScript.RegisterClientScriptBlock(typeof(MessageActionView),
                                                                "{5E6C09D3-A9EB-4ffd-9377-C97C52972E9E}",
                                                                " var uploadedFiles  = " +
                                                                JavaScriptSerializer.Serialize(uploadedFiles).Replace("\"function", "function").Replace(";}\"", ";}"), true);

                }
            }
        }

        protected void InitUserSelector()
        {
            List<Guid> projectTeam = ProjectFat.GetTeam().ConvertAll(rec => rec.ID);
            List<Guid> subscribersNotInProject = new List<Guid>();

            if (Message != null)
            {

                IRecipient[] recipients =
                    NotifySource.Instance.GetSubscriptionProvider().GetRecipients(
                        NotifyConstants.Event_NewCommentForMessage, Message.UniqID + "_" + ProjectFat.Project.ID);

                foreach (var recipient in recipients)
                {
                    bool inProject = false;
                    foreach (var participantID in projectTeam)
                    {
                        Guid recipientID = new Guid(recipient.ID);
                        if (recipientID.Equals(participantID))
                        {
                            inProject = true;
                            break;
                        }
                    }

                    if (!inProject)
                        subscribersNotInProject.Add(new Guid(recipient.ID));
                }

            }

            UserSelector userSelector = (UserSelector)LoadControl(UserSelector.Location);
            userSelector.BehaviorID = "UserSelector";
            userSelector.DisabledUsers = projectTeam;
            userSelector.Title = MessageResource.ManageSubscribers;
            userSelector.SelectedUserListTitle = ProjectsCommonResource.PepleSubscribedToMessage;
            userSelector.SelectedUsers = subscribersNotInProject;

            _phUserSelector.Controls.Add(userSelector);
        }

        protected void SaveOrUpdateMessage()
        {
            var uploadedFiles = new List<ASC.Files.Core.File>();
            if (Message == null)
            {
                Message = new Message();
                _isEdit = false;
            }
            else
            {
                uploadedFiles.AddRange(FileEngine2.GetMessageFiles(Message));
                _isEdit = true;
            }

            Message.Title = tbxTitle.Text;
            Message.Content = _mobileVer ? (Request["mobiletext"] ?? "") : _fcKeditor.Value;

            Message.Project = ProjectFat.Project;
            var participants = new List<Guid>
                (Request.Form["notify_participant_checked"].Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries).
                     Select(x => new Guid(x.Split('_')[2])));
            participants.AddRange(
                Request.Form["another_notify_participant_checked"].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).
                     Select(x => new Guid(x.Split('_')[2])));
            IEnumerable<int > fileIdList = null;
            if (!string.IsNullOrEmpty(Request.Form["attachment_list"]))
                fileIdList = Request.Form["attachment_list"].Split(';').Select(x => Convert.ToInt32(x.Split('_')[0]));
            var notify = true;
            if (!string.IsNullOrEmpty(Request.Form["notify_edit_checked"]))
            {
                bool.TryParse(Request.Form["notify_edit_checked"], out notify);
            }
            Global.EngineFactory.GetMessageEngine().SaveOrUpdate(Message, notify, participants, fileIdList);

            CommonControlsConfigurer.FCKEditingComplete("projects", Message.ID.ToString(), Message.Content, _isEdit);

            Response.Redirect(String.Format("messages.aspx?prjID={0}&id={1}", ProjectFat.Project.ID, Message.ID));

        }

        [AjaxMethod]
        public void DeleteMessage(int messageID)
        {
            var message = Global.EngineFactory.GetMessageEngine().GetByID(messageID);

            ProjectSecurity.DemandEdit(message);

            foreach (var file in FileEngine2.GetMessageFiles(message))
            {
                FileEngine2.RemoveFile(file.ID);
            }
            CommonControlsConfigurer.FCKUploadsRemoveForItem("projects", messageID.ToString());
            Global.EngineFactory.GetMessageEngine().Delete(message);
        }

        //NOTE: not used since it moved to engine
        //protected void NotifyParticipiant(List<ASC.Files.Core.File> uploadedFiles)
        //{
        //    //TODO:
        //    var objectID = Message.NotifyId;
        //    var subscriptionRecipients = NotifySource.Instance.GetSubscriptionProvider().GetRecipients(NotifyConstants.Event_NewCommentForMessage, objectID);
        //    var recipients = new List<Guid>();

        //    foreach (var item in Request.Form["notify_participant_checked"].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
        //    {
        //        recipients.Add(new Guid(item.Split('_')[2]));
        //    }
        //    foreach (var item in Request.Form["another_notify_participant_checked"].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
        //    {
        //        recipients.Add(new Guid(item.Split('_')[2]));
        //    }
        //    foreach (var subscriptionRecipient in subscriptionRecipients)
        //    {
        //        var subscriptionRecipientID = new Guid(subscriptionRecipient.ID);
        //        if (!recipients.Contains(subscriptionRecipientID))
        //        {
        //            NotifySource.Instance.GetSubscriptionProvider().UnSubscribe(NotifyConstants.Event_NewCommentForMessage, objectID, subscriptionRecipient);
        //        }
        //        else
        //        {
        //            recipients.Remove(subscriptionRecipientID);
        //        }
        //    }

        //    recipients
        //        .Select(r => NotifySource.Instance.GetRecipientsProvider().GetRecipient(r.ToString()))
        //        .Where(r => r != null)
        //        .ToList()
        //        .ForEach(r => NotifySource.Instance.GetSubscriptionProvider().Subscribe(NotifyConstants.Event_NewCommentForMessage, objectID, r));

        //    var fileListInfoHashtable = new Hashtable();
        //    var fileList = new List<string>();

        //    foreach (var file in uploadedFiles)
        //    {
        //        var fileInfo = String.Format("{0} ({1}, {2})", file.Title, Global.GetFileExtension(file.Title).ToUpper(), Global.ContentLengthToString(file.ContentLength));
        //        fileListInfoHashtable.Add(fileInfo, file.PreviewUrl);
        //    }
        //    foreach (var file in uploadedFiles)
        //    {
        //        if (file == null) continue;
        //        fileList.Add(String.Format("{0} ({1}, {2})", file.Title, Global.GetFileExtension(file.Title).ToUpper(), Global.ContentLengthToString(file.ContentLength)));
        //    }

        //    if (String.IsNullOrEmpty(Request.Form["notify_edit_checked"]))
        //        NotifyClient.Instance.SendAboutMessageAction(Message, "NewMessage", fileListInfoHashtable);
        //    else if (Convert.ToBoolean(Request.Form["notify_edit_checked"]))
        //        NotifyClient.Instance.SendAboutMessageAction(Message, "EditMessage", fileListInfoHashtable);
        //}

        protected String RenderNotifyBlock()
        {

            List<Participant> team = ProjectFat.GetTeam();
            List<Guid> subscribers = new List<Guid>();
            List<Guid> subscribersNotInProject = new List<Guid>();

            if (Message != null)
            {

                IRecipient[] recipients =
                    NotifySource.Instance.GetSubscriptionProvider().GetRecipients(
                        NotifyConstants.Event_NewCommentForMessage, Message.UniqID + "_" + ProjectFat.Project.ID);

                foreach (var recipient in recipients)
                {
                    bool inProject = false;
                    foreach (var participant in team)
                    {
                        Guid recipientID = new Guid(recipient.ID);
                        if (recipientID.Equals(participant.ID))
                        {
                            inProject = true;
                            break;
                        }
                    }

                    if (inProject)
                        subscribers.Add(new Guid(recipient.ID));
                    else
                        subscribersNotInProject.Add(new Guid(recipient.ID));
                }

            }

            if (subscribersNotInProject.Count > 0)
                ltlSelectedUsers.Text = RenderNotifyBlock(subscribersNotInProject.ToArray());

            StringBuilder innerHTML = new StringBuilder();

            int countColumn = 3;
            int counter = 0;

            var users = new List<Participant>();
            foreach (Participant prt in team)
            {
                if (CoreContext.UserManager.UserExists(prt.ID) && (CoreContext.UserManager.GetUsers(prt.ID).Status != EmployeeStatus.Terminated))
                {
                    users.Add(prt);
                }
            }

            int countRow = users.Count / countColumn;

            users.Sort((oX, oY) => String.Compare(oX.UserInfo.DisplayUserName(), oY.UserInfo.DisplayUserName()));

            if (users.Count % countColumn != 0) countRow++;

            innerHTML.AppendLine("<table cellspacing='0' cellpadding='0' style='width:100%'  id='notify_participant'>");
            innerHTML.AppendLine("<thead>");
            innerHTML.AppendLine("<tr>");

            innerHTML.AppendFormat(@"<td colspan='{1}'>
                                         <input type='checkbox' {2} onclick=" + "\"javascript: jq('#notify_participant').find('input[type=checkbox]').attr('checked', jq(this).is(':checked'))\"" + @"id='notify_all'/>
                                            <label for='notify_all'>
                                               {0} 
                                           </label>
                                     </td>
                                   ", ProjectsCommonResource.AllPeopleInProject, countColumn, (Message == null || users.Count == subscribers.Count) ? "checked='checked'" : string.Empty);

            innerHTML.AppendLine("</tr>");

            innerHTML.AppendLine("</thead>");

            innerHTML.AppendLine("<tbody>");

            int columnWidth = 100 / countColumn;

            for (int rowIndex = 1; rowIndex <= countRow; rowIndex++)
            {
                innerHTML.AppendLine("<tr>");

                for (int columnIndex = 1; columnIndex <= countColumn; columnIndex++)
                {

                    innerHTML.AppendFormat("<td style='width:{0}%'>", columnWidth);

                    innerHTML.AppendLine("<p>");

                    if (counter < users.Count)
                    {
                        string isChecked = "checked='checked'";
                        if (Message != null)
                        {
                            isChecked = subscribers.Contains(users[counter].ID) ? "checked='checked'" : string.Empty;
                        }

                        innerHTML.AppendFormat(
                                                @"
                                            <input type='checkbox' {3} id='notify_user_{1}' name='notify_user_{1}' />
                                            <label for='notify_user_{1}'>
                                              <img title='' src='{2}' >
                                                {0}
                                            </label>                                   
                                           ",
                                                users[counter].UserInfo.DisplayUserName(true), users[counter].ID,
                                                WebImageSupplier.GetAbsoluteWebPath("profile.gif"),
                                                isChecked
                           );
                    }
                    innerHTML.AppendLine("</p>");

                    innerHTML.Append("</td>");

                    counter++;

                }

                innerHTML.Append("</tr>");
            }

            innerHTML.AppendLine("</tbody>");
            innerHTML.AppendFormat("</table>");

            return innerHTML.ToString();



        }

        protected String RenderFileListBlock()
        {

            if (Message == null) return String.Empty;

            StringBuilder innerHTML = new StringBuilder();

            foreach (var file in FileEngine2.GetMessageFiles(Message))
            {
                innerHTML.AppendFormat(
                    String.Format(
                        @" 
                <tr id='file_info_{0}_{1}'>
                    <td>
                        <div class='pm-progress-wrapper borderLight cornerAll'>
                            <div class='pm-progress' style='width: 100%' />
                        </div>
                        <span class='textMediumDescribe pm-progress-status' />
                    </td>
                    <td style='width: 100%;'>
                        <img hspace='3px' height='16px' align='bottom' width='16px' src='{4}' />{2}
                    </td>
                    <td>
                        <span class='describeText'>{3}</span>
                    </td>
                    <td>
                        <a class='linkAction pm-cancel' href='javascript:ASC.Projects.Messages.deleteFile({0},{1})'>{5}</a>
                    </td>
                </tr>", file.ID, file.Version, file.Title, Global.ContentLengthToString(file.ContentLength), Global.GetImgFilePath(file.Title, false), ProjectsCommonResource.Delete));
            }

            return innerHTML.ToString();

        }

        [AjaxMethod]
        public void DeleteFile(int prjID, int id, int version)
        {
            var file = FileEngine2.GetFile(id, version);
            if(!FileEngine2.CanDelete(file, prjID))
                throw ProjectSecurity.CreateSecurityException();

            FileEngine2.RemoveFile(id);
        }

        [AjaxMethod]
        public string PreviewMessage(string prjID, string ID, string title, string content)
        {
            ProjectSecurity.DemandAuthentication();

            Message message = new Message();

            if (ID != string.Empty && ID != null)
            {
                message = Global.EngineFactory.GetMessageEngine().GetByID(Convert.ToInt32(ID));
                message.Title = title;
                message.Content = content;
            }
            else
            {
                message.Project = Global.EngineFactory.GetProjectEngine().GetByID(Convert.ToInt32(prjID));
                message.Title = title;
                message.Content = content;
            }


            System.Web.UI.Page page = new System.Web.UI.Page();
            MessageView cntrl = (MessageView)LoadControl(PathProvider.GetControlVirtualPath("MessageView.ascx"));

            cntrl.Message = message;
            cntrl.IsPreview = true;


            page.Controls.Add(cntrl);
            var writer = new System.IO.StringWriter();
            HttpContext.Current.Server.Execute(page, writer, false);
            string output = writer.ToString();
            writer.Close();


            return output;

        }

        [AjaxMethod]
        protected string RenderRedirectUpload()
        {
            ProjectSecurity.DemandAuthentication();

            return string.Format("{0}://{1}:{2}{3}", Request.Url.Scheme, Request.Url.Host, Request.Url.Port,
                VirtualPathUtility.ToAbsolute("~/") + "fckuploader.ashx?esid=projects" + (_isEdit ? "&iid=" + Message.ID.ToString() : ""));
        }

        [AjaxMethod]
        public string RenderNotifyBlock(Guid[] usersID)
        {
            ProjectSecurity.DemandAuthentication();

            if (usersID.Length == 0) return string.Empty;

            StringBuilder innerHTML = new StringBuilder();

            int countColumn = 3;
            int counter = 0;
            var users = new List<Participant>();

            foreach (var id in usersID)
            {
                if (CoreContext.UserManager.UserExists(id) && (CoreContext.UserManager.GetUsers(id).Status != EmployeeStatus.Terminated))
                {
                    users.Add(Global.EngineFactory.GetParticipantEngine().GetByID(id));
                }
            }

            int countRow = users.Count / countColumn;

            users.Sort((oX, oY) => String.Compare(oX.UserInfo.DisplayUserName(), oY.UserInfo.DisplayUserName()));

            if (users.Count % countColumn != 0) countRow++;

            innerHTML.AppendLine("<table cellspacing='0' cellpadding='0' style='width:100%'  id='another_notify_participant'>");
            innerHTML.AppendLine("<thead></thead>");
            innerHTML.AppendLine("<tbody>");

            int columnWidth = 100 / countColumn;

            for (int rowIndex = 1; rowIndex <= countRow; rowIndex++)
            {
                innerHTML.AppendLine("<tr>");

                for (int columnIndex = 1; columnIndex <= countColumn; columnIndex++)
                {

                    innerHTML.AppendFormat("<td style='width:{0}%'>", columnWidth);

                    innerHTML.AppendLine("<p>");

                    if (counter < users.Count)
                    {
                        string isChecked = "checked='checked'";

                        innerHTML.AppendFormat(
                                                @"
                                            <input type='checkbox' {3} id='notify_user_{1}' name='notify_user_{1}' />
                                            <label for='notify_user_{1}'>
                                              <img title='' src='{2}' >
                                                {0}
                                            </label>                                   
                                           ",
                                                users[counter].UserInfo.DisplayUserName(), users[counter].ID,
                                                WebImageSupplier.GetAbsoluteWebPath("profile.gif"),
                                                isChecked
                           );
                    }
                    innerHTML.AppendLine("</p>");

                    innerHTML.Append("</td>");

                    counter++;

                }

                innerHTML.Append("</tr>");
            }

            innerHTML.AppendLine("</tbody>");
            innerHTML.AppendFormat("</table>");

            return innerHTML.ToString();
        }

        #endregion

    }
}
