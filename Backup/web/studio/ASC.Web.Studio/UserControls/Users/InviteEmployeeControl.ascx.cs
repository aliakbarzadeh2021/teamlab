using System;
using System.Security.Permissions;
using System.Web;
using ASC.Web.Core.Security.Ajax;
using AjaxPro;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Utility;
using ASC.Core;
using System.Text;
using ASC.Core.Users;
using System.Collections;
using System.Collections.Generic;
using ASC.Core.Tenants;
using ASC.Web.Studio.Core.Users;
using ASC.Data.Storage;
using ASC.Web.Core.Utility.Skins;


namespace ASC.Web.Studio.UserControls.Users
{
	[AjaxNamespace("InviteEmployeeControl")]
	public partial class InviteEmployeeControl : System.Web.UI.UserControl
	{
		protected bool _defaultInviteAllRightsValue = false;

		public static string Location
		{
			get { return "~/UserControls/Users/InviteEmployeeControl.ascx"; }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			_inviteEmpContainer.Options.IsPopup = true;
			_inviteEmpContainer.Options.InfoType = ASC.Web.Controls.InfoType.Alert;
			_inviteEmpContainer.Options.InfoMessageText = "";

			_inviteJoinContainer.Options.IsPopup = true;

			AjaxPro.Utility.RegisterTypeForAjax(this.GetType());

			
			var userMakerCss = "studio_usermaker_style";
			if (!Page.ClientScript.IsClientScriptBlockRegistered(userMakerCss))
			{
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), userMakerCss, "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebSkin.GetUserSkin().GetAbsoluteWebPath("usercontrols/users/usermaker/css/<theme_folder>/usermaker.css") + "\">", false);
			}

