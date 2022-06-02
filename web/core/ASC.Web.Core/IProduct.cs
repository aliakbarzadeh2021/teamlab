using System;
using ASC.Web.Core.WebZones;

namespace ASC.Web.Core
{
    [WebZoneAttribute(WebZoneType.TopNavigationProductList | WebZoneType.StartProductList)]
    public interface IProduct
    {
        Guid ProductID { get; }

        string ProductName{get;}

        string ProductDescription { get; }

        /// <summary>
        /// Virtual start url for module (start with ~/)
        /// </summary>
        string StartURL { get; }
       

        IModule[] Modules { get; }

        void Init(ProductContext context);

        ProductContext Context { get; }

        void Shutdown();
    }

    [WebZoneAttribute(WebZoneType.TopNavigationProductList | WebZoneType.StartProductList)]
    public abstract class AbstractProduct : IProduct, IWebItem
    {
        public AbstractProduct()
        {
            WebItemManager.Instance.RegistryItem(this);
        }

        #region IProduct Members

        public abstract Guid ProductID { get; }

        public abstract string ProductName { get; }

        public abstract string ProductDescription { get; }

        public abstract string StartURL { get; }

        public abstract IModule[] Modules { get; }

        public abstract void Init(ProductContext context);

        public abstract ProductContext Context { get; }

        public abstract void Shutdown();

        #endregion

        #region IWebItem Members

        public Guid ID
        {
            get
            {
                return this.ProductID;
            }

        }

        public string Name
        {
            get
            {
                return this.ProductName;
            }
        }

        public string Description
        {
            get
            {
                return this.ProductDescription;
            }

        }

        WebItemContext IWebItem.Context
        {
            get
            {
                return (this as IProduct).Context;
            }
        }

        #endregion
    }
    
}
