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
using ASC.Projects.Engine;

#endregion

namespace ASC.Web.Projects.Controls.Messages
{
    public partial class MessageView : BaseUserControl
    {
        public Message Message { get; set; }
        public int CommentsCount = -1;
        public List<ASC.Files.Core.File> Files = null;

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
                if (Message.ID == 0)
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
                if (Message.ID == 0)
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

                return Global.EngineFactory.GetParticipantEngine().GetByID(Message.CreateBy).UserInfo.DisplayUserName(true);

            }
            else
            {
                if (Message.ID == 0)
                {
                    Guid currentUserID = SecurityContext.CurrentAccount.ID;
                    UserInfo currentUser = CoreContext.UserManager.GetUsers(currentUserID);
                    return ASC.Core.Users.UserInfoExtension.DisplayUserName(currentUser, true);
                }
                else return Global.EngineFactory.GetParticipantEngine().GetByID(Message.CreateBy).UserInfo.DisplayUserName(true);
            }
        }
        public string GetUserTitle()
        {
            if (!IsPreview)
            {

                return Global.EngineFactory.GetParticipantEngine().GetByID(Message.CreateBy).UserInfo.Title.HtmlEncode();

            }
            else
            {
                if (Message.ID == 0)
                {
                    Guid currentUserID = SecurityContext.CurrentAccount.ID;
                    UserInfo currentUser = CoreContext.UserManager.GetUsers(currentUserID);
                    return currentUser.Title.HtmlEncode();
                }
                else return Global.EngineFactory.GetParticipantEngine().GetByID(Message.CreateBy).UserInfo.Title.HtmlEncode();
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
                if (Message.ID == 0)
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
                                Message.Project.ID, Message.ID.ToString(), MessageResource.SeeMore);
            }
            else
            {
                sb.AppendFormat("<br/><a href='javascript:void(0)'>{0}</a><font style='font-size:14px;color:#0961AB;text-decoration:none;'> →</font>",
                                MessageResource.SeeMore);
            }

            return HtmlUtility.GetPreview(Message.Content, sb.ToString(), ProductEntryPoint.ID);
        }
        public string GetMessageTitle()
        {
            return Message.Title.HtmlEncode();
        }
        public int GetMessageID()
        {
            if (!IsPreview)
                return Message.ID;
            else return 0;
        }
        public int GetPrjID()
        {
            return Message.Project.ID;
        }


        public string GetAttachedFiles()
        {
            StringBuilder innerHTML = new StringBuilder();

            var anotherFiles = new List<ASC.Files.Core.File>();

            foreach (var file in Files)
            {
                if (string.IsNullOrEmpty(file.ThumbnailURL))
                {
                    anotherFiles.Add(file);
                }
            }
            if (anotherFiles.Count > 0)
            {
                innerHTML.Append("<table><tr><td valign=top>");
                innerHTML.AppendFormat("<img align='absmiddle' style='float:left' src='{0}' alt='{1}' title='{2}'/></td>",
                    GetFilesImgURL(), ProjectsFileResource.Files, ProjectsFileResource.Files);
                innerHTML.Append("<td><div style='width:440px;'>");

                int i = anotherFiles.Count;
                int j = 1;

                foreach (var file in anotherFiles)
                {
                    var filePath = file.FileUri;
                    innerHTML.AppendFormat("<a class='linkDescribe' href='{1}' >{0}</a>", HtmlUtility.GetText(file.Title, 25), filePath);
                    if (j < i) innerHTML.AppendLine(", ");
                    j++;
                }

                innerHTML.Append("</div></td></tr></table>");
            }
            return innerHTML.ToString();

        }
        public string GetAttachedImages()
        {
            if (!Global.ModuleManager.IsVisible(ModuleType.TMDocs) || !ProjectSecurity.CanReadFiles(Message.Project))
                return string.Empty;

            StringBuilder innerHTML = new StringBuilder();

            var images = new List<ASC.Files.Core.File>();

            foreach (var file in Files)
            {
                if (!string.IsNullOrEmpty(file.ThumbnailURL))
                {
                    images.Add(file);
                }
            }
            AttachedImagesCount = images.Count;

            if (images.Count > 0)
            {
                innerHTML.Append("<div id='gallery' style='padding-top:15px;'>");
                innerHTML.AppendLine("<table width='100%' cellpadding='5'><tr>");
                int i = 1;
                int j = 1;

                foreach (var file in images)
                {

                    string thumbUrl = file.ThumbnailURL;
                    string fileUrl = file.FileUri;

                    innerHTML.AppendLine("<td width='25%' align='center'>");

                    innerHTML.AppendFormat("<img style='border:none;cursor:pointer;' src='{0}' title='{1}' _zoom='{2}' />", thumbUrl, file.Title, fileUrl);
                    innerHTML.AppendFormat("<br/><a class='linkDescribe' href='{1}' title='{2}' >{0}</a>", HtmlUtility.GetText(file.Title, 20), fileUrl, file.Title);

                    innerHTML.AppendLine("</td>");
                    j++;
                    if (i % 4 == 0) { innerHTML.AppendLine("</tr><tr>"); j = 1; }
                    i++;
                }


                if (j == 2) innerHTML.AppendLine("<td width='25%' align='center'></td><td width='25%' align='center'></td><td width='25%' align='center'></td>");
                if (j == 3) innerHTML.AppendLine("<td width='25%' align='center'><td width='25%' align='center'></td>");
                if (j == 4) innerHTML.AppendLine("<td width='25%' align='center'>");

                innerHTML.AppendLine("</tr></table></div>");
            }

            return HtmlUtility.GetFull(innerHTML.ToString(), ProductEntryPoint.ID);
        }
        public string GetActions()
        {
            StringBuilder innerHTML = new StringBuilder();
            innerHTML.AppendFormat("<a class='linkAction' href='messages.aspx?prjID={1}&id={2}'>{0}</a>", MessageResource.Comments + ": " + CommentsCount, Message.Project.ID, Message.ID);
            if (ProjectSecurity.CanEdit(Message))
            {
                innerHTML.AppendLine("<span class='splitter'>|</span>");
                innerHTML.AppendFormat("<a class='linkAction' href='messages.aspx?prjID={0}&id={1}&action=edit'>{2}</a>", Message.Project.ID, Message.ID, MilestoneResource.Edit);
                innerHTML.AppendLine("<span class='splitter'>|</span>");
                innerHTML.AppendFormat("<a class='linkAction' href='javascript:void(0)' onclick='javascript:ASC.Projects.Messages.deleteMessage({0},1)'>{1}</a>", Message.ID, MessageResource.DeleteMessage);
            }
            return innerHTML.ToString();
        }
        public string GetHrefMessage()
        {
            if (IsPreview) return "javascript:void(0)";
            else
            {
                StringBuilder innerHTML = new StringBuilder();
                innerHTML.AppendFormat("messages.aspx?prjID={1}&id={2}", MessageResource.PostNewComment, Message.Project.ID, Message.ID);
                return innerHTML.ToString();
            }
        }

        #endregion


    }
}