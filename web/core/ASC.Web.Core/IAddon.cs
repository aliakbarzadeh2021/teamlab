using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Reflection;
using ASC.Web.Core.WebZones;

namespace ASC.Web.Core
{
    /// <summary>
    /// Type attribute for find addon in assembly
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class AddonAttribute : Attribute
    {

        public AddonAttribute(Type addonType, string version)
        {
            this.Version = new Version(version);
            this.AddonType = addonType;
        }

        public Type AddonType { get; private set; }

        public Version Version { get; private set; }


        public IAddon CreateAddonsInstance()
        {
            return (IAddon)Activator.CreateInstance(this.AddonType);
        }
    }

    public class AddonContext : WebItemContext
    {
    }

    [WebZoneAttribute(WebZoneType.Nowhere)]
    public interface IAddon
    {
        Guid ID { get; }

        string Name { get;}

        string Description { get;}

        string StartURL { get; }

        void Init(AddonContext context);

        AddonContext Context { get; }

        void Shutdown();
    }

    [WebZoneAttribute(WebZoneType.Nowhere)]
    public abstract class AbstractAddon : IAddon, IWebItem
    {
        public AbstractAddon()
        {
            WebItemManager.Instance.RegistryItem(this);
        }

        #region IAddon Members

        public abstract Guid ID { get; }

        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract void Init(AddonContext context);

        public abstract AddonContext Context { get; }

        public abstract string StartURL { get; }

        public abstract void Shutdown();

        #endregion

        #region IWebItem Members

        WebItemContext IWebItem.Context
        {
            get { return (this as IAddon).Context; }
        }

        #endregion
    }

}
