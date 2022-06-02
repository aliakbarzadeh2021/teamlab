using System;
using ASC.Web.Core.WebZones;

namespace ASC.Web.Core
{
    [WebZoneAttribute(WebZoneType.Nowhere)]
    public interface IModule
    {
        Guid ModuleID { get; }

        string ModuleName { get; }
        
        string ModuleDescription { get; }

        /// <summary>
        /// Virtual start url for module (start with ~/)
        /// </summary>
        string StartURL { get; }

        ModuleContext Context { get; }
    }

    [WebZoneAttribute(WebZoneType.Nowhere)]
    public abstract class AbstractModule : IModule, IWebItem
    {
        public AbstractModule(bool autoregister)
        {
            if(autoregister)
                WebItemManager.Instance.RegistryItem(this);
        }
        public AbstractModule()
            :this(true)
        {
            
        }

        #region IModule Members

        public abstract Guid ModuleID { get; }

        public abstract string ModuleName { get; }

        public abstract string ModuleDescription { get; }

        public abstract string StartURL { get; }

        public abstract ModuleContext Context { get; }

        #endregion

        #region IWebItem Members

        public Guid ID
        {
            get { return this.ModuleID; }
        }

        public string Name
        {
            get { return this.ModuleName; }
        }

        public string Description
        {
            get { return this.ModuleDescription; }
        }

        WebItemContext IWebItem.Context
        {
            get { return (this as IModule).Context; }
        }

        #endregion
    }

}
