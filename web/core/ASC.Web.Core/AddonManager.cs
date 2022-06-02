using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using ASC.Web.Core.Utility;
using log4net;

namespace ASC.Web.Core
{

    public class AddonManager
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Web");

        public static AddonManager Instance
        {
            get;
            private set;
        }

        public List<IAddon> Addons { get; private set; }

        public GlobalHandlerComposite AddonGlobalHandlers { get; private set; }


        static AddonManager()
        {
            Instance = new AddonManager();
        }

        private AddonManager()
        {
            Addons = new List<IAddon>();
            AddonContexts = Hashtable.Synchronized(new Hashtable());
        }

        public Hashtable AddonContexts { get; private set; }

        public void LoadAddons()
        {
            if (HttpContext.Current == null) return;

            //set global handlers
            var addonGHandlers = new List<IGlobalHandler>();
            AddonGlobalHandlers = new GlobalHandlerComposite(addonGHandlers);

            foreach (var path in Directory.GetDirectories(HttpContext.Current.Server.MapPath("~/addons")))
            {
                var productAssemblyPath = Path.Combine(path, "bin\\ASC.Web." + Path.GetFileName(path) + ".dll");
                if (File.Exists(productAssemblyPath))
                {
                    try
                    {
                        var addonAssembly = Assembly.LoadFrom(productAssemblyPath);
                        var attr = addonAssembly.GetCustomAttributes(typeof(AddonAttribute), false).Cast<AddonAttribute>().FirstOrDefault();
                        if (attr != null)
                        {
                            var addon = attr.CreateAddonsInstance();

                            var addonContext = new AddonContext { AssemblyName = addonAssembly.FullName };

                            addon.Init(addonContext);
                            AddonContexts.Add(addon.ID, addonContext);

                            if (addonContext.GlobalHandler != null)
                            {
                                addonGHandlers.Add(addonContext.GlobalHandler);
                            }
                            //todo: register useractivity handler

                            Addons.Add(addon);

                            log.DebugFormat("Addon {0} loaded", addon.Name);
                        }
                    }
                    catch (Exception exc)
                    {
                        log.Error(String.Format("Couldn't load addon {0}", Path.GetFileName(path)), exc);
                    }
                }
            }
        }
    }
}
