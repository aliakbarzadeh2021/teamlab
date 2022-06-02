using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using AjaxPro;
using ASC.Core;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.Web.Core.Users.Import;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Utility;
using ASC.Core.Tenants;
using System.Linq;

namespace ASC.Web.Studio.UserControls.Users
{
    public enum UserImportType
    {
        Selector = 0,
        CSV = 1,
        Outlook = 2,
        ActiveDirectory = 3,
        EmailInvite = 4
    }


    internal class ImportFileUploader : IFileUploadHandler
    {
        #region IFileUploadHandler Members

        public FileUploadResult ProcessUpload(HttpContext context)
        {
            FileUploadResult result = new FileUploadResult();
            try
            {
                if (context.Request.Files.Count != 0)
                {
                    var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "tempFiles");
                    var uploadFile = context.Request.Files[0];

                    if (uploadFile.ContentLength > SetupInfo.MaxTextFileUploadSize)
                    {
                        result.Success = false;
                        result.Message = Resources.Resource.ErrorFileSizeLimitText;
                        return result;
                    }

                    store.Save("importfiles/" + SecurityContext.CurrentAccount.ID.ToString() + ".tmp", uploadFile.InputStream);
                    result.Success = true;
                    result.Data = HttpUtility.HtmlEncode(context.Request.Files[0].FileName);
                }
                else
                {
                    result.Success = false;
                    result.Message = Resources.Resource.ErrorEmptyUploadFileSelected;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message.HtmlEncode();
            }

            return result;
        }

        #endregion
    }

    [AjaxNamespace("UserImporter")]
    public partial class UserImporter : System.Web.UI.UserControl
    {
        public static string Location
        {
            get
            {
                return "~/UserControls/Users/UserImporter/UserImporter.ascx";
            }
        }

        protected string _domainUserName = "";

        protected static bool DefaultInviteWithFullAccess
        {
            get
            {
                return CoreContext.UserManager.GetUsers().Length < 5;
            }
        }

