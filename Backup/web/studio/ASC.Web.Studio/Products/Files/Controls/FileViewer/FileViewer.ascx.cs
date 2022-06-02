#region Import

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Web.Core.Utility.Skins;

#endregion

namespace ASC.Web.Files.Controls
{
    public partial class FileViewer : System.Web.UI.UserControl
    {
        public static string Location { get { return Files.Classes.PathProvider.GetFileStaticRelativePath("FileViewer/FileViewer.ascx"); } }

        protected void Page_Load(object sender, EventArgs e)
        {
           
            
        }
    }
}