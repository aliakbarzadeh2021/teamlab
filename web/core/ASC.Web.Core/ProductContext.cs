using ASC.Common.Security;
using ASC.Web.Core.Subscriptions;
using ASC.Web.Core.Users.Activity;

namespace ASC.Web.Core
{
    public class ProductContext : WebItemContext
    {
        public ISecurityObject AdminSecurityObject { get; set; }

        public string MasterPageFile { get; set; }

        public string ProductHTMLOverview {get; set;}

        public IUserActivityControlLoader UserActivityControlLoader { get; set; }

        public new IProductSubscriptionManager SubscriptionManager { get; set; }

		public IWhatsNewHandler WhatsNewHandler { get; set; }

    }
    
}
