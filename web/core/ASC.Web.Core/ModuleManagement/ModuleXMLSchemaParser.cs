using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace ASC.Web.Core.ModuleManagement
{
    static class ModuleXMLSchemaParser
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(Module));


        public static List<Module> Parse(string modulesPhysicalDirPath, string moduleXMLFileName)
        {
            var modules = new List<Module>();
            foreach (var modulePath in Directory.GetDirectories(modulesPhysicalDirPath))
            {
                var xmlPath = modulePath + "\\" + moduleXMLFileName;
                if (File.Exists(xmlPath))
                {
                    using (var reader = new StreamReader(xmlPath))
                    {
                        var module = (Module)serializer.Deserialize(reader);
                        modules.Add(module);
                        WebItemManager.Instance.RegistryItem(module);
                    }
                }
            }
            return modules;
        }
    }
}
