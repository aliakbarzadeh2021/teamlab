using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Data.Storage;
using ASC.Web.Core.Utility.Skins;
using ASC.Core;
using ASC.Core.Users;


namespace ASC.Web.Studio.UserControls.Common
{
    public enum PremiumFeatureType
    { 
        PrivateProjects = 0,
        ManageAccessRights = 1,
        SharePersonalDocs =2

    }
    public partial class PremiumStub : System.Web.UI.UserControl
    {
        public PremiumFeatureType Type { get; set; }
        
        protected string _text;

        protected string _projText;
        protected string _accessRightsText;
        protected string _sharedText;

        public static string  Location{get{return "~/UserControls/Common/PremiumStub/PremiumStub.ascx";}}

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.ClientScript.RegisterClientScriptInclude(typeof(string), "premiumstub_script", WebPath.GetPath("usercontrols/common/premiumstub/js/premiumstub.js"));
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "premiumstub_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebSkin.GetUserSkin().GetAbsoluteWebPath("usercontrols/common/premiumstub/css/<theme_folder>/premiumstub.css") + "\">", false);

            _stubContainer.Options.IsPopup = true;


            _projText = Resources.Resource.PremiumPrivateProjectLimitation;
            _accessRightsText = Resources.Resource.PremiumAccessRightsLimitation;
            _sharedText = Resources.Resource.PremiumShareDocsLimitation;

            switch (Type)
            {
                case PremiumFeatureType.ManageAccessRights:
                    _text = Resources.Resource.PremiumAccessRightsLimitation;
                    break;
                case PremiumFeatureType.PrivateProjects:
                    _text = Resources.Resource.PremiumPrivateProjectLimitation;
                    break;
                case PremiumFeatureType.SharePersonalDocs:
                    _text = Resources.Resource.PremiumShareDocsLimitation;
                    break;
            }
        }
    }
}