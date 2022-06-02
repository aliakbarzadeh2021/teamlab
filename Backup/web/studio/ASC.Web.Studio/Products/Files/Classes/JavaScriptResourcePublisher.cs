using System;
using System.Threading;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Text;

namespace ASC.Web.Files.Classes
{
    public class JavaScriptResourcePublisher
    {
        public static void Execute(String jsFilePath, String jsObjectName, Type resourceType)
        {
            var assemblyModified = File.GetLastWriteTime(Assembly.GetCallingAssembly().Location);
            var cultures = ConfigurationSettings.AppSettings["enabledcultures"];
            if (!string.IsNullOrEmpty(cultures))
            {
                var currentCulture = Thread.CurrentThread.CurrentCulture;
                var currentUICulture = Thread.CurrentThread.CurrentUICulture;

                foreach (var lng in cultures.Split(','))
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(lng);
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(lng);

                    var path = String.Format("{0}resources.{1}.js", jsFilePath, Thread.CurrentThread.CurrentUICulture.Name.ToLower());
                    var jsModified = File.GetLastWriteTime(path);
                    if (jsModified < assemblyModified)
                    {
                        File.WriteAllText(path, Serialize(resourceType, jsObjectName));
                    }
                }

                Thread.CurrentThread.CurrentCulture = currentCulture;
                Thread.CurrentThread.CurrentUICulture = currentUICulture;
            }
        }

        private static String Serialize(Type resourceType, String jsObjectName)
        {
            var jsObject = new StringBuilder((string.IsNullOrEmpty(jsObjectName) ? resourceType.FullName : jsObjectName) + "={");
            foreach (var p in resourceType.GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                if (p.PropertyType == typeof(string))
                {
                    jsObject.AppendFormat("\"{0}\":\"{1}\",\n", p.Name, p.GetValue(null, null));
                }
            }

            jsObject.Remove(jsObject.Length - 2, 2);
            jsObject.Append("\n};");

            return jsObject.ToString();
        }
    }
}
