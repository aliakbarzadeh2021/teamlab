using System;

namespace ASC.Web.Core
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class ProductAttribute : Attribute
    {
        public ProductAttribute(Type productType)
        {
            this.ProductType = productType;
        }

        public ProductAttribute(Type productType, string version)
            : this(productType)
        {
        }

        public Type ProductType { get; private set; }

        public IProduct CreateProductInstance()
        {
            return (IProduct)Activator.CreateInstance(ProductType);
        }
    }
}
