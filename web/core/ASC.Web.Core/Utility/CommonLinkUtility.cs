using System;
using System.Text.RegularExpressions;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using System.Collections;
using ASC.Security.Cryptography;

namespace ASC.Web.Studio.Utility
{
    public enum ManagementType
    {
        General = 0,
        Statistic = 5,
        Account = 6,
        Customization = 7,
        WebItems = 1,
        Mail = 2,
        Administrators = 3,
        Tariff = 4
    }

    public enum UserProfileType
    {
        General,
        Activity,
        Statistic,
        Subscriptions
    }

    public enum MyStaffType
    {
        General,
        Activity,
        Subscriptions
    }

    public static class CommonLinkUtility
    {
        public static string GoPremiumUrl()
        {
            var lang = CoreContext.TenantManager.GetCurrentTenant().Language;
            if (lang.Contains("-"))
                lang = lang.Split('-')[0];

            var baseUrl = System.Configuration.ConfigurationManager.AppSettings["gopremiumurl"].Replace("{lng}", lang);
            var validationKey = EmailValidationKeyProvider.GetEmailKey("tenant" + TenantProvider.CurrentTenantID.ToString());
            return baseUrl + (baseUrl.IndexOf("?") >= 0 ? "&" : "?")
                    + "tenant=" + TenantProvider.CurrentTenantID.ToString()
                    + "&key=" + validationKey;
        }

        public static string ShoppingCardUrl()
        {
            var uri = CoreContext.TenantManager.GetShoppingUri(CoreContext.TenantManager.GetCurrentTenant().TenantId);
            return uri != null ? uri.ToString() : string.Empty;
        }

        public static void Initialize()
        {
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
            {
                var url = HttpContext.Current.Request.Url;

                if (host == null && CoreContext.Configuration.Standalone)
                {
                    host = System.Web.Configuration.WebConfigurationManager.AppSettings["web.host"];
                    if (host == null && Uri.CheckHostName(url.Host) == UriHostNameType.Dns)
                    {
                        host = url.Host;
                    }
                    if (host == "localhost")
                    {
                        try
                        {
                            host = System.Net.Dns.GetHostName();
                        }
                        catch { }
                    }
                }
                if (port == null)
                {
                    port = url.IsDefaultPort ? string.Empty : ":" + url.Port;
                }
            }
        }

        public static string Logout
        {
            get
            {
                return VirtualPathUtility.ToAbsolute("~/auth.aspx") + "?t=logout";
            }
        }

        public static string GetDefault()
        {
            return VirtualPathUtility.ToAbsolute("~/");
        }


        public static string GetMyStaff()
        {
            return GetMyStaff(MyStaffType.General);
        }

        public static string GetMyStaff(MyStaffType type)
        {
            return GetMyStaff((type).ToString().ToLower());
        }

        public static string GetMyStaff(string type)
        {
            return GetFullAbsolutePath("~/my.aspx") + ((type != MyStaffType.General.ToString().ToLower()) ? ("?type=" + (type)) : "");
        }

        public static string GetEmployees()
        {
            return GetEmployees(GetProductID());
        }

        public static string GetEmployees(Guid productID)
        {
            return GetEmployees(productID, EmployeeStatus.Active);
        }

        public static string GetEmployees(Guid productID, EmployeeStatus empStatus)
        {
            return VirtualPathUtility.ToAbsolute("~/employee.aspx") + "?" + GetProductParamsPair(productID) +
                (empStatus == EmployeeStatus.Terminated ? "&es=0" : string.Empty);
        }

        public static string GetUserDepartment(Guid userID)
        {
            return GetUserDepartment(userID, GetProductID());
        }

        public static string GetUserDepartment(Guid userID, Guid productID)
        {
            var groups = CoreContext.UserManager.GetUserGroups(userID);
            if (groups != null && groups.Length > 0)
                return VirtualPathUtility.ToAbsolute("~/employee.aspx") + "?" + CommonLinkUtility.GetProductParamsPair(productID) + "&deplist=" + groups[0].ID.ToString();

            return GetEmployees(productID);
        }

        public static string GetDepartment(Guid productID, Guid departmentID)
        {
            return VirtualPathUtility.ToAbsolute("~/employee.aspx") + "?" + CommonLinkUtility.GetProductParamsPair(productID) + "&deplist=" + departmentID.ToString();
        }

        #region user profile link
        public static string GetUserProfile(string user)
        {
            return GetUserProfile(user, GetProductID());
        }

        public static string GetUserProfile(string user, Guid productID)
        {
            return GetUserProfile(user, productID, UserProfileType.General);
        }
        public static string GetUserProfile(Guid productID)
        {
            return GetUserProfile(null, productID, UserProfileType.General);
        }

        public static string GetUserProfile(Guid userID, Guid productID)
        {
            return GetUserProfile(userID, productID, UserProfileType.General);
        }

        public static string GetUserProfile(Guid userID, Guid productID, UserProfileType userProfileType)
        {
            if (!CoreContext.UserManager.UserExists(userID))
                return GetEmployees(productID);

            return GetUserProfile(userID.ToString(), productID, userProfileType);
        }

