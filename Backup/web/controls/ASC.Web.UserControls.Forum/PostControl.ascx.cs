using System;
using System.Text;
using System.Web;
using System.Web.UI;
using AjaxPro;
using ASC.Core.Users;
using ASC.Forum;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Forum.Common;

namespace ASC.Web.UserControls.Forum
{
    [AjaxNamespace("PostManager")]
    public partial class PostControl : UserControl                                                                 
    {
        public Post Post { get; set; }
        public Guid SettingsID { get; set; }
        protected Settings _settings;
        private ForumManager _forumManager;
		public int PostsCount { get; set; }

        public int CurrentPageNumber { get; set; } 

        public PostControl()
        {
            CurrentPageNumber = -1;
        }

        public bool IsEven { get; set; }

        protected string _postCSSClass;
        protected string _messageCSSClass;

        protected void Page_Load(object sender, EventArgs e)
        {
            Utility.RegisterTypeForAjax(this.GetType());

            _settings = ForumManager.GetSettings(SettingsID);
            _forumManager = _settings.ForumManager;

            _postCSSClass = IsEven ? "tintMedium" : "";

            _messageCSSClass = "forum_mesBox";
            if (!Post.IsApproved && _forumManager.ValidateAccessSecurityAction(ForumAction.ApprovePost, Post))
                _messageCSSClass = "tintDangerous forum_mesBox";
        }

        protected string RenderEditedData()
        {
            if (Post.EditCount <= 0)
                return "";

            StringBuilder sb = new StringBuilder();
            sb.Append("<div class='textMediumDescribe' style='padding:2px 5px;'>" + Resources.ForumUCResource.Edited + "&nbsp;&nbsp;");
            sb.Append(Post.EditDate.ToShortDateString() +" "+Post.EditDate.ToShortTimeString());
            sb.Append("<span style='margin-left:5px;'>"+Post.Editor.RenderProfileLink(_settings.ProductID)+"</span>");            
            sb.Append("</div>");

            return sb.ToString();
        }
     
        protected void ReferenceToPost()
        {
            string refToPost = "";
            if (CurrentPageNumber != -1)
                refToPost = "<a class=\"linkDescribe\" href=\""+_settings.LinkProvider.Post(Post.ID, Post.TopicID, CurrentPageNumber)+ "\">#" + Post.ID.ToString() + "</a>";
            else
                refToPost = "&nbsp;";
            Response.Write(refToPost);
        }
        
