using System;
using System.Linq;
using System.Text;
using System.Web.UI;
using ASC.Web.Files.Resources;
using ASC.Files.Core.Security;
using ASC.Core;
using System.Collections.Generic;

namespace ASC.Web.Files.Controls
{
    public partial class AccessRights : UserControl
    {
        public static string Location { get { return Files.Classes.PathProvider.GetFileStaticRelativePath("AccessRights/AccessRights.ascx"); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitControl();

            RegisteredScript();
        }

        protected void InitControl()
        {
            //newInShareDialog.Options.IsPopup = true;
            shareAccessDialog.Options.IsPopup = true;
            confirmGroupDialog.Options.IsPopup = true;
            UserAccess.TabName = FilesUCResource.UsersTabName;
            GroupAccess.TabName = FilesUCResource.GroupsTabName;
        }

        protected void RegisteredScript()
        {
            if (Page.ClientScript.IsStartupScriptRegistered(GetType(), "{BC0B4987-672B-416a-8828-C80065BEAC4D}"))
                return;

            var script = "ASC.Files.Constants.AceStatusEnum = {};";
            script += String.Format(@"
                                      ASC.Files.Constants.AceStatusEnum.None = '{0}';
                                      ASC.Files.Constants.AceStatusEnum.ReadWrite = '{1}';
                                      ASC.Files.Constants.AceStatusEnum.Read = '{2}';
                                      ASC.Files.Constants.AceStatusEnum.Restrict = '{3}';
                                    ",
                                     FileShare.None,
                                     FileShare.ReadWrite,
                                     FileShare.Read,
                                     FileShare.Restrict
                );

            Page.ClientScript.RegisterStartupScript(GetType(), "{BC0B4987-672B-416a-8828-C80065BEAC4D}", script, true);
        }

        public string RenderGroupsList()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("<option id='sg_{0}' value='{0}'>{1}</option>", ASC.Core.Users.Constants.GroupEveryone.ID, FilesUCResource.Everyone);
            sb.AppendFormat("<option id='sg_{0}' value='{0}'>{1}</option>", ASC.Core.Users.Constants.GroupAdmin.ID, FilesUCResource.Administrators);

            var groups = new List<ASC.Core.Users.GroupInfo>(CoreContext.GroupManager.GetGroups()).OrderBy(g => g.Name);
            foreach (var g in groups)
            {
                sb.AppendFormat("<option id='sg_{0}' value='{0}'>{1}</option>", g.ID, g.Name.HtmlEncode());
            }

            return sb.ToString();
        }
    }
}