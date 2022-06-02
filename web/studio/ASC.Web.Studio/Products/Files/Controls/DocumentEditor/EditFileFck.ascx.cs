using System;
using System.Collections.Generic;
using System.Drawing;
using System.Web;
using System.Web.Configuration;
using ASC.Data.Storage;
using ASC.Web.Studio.Helpers;

using System.Text;
using System.Web.UI;

using ASC.Web.Studio.Utility;
using FredCK.FCKeditorV2;

namespace ASC.Web.UserControls.Files
{
    public partial class EditFileFck : UserControl
    {
        public static string Location { get { return "EditFileFck.ascx"; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            _fckEditor.BasePath = CommonControlsConfigurer.FCKEditorBasePath;
            _fckEditor.ToolbarSet = "BlogToolbar";
            _fckEditor.EditorAreaCSS = ASC.Web.Core.Utility.Skins.WebSkin.GetUserSkin().BaseCSSFileAbsoluteWebPath;

        }
    }
}