        protected string ControlButtons()
        {
            StringBuilder sb = new StringBuilder();
            
            if (_forumManager.ValidateAccessSecurityAction(ForumAction.PostCreate, new Topic() {ID = Post.TopicID }))
            {
                sb.Append("<a class=\"baseLinkButton" + (SetupInfo.WorkMode == WorkMode.Promo ? " promoAction" : "") + "\" style=\"float:left;\" href=\"" + _settings.LinkProvider.NewPost(Post.TopicID, PostAction.Quote, Post.ID) + "\">" + Resources.ForumUCResource.Quote + "</a>");
                sb.Append("<a class=\"baseLinkButton" + (SetupInfo.WorkMode == WorkMode.Promo ? " promoAction" : "") + "\" style=\"float:left;  margin-left:8px;\" href=\"" + _settings.LinkProvider.NewPost(Post.TopicID, PostAction.Reply, Post.ID) + "\">" + Resources.ForumUCResource.Reply + "</a>");
                sb.Append("<a class=\"baseLinkButton" + (SetupInfo.WorkMode == WorkMode.Promo ? " promoAction" : "") + "\" style=\"float:left; margin-left:8px;\" href=\"" + _settings.LinkProvider.NewPost(Post.TopicID) + "\">" + Resources.ForumUCResource.NewPostButton + "</a>");
            }

            bool isFirst = true;

            if (_forumManager.ValidateAccessSecurityAction(ForumAction.PostDelete, Post) && ShowDeletePostLink())
            {
                sb.AppendFormat("<a class=\"linkAction" + (SetupInfo.WorkMode == WorkMode.Promo ? " promoAction" : "") + "\" style=\"float:right;\" id='PostDeleteLink{0}' href=\"javascript:ForumManager.DeletePost('" + Post.ID + "')\">" + Resources.ForumUCResource.DeleteButton + "</a>", Post.ID);
                isFirst = false;
            }

            
            if (_forumManager.ValidateAccessSecurityAction(ForumAction.PostEdit, Post))
            {
				if (!isFirst && ShowDeletePostLink())
                    sb.AppendFormat("<span class='splitter' id='PostDeleteSplitter{0}' style='float:right;'>|</span>", Post.ID);

                sb.Append("<a class=\"linkAction" + (SetupInfo.WorkMode == WorkMode.Promo ? " promoAction" : "") + "\" style=\"float:right;\" href=\"" + _settings.LinkProvider.NewPost(Post.TopicID, PostAction.Edit, Post.ID) + "\">" + Resources.ForumUCResource.EditButton + "</a>");
                isFirst = false;
            }

            if (!Post.IsApproved && _forumManager.ValidateAccessSecurityAction(ForumAction.ApprovePost, Post))
            {
                if (!isFirst)
                    sb.Append("<span class='splitter' style='float:right;'>|</span>");

                sb.Append("<a id=\"forum_btap_" + Post.ID + "\" class=\"linkAction" + (SetupInfo.WorkMode == WorkMode.Promo ? " promoAction" : "") + "\" style=\"margin-left:5px; float:right;\" href=\"javascript:ForumManager.ApprovePost('" + Post.ID + "')\">" + Resources.ForumUCResource.ApproveButton + "</a>");
            }

            return sb.ToString();
        }

		private bool ShowDeletePostLink()
		{
			return PostsCount > 1;
		}
        
