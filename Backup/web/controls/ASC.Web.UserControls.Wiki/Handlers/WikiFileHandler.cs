using System;
using System.IO;
using System.Net;
using System.Web;
using ASC.Core;
using ASC.Data.Storage;
using ASC.Web.Core;
using ASC.Web.UserControls.Wiki.Data;

namespace ASC.Web.UserControls.Wiki.Handlers
{
    public class WikiFileHandler : System.Web.UI.Page, IHttpHandler,
                                   System.Web.SessionState.IRequiresSessionState
    {

        private HttpContext _context;
        private WikiDAO _wikiDAO = null;
        protected WikiDAO wikiDAO
        {
            get
            {
                if (_wikiDAO == null)
                {
                    _wikiDAO = new WikiDAO();
                    _wikiDAO.ConnectionStringName = WikiSection.Section.DB.ConnectionStringName;

                    _wikiDAO.InitDAO(tenantId);
                }

                return _wikiDAO;
            }
        }

        private int tenantId
        {
            get
            {
                return CoreContext.TenantManager.GetCurrentTenant().TenantId;
            }
        }

        public static string ImageExtentions = ".png.jpg.bmp.gif";


        #region IHttpHandler Members

        bool IHttpHandler.IsReusable
        {
            get { return true; }
        }

        

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            if (!SecurityContext.IsAuthenticated)
            {
                //Try auth
                if (!SecurityContext.AuthenticateMe(CookiesManager.GetCookies(CookiesType.AuthKey)))
                {
                    throw new HttpException((int)HttpStatusCode.Forbidden, "Forbidden");
                }
            }

            _wikiDAO = null;
            context.Response.Clear();
            _context = context;
            if (string.IsNullOrEmpty(context.Request["file"]))
            {
                context.Response.End();
                return;
            }
            Files file = wikiDAO.FilesGetByName(context.Request["file"]);

            if (file == null)
            {
                context.Response.End();
                return;
            }
            if (string.IsNullOrEmpty(file.FileLocation))
            {
                context.Response.End();
                return;
            }

            //Check cache
            //if (context.Request.Headers["If-Modified-Since"]!=null)
            //{
            //    DateTime date;
            //    DateTime.TryParse(context.Request.Headers["If-Modified-Since"], out date);
            //    if (date>=file.Date)
            //    {
            //        context.Response.StatusCode = 304;//Not modified
            //        context.Response.Close();
            //    }
            //}
            ////set cache
            //context.Response.Cache.SetAllowResponseInBrowserHistory(true);
            //context.Response.Cache.SetCacheability(HttpCacheability.Public);
            //context.Response.Cache.SetLastModified(file.Date);
            //context.Response.Cache.SetOmitVaryStar(false);
            //context.Response.Cache.SetValidUntilExpires(false);
            //context.Response.Cache.SetSlidingExpiration(true);
            //context.Response.Cache.VaryByParams["file"] = true;
            //context.Response.Cache.SetExpires(DateTime.UtcNow.AddHours(1));

            ////Set content type
            //var fileName = Path.GetFileName(file.FileLocation);
            //var contentDisposition = string.Format("inline; filename={0};",
            //                                       HttpUtility.UrlPathEncode(fileName));
            //if (fileName.Any(c => (int)c >= 0 && (int)c <= 127))
            //{
            //    contentDisposition = string.Format("inline; filename*=utf-8''{0};",
            //                                       HttpUtility.UrlPathEncode(fileName));
            //}
            //context.Response.AddHeader("Content-Disposition", contentDisposition);
            //context.Response.ContentType = Common.Web.MimeMapping.GetMimeMapping(fileName);
            
            ////Get file by handler
            //IDataStore storage = StorageFactory.GetStorage(tenantId.ToString(), WikiSection.Section.DataStorage.ModuleName);
            //using (var stream = storage.GetReadStream(WikiSection.Section.DataStorage.DefaultDomain, file.FileLocation))
            //{
            //    stream.StreamCopyTo(context.Response.OutputStream);
            //}
            IDataStore storage = StorageFactory.GetStorage(tenantId.ToString(), WikiSection.Section.DataStorage.ModuleName);
            var locationEncoded = file.FileLocation.Replace(Path.GetFileName(file.FileLocation),
                                                            Uri.EscapeDataString(Path.GetFileName(file.FileLocation)));
            context.Response.Redirect(storage.GetUri(WikiSection.Section.DataStorage.DefaultDomain, locationEncoded).OriginalString);
        }

        #endregion
    }
}
