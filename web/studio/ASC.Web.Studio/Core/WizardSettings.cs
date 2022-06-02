using System;
using ASC.Core;
using ASC.Web.Core.Utility.Settings;

namespace ASC.Web.Studio.Core
{
	[Serializable]
	public class WizardSettings : ISettings
	{
        public bool Completed { get; set; }

		public Guid ID
		{
			get { return new Guid("{9A925891-1F92-4ed7-B277-D6F649739F06}"); }
		}


        public ISettings GetDefault()
		{
			return new WizardSettings()
			{
                Completed = CoreContext.TenantManager.GetCurrentTenant().TenantAlias != "localhost" || CoreContext.TenantManager.GetCurrentTenant().TenantId != 0
			};
		}
	}
}