        public static string GetUserProfile(string user, Guid productID, UserProfileType userProfileType)
        {
            string queryParams = "";

            if (!String.IsNullOrEmpty(user))
            {
                Guid guid = Guid.Empty;

                //firts fast check on guid
                if (!String.IsNullOrEmpty(user) && user.Length == 36 && user[8] == '-')
                {
                    try { guid = new Guid(user); }
                    catch { }
                }

                if (guid != Guid.Empty)
                    queryParams = GetUserParamsPair(guid);
                else
                    queryParams = ParamName_UserUserName + "=" + HttpUtility.UrlEncode(user);
            }

            if (productID != Guid.Empty)
                queryParams += (String.IsNullOrEmpty(queryParams) ? "?" : "&") + CommonLinkUtility.GetProductParamsPair(productID);


            var url = VirtualPathUtility.ToAbsolute("~/userprofile.aspx") + "?" + queryParams;
            switch (userProfileType)
            {
                case UserProfileType.General:
                    break;

                case UserProfileType.Activity:
                    url += "#activity";
                    break;

                case UserProfileType.Statistic:
                    url += "#statistic";
                    break;

                case UserProfileType.Subscriptions:
                    url += "#subscriptions";
                    break;
            }

            return url;
        }
        #endregion


        public static Guid GetProductID()
        {
            Guid productID = Guid.Empty;

            if (HttpContext.Current != null)
            {
                IProduct product;
                IModule module;
                GetLocationByRequest(HttpContext.Current.Request, out product, out module);
                if (product != null) productID = product.ProductID;
            }

            if (productID == Guid.Empty)
            {
                productID = CommonLinkUtility.StartProductID;
            }
            if (productID == Guid.Empty)
            {
                object pid = System.Runtime.Remoting.Messaging.CallContext.GetData("asc.web.product_id");
                if (pid != null) productID = (Guid)pid;
            }

            return productID;
        }

        public static void GetLocationByRequest(HttpRequest request, out IProduct currentProduct, out IModule currentModule)
        {
            string currentURL = "";
            if (request != null && request.Url != null)
            {
                currentURL = request.Url.AbsoluteUri;
                // http://[hostname]/[virtualpath]/[AjaxPro.Utility.HandlerPath]/[assembly],[classname].ashx
                //
                if (currentURL.Contains("/" + AjaxPro.Utility.HandlerPath + "/") && request.UrlReferrer != null)
                {
                    currentURL = request.UrlReferrer.AbsoluteUri;
                }
            }

            GetLocationByUrl(currentURL, out currentProduct, out currentModule);
        }


