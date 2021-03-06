using System;
using System.Collections.Generic;
using System.Globalization;

namespace ASC.Core.Tenants
{
    [Serializable]
    public class Tenant
    {
        public const int DEFAULT_TENANT = -1;


        public Tenant()
        {
            TenantId = DEFAULT_TENANT;
            TimeZone = TimeZoneInfo.Local;
            Language = CultureInfo.CurrentCulture.Name;
            TrustedDomains = new List<string>();
            TrustedDomainsType = TenantTrustedDomainsType.None;
            CreatedDateTime = DateTime.UtcNow;
            Status = TenantStatus.Active;
            StatusChangeDate = DateTime.UtcNow;
            Public = false;
            PublicVisibleProducts = new List<string>();
        }

        public Tenant(string alias)
            : this()
        {
            TenantAlias = alias.ToLowerInvariant();
        }

        internal Tenant(int id, string alias)
            : this(alias)
        {
            TenantId = id;
        }


        public int TenantId { get; internal set; }

        public string TenantAlias { get; set; }

        public string MappedDomain { get; set; }

        public string TenantDomain { get; internal set; }

        public string Name { get; set; }

        public string Language { get; set; }

        public TimeZoneInfo TimeZone { get; set; }

        public List<string> TrustedDomains { get; private set; }

        public TenantTrustedDomainsType TrustedDomainsType { get; set; }

        public Guid OwnerId { get; set; }

        public DateTime CreatedDateTime { get; internal set; }

        public CultureInfo GetCulture() { return CultureInfo.GetCultureInfo(Language); }

        public DateTime LastModified { get; set; }

        public TenantStatus Status { get; internal set; }

        public DateTime StatusChangeDate { get; internal set; }

        public void SetStatus(TenantStatus status)
        {
            Status = status;
            StatusChangeDate = DateTime.UtcNow;
        }

        public bool Public
        {
            get;
            set;
        }

        public List<string> PublicVisibleProducts
        {
            get;
            internal set;
        }


        public override bool Equals(object obj)
        {
            var t = obj as Tenant;
            return t != null && t.TenantId == TenantId;
        }

        public override int GetHashCode()
        {
            return TenantId;
        }

        public override string ToString()
        {
            return TenantDomain ?? TenantAlias;
        }


        internal string GetTrustedDomains()
        {
            TrustedDomains.RemoveAll(d => string.IsNullOrEmpty(d));
            if (TrustedDomains.Count == 0) return null;
            return string.Join("|", TrustedDomains.ToArray());
        }

        internal void SetTrustedDomains(string trustedDomains)
        {
            if (string.IsNullOrEmpty(trustedDomains))
            {
                TrustedDomains.Clear();
            }
            else
            {
                TrustedDomains.AddRange(trustedDomains.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }
    }
}