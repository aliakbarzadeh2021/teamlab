#if DEBUG
namespace ASC.Core.Common.Tests
{
    using System;
    using System.Configuration;
    using ASC.Core.Billing;
    using NUnit.Framework;

    [TestFixture]
    public class TariffServiceTest
    {
        private readonly ITariffService tariffService;


        public TariffServiceTest()
        {
            tariffService = new TariffService(ConfigurationManager.ConnectionStrings["core"], null);
        }


        [Test]
        public void TestShoppingUri()
        {
            var uri = tariffService.GetShoppingUri(10, TariffPlan.Premium);
            Assert.AreEqual(uri, new Uri("https://secure.avangate.com/order/checkout.php?PRODS=4542487&QTY=1&CART=2&REF=10&ORDERSTYLE=nLW04pa5iH4="));
        }

        [Test]
        public void TestPaymentInfo()
        {
            var payments = tariffService.GetPayments(1234567);
        }

        [Test]
        public void TestTariff()
        {
            var tariff = tariffService.GetTariff(1234567);
        }
    }
}
#endif
