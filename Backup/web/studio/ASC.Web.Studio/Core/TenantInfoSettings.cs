using System;
using System.IO;
using ASC.Data.Storage;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Utility;
using ASC.Core;
using System.Drawing;

namespace ASC.Web.Studio.Core
{
    [Serializable]
    public class TenantInfoSettings : ISettings
    {        
        public string CompanyName
        {
            get
            {
                return CoreContext.TenantManager.GetCurrentTenant().Name;

            }
        }
        public string CompanyLogoFileName { get; set; }

        public Size CompanyLogoSize { get; private set; }

        private string _companyLogoFileName;

        private bool _isDefault { get; set; }

        #region ISettings Members

        public ISettings GetDefault()
        {
            return new TenantInfoSettings()
            {
                //CompanyName = Resources.Resource.StudioWelcomeHeader,
                _isDefault = true
            };
        }

        public void RestoreDefault()
        {
            _isDefault = true;

            var currentTenant = CoreContext.TenantManager.GetCurrentTenant();
            currentTenant.Name = Resources.Resource.StudioWelcomeHeader;
            CoreContext.TenantManager.SaveTenant(currentTenant);

            var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "logo");
            try { store.DeleteFiles("", "*", false); }
            catch { };
        }

        public void SetCompanyLogo(string companyLogoFileName, byte[] data)
        {
            var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "logo");
            this.CompanyLogoFileName = companyLogoFileName;

            if (!_isDefault)
            {
                try { store.DeleteFiles("", "*", false); }
                catch { };
            }
            var memory = new MemoryStream(data);
            this.CompanyLogoSize = Image.FromStream(memory).Size;
            memory.Seek(0, SeekOrigin.Begin);

            Uri uri = store.Save(companyLogoFileName, memory);
            _companyLogoFileName = companyLogoFileName;

            _isDefault = false;
           
        }

        public string GetAbsoluteCompanyLogoPath()
        {
            if (_isDefault)
                return WebImageSupplier.GetAbsoluteWebPath("logo.png");
            else
            {
                var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "logo");
                return store.GetUri(_companyLogoFileName??"").ToString();
            }
            
        }

        public Guid ID
        {
            get { return new Guid("{5116B892-CCDD-4406-98CD-4F18297C0C0A}"); }
        }

        #endregion
    }
}
