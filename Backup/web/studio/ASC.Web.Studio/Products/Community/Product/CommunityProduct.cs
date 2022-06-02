using System;
using System.Collections.Generic;
using System.Web;
using ASC.Web.Core;
using ASC.Web.Core.ModuleManagement;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core.Users;

namespace ASC.Web.Community.Product
{
    public class CommunityProduct : AbstractProduct
    {
        public new static Guid ID
        {
            get
            {
                return new Guid("{EA942538-E68E-4907-9394-035336EE0BA8}");
            }
        }

        internal static ModuleManager ModuleManager { get; private set; }

        private ProductContext _context;

        #region IProduct Members

        public override void Init(ProductContext context)
        {
            ModuleManager = new ModuleManager(HttpContext.Current.Server.MapPath("~/products/community/modules"), "module.xml");
            ModuleManager.LoadModules();

            var globalHandlers = new List<IGlobalHandler>(0);
            context.GlobalHandler = new GlobalHandlerComposite(globalHandlers);

            foreach (var module in ModuleManager.Modules)
            {
                if (module.GlobalHandler != null) globalHandlers.Add(module.GlobalHandler);
            }

            context.MasterPageFile = "~/products/community/community.master";
            context.ImageFolder = "images";
            context.ThemesFolderVirtualPath = "~/products/community/app_themes";
            context.IconFileName = "product_logo.png";
            context.LargeIconFileName = "product_logolarge.png";

            context.UserActivityControlLoader = new UserActivityControlLoader();
            context.DefaultSortOrder = 10;

            context.SubscriptionManager = new CommunitySubscriptionManager();

            context.WhatsNewHandler = new CommunityWhatsNewHandler();
            context.SpaceUsageStatManager = new CommunitySpaceUsageStatManager();

            this._context = context;

        }

        public override IModule[] Modules
        {
            get
            {
                return ModuleManager.Modules.ToArray();
            }
        }

        public override string ProductDescription
        {
            get { return Resources.CommunityResource.ProductDescription; }
        }

        public override Guid ProductID
        {
            get { return CommunityProduct.ID; }
        }

        public override string ProductName
        {
            get { return Resources.CommunityResource.ProductName; }
        }

        public override void Shutdown()
        {
        }

        public override string StartURL
        {
            get { return "~/products/community/"; }
        }

        public override ProductContext Context
        {
            get { return this._context; }
        }

        #endregion
    }
}
