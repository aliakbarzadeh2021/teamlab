using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using ASC.Data.Storage;
using ASC.Web.Core.Utility.Skins;

namespace ASC.Web.Projects.Classes
{

    public class PathProvider
    {

        static PathProvider()
        {

            _virtualPathCache = new Dictionary<String, String>();

        }

        #region Public

        private static object locker = new object();

        protected static Dictionary<String, String> _virtualPathCache;

        public static readonly String BaseVirtualPath = "~/Products/Projects/".ToLower();

        public static readonly String BaseAbsolutePath = VirtualPathUtility.ToAbsolute(BaseVirtualPath).ToLower();

        public static readonly String JsPhysicalResourcesPath = HttpContext.Current.Server.MapPath(@"~\Products\Projects\js\");


        public static String GetFileStaticRelativePath(String fileName)
        {

            if (fileName.EndsWith(".js"))
                return WebPath.GetPath("/Products/Projects/js/" + fileName);


            if (fileName.EndsWith(".css"))
                return WebSkin.GetUserSkin().GetAbsoluteWebPath("/Products/Projects/App_Themes/<theme_folder>/css/"+fileName);

            return String.Empty;

        }


        public static String GetControlVirtualPath(String fileName)
        {
            String findedVirtualPath = "";

            lock (locker)
            {
                if (_virtualPathCache.ContainsKey(fileName))
                    return _virtualPathCache[fileName];

                String controlsAbsolutePath = HttpContext.Current.Server.MapPath(String.Concat(BaseVirtualPath, "Controls"));

                List<String> findedFiles = FindedFile(controlsAbsolutePath, fileName);

                if (findedFiles.Count == 0)
                    throw new FileNotFoundException();

                if (findedFiles.Count > 1)
                    throw new Exception("Found " + findedFiles.Count + " files");

                findedVirtualPath = GetVirtualPath(findedFiles[0]);

                if (!String.IsNullOrEmpty(findedVirtualPath))
                    _virtualPathCache.Add(fileName, findedVirtualPath);
            }

            return findedVirtualPath;

        }

        public static string GetVirtualPath(string physicalPath)
        {
            string rootpath = HttpContext.Current.Server.MapPath("~/");
            physicalPath = physicalPath.Replace(rootpath, "");
            physicalPath = physicalPath.Replace("\\", "/");

            return "~/" + physicalPath;
        }

        #endregion

        #region Protected

        protected static List<String> FindedFile(String directoryPath, String fileName)
        {

            List<String> result = new List<string>();

            result.AddRange(Directory.GetFiles(directoryPath, fileName));

            foreach (String localDirectoryPath in Directory.GetDirectories(directoryPath))
                result.AddRange(FindedFile(localDirectoryPath, fileName));

            return result;

        }

        #endregion

    }
}