			_defaultInviteAllRightsValue = CoreContext.UserManager.GetUsers().Length < 5;
		}

		public static string RenderTrustedDominTitle()
		{
			var tenant = CoreContext.TenantManager.GetCurrentTenant();
            if (tenant.TrustedDomainsType == TenantTrustedDomainsType.Custom)
            {
                string domains = "";
                int i = 0;
                foreach (var d in tenant.TrustedDomains)
                {
                    if (i != 0)
                        domains += ", ";

                    domains += d;
                    i++;
                }
                return String.Format(Resources.Resource.TrustedDomainsInviteTitle, domains);
            }
            else if (tenant.TrustedDomainsType == TenantTrustedDomainsType.All)
                return Resources.Resource.SignInFromAnyDomainInviteTitle;

            return "";
		}

		public static string RenderTrustedDominAuthTitle()
		{
			var tenant = CoreContext.TenantManager.GetCurrentTenant();
            if (tenant.TrustedDomainsType == TenantTrustedDomainsType.Custom)
            {
                string domains = "";
                int i = 0;
                foreach (var d in tenant.TrustedDomains)
                {
                    if (i != 0)
                        domains += ", ";

                    domains += d;
                    i++;
                }
                return String.Format(Resources.Resource.TrustedDomainAuthTitle, domains);
            }
            else if(tenant.TrustedDomainsType == TenantTrustedDomainsType.All)
                return Resources.Resource.SignInFromAnyDomainAuthTitle;

            return "";
		}

		[AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        [AjaxSecurityPassthrough]
		public AjaxResponse SendJoinInviteMail(string email)
		{
           
			email = (email ?? "").Trim();
			AjaxResponse resp = new AjaxResponse();
			resp.rs1 = "0";

			try
			{
				if (!email.TestEmailRegex())
					resp.rs2 = Resources.Resource.ErrorNotCorrectEmail;

				var user = CoreContext.UserManager.GetUserByEmail(email);
				if (!user.ID.Equals(ASC.Core.Users.Constants.LostUser.ID))
				{
					resp.rs1 = "0";
					resp.rs2 = CustomNamingPeople.Substitute<Resources.Resource>("ErrorEmailAlreadyExists");
					return resp;
				}

				var tenant = CoreContext.TenantManager.GetCurrentTenant();

                if (tenant.TrustedDomainsType == TenantTrustedDomainsType.Custom)
                {
                    foreach (var d in tenant.TrustedDomains)
                    {
                        if (email.EndsWith("@" + d, StringComparison.InvariantCultureIgnoreCase))
                        {
                            StudioNotifyService.Instance.InviteUsers(email, "", true, false);
                            resp.rs1 = "1";
                            resp.rs2 = Resources.Resource.FinishInviteJoinEmailMessage;
                            return resp;
                        }
                    }
                }
                else if (tenant.TrustedDomainsType == TenantTrustedDomainsType.All)
                {
                    StudioNotifyService.Instance.InviteUsers(email, "", true, false);
                    resp.rs1 = "1";
                    resp.rs2 = Resources.Resource.FinishInviteJoinEmailMessage;
                    return resp;
                }

				resp.rs2 = Resources.Resource.ErrorNotCorrectEmail;


			}
			catch (Exception e)
			{
				resp.rs2 = HttpUtility.HtmlEncode(e.Message);
			}

			return resp;
		}

		[AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
		public AjaxResponse SendInviteMails(string text, string emails, bool withFullAccessPrivileges)
		{
            SecurityContext.DemandPermissions(ASC.Core.Users.Constants.Action_AddRemoveUser);

			AjaxResponse resp = new AjaxResponse();
			resp.rs1 = "0";
			try
			{
                SecurityContext.DemandPermissions(ASC.Core.Users.Constants.Action_AddRemoveUser);

				string[] addresses = null;
				try
				{
					addresses = StudioNotifyService.Instance.GetEmails(emails);
					if (addresses == null || addresses.Length == 0)
						resp.rs2 = Resources.Resource.ErrorNoEmailsForInvite;
					else
					{
						foreach (var emailStr in addresses)
						{
							var email = (emailStr ?? "").Trim();
							if (!email.TestEmailRegex())
							{
								resp.rs1 = "0";
								resp.rs2 = email + " - " + Resources.Resource.ErrorNotCorrectEmail;
								return resp;
							}

							var user = CoreContext.UserManager.GetUserByEmail(email);
							if (!user.ID.Equals(ASC.Core.Users.Constants.LostUser.ID))
							{
								resp.rs1 = "0";
								resp.rs2 = email + " - " + CustomNamingPeople.Substitute<Resources.Resource>("ErrorEmailAlreadyExists");
								return resp;
							}
						}

						StudioNotifyService.Instance.InviteUsers(emails, text, false, withFullAccessPrivileges);
						resp.rs1 = "1";
						resp.rs2 = Resources.Resource.FinishInviteEmailTitle;
					}
				}
				catch
				{
					resp.rs2 = Resources.Resource.ErrorNoEmailsForInvite;
				}

			}
			catch (Exception e)
			{
				resp.rs2 = HttpUtility.HtmlEncode(e.Message);
			}

			return resp;
		}

		public class ContactInfo
		{
			public string FirstName { get; set; }
			public string LastName { get; set; }
			public string Email { get; set; }
			public int ID { get; set; }

			public bool Add { get; set; }
			public bool Invite { get; set; }

			public bool Skip { get; set; }
		}


		[AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
		public AjaxResponse InviteUsersFromWeb(List<ContactInfo> users, Guid deptID, bool withFullAccessPrivileges)
		{
            SecurityContext.DemandPermissions(ASC.Core.Users.Constants.Action_AddRemoveUser);


			AjaxResponse resp = new AjaxResponse();
			resp.rs1 = "0";
			try
			{
                SecurityContext.DemandPermissions(ASC.Core.Users.Constants.Action_AddRemoveUser);

				var usersToInvite = new List<UserInfo>();
				foreach (var curUser in users)
				{
					if (curUser.Skip || !(curUser.Invite || curUser.Add))
					{
						continue;
					}

					var email = curUser.Email.Trim();
					if (!email.TestEmailRegex())
					{
						resp.rs1 = "0";
						resp.rs2 = email + " - " + Resources.Resource.ErrorNotCorrectEmail;
						continue;
					}

					var user = CoreContext.UserManager.GetUserByEmail(email);
					if (user.ID.Equals(ASC.Core.Users.Constants.LostUser.ID))
					{
						var curUserInfo = new UserInfo() { FirstName = curUser.FirstName, LastName = curUser.LastName, Email = curUser.Email, ID = Guid.NewGuid() };

						if (curUser.Add && !string.IsNullOrEmpty(curUser.FirstName) && !string.IsNullOrEmpty(curUser.LastName))
						{
							if (!curUserInfo.WorkFromDate.HasValue)
								curUserInfo.WorkFromDate = TenantUtil.DateTimeNow();

							try
							{
								UserManagerWrapper.AddUser(curUserInfo, UserManagerWrapper.GeneratePassword());
								if (withFullAccessPrivileges)
								{
									ASC.Core.CoreContext.UserManager.AddUserIntoGroup(curUserInfo.ID, ASC.Core.Users.Constants.GroupAdmin.ID);
								}
								if (deptID != null && !Guid.Empty.Equals(deptID))
								{
									ASC.Core.CoreContext.UserManager.AddUserIntoGroup(curUserInfo.ID, deptID);
								}
							}
							catch(Exception e) {
								resp.rs2 = HttpUtility.HtmlEncode(e.Message);
							}
							continue;
						}
						
						if (curUser.Invite)
						{
							if (deptID != null && !Guid.Empty.Equals(deptID))
							{
								curUserInfo.Department = deptID.ToString();
							}
							usersToInvite.Add(curUserInfo);
						}
					}
				}

				StudioNotifyService.Instance.InviteOrJoinUsers(usersToInvite, string.Empty, false, withFullAccessPrivileges);
				resp.rs1 = "1";
				if (string.IsNullOrEmpty(resp.rs2))
				{
					resp.rs2 = Resources.Resource.FinishInviteEmailTitle;
				}
			}
			catch(Exception e)
			{
				resp.rs2 = HttpUtility.HtmlEncode(e.Message);
			}

			return resp;
		}
	}
}