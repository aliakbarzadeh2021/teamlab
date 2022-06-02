using System;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Security.Cryptography;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Masters;
using ASC.Web.Studio.UserControls.Management;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio
{
    //  emp-invite - confirm ivite by email
    //  portal-suspend - confirm portal suspending - Tenant.SetStatus(TenantStatus.Suspended)
    //  portal-continue - confirm portal continuation  - Tenant.SetStatus(TenantStatus.Active)
    //  portal-remove - confirm portal deletation - Tenant.SetStatus(TenantStatus.RemovePending)
    //  DnsChange - change Portal Address and/or Custom domain name
    public enum ConfirmType
    {
        EmpInvite,
        PortalSuspend,
        PortalContinue,
        PortalRemove,
        DnsChange,
        PortalOwnerChange
    }

    public partial class confirm : MainPage
    {
        protected TenantInfoSettings _tenantInfoSettings;

        public string ErrorMessage { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            ((IStudioMaster)this.Master).DisabledSidePanel = true;

            this.Page.Title = HeaderStringHelper.GetPageTitle(Resources.Resource.AccountControlPageTitle, null, null);

            var type = typeof(ConfirmType).TryParseEnum<ConfirmType>(Request["type"] ?? "", ConfirmType.EmpInvite);

            _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);

            var email = Request["email"] ?? "";
            var key = Request["key"] ?? "";
            var fap = Request["fap"] ?? "";

            TimeSpan validInterval = TimeSpan.FromDays(2);
            var checkKeyResult = false;

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            if (tenant.Status != TenantStatus.Active && type != ConfirmType.PortalContinue)
            {
                Response.Redirect(SetupInfo.NoTenantRedirectURL, true);
            }

            if (type == ConfirmType.DnsChange)
            {
                var dnsChangeKey = string.Join(string.Empty, new string[] { email.ToLower(), type.ToString().ToLower(), Request["dns"], Request["alias"] });
                checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(dnsChangeKey, key, validInterval);
            }
            else if (type == ConfirmType.PortalContinue)
                checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(email + type.ToString().ToLower(), key);

            else if (type == ConfirmType.EmpInvite && !String.IsNullOrEmpty(fap) && String.Equals(fap, "1"))
                checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(email + type.ToString().ToLower() + "allrights", key, validInterval);

            else if (type == ConfirmType.PortalOwnerChange && !String.IsNullOrEmpty(Request["uid"]))
            {
                Guid uid = Guid.Empty;
                try
                {
                    uid = new Guid(Request["uid"]);
                }
                catch { };
                checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(email + type.ToString().ToLower() + uid.ToString(), key, validInterval);
            }

            else
                checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(email + type.ToString().ToLower(), key, validInterval);



            if (!email.TestEmailRegex() || !checkKeyResult)
            {
                ErrorMessage = Resources.Resource.ErrorConfirmURLError;
                _confirmHolder.Visible = false;
                return;
            }

            switch (type)
            {
                //Invite
                case ConfirmType.EmpInvite:
                    _confirmHolder.Controls.Add(LoadControl(ConfirmInvite.Location));
                    break;



                case ConfirmType.PortalRemove:
                case ConfirmType.PortalSuspend:
                case ConfirmType.PortalContinue:
                case ConfirmType.DnsChange:
                    _confirmHolder.Controls.Add(LoadControl(ConfirmPortalActivity.Location));
                    break;

                case ConfirmType.PortalOwnerChange:
                    _confirmHolder.Controls.Add(LoadControl(ConfirmPortalOwner.Location));
                    break;
            }
        }
    }
}
