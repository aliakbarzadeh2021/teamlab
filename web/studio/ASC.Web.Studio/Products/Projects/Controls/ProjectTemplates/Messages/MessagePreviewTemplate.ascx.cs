#region Import

using System;
using System.Text;
using ASC.Core;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;
using ASC.Web.Core.Utility.Skins;
using System.Collections.Generic;
using ASC.Web.Studio.Helpers;
using System.Web;
using ASC.Web.Controls;
using ASC.Files.Core;

#endregion

namespace ASC.Web.Projects.Controls.ProjectTemplates.Messages
{
    public partial class MessagePreviewTemplate : BaseUserControl
    {
        public TemplateMessage Message { get; set; }
        public int CommentsCount = -1;
        public List<File> Files = null;

        public bool IsPreview { get; set; }
        public int AttachedImagesCount { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        #region Methods

        public string GetAvatarURL()
        {
            if (!IsPreview)
            {
                var currentUser = Global.EngineFactory.GetParticipantEngine().GetByID(Message.CreateBy);
                return currentUser.UserInfo.GetBigPhotoURL();
            }
            else
            {
                if (Message.Id == 0)
                {
                    var currentUser = Global.EngineFactory.GetParticipantEngine().GetByID(SecurityContext.CurrentAccount.ID);
                    return currentUser.UserInfo.GetBigPhotoURL();
                }
                else return Global.EngineFactory.GetParticipantEngine().GetByID(Message.CreateBy).UserInfo.GetBigPhotoURL();
            }
        }
        public string GetFilesImgURL()
        {
            return WebImageSupplier.GetAbsoluteWebPath("skrepka.gif", ProductEntryPoint.ID); ;
        }
        public string GetUserProfileLink()
        {
            if (!IsPreview)
            {

                return Global.EngineFactory.GetParticipantEngine().GetByID(Message.CreateBy).UserInfo.RenderProfileLink(ProductEntryPoint.ID);

            }
            else
            {
                if (Message.Id == 0)
                {
                    Guid currentUserID = SecurityContext.CurrentAccount.ID;
                    Participant currentUser = Global.EngineFactory.GetParticipantEngine().GetByID(currentUserID);
                    return currentUser.UserInfo.RenderProfileLink(ProductEntryPoint.ID);
                }
                else return Global.EngineFactory.GetParticipantEngine().GetByID(Message.CreateBy).UserInfo.RenderProfileLink(ProductEntryPoint.ID);
            }
        }
        public string GetUserName()
        {
            if (!IsPreview)
            {

                return Global.EngineFactory.GetParticipantEngine().GetByID(Message.CreateBy).UserInfo.DisplayUserName();

            }
            else
            {
                if (Message.Id == 0)
                {
                    Guid currentUserID = SecurityContext.CurrentAccount.ID;
                    UserInfo currentUser = CoreContext.UserManager.GetUsers(currentUserID);
                    return ASC.Core.Users.UserInfoExtension.DisplayUserName(currentUser);
                }
                else return Global.EngineFactory.GetParticipantEngine().GetByID(Message.CreateBy).UserInfo.DisplayUserName();
            }
        }
        public string GetUserTitle()
        {
            if (!IsPreview)
            {

                return Global.EngineFactory.GetParticipantEngine().GetByID(Message.CreateBy).UserInfo.Title;

            }
            else
            {
                if (Message.Id == 0)
                {
                    Guid currentUserID = SecurityContext.CurrentAccount.ID;
                    UserInfo currentUser = CoreContext.UserManager.GetUsers(currentUserID);
                    return currentUser.Title;
                }
                else return Global.EngineFactory.GetParticipantEngine().GetByID(Message.CreateBy).UserInfo.Title;
            }
        }
        public string GetHrefUser()
        {
            if (IsPreview) return "javascript:void(0)";
            else
            {
                return Global.EngineFactory.GetParticipantEngine().GetByID(Message.CreateBy).UserInfo.GetUserProfilePageURL(ProductEntryPoint.ID);
            }
        }
        public string GetMessageDate()
        {
            if (!IsPreview)
            {

                return Message.CreateOn.ToShortString();

            }
            else
            {
                if (Message.Id == 0)
                {
                    return ASC.Core.Tenants.TenantUtil.DateTimeNow().ToShortString();
                }
                else return Message.CreateOn.ToShortString();
            }
        }
        public string GetMessageText()
        {
            StringBuilder sb = new StringBuilder();

            if (!IsPreview)
            {
                sb.AppendFormat("<br/><a href='messages.aspx?prjID={0}&id={1}'>{2}</a><font style='font-size:14px;color:#0961AB;text-decoration:none;'> →</font>",
                                Message.ProjectId, Message.Id.ToString(), MessageResource.SeeMore);
            }
            else
            {
                sb.AppendFormat("<br/><a href='javascript:void(0)'>{0}</a><font style='font-size:14px;color:#0961AB;text-decoration:none;'> →</font>",
                                MessageResource.SeeMore);
            }

            return HtmlUtility.GetPreview(Message.Text, sb.ToString(), ProductEntryPoint.ID);
        }
        public string GetMessageTitle()
        {
            return Message.Title.HtmlEncode();
        }
        public int GetMessageID()
        {
            if (!IsPreview)
                return Message.Id;
            else return 0;
        }
        public int GetPrjID()
        {
            return Message.ProjectId;
        }
       

        public string GetActions()
        {
            StringBuilder innerHTML = new StringBuilder();
            innerHTML.AppendFormat("<a class='linkAction' href='messages.aspx?prjID={1}&id={2}'>{0}</a>", MessageResource.Comments + ": " + CommentsCount, Message.ProjectId, Message.Id);
            if (Global.IsAdmin || SecurityContext.CurrentAccount.ID == Message.CreateBy)
            {
                innerHTML.AppendLine("<span class='splitter'>|</span>");
                innerHTML.AppendFormat("<a class='linkAction' href='messages.aspx?prjID={0}&id={1}&action=edit'>{2}</a>", Message.ProjectId, Message.Id, MilestoneResource.Edit);
                innerHTML.AppendLine("<span class='splitter'>|</span>");
                innerHTML.AppendFormat("<a class='linkAction' href='javascript:void(0)' onclick='javascript:ASC.Projects.Messages.deleteMessage({0},1)'>{1}</a>", Message.Id, MessageResource.DeleteMessage);
            }
            return innerHTML.ToString();
        }
        public string GetHrefMessage()
        {
            if (IsPreview) return "javascript:void(0)";
            else
            {
                StringBuilder innerHTML = new StringBuilder();
                innerHTML.AppendFormat("messages.aspx?prjID={1}&id={2}", MessageResource.PostNewComment, Message.ProjectId, Message.Id);
                return innerHTML.ToString();
            }
        }

        #endregion


    }
}