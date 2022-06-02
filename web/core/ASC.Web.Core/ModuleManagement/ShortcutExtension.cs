using System;

namespace ASC.Web.Core.ModuleManagement
{
    public partial class Shortcut : ICloneable
    {
        public Guid Guid { get { return new Guid(this.ID); } }

        public Module Module { get; set; }

        public string Name
        {
            get
            {
                if (String.IsNullOrEmpty(this.ResourceClassTypeName))
                    return ModuleManager.GetModuleResource(Module.ResourceClassTypeName, this.NameResourceKey);
                else
                    return ModuleManager.GetModuleResource(this.ResourceClassTypeName, this.NameResourceKey);
            }
        }

        public string Description
        {
            get
            {
                if (String.IsNullOrEmpty(this.ResourceClassTypeName))
                    return ModuleManager.GetModuleResource(Module.ResourceClassTypeName, this.DescriptionResourceKey);
                else
                    return ModuleManager.GetModuleResource(this.ResourceClassTypeName, this.DescriptionResourceKey);
            }
        }

        //public string ImageURL 
        //{
        //    get 
        //    {
        //        if (!String.IsNullOrEmpty(this.ImageFileName))
        //            return WebImageSupplier.GetAbsoluteWebPath(this.ImageFileName);

        //        return "";
        //    }
        //}

        public string AbsoluteActionURL { get; set; }

        public ShortcutCategory Category { get; set; }

        #region ICloneable Members

        public object Clone()
        {
            return new Shortcut()
            {
                ID = this.ID,
                Module = this.Module,
                ImageFileName = this.ImageFileName,
                IsDynamic = this.IsDynamic,
                IsMenuOnly = this.IsMenuOnly,
                IsProtected = this.IsProtected,
                RefName = this.RefName,
                ActionURL = this.ActionURL,
                NameResourceKey = this.NameResourceKey,
                ResourceClassTypeName = this.ResourceClassTypeName,
                DescriptionResourceKey = this.DescriptionResourceKey,
                Type = this.Type
            };
        }

        #endregion
    }
}
