using System;
using System.Text.RegularExpressions;
using ASC.Notify;
using ASC.Notify.Engine;
using ASC.Notify.Messages;
using ASC.Notify.Patterns;
using ASC.Web.Studio.Utility;
using ASC.Web.Core;
using ASC.Core;
using ASC.Core.Users;
using ASC.Core.Tenants;

namespace ASC.Web.Studio.Core.Notify
{
    class NotifyConfiguration
    {
        static Regex urlReplacer = new Regex(@"(<a [^>]*href=(('(?<url>[^>']*)')|(""(?<url>[^>""]*)""))[^>]*>)|(<img [^>]*src=(('(?<url>[^>']*)')|(""(?<url>[^>""]*)""))[^/>]*/?>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        static object GetCurrentProductUrl()
        {
            var product = ProductManager.Instance[CommonLinkUtility.GetProductID()];
            if (product == null)
                return CommonLinkUtility.GetFullAbsolutePath("~");
            else
                return product.StartURL;
        }

        static object GetRecipientSubscriptionConfigURL()
        {
            return CommonLinkUtility.GetFullAbsolutePath(
                        CommonLinkUtility.GetMyStaff(MyStaffType.Subscriptions));
            //Context.SYS_RECIPIENT_ID,
            //CommonLinkUtility.GetProductID(),
            //UserProfileType.Subscriptions));
        }


        static ITagValue[] _CommonTags = new ITagValue[] { 
            //__VirtualRootPath
            new DynamicTagValue(CommonTags.VirtualRootPath, ()=>(CommonLinkUtility.GetFullAbsolutePath("~")??"").TrimEnd('/')), 
            //__ProductID
            new DynamicTagValue(CommonTags.ProductID,()=>CommonLinkUtility.GetProductID()),
            //__ProductUrl
            new DynamicTagValue(CommonTags.ProductUrl,GetCurrentProductUrl),
            //__DateTime
            new DynamicTagValue(CommonTags.DateTime,()=>TenantUtil.DateTimeNow()),

            new DynamicTagValue(CommonTags.Helper,()=>new PatternHelper()),
            //__AuthorID
            //          add to NotifyEngine_BeforeTransferRequest
            //__AuthorName
            //          add to NotifyEngine_BeforeTransferRequest
            //__AuthorUrl
            //          add to NotifyEngine_BeforeTransferRequest
            //__RecipientID
            new TagValue(CommonTags.RecipientID,Context.SYS_RECIPIENT_ID),
            //RecipientSubscriptionConfigURL
            new DynamicTagValue(CommonTags.RecipientSubscriptionConfigURL, GetRecipientSubscriptionConfigURL)
        };


        public static void NotifyClientRegisterCallback(Context context, INotifyClient client)
        {
            client.SetStaticTags(_CommonTags);

            client.AddInterceptor(
                    new SendInterceptorSkeleton(
                        "Web.UrlAbsoluter",           
                        InterceptorPlace.MessageSend, 
                        InterceptorLifetime.Global,   
                        (nreq, place) =>
                        {
                            if (nreq != null && nreq.CurrentMessage != null && nreq.CurrentMessage.ContentType == Pattern.HTMLContentType)
                                DoNotifyRequestAbsoluteUrl(nreq.CurrentMessage);
                            return false;
                        }
                        )
                    );


        }


        internal static void Configure()
        {
            WorkContext.NotifyContext.NotifyClientRegistration += NotifyConfiguration.NotifyClientRegisterCallback;

            WorkContext.NotifyContext.NotifyEngine.BeforeTransferRequest += NotifyEngine_BeforeTransferRequest;
            WorkContext.NotifyContext.NotifyEngine.AfterTransferRequest += NotifyEngine_AfterTransferRequest;

            //Register what's new
            StudioNotifyService.Instance.RegisterSendMethod();
        }

        static void NotifyEngine_AfterTransferRequest(ASC.Notify.Engine.NotifyEngine sender, ASC.Notify.Engine.NotifyRequest request)
        {
            Guid productID = (Guid)request.Properties["asc.web.product_id"];
            System.Runtime.Remoting.Messaging.CallContext.SetData("asc.web.product_id", productID);
        }

        static void NotifyEngine_BeforeTransferRequest(ASC.Notify.Engine.NotifyEngine sender, ASC.Notify.Engine.NotifyRequest request)
        {
            request.Properties.Add("asc.web.product_id", CommonLinkUtility.GetProductID());

            Guid aid = Guid.Empty;
            string aname = "";
            if (SecurityContext.IsAuthenticated)
            {
                aid = SecurityContext.CurrentAccount.ID;
                if (CoreContext.UserManager.UserExists(aid))
                    aname = CoreContext.UserManager.GetUsers(aid).DisplayUserName();
            }

            //__AuthorID
            request.Arguments.Add(new TagValue(CommonTags.AuthorID, aid));
            //__AuthorName
            request.Arguments.Add(new TagValue(CommonTags.AuthorName, aname));
            //__AuthorUrl
            request.Arguments.Add(new TagValue(CommonTags.AuthorUrl, CommonLinkUtility.GetUserProfile(aid, CommonLinkUtility.GetProductID())));
        }


        static void DoNotifyRequestAbsoluteUrl(INoticeMessage msg)
        {
            var body = msg.Body;

            
            body = urlReplacer.Replace(body, (m =>
            {
                var url = m.Groups["url"].Value ?? "";
                var ind = m.Groups["url"].Index - m.Index;
                if (String.IsNullOrEmpty(url) && ind > 0)
                    return (m.Value ?? "").Insert(ind, CommonLinkUtility.GetFullAbsolutePath(""));
                else
                    return (m.Value ?? "").Replace(url, CommonLinkUtility.GetFullAbsolutePath(url));
            }));

            msg.Body = body;
        }
    }
}
