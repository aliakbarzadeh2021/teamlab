using System.Web.UI;

namespace ASC.Web.Core.WebZones
{
    public interface IRenderControlNavigation : IRenderWebItem
    {
        string RenderCustomNavigation(Page page);
    }
}