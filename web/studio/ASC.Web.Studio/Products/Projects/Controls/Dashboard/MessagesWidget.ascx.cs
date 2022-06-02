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
using ASC.Web.Projects.Configuration;
using ASC.Web.Core.Utility.Settings;
#endregion

namespace ASC.Web.Projects.Controls.Dashboard
{
    [Serializable]
    public class MessagesWidgetSettings : ISettings
    {
        public int MesagesCount { get; set; }
        public bool ShowOnlyItemsInFollowingProjects { get; set; }

        #region ISettings Members

        public Guid ID
        {
            get { return new Guid("{3BFEAF86-AF4A-483a-973A-3AA6A98A643A}"); }
        }

        public ISettings GetDefault()
        {
            return new MessagesWidgetSettings() { MesagesCount = 5, ShowOnlyItemsInFollowingProjects = false };
        }

        #endregion

    }

    public partial class MessagesWidget : BaseUserControl
    {

        #region Properties

        public static Guid WidgetID { get { return new Guid("{77B24DA8-3A8C-4bb6-8E06-C814298764D9}"); } }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {

            var widgetSettings = SettingsManager.Instance.LoadSettingsFor<MessagesWidgetSettings>(SecurityContext.CurrentAccount.ID);
            List<Message> messages = null;

            Guid participant = SecurityContext.CurrentAccount.ID;

            if (widgetSettings.ShowOnlyItemsInFollowingProjects)
            {
                var projectsID = Global.EngineFactory
                        .GetParticipantEngine()
                        .GetInterestedProjects(participant);

                //TODO: Not Now
                //projectsID = FilterProject(projectsID);//here he check project visability

                messages = Global.EngineFactory.GetMessageEngine().GetRecentMessages(widgetSettings.MesagesCount, projectsID.ToArray());
            }
            else
                messages = Global.EngineFactory.GetMessageEngine().GetRecentMessages(widgetSettings.MesagesCount);


            List<MessageVM> wrapperedMessages = new List<MessageVM>(messages.Count);
            if (messages.Count > 0)
            {
                List<int> commentCounts = Global.EngineFactory.GetCommentEngine().GetCommentsCount(messages.ConvertAll(m => (ProjectEntity)m));
                List<bool> isReaded = Global.EngineFactory.GetParticipantEngine().IsReaded(participant, messages.ConvertAll(m => (ProjectEntity)m));

                for (int i = 0; i < messages.Count; i++)
                    wrapperedMessages.Add(new MessageVM(messages[i])
                    {
                        CommentCount = commentCounts[i],
                        IsReaded = isReaded[i]
                    });
            }

            MessagesRepeater.DataSource = wrapperedMessages;
            MessagesRepeater.DataBind();
        }

        #endregion


        public class MessageVM
        {

            public MessageVM(Message message)
            {
                Message = message;
                CreatedBy = Global.EngineFactory.GetParticipantEngine().GetByID(message.CreateBy).UserInfo;
            }

            public Message Message;

            public UserInfo CreatedBy;
            public string CreatedByAvatarUrl { get { return CreatedBy.GetSmallPhotoURL(); } }
            public string CreatedByProfileLink { get { return CreatedBy.RenderProfileLink(ProductEntryPoint.ID); } }

            public string CreatedDateString { get { return Message.CreateOn.ToString(DateTimeExtension.DateFormatPattern); } }
            public string CreatedTimeString { get { return Message.CreateOn.ToString("HH:mm"); } }

            public string MessageUrl { get { return String.Format("messages.aspx?prjID={1}&id={2}", MessageResource.PostNewComment, Message.Project.ID, Message.ID); } }

            public int CommentCount;
            public bool IsReaded;

        }
    }
}