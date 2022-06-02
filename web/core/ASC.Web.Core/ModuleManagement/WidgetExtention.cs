using System;
using System.Web;

namespace ASC.Web.Core.ModuleManagement
{
    public partial class Widget
    {
        public Guid Guid
        {
            get { return new Guid(this.ID); }
        }

        public string Name {
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

        public Module Module { get; set; }

        public string WidgetAbsoluteURL
        {
            get
            {
                if (String.IsNullOrEmpty(this.WidgetVirtualURL))
                    return "";

                int ind = this.WidgetVirtualURL.IndexOf("?");
                if (ind != -1)
                    return VirtualPathUtility.ToAbsolute(this.WidgetVirtualURL.Substring(0, ind)) + this.WidgetVirtualURL.Substring(ind);
                else
                    return VirtualPathUtility.ToAbsolute(this.WidgetVirtualURL);
            }
        }
    }
}
