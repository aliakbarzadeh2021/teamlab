#if DEBUG
namespace ASC.Core.Common.Tests
{
    using System;
    using ASC.Core.Security.Authentication;
    using NUnit.Framework;

    [TestFixture]
    public class CookieStorageTest
    {
        [Test]
        public void Validate()
        {
            var credential = new Credential(Guid.NewGuid().ToString(), "AsdljkjsdiwJDkak==", 1024);
            var cookie = CookieStorage.Save(credential);
            Assert.AreEqual(credential, CookieStorage.Get(cookie));
        }
    }
}
#endif
