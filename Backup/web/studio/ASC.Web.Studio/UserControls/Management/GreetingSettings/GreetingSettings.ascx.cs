using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxPro;
using ASC.Data.Storage;
using ASC.Web.Core.Utility.Skins;
using ASC.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using ASC.Web.Core.Users;
using System.IO;
using ASC.Web.Core.Utility;

namespace ASC.Web.Studio.UserControls.Management
{
    internal class LogoUploader : IFileUploadHandler
    {
        #region IFileUploadHandler Members

        public FileUploadResult ProcessUpload(HttpContext context)
        {
            FileUploadResult result = new FileUploadResult();
            try
            {
                if (context.Request.Files.Count != 0)
                {
                    HttpPostedFile logo = context.Request.Files[0];
                    byte[] data = new byte[logo.InputStream.Length];

                    BinaryReader br = new BinaryReader(logo.InputStream);
                    br.Read(data, 0, (int)logo.InputStream.Length);
                    br.Close();

                    string ap = UserPhotoManager.SaveTempPhoto(data, SetupInfo.MaxImageUploadSize, 250, 100);

                    result.Success = true;
                    result.Message = ap;
                }
                else
                {
                    result.Success = false;
                    result.Message = Resources.Resource.ErrorEmptyUploadFileSelected;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message.HtmlEncode();
            }

            return result;
        }

        #endregion

    }


    [AjaxNamespace("GreetingSettingsController")]
    public partial class GreetingSettings : System.Web.UI.UserControl
    {
        public static string Location { get { return "~/UserControls/Management/GreetingSettings/GreetingSettings.ascx"; } }

        protected TenantInfoSettings _tenantInfoSettings;

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(this.GetType());
            
            this.Page.ClientScript.RegisterClientScriptInclude(GetType(), "ajaxupload", VirtualPathUtility.ToAbsolute("~/js/ajaxupload.3.5.js"));
            Page.ClientScript.RegisterClientScriptInclude(typeof(string), "greetingsettings_script", WebPath.GetPath("usercontrols/management/greetingsettings/js/greetingsettings.js"));
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "greetingsettings_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebSkin.GetUserSkin().GetAbsoluteWebPath("usercontrols/management/greetingsettings/css/<theme_folder>/greetingsettings.css") + "\">", false);

            _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);
        }


        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SaveGreetingSettings(string logoVP, string header)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);                

                if (!String.IsNullOrEmpty(logoVP))
                {
                    var fileName = Path.GetFileName(logoVP);
                    var data = UserPhotoManager.GetTempPhotoData(fileName);
                    _tenantInfoSettings.SetCompanyLogo(fileName, data);

                    try
                    {
                        UserPhotoManager.RemoveTempPhoto(fileName);
                    }
                    catch { };
                }

                var currentTenant = CoreContext.TenantManager.GetCurrentTenant();
                currentTenant.Name = header;
                CoreContext.TenantManager.SaveTenant(currentTenant);

                SettingsManager.Instance.SaveSettings<TenantInfoSettings>(_tenantInfoSettings, TenantProvider.CurrentTenantID);              

                return new { Status = 1, Message = Resources.Resource.SuccessfullySaveGreetingSettingsMessage};

            }
            catch (Exception e)
            {
                return new { Status = 0, Message = e.Message.HtmlEncode() };
            }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object RestoreGreetingSettings()
        {            
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);
                _tenantInfoSettings.RestoreDefault();
                SettingsManager.Instance.SaveSettings<TenantInfoSettings>(_tenantInfoSettings, TenantProvider.CurrentTenantID);

                return new {Status = 1, Message =  Resources.Resource.SuccessfullySaveGreetingSettingsMessage,
                            CompanyName = _tenantInfoSettings.CompanyName, LogoPath = _tenantInfoSettings.GetAbsoluteCompanyLogoPath()};                     

            }
            catch (Exception e)
            {
                return new { Status = 0, Message = e.Message.HtmlEncode() };
            }
        }
    }
}