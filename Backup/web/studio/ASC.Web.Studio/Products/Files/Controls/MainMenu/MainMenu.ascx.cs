using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Controls.Common;
using ASC.Core;
using ASC.Web.Studio.UserControls.Common;

namespace ASC.Web.Files.Controls
{
    public partial class MainMenu : UserControl
    {
        public static string Location { get { return Files.Classes.PathProvider.GetFileStaticRelativePath("MainMenu/MainMenu.ascx"); } }

        public bool EnableShare = ASC.Web.Files.Classes.Global.EnableShare && ASC.Core.SecurityContext.IsAuthenticated;

        protected void Page_Load(object sender, EventArgs e)
        {
            _uploadSwitchHolder.Controls.Add(new FileUploaderModeSwitcher());
            uploadDialog.Options.IsPopup = true;

            var _securityEnable = CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId).SecurityEnable;
            if (!_securityEnable)
            {
                var stub = LoadControl(PremiumStub.Location) as PremiumStub;
                stub.Type = PremiumFeatureType.ManageAccessRights;
                _premiumStubHolder.Controls.Add(stub);
            }
        }
    }
}