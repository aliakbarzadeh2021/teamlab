using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Core;

namespace ASC.Web.Files.Controls
{
    public partial class ImportWidget : UserControl
    {
        public static string Location { get { return Files.Classes.PathProvider.GetFileStaticRelativePath("ImportWidget/ImportWidget.ascx"); } }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            ImportDialog.Options.IsPopup = true;
            LoginDialog.Options.IsPopup = true;
                       
        }
    }
}