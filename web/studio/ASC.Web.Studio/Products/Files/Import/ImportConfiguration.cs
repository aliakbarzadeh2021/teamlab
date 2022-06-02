using ASC.Thrdparty;
using ASC.Thrdparty.Configuration;
using ASC.Thrdparty.TokenManagers;

namespace ASC.Web.Files.Import
{
    public static class ImportConfiguration
    {
        public static bool SupportGoogleImport
        {
            get;
            private set;
        }

        public static bool SupportZohoImport
        {
            get;
            private set;
        }

        public static bool SupportBoxNetImport
        {
            get;
            private set;
        }


        public static IAssociatedTokenManager GoogleTokenManager
        {
            get;
            private set;
        }

        public static string ZohoApiKey
        {
            get;
            private set;
        }

        public static string BoxNetApiKey
        {
            get;
            private set;
        }

        public static string BoxNetIFrameAddress
        {
            get;
            private set;
        }

        static ImportConfiguration()
        {
            SupportGoogleImport = !string.IsNullOrEmpty(KeyStorage.Get("googleConsumerKey"));
            SupportZohoImport = !string.IsNullOrEmpty(KeyStorage.Get("zoho"));
            SupportBoxNetImport = !string.IsNullOrEmpty(KeyStorage.Get("box.net"));

            if (SupportGoogleImport)
            {
                GoogleTokenManager = TokenManagerHolder.Get("google", "googleConsumerKey", "googleConsumerSecret");
            }
            if (SupportZohoImport)
            {
                ZohoApiKey = KeyStorage.Get("zoho");
            }
            if (SupportBoxNetImport)
            {
                BoxNetApiKey = KeyStorage.Get("box.net");
                BoxNetIFrameAddress = KeyStorage.Get("box.net.framehandler");
            }
        }
    }
}