        public static string AttachmentsList(Post post, Guid settingsID)
        {
            var forumManager = ForumManager.GetForumManager(settingsID);
            StringBuilder sb = new StringBuilder();
            if (post.Attachments.Count <= 0)
                return "";

            sb.Append("<div class=\"tintLight cornerAll borderBase forum_attachmentsBox\">");
            sb.Append("<div class='headerPanel'>" + Resources.ForumUCResource.AttachFiles + "</div>");
            foreach (Attachment attachment in post.Attachments)
            {
                sb.Append("<div id=\"forum_attach_" + attachment.ID + "\" class=\"borderBase  forum_attachItem clearFix\">");
                sb.Append("<table cellspacing='0' cellpadding='0' style='width:100%;'><tr>");
                sb.Append("<td style=\"width:350px;\">");
                sb.Append("<a target=\"_blank\" href=\"" + forumManager.GetAttachmentWebPath(attachment) + "\">" + HttpUtility.HtmlEncode(attachment.Name) + "</a>");
                sb.Append("</td>");

                if (forumManager.ValidateAccessSecurityAction(ForumAction.AttachmentDelete, post))
                {
                    sb.Append("<td style=\"width:100px;\">");
                    sb.Append("<a class=\"linkDescribe" + (SetupInfo.WorkMode == WorkMode.Promo ? " promoAction" : "") + "\" href=\"javascript:ForumManager.DeleteAttachment('" + attachment.ID + "','" + post.ID + "');\">" + Resources.ForumUCResource.DeleteButton + "</a>");
                    sb.Append("</td>");
                }
                sb.Append("<td style=\"text-align:right;\"><span class=\"textMediumDescribe\">" + ((float)attachment.Size / 1024f).ToString("####0.##") + " KB</span></td>");
                sb.Append("</tr></table>");
                sb.Append("</div>");
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse DoDeletePost(int idPost, Guid settingsID)
        {
            _forumManager = ForumManager.GetForumManager(settingsID);
            AjaxResponse resp = new AjaxResponse();
            resp.rs2 = idPost.ToString();

            var post = ForumDataProvider.GetPostByID(TenantProvider.CurrentTenantID,idPost);
            if (post == null)
            {
                resp.rs1 = "0";
                resp.rs3 = Resources.ForumUCResource.ErrorAccessDenied;
                return resp;
            }

            if (!_forumManager.ValidateAccessSecurityAction(ForumAction.PostDelete, post))
            {
                resp.rs1 = "0";
                resp.rs3 = Resources.ForumUCResource.ErrorAccessDenied;
                return resp;
            }
            
            try
            {
                var result = ForumDataProvider.RemovePost(TenantProvider.CurrentTenantID, post.ID);
                if (result == DeletePostResult.Successfully)
                {
                    resp.rs1 = "1";
                    resp.rs3 = Resources.ForumUCResource.SuccessfullyDeletePostMessage;
                    _forumManager.RemoveAttachments(post);

                    CommonControlsConfigurer.FCKUploadsRemoveForItem(_forumManager.Settings.FileStoreModuleID, idPost.ToString());
                }
                else if (result == DeletePostResult.ReferencesBlock)
                {
                    resp.rs1 = "0";
                    resp.rs3 = Resources.ForumUCResource.ExistsReferencesChildPosts;

                }
                else
                {
                    resp.rs1 = "0";
                    resp.rs3 = Resources.ForumUCResource.ErrorDeletePost;
                }
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs3 = HttpUtility.HtmlEncode(e.Message);
            }

            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse DoApprovedPost(int idPost, Guid settingsID)
        {
            _forumManager = ForumManager.GetForumManager(settingsID);
            AjaxResponse resp = new AjaxResponse();
            resp.rs2 = idPost.ToString();

            var post = ForumDataProvider.GetPostByID(TenantProvider.CurrentTenantID, idPost);
            if (post == null)
            {
                resp.rs1 = "0";
                resp.rs3 = Resources.ForumUCResource.ErrorAccessDenied;
                return resp;
            }

            if (!_forumManager.ValidateAccessSecurityAction(ForumAction.ApprovePost, post))
            {
                resp.rs1 = "0";
                resp.rs3 = Resources.ForumUCResource.ErrorAccessDenied;
                return resp;
            }

            try
            {
                ForumDataProvider.ApprovePost(TenantProvider.CurrentTenantID, post.ID);
                resp.rs1 = "1";
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs3 = HttpUtility.HtmlEncode(e.Message);
            }          
            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse DoDeleteAttachment(int idAttachment, int idPost, Guid settingsID)
        {
            _forumManager = ForumManager.GetForumManager(settingsID);
            AjaxResponse resp = new AjaxResponse();
            resp.rs2 = idAttachment.ToString();

            var post = ForumDataProvider.GetPostByID(TenantProvider.CurrentTenantID, idPost);
            if (post == null)
            {
                resp.rs1 = "0";
                resp.rs3 = Resources.ForumUCResource.ErrorAccessDenied;
                return resp;
            }

            if (!_forumManager.ValidateAccessSecurityAction(ForumAction.AttachmentDelete, post))
            {
                resp.rs1 = "0";
                resp.rs3 = Resources.ForumUCResource.ErrorAccessDenied;
                return resp;
            }
            
            try
            {
                var attachment = post.Attachments.Find(a => a.ID == idAttachment);
                if (attachment != null)
                {
                    ForumDataProvider.RemoveAttachment(TenantProvider.CurrentTenantID, attachment.ID);
                    _forumManager.RemoveAttachments(attachment.OffsetPhysicalPath);


                }

                resp.rs1 = "1";
                resp.rs3 = Resources.ForumUCResource.SuccessfullyDeleteAttachmentMessage;
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs3 = HttpUtility.HtmlEncode(e.Message);
            }

            return resp;
        }       
    }
}