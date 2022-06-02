﻿using System;
using System.Text;
using System.Web;
using AjaxPro;
using ASC.Blogs.Core.Resources;
using ASC.Core;
using ASC.Notify.Model;
using ASC.Notify.Recipients;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Core;

namespace ASC.Web.Community.Blogs
{
    [AjaxNamespace("Subscriber")]
    public class Subscriber 
    {
        private ISubscriptionProvider _subscriptionProvider;
        private IRecipientProvider _recipientProvider; 


        public IDirectRecipient IAmAsRecipient
        {
            get
            {

                return (IDirectRecipient)_recipientProvider.GetRecipient(SecurityContext.CurrentAccount.ID.ToString());
            }
        }

        
        public Subscriber()
        {
            var engine = BasePage.GetEngine();
            _subscriptionProvider = engine.NotifySource.GetSubscriptionProvider();
            _recipientProvider = engine.NotifySource.GetRecipientsProvider();
        }

        #region Comments
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SubscribeOnComments(Guid postID, int statusNotify)
        {
            AjaxResponse resp = new AjaxResponse();
            try
            {   
                if (statusNotify == 1)
                {
                    _subscriptionProvider.Subscribe(
                             ASC.Blogs.Core.Constants.NewComment,           
                             postID.ToString(),                             
                             IAmAsRecipient                                 
                        );
                   

                    resp.rs1 = "1";
                    resp.rs2 = RenderCommentsSubscription(false, postID);
                }
                else
                {
                    
                    _subscriptionProvider.UnSubscribe(
                             ASC.Blogs.Core.Constants.NewComment,           
                             postID.ToString(),                             
                             IAmAsRecipient                                 
                        );

                    resp.rs1 = "1";
                    resp.rs2 = RenderCommentsSubscription(true, postID);
                }
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs2 = HttpUtility.HtmlEncode(e.Message);
            }

            return resp;
        }
        

        public string RenderCommentsSubscription(bool isSubscribe, Guid postID)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<div id=\"blogs_subcribeOnCommentsBox\">");

            sb.AppendFormat("<a class='linkAction"+(SetupInfo.WorkMode == WorkMode.Promo?" promoAction":"")+"' title='{3}' href=\"#\" onclick=\"BlogSubscriber.SubscribeOnComments('{0}', {1});\">{2}</a>",
                postID,
                (isSubscribe ? 1 : 0),
                (!isSubscribe ? BlogsResource.UnSubscribeOnNewCommentsAction : BlogsResource.SubscribeOnNewCommentsAction),
                BlogsResource.SubscribeOnNewCommentsDescription);
            
            sb.Append("</div>");

            return sb.ToString();
        }

      
        public bool IsCommentsSubscribe(Guid postID)
        {
            return
                _subscriptionProvider.IsSubscribed(
                    ASC.Blogs.Core.Constants.NewComment,
                    IAmAsRecipient,
                    postID.ToString());
        }


        #endregion

        #region PersonalBlog
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SubscribeOnPersonalBlog(Guid userID, int statusNotify)
        {
            AjaxResponse resp = new AjaxResponse();
            try
            {
                if (statusNotify == 1)
                {
                    _subscriptionProvider.Subscribe(
                             ASC.Blogs.Core.Constants.NewPostByAuthor,         
                             userID.ToString(),                                
                             IAmAsRecipient                                 
                        );


                    resp.rs1 = "1";
                    resp.rs2 = RenderPersonalBlogSubscription(false, userID);
                }
                else
                {
                    _subscriptionProvider.UnSubscribe(
                             ASC.Blogs.Core.Constants.NewPostByAuthor,      
                             userID.ToString(),                             
                             IAmAsRecipient                                 
                        );

                    resp.rs1 = "1";
                    resp.rs2 = RenderPersonalBlogSubscription(true, userID);
                }
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs2 = HttpUtility.HtmlEncode(e.Message);
            }

            return resp;
        }


        public string RenderPersonalBlogSubscription(bool isSubscribe, Guid userID)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<div id=\"blogs_subcribeOnPersonalBlogBox\">");
            sb.AppendFormat("<a class='linkAction" + (SetupInfo.WorkMode == WorkMode.Promo ? " promoAction" : "") + "' title='{3}' href=\"#\" onclick=\"BlogSubscriber.SubscribeOnPersonalBlog('{0}', {1});\">{2}</a>",
                userID, 
                (isSubscribe ? 1 : 0), 
                (!isSubscribe ? BlogsResource.UnSubscribeOnAuthorAction : BlogsResource.SubscribeOnAuthorAction),
                BlogsResource.SubscribeOnAuthorDescription);
            sb.Append("</div>");

            return sb.ToString();
        }

        public bool IsPersonalBlogSubscribe(Guid userID)
        {
            return 
                _subscriptionProvider.IsSubscribed(
                    ASC.Blogs.Core.Constants.NewPostByAuthor,
                    IAmAsRecipient,
                    userID.ToString());
        }


        #endregion

        #region NewPosts
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SubscribeOnNewPosts(int statusNotify)
        {
            AjaxResponse resp = new AjaxResponse();
            try
            {
                if (statusNotify == 1)
                {
                    _subscriptionProvider.Subscribe(
                             ASC.Blogs.Core.Constants.NewPost,
                             null,
                             IAmAsRecipient
                        );


                    resp.rs1 = "1";
                    resp.rs2 = RenderNewPostsSubscription(false);
                }
                else
                {
                    _subscriptionProvider.UnSubscribe(
                             ASC.Blogs.Core.Constants.NewPost,
                             null,
                             IAmAsRecipient
                        );

                    resp.rs1 = "1";
                    resp.rs2 = RenderNewPostsSubscription(true);
                }
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs2 = HttpUtility.HtmlEncode(e.Message);
            }

            return resp;
        }


        public string RenderNewPostsSubscription(bool isSubscribe)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<div id=\"blogs_subcribeOnNewPostsBox\">");
            sb.AppendFormat("<a class='linkAction" + (SetupInfo.WorkMode == WorkMode.Promo ? " promoAction" : "") + "' title='{2}' href=\"#\" onclick=\"BlogSubscriber.SubscribeOnNewPosts({0});\">{1}</a>",
                (isSubscribe ? 1 : 0),
                (!isSubscribe ? ASC.Blogs.Core.Resources.BlogsResource.UnSubscribeOnNewPostAction : ASC.Blogs.Core.Resources.BlogsResource.SubscribeOnNewPostAction),
                ASC.Blogs.Core.Resources.BlogsResource.SubscribeOnNewPostDescription);
            sb.Append("</div>");

            return sb.ToString();
        }

        public bool IsNewPostsSubscribe()
        {
            return
               _subscriptionProvider.IsSubscribed(
                   ASC.Blogs.Core.Constants.NewPost,
                   IAmAsRecipient,
                   null);
        }


        #endregion

        
    }
}
