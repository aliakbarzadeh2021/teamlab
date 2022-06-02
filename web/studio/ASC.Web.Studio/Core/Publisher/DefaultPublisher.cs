using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using ASC.Core;
using ASC.Core.Common.Publisher;
using ASC.Core.Users;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core.Publisher.Greetings;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Core.Publisher
{
    public class DefaultPublisher
        : IPublisher
    {

        public DefaultPublisher()
        {
            Version = new Version("0.1");
            PublisherVersion = new Version("0.1");
        }

        #region IPublisher

        public void Initialize(Hashtable properties)
        {
            Properties = properties ?? new Hashtable();
        }

        public List<Article> HandleRequest(RequestContext context, List<Zone> visibleZones)
        {
            List<Article> result = new List<Article>();

            foreach (var zone in visibleZones)
            {
                if (zone == null) continue;
                if (String.Compare(zone.ID, ASC.Core.Common.Publisher.Constants.GreetingZoneID, true) == 0)
                {
                    bool isOneUser = false;
                    if (!SecurityContext.DemoMode)
                    {
                        var users = CoreContext.UserManager.GetUsers(EmployeeStatus.Active);
                        isOneUser = (users.Length == 1);
                    }
                    
                    string html = "";
                    if (isOneUser)
                    {
                        html = String.Format("<div class='RandomGreetingMain'><span class='phrase'>{0}</span><span class='description'>{1}</span></div>",
                        String.Format(Resources.Resource.WelcomeUserMessage, CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).DisplayUserName(true)),
                        String.Format(Resources.Resource.GreetingInviteUserMessage, 
                                      string.Format("<a href=\"{0}\">",CommonLinkUtility.GetEmployees(context.ProductID)),
                                      "</a>"));
                    }
                    else
                    {
                        Greeting greeting = StupidRandomWelcome.DefaultInstance.GetRandomGreeting();

                        html = String.Format("<div class='RandomGreetingMain'><span class='phrase'>{0}</span><span class='description'>{1}</span></div>",
                            HttpUtility.HtmlEncode(greeting.Phrase),
                            HttpUtility.HtmlEncode(greeting.Description));
                    }

                    Article article = new Article(zone, (isOneUser) ? ArticleType.Promo : ArticleType.Info, html);
                    result.Add(article);
                }
                else if (String.Compare(zone.ID, ASC.Core.Common.Publisher.Constants.ProductDashboardZoneID, true) == 0)
                {
                    
                }
                else if (String.Compare(zone.ID, ASC.Core.Common.Publisher.Constants.TopBannerZoneID, true) == 0)
                {
                    
                }
                else if (String.Compare(zone.ID, ASC.Core.Common.Publisher.Constants.UnderProfileZoneID, true) == 0)
                {
                    
                }
            }

            return result;
        }

        public Hashtable Properties { get; internal set; }

        public Version PublisherVersion { get; internal set; }

        public Version Version { get; internal set; }

        #endregion
    }
}
