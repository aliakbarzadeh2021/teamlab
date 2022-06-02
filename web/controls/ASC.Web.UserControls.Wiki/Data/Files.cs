using System;
using System.Collections.Generic;
using System.Web;
using ASC.Core.Tenants;

namespace ASC.Web.UserControls.Wiki.Data
{
    public class Files : BaseContainer
    {
        private string _FileName = string.Empty;
        public string FileName
        {
            get
            {
                return _FileName;
            }
            set
            {
                _FileName = value.Replace("[", "(").Replace("]", ")");
            }
        }

        public string UploadFileName { get; set; }
        public string FileLocation { get; set; }
        public int FileSize { get; set; }

        public Files()
        {
            UserID = Guid.Empty;
            OwnerID = Guid.Empty;
            UploadFileName = FileLocation = string.Empty;
            Version = FileSize= 0;
            Date = TenantUtil.DateTimeNow();
            Tenant = 0;
        }

        public override object GetObjectId()
        {
            return FileName;
        }
        
    }
}
