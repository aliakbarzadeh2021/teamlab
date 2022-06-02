using System;
using System.ServiceModel;
using ASC.Web.Studio.Core;
using Microsoft.ServiceModel.Web;

namespace ASC.Web.Files.Services.WCFService
{
    class ServiceFactory : WebServiceHost2Factory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            var service = (WebServiceHost2)base.CreateServiceHost(serviceType, baseAddresses);
            service.MaxMessageSize = SetupInfo.MaxUploadSize != 0 ? SetupInfo.MaxUploadSize : 25 * 1024 * 1024;
            return service;
        }
    }
}
