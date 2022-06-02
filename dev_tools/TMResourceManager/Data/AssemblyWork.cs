using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace TMResourceData
{
    public class AssemblyWork
    {
        static List<Assembly> listAssembly = new List<Assembly>();

        public static void UploadResourceData(Assembly[] assemblies)
        {
            Assembly _callingAssembly = Assembly.GetCallingAssembly();

            if (!listAssembly.Contains<Assembly>(_callingAssembly))
            {
                listAssembly.Add(_callingAssembly);
                RemoveResManager(_callingAssembly);
            }

            foreach (Assembly _assembly in assemblies.Except<Assembly>(listAssembly))
            {
                if (listAssembly.IndexOf(_assembly) < 0 && (_assembly.GetName().Name.IndexOf("ASC") >= 0 || _assembly.GetName().Name.IndexOf("App_GlobalResources") >= 0))
                {
                    listAssembly.Add(_assembly);
                    RemoveResManager(_assembly);
                }
            }
        }

        static void RemoveResManager(Assembly assembly)
        {
            DBResourceManager _databaseResourceManager = null;

            foreach (Type _type in assembly.GetTypes())
            {
                if (_type.GetProperty("ResourceManager", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public) != null)
                {
                    object _resManager = _type.InvokeMember("ResourceManager", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public, null, _type, new object[] { });
                    string _fileName = _type.Name + ".resx";

                    _databaseResourceManager = new DBResourceManager(_fileName, _resManager as ResourceManager);
                    _type.InvokeMember("resourceMan", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.SetField, null, _type, new object[] { _databaseResourceManager });
                }
            }
        }

        static void FindAssembly()
        {
 
        }
    }
}