        static Regex regFilePathTrim = new Regex("/[^/]*\\.aspx", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static void GetLocationByUrl(string currentURL, out IProduct currentProduct, out IModule currentModule)
        {
            currentProduct = null;
            currentModule = null;

            if (!String.IsNullOrEmpty(currentURL))
            {
                var urlParams = HttpUtility.ParseQueryString(new Uri(currentURL).Query);
                var productByName = GetProductBySysName(urlParams[ParamName_ProductSysName]);
                var pid = productByName == null ? Guid.Empty : productByName.ProductID;

                if (pid == Guid.Empty && !String.IsNullOrEmpty(urlParams["pid"]))
                {
                    try
                    {
                        pid = new Guid(urlParams["pid"]);
                    }
                    catch { pid = Guid.Empty; }
                }

                var productName = GetProductNameFromUrl(currentURL);
                var moduleName = GetModuleNameFromUrl(currentURL);

                if (!string.IsNullOrEmpty(productName) || !string.IsNullOrEmpty(moduleName))
                {
                    foreach (var product in ProductManager.Instance.Products)
                    {
                        var _productName = GetProductNameFromUrl(product.StartURL);
                        if (!string.IsNullOrEmpty(_productName))
                        {
                            if (String.Compare(productName, _productName, StringComparison.InvariantCultureIgnoreCase) == 0)
                            {
                                currentProduct = product;

                                if (!String.IsNullOrEmpty(moduleName))
                                {
                                    foreach (var module in product.Modules)
                                    {
                                        var _moduleName = GetModuleNameFromUrl(module.StartURL);
                                        if (!string.IsNullOrEmpty(_moduleName))
                                        {
                                            if (String.Compare(moduleName, _moduleName, StringComparison.InvariantCultureIgnoreCase) == 0)
                                            {
                                                currentModule = module;
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var module in product.Modules)
                                    {
                                        if (!module.StartURL.Equals(product.StartURL) && currentURL.Contains(regFilePathTrim.Replace(module.StartURL, string.Empty)))
                                        {
                                            currentModule = module;
                                            break;
                                        }
                                    }
                                }

                                break;
                            }
                        }
                    }
                }

                if (pid != Guid.Empty)
                    currentProduct = ProductManager.Instance[pid];
            }
        }

        private static string GetProductNameFromUrl(string url)
        {
            try
            {
                var pos = url.IndexOf("/products/", StringComparison.InvariantCultureIgnoreCase);
                if (0 <= pos)
                {
                    url = url.Substring(pos + 10).ToLower();
                    pos = url.IndexOf('/');
                    return 0 < pos ? url.Substring(0, pos) : url;
                }
            }
            catch { }
            return null;
        }

        private static string GetModuleNameFromUrl(string url)
        {
            try
            {
                var pos = url.IndexOf("/modules/", StringComparison.InvariantCultureIgnoreCase);
                if (0 <= pos)
                {
                    url = url.Substring(pos + 9).ToLower();
                    pos = url.IndexOf('/');
                    return 0 < pos ? url.Substring(0, pos) : url;
                }
            }
            catch { }
            return null;
        }

        public static Guid StartProductID
        {
            get
            {
                Guid start_pid = Guid.Empty;
                string startProductID = System.Configuration.ConfigurationManager.AppSettings["StartProductID"];
                if (!String.IsNullOrEmpty(startProductID))
                {
                    try
                    {
                        start_pid = new Guid(startProductID);
                    }
                    catch { start_pid = Guid.Empty; }
                }
                return start_pid;
            }
        }


        public const string ParamName_ProductSysName = "product";
        public static string GetProductParamsPair(Guid productId)
        {
            string result = "";
            if (productId != Guid.Empty)
            {
                var currentProduct = ProductManager.Instance[productId];
                if (currentProduct != null)
                    result = String.Format("{0}={1}",
                                            ParamName_ProductSysName,
                                            WebItemExtension.GetSysName(currentProduct as IWebItem));
            }

            return result;
        }
        static IProduct GetProductBySysName(string sysName)
        {
            IProduct result = null;

            if (!String.IsNullOrEmpty(sysName))
                foreach (var product in ProductManager.Instance.Products)
                {
                    if (String.CompareOrdinal(sysName, WebItemExtension.GetSysName(product as IWebItem)) == 0)
                    {
                        result = product;
                        break;
                    }
                }

            return result;
        }

        public const string ParamName_UserUserName = "user";
        public const string ParamName_UserUserID = "uid";
        public static string GetUserParamsPair(Guid userID)
        {
            if (CoreContext.UserManager.UserExists(userID))
                return GetUserParamsPair(CoreContext.UserManager.GetUsers(userID));
            else
                return "";
        }
        public static string GetUserParamsPair(UserInfo user)
        {
            if (user == null || string.IsNullOrEmpty(user.UserName))
                return "";
            else
                return String.Format("{0}={1}", ParamName_UserUserName, HttpUtility.UrlEncode(user.UserName.ToLowerInvariant()));
        }


        #region management links
        public static string GetAdministration()
        {
            return GetAdministration(ManagementType.General);
        }

        public static string GetAdministration(Guid productID)
        {
            return GetAdministration(ManagementType.General, productID);
        }

        public static string GetAdministration(ManagementType managementType)
        {
            return GetAdministration(managementType, Guid.Empty);
        }

        public static string GetAdministration(ManagementType managementType, Guid productID)
        {
            string query = "";
            if (productID != Guid.Empty)
                query = "?" + CommonLinkUtility.GetProductParamsPair(productID);

            if (managementType == ManagementType.General)
                return VirtualPathUtility.ToAbsolute("~/management.aspx") + query;

            return VirtualPathUtility.ToAbsolute("~/management.aspx") + query + (String.IsNullOrEmpty(query) ? "?" : "&") + "type=" + ((int)managementType).ToString();
        }
        #endregion

        #region heper methods

        /// <summary>
        /// VirtualPathUtility.ToAbsolute("~")
        /// </summary>
        public static string VirtualRoot
        {
            get { return VirtualPathUtility.ToAbsolute("~"); }
        }

        public static string GetFullAbsolutePath(string virtualPath)
        {
            if (String.IsNullOrEmpty(virtualPath))
                return ServerRootPath;

            if (virtualPath.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) ||
                virtualPath.StartsWith("mailto:", StringComparison.InvariantCultureIgnoreCase) ||
                virtualPath.StartsWith("javascript:", StringComparison.InvariantCultureIgnoreCase) ||
                virtualPath.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                return virtualPath;


            if (virtualPath.StartsWith("~"))
                virtualPath = VirtualRoot + "/" + virtualPath.Substring(1);

            virtualPath = ("/" + virtualPath).Replace("//", "/").Replace("//", "/");

            string result = ServerRootPath + virtualPath;

            return result;
        }


        private static string host;
        private static string port;

        //http://localhost
        public static string ServerRootPath
        {
            get
            {
                return string.Format("{0}://{1}{2}",
                    HttpContext.Current != null ? HttpContext.Current.Request.Url.Scheme : Uri.UriSchemeHttp,
                    host ?? (HttpContext.Current != null ? HttpContext.Current.Request.Url.Host : CoreContext.TenantManager.GetCurrentTenant().TenantDomain),
                    port);
            }
        }
        #endregion

    }
}