        protected List<UserInfo> _importUserList
        {
            get
            {
                return HttpContext.Current.Session["importUserCollections"] as List<UserInfo>;
            }
            set
            {
                var allUsers = CoreContext.UserManager.GetUsers();

                var userList = value;
                if (userList != null)
                {
                    userList.RemoveAll(u =>
                    {
                        if (String.IsNullOrEmpty(u.FirstName) || String.IsNullOrEmpty(u.LastName) || !u.Email.TestEmailRegex())
                            return true;

                        var alreadyExistsUser = Array.Find(allUsers, user => !String.IsNullOrEmpty(user.Email) && String.Equals(user.Email, u.Email, StringComparison.InvariantCultureIgnoreCase));

                        return alreadyExistsUser != null;
                    });

                    for (int i = 0; i < userList.Count; i++)
                    {
                        for (int j = 0; j < userList.Count; j++)
                        {
                            if (i != j &&
                                String.Equals(userList[i].Email, userList[j].Email, StringComparison.InvariantCultureIgnoreCase))
                            {
                                userList.RemoveAt(j);
                                i = 0;
                                break;
                            }
                        }

                    }

                    HttpContext.Current.Session["importUserCollections"] = userList;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.ClientScript.RegisterClientScriptInclude(typeof(string), "userimporter_script", WebPath.GetPath("usercontrols/users/userimporter/js/userimporter.js"));

            StringBuilder script = new StringBuilder();
            script.Append(" UserImporterManager.SelectImportTitle = '" + CustomNamingPeople.Substitute<Resources.Resource>("UserImport").Replace("\"", "&quot;").Replace("\'", "&rsquo;") + "';");
            script.Append(" UserImporterManager.CSVImportTitle = '" + Resources.Resource.CSVImportName.Replace("\"", "&quot;").Replace("\'", "&rsquo;") + "';");
            script.Append(" UserImporterManager.OutlookImportTitle = '" + Resources.Resource.OutlookImportName.Replace("\"", "&quot;").Replace("\'", "&rsquo;") + "';");
            script.Append(" UserImporterManager.ADImportTitle = '" + Resources.Resource.ActiveDirectoryImportName.Replace("\"", "&quot;").Replace("\'", "&rsquo;") + "';");
            script.Append(" UserImporterManager.FileNotFoundMessage = '" + Resources.Resource.ErrorParseUserImportFile.Replace("\"", "&quot;").Replace("\'", "&rsquo;") + "';");
            script.Append(" importUsersDomain ='" + SetupInfo.ImportDomain + "';");


            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "userimporter_init_script", script.ToString(), true);

            AjaxPro.Utility.RegisterTypeForAjax(this.GetType());
            _importContainer.Options.IsPopup = true;

            if (SecurityContext.CurrentAccount.ID.Equals(UserManagerWrapper.AdminID))
            {
                SetupInfo setupInfo = null;
                try
                {
                    setupInfo = SetupInfo.ReadFromFile(SetupInfo.SetupXMLPath);
                }
                catch { };
                if (setupInfo != null && !String.IsNullOrEmpty(setupInfo.Domain))
                {
                    _domainUserName = setupInfo.Domain + "\\" + setupInfo.LogonUser ?? "";
                }
            }

            _importUserList = null;

        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse ImportFromFile(bool isCSV)
        {
            SecurityContext.CheckPermissions(ASC.Core.Users.Constants.Action_AddRemoveUser);

            AjaxResponse resp = new AjaxResponse();
            resp.rs1 = "0";
            resp.rs3 = ((int)(isCSV ? UserImportType.CSV : UserImportType.Outlook)).ToString();

            try
            {
                var path = "ImportFiles/" + SecurityContext.CurrentAccount.ID.ToString() + ".tmp";
                var store = StorageFactory.GetStorage(TenantProvider.CurrentTenantID.ToString(), "tempFiles");
                if (store.IsFile(path) == false)
                {
                    resp.rs2 = Resources.Resource.ErrorParseUserImportFile.ReplaceSingleQuote();
                    return resp;
                }

                using (var stream = store.GetReadStream(path))
                {
                    var importer = isCSV ?
                        new TextFileUserImporter(stream) { DefaultHeader = "FirstName;LastName;EMail", } :
                        new OutlookCSVUserImporter(stream);
                    _importUserList = new List<UserInfo>(importer.GetDiscoveredUsers());
                }
                resp.rs1 = "1";
                resp.rs2 = RenderImportUsers();
            }
            catch (Exception)
            {
                resp.rs2 = Resources.Resource.ErrorParseUserImportFile.ReplaceSingleQuote();
            }
            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse ImportFromAD(string usrName, string pwd)
        {

            throw new NotSupportedException();
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string GetImportDomain(string usrName, string pwd)
        {
            return SetupInfo.ImportDomain;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SaveUserImport(Guid[] userIDs)
        {
            SecurityContext.CheckPermissions(ASC.Core.Users.Constants.Action_AddRemoveUser);
            AjaxResponse resp = new AjaxResponse();
            try
            {
                if (_importUserList != null)
                {
                    var importUsers = _importUserList.FindAll(ui => Array.Exists<Guid>(userIDs, id => id.Equals(ui.ID)));
                    foreach (var ui in importUsers)
                    {
                        if (!ui.WorkFromDate.HasValue)
                            ui.WorkFromDate = TenantUtil.DateTimeNow();

                        UserManagerWrapper.AddUser(ui, UserManagerWrapper.GeneratePassword());
                    }
                }
                _importUserList = null;
                resp.rs1 = "1";

            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs2 = HttpUtility.HtmlEncode(e.Message);
            }
            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string GetExistingUsersEmails()
        {
            try
            {
                var sb = new StringBuilder();

                var users = CoreContext.UserManager.GetUsers();
                foreach (var u in users)
                {
                    if (!string.IsNullOrEmpty(u.Email))
                    {
                        sb.AppendFormat(",{0},", u.Email.Replace(@"""", "'"));
                    }
                }
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        private string RenderImportUsers()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<table cellpadding=""3"" cellspacing=""0"" style=""width: 100%;"">");

            int i = 0;
            foreach (var user in _importUserList)
            {

                sb.Append("<tr class=" + (i % 2 == 0 ? "tintLight" : "tintMedium") + ">");
                sb.Append(@"<td align=""center"" style=""width:30px;"">");

                sb.Append("<input id=\"studio_importusr_" + user.ID + "\"");
                if (String.IsNullOrEmpty(user.FirstName) || String.IsNullOrEmpty(user.LastName) || String.IsNullOrEmpty(user.Email))
                    sb.Append(" disabled=\"disabled\"");
                else
                    sb.Append(" checked=\"checked\"");

                sb.Append("type=\"checkbox\" value=\"" + user.ID + "\" />");
                sb.Append("</td>");
                sb.Append("<td style=\"width:180px;\">");
                sb.Append(HttpUtility.HtmlEncode(user.FirstName));
                sb.Append("</td>");

                sb.Append("<td style=\"width:180px;\">");
                sb.Append(HttpUtility.HtmlEncode(user.LastName));
                sb.Append("</td>");

                sb.Append("<td style=\"width:180px;\">");
                sb.Append(HttpUtility.HtmlEncode(user.Email));
                sb.Append("</td>");

                sb.Append("</tr>");
                i++;
            }
            sb.Append("</table>");
            return sb.ToString();
        }

        protected bool HasDepartments
        {
            get
            {
                var depts = CoreContext.UserManager.GetDepartments();
                if (depts != null)
                {
                    return depts.Length > 0;
                }
                return false;
            }
        }

        protected string RenderDepartOptions()
        {
            var deps = new List<GroupInfo>();
            foreach (var department in CoreContext.UserManager.GetDepartments())
            {
                deps.Add(department);
                deps.AddRange(GetChildDepartments(department));
            }

            deps.Sort((d1, d2) => String.Compare(d1.Name, d2.Name, StringComparison.InvariantCultureIgnoreCase));
            StringBuilder sb = new StringBuilder();
            foreach (var dep in deps)
            {
                sb.Append("<option value='" + dep.ID + "'>" + dep.Name.HtmlEncode() + "</option>");
            }
            return sb.ToString();
        }

        private List<GroupInfo> GetChildDepartments(GroupInfo dep)
        {
            var deps = new List<GroupInfo>();
            foreach (var childDep in dep.Descendants)
            {
                deps.Add(childDep);
                deps.AddRange(GetChildDepartments(childDep));
            }

            return deps;
        }
    }
}