using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using ASC.Common.Data.Sql;
using ASC.Core.Caching;
using ASC.Core.Data;
using log4net;

namespace ASC.Core.Billing
{
    class TariffService : DbBaseService, ITariffService
    {
        private readonly static ILog log = LogManager.GetLogger(typeof(TariffService));
        private readonly IQuotaService quotaService;
        private readonly ICache cache;


        public TimeSpan CacheExpiration
        {
            get;
            set;
        }


        public TariffService(ConnectionStringSettings connectionString, IQuotaService quotaService)
            : base(connectionString, null)
        {
            this.quotaService = quotaService;
            cache = new AspCache();
            CacheExpiration = TimeSpan.FromMinutes(2);
        }


        public Tariff GetTariff(int tenantId)
        {
            var key = "tariff/" + tenantId;
            var tariff = cache.Get(key) as Tariff;
            if (tariff == null)
            {
                tariff = new Tariff { Plan = TariffPlan.Free, DueDate = DateTime.MaxValue };
                try
                {
                    using (var client = new BillingClient())
                    {
                        var xelement = client.GetLatestActiveResource(tenantId);

                        var productid = xelement.Element("product-id");
                        if (productid != null)
                        {
                            var id = int.Parse(productid.Value);
                            if (id == 1)
                            {
                                tariff.Plan = TariffPlan.Premium;
                            }
                            else
                            {
                                throw new ArgumentOutOfRangeException("Unknown product id: " + id + ".");
                            }
                        }

                        var enddate = xelement.Element("end-date");
                        if (enddate != null)
                        {
                            tariff.DueDate = DateTime.ParseExact(enddate.Value, "yyyy-MM-dd HH:mm:ss", null);
                        }
                    }
                }
                catch (Exception error)
                {
                    log.Error(error);

                    var savedbi = GetBillingInfoFromDb(tenantId);
                    if (savedbi != null)
                    {
                        tariff.Plan = savedbi.Item1;
                        tariff.DueDate = savedbi.Item2;
                    }
                }

                if (tariff.Plan != TariffPlan.Free && tariff.Plan != TariffPlan.Standalone)
                {
                    SaveBillingInfoToDb(tenantId, Tuple.Create(tariff.Plan, tariff.DueDate));
                }
                tariff.Quota = !tariff.Expired ? quotaService.GetTenantQuota((int)tariff.Plan) : quotaService.GetTenantQuota((int)TariffPlan.Free);

                cache.Insert(key, tariff, DateTime.UtcNow.Add(CacheExpiration));
            }

            return tariff;
        }

        public IEnumerable<PaymentInfo> GetPayments(int tenantId)
        {
            var key = "billing/payments/" + tenantId;
            var payments = cache.Get(key) as List<PaymentInfo>;
            if (payments == null)
            {
                payments = new List<PaymentInfo>();
                try
                {
                    using (var client = new BillingClient())
                    {
                        var xelement = client.GetPayments(tenantId);
                        foreach (var x in xelement.Elements("payment"))
                        {
                            var pi = new PaymentInfo();

                            var name = string.Empty;
                            var fname = x.Element("fname");
                            if (fname != null)
                            {
                                name += fname.Value;
                            }
                            var lname = x.Element("lname");
                            if (lname != null)
                            {
                                name += " " + lname.Value;
                            }
                            pi.Name = name.Trim();

                            var email = x.Element("email");
                            if (email != null)
                            {
                                pi.Email = email.Value;
                            }

                            var paymentdate = x.Element("payment-date");
                            if (paymentdate != null)
                            {
                                pi.Date = DateTime.ParseExact(paymentdate.Value, "yyyy-MM-dd HH:mm:ss", null);
                            }

                            var price = x.Element("price");
                            if (price != null)
                            {
                                var separator = CultureInfo.InvariantCulture.NumberFormat.CurrencyDecimalSeparator;
                                pi.Price = Decimal.Parse(price.Value.Replace(".", separator).Replace(",", separator), NumberStyles.Currency, CultureInfo.InvariantCulture);
                            }

                            var currency = x.Element("payment-currency");
                            if (currency != null)
                            {
                                pi.Currency = currency.Value;
                            }

                            var method = x.Element("payment-method");
                            if (method != null)
                            {
                                pi.Method = method.Value;
                            }

                            var cartid = x.Element("cart-id");
                            if (cartid != null)
                            {
                                pi.CartId = cartid.Value;
                            }
                            
                            payments.Add(pi);
                        }
                    }
                }
                catch (Exception error)
                {
                    log.Error(error);
                }

                cache.Insert(key, payments, DateTime.UtcNow.Add(CacheExpiration));
            }

            return payments;
        }

        public Uri GetShoppingUri(int tenant, TariffPlan plan)
        {
            if (plan != TariffPlan.Premium)
            {
                throw new ArgumentOutOfRangeException("Restricted tariff plan: " + plan + ".");
            }

            var paymentUrl = ConfigurationManager.AppSettings["gopaymenturl"];
            if (!string.IsNullOrEmpty(paymentUrl))
            {
                paymentUrl = paymentUrl.Replace("{tenant}", tenant.ToString());
                return new Uri(paymentUrl);
            }
            else
            {
                return null;
            }
        }


        private Tuple<TariffPlan, DateTime> GetBillingInfoFromDb(int tenant)
        {
            var q = new SqlQuery("tenants_tariff")
                .Select("tariff", "stamp")
                .Where("tenant", tenant)
                .OrderBy("id", false)
                .SetMaxResults(1);

            return ExecList(q)
                .ConvertAll(r => Tuple.Create((TariffPlan)Convert.ToInt32(r[0]), (DateTime)r[1]))
                .SingleOrDefault();
        }

        private void SaveBillingInfoToDb(int tenant, Tuple<TariffPlan, DateTime> bi)
        {
            var oldbi = GetBillingInfoFromDb(tenant);
            if (oldbi == null || !Equals(oldbi, bi))
            {
                var i = new SqlInsert("tenants_tariff")
                    .InColumnValue("tenant", tenant)
                    .InColumnValue("tariff", (int)bi.Item1)
                    .InColumnValue("stamp", bi.Item2);
                ExecNonQuery(i);
            }
        }


        private class BillingInfo
        {
            public TariffPlan Plan
            {
                get;
                set;
            }

            public DateTime DueDate
            {
                get;
                set;
            }
        }
    }
}
