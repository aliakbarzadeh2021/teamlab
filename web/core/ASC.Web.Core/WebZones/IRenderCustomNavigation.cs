using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace ASC.Web.Core.WebZones
{
    public interface IRenderCustomNavigation : IRenderWebItem
    {
        string RenderCustomNavigation();

        Control LoadCustomNavigationControl(Page page);
    }
}
