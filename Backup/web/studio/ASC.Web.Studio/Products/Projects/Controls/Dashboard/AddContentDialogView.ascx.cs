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

namespace ASC.Web.Projects.Controls.Dashboard
{
    public partial class AddContentDialogView : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AddContentContainer.Options.IsPopup = true;
        }
    }
}