using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace ASC.Web.Files.Controls
{
    public partial class FavoriteWidget : UserControl
    {
        public static string Location { get { return Files.Classes.PathProvider.GetFileStaticRelativePath("FavoriteWidget/FavoriteWidget.ascx"); } }

        protected void Page_Load(object sender, EventArgs e)
        {
			promptFavoritesDialog.Options.IsPopup = true;
			confirmFavoritesDialog.Options.IsPopup = true;
        }
    }
}