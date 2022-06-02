#region Import

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using AjaxPro;
using ASC.Common;
using ASC.Core;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Controls.Tasks;
using ASC.Web.Projects.Resources;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Utility;
using ASC.Web.Core.Users.Activity;
using ASC.Web.Studio.Controls.Common;
using ASC.Projects.Core;
using ASC.Web.Studio.UserControls.Users;
using ASC.Web.Projects.Controls.Messages;
using ASC.Projects.Engine;

#endregion

namespace ASC.Web.Projects.Controls.ProjectTemplates.Messages
{
    [AjaxNamespace("AjaxPro.MessageActionView")]
    public partial class MessageViewTemplate : BaseUserControl
    {

        #region Members

        private bool _isEdit;

        #endregion

        #region Property

        public TemplateMessage Message { get; set; }
        public TemplateProject Template { get; set; }
		public bool Permission = true;

		public TemplateEngine Engine
		{
			get
			{
				return Global.EngineFactory.GetTemplateEngine();
			}
		}

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(MessageViewTemplate));
            Permission = true;

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
                CommonControlsConfigurer.FCKEditingCancel("projects", Message.Id.ToString());

            Response.Redirect("messages.aspx?prjID=" + Template.Id);
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
                _fcKeditor.Value = Message.Text;
            }

            _fcKeditor.BasePath = CommonControlsConfigurer.FCKEditorBasePath;
            _fcKeditor.ToolbarSet = "BlogToolbar";
            _fcKeditor.EditorAreaCSS = WebSkin.GetUserSkin().BaseCSSFileAbsoluteWebPath;

            tbxTitle.Focus();
        }

        protected void InitUserSelector()
        {
            List<Guid> projectTeam = Template.Team;
            List<Guid> subscribersNotInProject = new List<Guid>();

            if (Message != null)
            {

                IRecipient[] recipients =
                    NotifySource.Instance.GetSubscriptionProvider().GetRecipients(
                        NotifyConstants.Event_NewCommentForMessage, Message.Id + "_" + Template.Id);

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

			//UserSelector userSelector = (UserSelector)LoadControl(UserSelector.Location);
			//userSelector.BehaviorID = "UserSelector";
			//userSelector.DisabledUsers = projectTeam;
			//userSelector.Title = MessageResource.ManageSubscribers;
			//userSelector.SelectedUserListTitle = ProjectsCommonResource.PepleSubscribedToMessage;
			//userSelector.SelectedUsers = subscribersNotInProject;

        }

        protected void SaveOrUpdateMessage()
        {
            if (Message == null)
            {
				Message = new TemplateMessage(Template.Id, tbxTitle.Text);
                _isEdit = false;
            }
            else
            {
                _isEdit = true;
            }



            Message.Title = tbxTitle.Text;
            Message.Text = _fcKeditor.Value;
            Message.ProjectId = Template.Id;

            Global.EngineFactory.GetTemplateEngine().SaveTemplateMessage(Message);

            CommonControlsConfigurer.FCKEditingComplete("projects", Message.Id.ToString(), Message.Text, _isEdit);

            Response.Redirect(String.Format("messages.aspx?prjID={0}&id={1}", Template.Id, Message.Id));
        }

        [AjaxMethod]
        public void DeleteMessage(int messageID)
        {
            ProjectSecurity.DemandAuthentication();

            TemplateMessage message = Engine.GetTemplateMessage(messageID);
            CommonControlsConfigurer.FCKUploadsRemoveForItem("projects", messageID.ToString());
			Engine.RemoveTemplateMessage(message.Id);
        }

        [AjaxMethod]
        public string PreviewMessage(string prjID, string ID, string title, string content)
        {
            ProjectSecurity.DemandAuthentication();

			var id = Convert.ToInt32(ID);

			TemplateMessage message = null;

            if (id > 0)
            {
				message = Engine.GetTemplateMessage(id);
                message.Title = title;                
            }
            else
            {
				message = new TemplateMessage(int.Parse(prjID), title);                
            }
			message.Text = content;


            System.Web.UI.Page page = new System.Web.UI.Page();
			MessagePreviewTemplate cntrl = (MessagePreviewTemplate)LoadControl(PathProvider.GetControlVirtualPath("MessagePreviewTemplate.ascx"));

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
                VirtualPathUtility.ToAbsolute("~/") + "fckuploader.ashx?esid=projects" + (_isEdit ? "&iid=" + Message.Id.ToString() : ""));
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
