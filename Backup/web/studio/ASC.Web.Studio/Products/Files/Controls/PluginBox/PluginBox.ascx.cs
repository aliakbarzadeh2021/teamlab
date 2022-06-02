#region Import

using System;
using System.Web.Configuration;
using System.Web.UI;
using ASC.Web.Files.Classes;
using System.Text;

#endregion

namespace ASC.Web.Files.Controls
{
    public partial class PluginBox : UserControl
    {

        #region Property

        public static string Location { get { return Files.Classes.PathProvider.GetFileStaticRelativePath("PluginBox/PluginBox.ascx"); } }

        #endregion

        #region Events

        protected void InitControl()
        {
            _installPlugin.Options.IsPopup = true;
            _howItWorks.Options.IsPopup = true;


        }

        protected void RegisteredScript()
        {
            if (Page.ClientScript.IsStartupScriptRegistered(GetType(), "{37C28301-9872-4d62-82F6-1D85D8277552}"))
                return;

            var inlineScript = new StringBuilder();

            inlineScript.AppendFormat(@"
                                ASC.Files.Constants.URL_PLUGIN_PATH = '{0}';
                                ASC.Files.Constants.PLUGIN_ID = '{1}';
                                ASC.Files.Constants.OOO_ID = '{2}';
                                ",
                                 PathProvider.GetPluginPath(),
                                 WebConfigurationManager.AppSettings["PluginId"] ?? "444436BC-A41D-46EE-9469-9BCBD820FDC6",
                                 WebConfigurationManager.AppSettings["OOOId"] ?? "7776529A-A9D0-49F9-B0C7-5B11EF29D01E"
                                 );

            inlineScript.AppendFormat(@"
                                ASC.Files.Constants.URL_HANDLER_TRACK = '{0}';
                                ",
                                 String.Concat(FileHandler.FileHandlerPath, ASC.Files.Core.FileConst.ParamsTrackActivity)
                                 );

            Page.ClientScript.RegisterStartupScript(GetType(), "{37C28301-9872-4d62-82F6-1D85D8277552}", inlineScript.ToString(), true);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitControl();

            RegisteredScript();

        }

        #endregion
    }
}