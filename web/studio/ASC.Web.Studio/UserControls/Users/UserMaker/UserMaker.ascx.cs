using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Linq;
using AjaxPro;
using ASC.Core;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.Web.Controls;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using System.Web.UI;
using ASC.Common.Security;

namespace ASC.Web.Studio.UserControls.Users
{
    internal class UserPhotoUploader : IFileUploadHandler
    {
        #region IFileUploadHandler Members

        public FileUploadResult ProcessUpload(HttpContext context)
        {
            FileUploadResult result = new FileUploadResult();
            try
            {
                if (context.Request.Files.Count != 0)
                {
                    HttpPostedFile userPhoto = context.Request.Files[0];
                    byte[] data = new byte[userPhoto.InputStream.Length];

                    BinaryReader br = new BinaryReader(userPhoto.InputStream);
                    br.Read(data, 0, (int)userPhoto.InputStream.Length);
                    br.Close();
                    result.Data = UserPhotoManager.SaveTempPhoto(data, SetupInfo.MaxImageUploadSize, UserPhotoManager.MaxFotoSize.Width, UserPhotoManager.MaxFotoSize.Height);
                    result.Success = true;

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

    [AjaxNamespace("UserMaker")]
    public partial class UserMaker : System.Web.UI.UserControl
    {
        public static string Location { get { return "~/UserControls/Users/UserMaker/UserMaker.ascx"; } }

        protected override void OnInit(EventArgs e)
        {
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "studio_usermaker_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebSkin.GetUserSkin().GetAbsoluteWebPath("usercontrols/users/usermaker/css/<theme_folder>/usermaker.css") + "\">", false);
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "studio_usermaker_textoverflow", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + VirtualPathUtility.ToAbsolute("~/usercontrols/users/usermaker/css/" + WebSkin.DefaultSkin.FolderName + "/usermaker.text-overflow.css") + "\" />", false);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            UserMakerContainer.Options.IsPopup = true;

            Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "usermaker_script", WebPath.GetPath("usercontrols/users/usermaker/js/usermaker.js"));
            Page.ClientScript.RegisterClientScriptInclude(this.GetType(), "usermaker_img_script", WebPath.GetPath("js/ajaxupload.3.5.js"));

            string script = " StudioUserMaker.NewUserDialogTitle = '" + CustomNamingPeople.Substitute<Resources.Resource>("NewUserDialogTitle") + "';";
            script += " StudioUserMaker.EditUserDialogTitle = '" + Resources.Resource.EditUserDialogTitle + "';";
            script += " StudioUserMaker.AddButton = '" + Resources.Resource.AddButton + "';";
            script += " StudioUserMaker.SaveButton = '" + Resources.Resource.SaveButton + "';";
            script += " StudioUserMaker.SignUpButton = '" + Resources.Resource.LoginRegistryButton + "';";
            script += " StudioUserMaker.MaleStatus = '" + Resources.Resource.MaleSexStatus + "';";
            script += " StudioUserMaker.FemaleStatus = '" + Resources.Resource.FemaleSexStatus + "';";
            script += " StudioUserMaker.InfoPanelID = '" + UserMakerContainer.ClientID + "_InfoPanel';";

            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "usermaker_init_script", script, true);
            AjaxPro.Utility.RegisterTypeForAjax(this.GetType());

            _jqueryDateMask.Value = DateTimeExtension.DateMaskForJQuery;

            UserMakerContainer.Options.InfoMessageText = "";
            UserMakerContainer.Options.InfoType = ASC.Web.Controls.InfoType.Alert;

            ViewSwitcher PropertiesTabs = new ViewSwitcher();
            PropertiesTabs.TabItems.Add(new ViewSwitcherTabItem()
            {
                TabName = Resources.Resource.GeneralProperties,
                DivID = "idMajorProperties",
                IsSelected = true,
                SkipRender = true
            });

            PropertiesTabs.TabItems.Add(new ViewSwitcherTabItem()
            {
                TabName = Resources.Resource.PersonalProperties,
                DivID = "idSocialProperties",
                SkipRender = true
            });

            idTabsContainer.Controls.Add(PropertiesTabs);
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

        public class UserInfoEx
        {
            public UserInfo Info { get; set; }
            public List<List<String>> Contacts { get; set; }
            public string Pwd { get; set; }
            public string PhotoPath { get; set; }

            public Guid DepartmentID { get; set; }

            public string Department { get; set; }

            public string WorkFromDate
            {
                get
                {
                    if (this.Info != null && this.Info.WorkFromDate.HasValue)
                        return this.Info.WorkFromDate.Value.ToString(DateTimeExtension.ShortDatePattern);
                    return "";
                }
                set
                {
                    if (this.Info != null)
                    {
                        var oldValue = this.Info.WorkFromDate;
                        try
                        {
                            this.Info.WorkFromDate = Convert.ToDateTime(value);
                        }
                        catch
                        {
                            this.Info.WorkFromDate = oldValue;
                        }
                    }
                }
            }

            public string BirthDate
            {
                get
                {
                    if (this.Info != null && this.Info.BirthDate.HasValue)
                        return this.Info.BirthDate.Value.ToString(DateTimeExtension.ShortDatePattern);
                    return "";
                }
                set
                {
                    if (this.Info != null)
                    {
                        var oldValue = this.Info.BirthDate;
                        try
                        {
                            this.Info.BirthDate = Convert.ToDateTime(value);
                        }
                        catch
                        {
                            this.Info.BirthDate = oldValue;
                        }
                    }
                }
            }

            public UserInfoEx()
            {
                this.Info = new UserInfo()
                {
                    Sex = null,
                    Status = EmployeeStatus.Active,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Email = string.Empty,
                    Title = string.Empty,
                    Location = string.Empty,
                };
                Contacts = new List<List<String>>();
                Pwd = string.Empty;
                Department = string.Empty;
            }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse SaveUser(UserInfoEx userInfoEx, string photoPath)
        {
            AjaxResponse resp = new AjaxResponse();

            if (!SecurityContext.IsAuthenticated)
            {
                resp.rs1 = "0";
                return resp;
            }

            bool isNew = userInfoEx.Info.ID.Equals(Guid.Empty);

            userInfoEx.Info.Email = (userInfoEx.Info.Email ?? "").Trim();

            if (String.IsNullOrEmpty(userInfoEx.Info.FirstName.Trim()))
            {
                resp.rs1 = "0";
                resp.rs2 = "<div>" + Resources.Resource.ErrorEmptyUserFirstName + "</div>";
                return resp;
            }
            else if (String.IsNullOrEmpty(userInfoEx.Info.LastName.Trim()))
            {
                resp.rs1 = "0";
                resp.rs2 = "<div>" + Resources.Resource.ErrorEmptyUserLastName + "</div>";
                return resp;
            }
            else if (String.IsNullOrEmpty(userInfoEx.Info.Email) || !userInfoEx.Info.Email.TestEmailRegex())
            {
                resp.rs1 = "0";
                resp.rs2 = "<div>" + Resources.Resource.ErrorNotCorrectEmail + "</div>";
                return resp;
            }

            if (isNew)
                userInfoEx.Pwd = UserManagerWrapper.GeneratePassword();

            try
            {
                bool self = SecurityContext.CurrentAccount.ID.Equals(userInfoEx.Info.ID);

                GroupInfo newDepartment = CoreContext.GroupManager.GetGroupInfo(userInfoEx.DepartmentID);
                if (newDepartment == ASC.Core.Users.Constants.LostGroupInfo)
                    newDepartment = null;
                else
                    userInfoEx.Info.Department = newDepartment.Name;

                UserInfo newUserInfo = null;

                if (isNew && SecurityContext.IsAuthenticated
                    && SecurityContext.CheckPermissions(ASC.Core.Users.Constants.Action_AddRemoveUser))
                {
                    bool disableEditGroups = false;
                    if (!SecurityContext.CheckPermissions(ASC.Core.Users.Constants.Action_EditGroups))
                    {
                        userInfoEx.Info.Title = "";
                        disableEditGroups = true;
                    }

                    newUserInfo = UserManagerWrapper.AddUser(userInfoEx.Info, userInfoEx.Pwd);

                    if (disableEditGroups == false && userInfoEx.DepartmentID != Guid.Empty)
                        CoreContext.UserManager.AddUserIntoGroup(newUserInfo.ID, userInfoEx.DepartmentID);

                    resp.rs3 = "add_user";
                    resp.rs10 = CustomNamingPeople.Substitute<Resources.Resource>("UserMakerAddUser");
                }
                else if (SecurityContext.CheckPermissions(ASC.Core.Users.Constants.Action_EditUser)
                        ||
                       (self && SecurityContext.CheckPermissions(
                                   new SecurityObjectId<UserInfo>(SecurityContext.CurrentAccount.ID),
                                   new UserSecurityProvider(),
                                   ASC.Core.Users.Constants.Action_EditUser)))
                {
                    newUserInfo = (UserInfo)((ICloneable)CoreContext.UserManager.GetUsers(userInfoEx.Info.ID)).Clone();
                    newUserInfo.FirstName = userInfoEx.Info.FirstName;
                    newUserInfo.LastName = userInfoEx.Info.LastName;
                    newUserInfo.Sex = userInfoEx.Info.Sex;

                    newUserInfo.Title = userInfoEx.Info.Title;
                    newUserInfo.BirthDate = userInfoEx.Info.BirthDate;
                    newUserInfo.WorkFromDate = userInfoEx.Info.WorkFromDate;

                    if (SecurityContext.CheckPermissions(ASC.Core.Users.Constants.Action_AddRemoveUser))
                        newUserInfo.Email = userInfoEx.Info.Email;

                    newUserInfo.Notes = userInfoEx.Info.Notes;
                    newUserInfo.Department = userInfoEx.Info.Department;

                    newUserInfo.Location = userInfoEx.Info.Location;

                    newUserInfo.Contacts.Clear();
                    userInfoEx.Info.Contacts.ForEach(c => newUserInfo.Contacts.Add(c));

                    if (SecurityContext.CheckPermissions(ASC.Core.Users.Constants.Action_EditGroups))
                    {
                        var oldDep = GetDepartmentForUser(userInfoEx.Info.ID);

                        if (oldDep != null && !oldDep.ID.Equals(userInfoEx.DepartmentID))
                        {
                            CoreContext.UserManager.RemoveUserFromGroup(newUserInfo.ID, oldDep.ID);
                            newUserInfo.Department = "";
                        }

                        if (((oldDep != null && !oldDep.ID.Equals(userInfoEx.DepartmentID)) || oldDep == null)
                                && userInfoEx.DepartmentID != Guid.Empty)
                        {
                            CoreContext.UserManager.AddUserIntoGroup(newUserInfo.ID, userInfoEx.DepartmentID);

                            var dep = CoreContext.GroupManager.GetGroupInfo(userInfoEx.DepartmentID);
                            newUserInfo.Department = dep.Name;
                        }
                    }

                    UserManagerWrapper.SaveUserInfo(newUserInfo);

                    resp.rs3 = "edit_user";
                    resp.rs10 = Resources.Resource.UserMakerEditUser;
                    resp.rs5 = SecurityContext.CurrentAccount.ID.Equals(newUserInfo.ID) ? "1" : "";

                    if (self && !CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, ASC.Core.Users.Constants.GroupAdmin.ID))
                        StudioNotifyService.Instance.SendMsgToAdminAboutProfileUpdated();

                }
                else
                {
                    resp.rs1 = "0";
                    return resp;
                }

                if (!String.IsNullOrEmpty(photoPath))
                {
                    var fileName = Path.GetFileName(photoPath);
                    var data = UserPhotoManager.GetTempPhotoData(fileName);
                    UserPhotoManager.SaveOrUpdatePhoto(newUserInfo.ID, data);
                    try
                    {
                        UserPhotoManager.RemoveTempPhoto(fileName);
                    }
                    catch { };
                }

                resp.rs1 = "1";
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs2 = "<div>" + e.Message.HtmlEncode() + "</div>";
            }
            return resp;
        }

        private GroupInfo GetDepartmentForUser(Guid userID)
        {
            var groups = CoreContext.UserManager.GetUserGroups(userID);
            if (groups.Length > 0)
                return groups[0];

            return null;
        }

        #region retriving user information

        private List<String> GetSocialContact(XmlElement node)
        {
            String type = node != null ? node.Attributes["id"].Value : "";
            String pattern = node != null ? node.GetElementsByTagName("pattern")[0].InnerXml : "";

            List<String> contact = new List<String>();
            contact.Add(type);
            contact.Add(pattern);
            return contact;
        }

        protected UserInfoEx LoadUserInfoEx(Guid userID)
        {
            var dep = GetDepartmentForUser(userID);

            UserInfoEx ui = new UserInfoEx()
            {
                Info = CoreContext.UserManager.GetUsers(userID),
                PhotoPath = UserPhotoManager.GetPhotoAbsoluteWebPath(userID),
                DepartmentID = (dep != null) ? dep.ID : Guid.Empty,
                Department = (dep != null) ? dep.Name.HtmlEncode() : "",
            };
            XmlNodeList nodes = SocialContactsManager.xmlSocialContacts.GetElementsByTagName("contact");
            if (nodes == null)
                return ui;

            for (Int32 i = 0, n = nodes.Count; i < n; i++)
            {
                ui.Contacts.Add(GetSocialContact((XmlElement)nodes[i]));
            }

            return ui;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public UserInfoEx LoadUserInfo(Guid userID)
        {
            return LoadUserInfoEx(userID);
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public void RemoveTempPhoto(string photoPath)
        {

            if (!String.IsNullOrEmpty(photoPath))
            {
                var fileName = Path.GetFileName(photoPath);
                try
                {
                    UserPhotoManager.RemoveTempPhoto(fileName);
                }
                catch { };
            }
        }

        [AjaxMethod]
        public UserInfoEx GetUserInfoClass()
        {
            return new UserInfoEx()
            {
                PhotoPath = UserPhotoManager.GetDefaultPhotoAbsoluteWebPath()
            };
        }
        #endregion

        public static UserMaker AddOnlyOne(Page page, Control parent)
        {
            bool allreadyexists;
            var um = CreateSingleOnPage(page, out allreadyexists);
            if (!allreadyexists)
                parent.Controls.Add(um);

            return um;
        }
        static UserMaker CreateSingleOnPage(Page page, out bool allreadyexists)
        {
            string key = "created-user-maker";
            UserMaker finded = null;
            if (HttpContext.Current != null)
                finded = HttpContext.Current.Items[key] as UserMaker;

            allreadyexists = finded != null;

            if (finded == null)
                finded = (UserMaker)(HttpContext.Current.Items[key] = page.LoadControl(Location));

            return finded;
        }
    }
}