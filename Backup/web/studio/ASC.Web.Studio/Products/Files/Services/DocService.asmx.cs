using System;
using System.Web;
using System.Web.Services;
using ASC.Files.Core;

namespace ASC.Web.Files.Services
{
    [WebService(Namespace = "http://services.virtual11.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class DocService : System.Web.Services.WebService
    {
        [WebMethod]
        public string GetConfig(string progGuid)
        {
            try
            {
                switch (progGuid.ToLower())
                {
                    case "7776529a-a9d0-49f9-b0c7-5b11ef29d01e":
                        return OOCfg();
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return string.Empty;
        }

        [WebMethod]
        public void OnActive(string appId, string cl)
        {
            var uri = new Uri(cl);
            if (string.IsNullOrEmpty(uri.Query)) return;

            string id = HttpUtility.ParseQueryString(uri.Query).Get("id");
            var fileID = 0;

            if (int.TryParse(id, out fileID))
            {
                lock (File.NowEditing) File.NowEditing[fileID] = DateTime.UtcNow;
            }
        }

        private string OOCfg()
        {
            return System.IO.File.ReadAllText(Server.MapPath("~/Products/Files/config.xml"));
        }
    }
}