using System;
using System.Collections.Generic;
using System.Web.Configuration;
using ASC.Common.Data;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Projects.Data;
using ASC.Projects.Engine;
using ASC.Web.Core;
using ASC.Web.Core.Users.Activity;
using ASC.Web.Files.Api;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Controls.Projects;
using ASC.Web.Projects.Resources;
using log4net;
using ConfigurationManager = ASC.Projects.Engine.ConfigurationManager;

namespace ASC.Web.Projects.Configuration
{
    public class ProductEntryPoint : AbstractProduct
    {
        private ProductContext context;

        private List<NavigationWebItem> navigationsItems = new List<NavigationWebItem>();

        public new static readonly Guid ID = new Guid("1E044602-43B5-4d79-82F3-FD6208A11960");


        public override Guid ProductID
        {
            get { return ID; }
        }

        public override string ProductName
        {
            get { return ProjectsCommonResource.ProductName; }
        }

        public override string ProductDescription
        {
            get { return ProjectsCommonResource.ProductDescription; }
        }

        public override string StartURL
        {
            get
            {
                try
                {
                    return string.Concat(PathProvider.BaseAbsolutePath, Global.IsAdmin || !SecurityContext.IsAuthenticated ? "default.aspx" : "mytasks.aspx");
                }
                catch
                {
                    return ProjectsCommonResource.StartURL.ToLower();
                }
            }
        }

        public override IModule[] Modules
        {
            get { return Global.ModuleManager.Modules.ToArray(); }
        }

        public override ProductContext Context
        {
            get { return context; }
        }

        public string ModuleDescription
        {
            get { return string.Empty; }
        }

        public Guid ModuleID
        {
            get { return ID; }
        }

        public string ModuleName
        {
            get { return ProjectsCommonResource.ModuleName; }
        }


        public override void Init(ProductContext productContext)
        {
            if (!DbRegistry.IsDatabaseRegistered(Global.DB_ID))
                DbRegistry.RegisterDatabase(Global.DB_ID, WebConfigurationManager.ConnectionStrings[Global.DB_ID]);

            new SearchHandler();

            JavaScriptResourcePublisher.Execute(PathProvider.JsPhysicalResourcesPath, "ASC.Projects.Resources", typeof(ProjectsJSResource));

            ConfigurationManager.Configure(ID, PathProvider.BaseVirtualPath, String.Empty, Global.FileStorageModule);

            productContext.ThemesFolderVirtualPath = String.Concat(PathProvider.BaseVirtualPath, "App_Themes");
            productContext.ImageFolder = "images";
            productContext.GlobalHandler = new GlobalHandler();
            productContext.MasterPageFile = String.Concat(PathProvider.BaseVirtualPath, "Masters/BasicTemplate.Master");
            productContext.IconFileName = "product_logo.png";
            productContext.LargeIconFileName = "product_logolarge.png";
            productContext.SubscriptionManager = new ProductSubscriptionManager();
            productContext.UserActivityControlLoader = new ProjectActivity();
            productContext.WhatsNewHandler = new WhatsNewHandler();
            productContext.UserActivityPublishers = new List<IUserActivityPublisher>() { new TimeLinePublisher() };
            productContext.DefaultSortOrder = 20;
            productContext.SpaceUsageStatManager = new ProjectsSpaceUsageStatManager();

            context = productContext;

            NotifyClient.Instance.Client.RegisterSendMethod(SendMsgMilestoneDeadline, TimeSpan.FromDays(1));
            NotifyClient.Instance.Client.RegisterSendMethod(ReportHelper.SendAutoReports, "0 0 * ? * *");
            NotifyClient.Instance.Client.RegisterSendMethod(TaskHelper.SendAutoReminderAboutTask, TimeSpan.FromHours(1), DateTime.UtcNow.Date.AddHours(DateTime.UtcNow.Hour));

            UserActivityManager.AddFilter(new WhatsNewHandler());

            FilesIntegration.RegisterFileSecurityProvider("projects", "project", new FileEngine2());
        }

        public override void Shutdown()
        {
            NotifyClient.Instance.Client.UnregisterSendMethod(SendMsgMilestoneDeadline);
            NotifyClient.Instance.Client.UnregisterSendMethod(ReportHelper.SendAutoReports);
            NotifyClient.Instance.Client.UnregisterSendMethod(TaskHelper.SendAutoReminderAboutTask);


        }

        private void SendMsgMilestoneDeadline(DateTime scheduleDate)
        {
            var date = DateTime.UtcNow.AddDays(2).Date;
            foreach (var r in new DaoFactory(Global.DB_ID, Tenant.DEFAULT_TENANT).GetMilestoneDao().GetInfoForReminder(date))
            {
                var tenant = CoreContext.TenantManager.GetTenant((int)r[0]);
                if (tenant == null || tenant.Status != TenantStatus.Active) continue;

                var localTime = TenantUtil.DateTimeFromUtc(tenant, date);
                if (localTime.Date == ((DateTime)r[2]).Date)
                {
                    try
                    {
                        CoreContext.TenantManager.SetCurrentTenant(tenant);
                        var m = new DaoFactory(Global.DB_ID, tenant.TenantId).GetMilestoneDao().GetById((int)r[1]);
                        if (m != null)
                        {
                            NotifyClient.Instance.SendMilestoneDeadline(m.Project.Responsible, m);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetLogger("ASC.Projects.Tasks").Error("SendMsgMilestoneDeadline", ex);
                    }
                }
            }
        }
    }
}