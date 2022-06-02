using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Notify;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Security.Cryptography;
using ASC.Web.Core;
using ASC.Web.Core.Users;
using ASC.Web.Core.Users.Activity;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Core.Notify
{
    class StudioNotifyService
    {
        private static StudioNotifyService instance;

        public static StudioNotifyService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (typeof(StudioNotifyService))
                    {
                        if (instance == null) instance = new StudioNotifyService();
                    }
                }
                return instance;
            }
        }


        private readonly INotifyClient client;
        internal readonly StudioNotifySource source;

        public StudioNotifyService()
        {
            source = new StudioNotifySource();
            client = WorkContext.NotifyContext.NotifyService.RegisterClient(source);
        }

        public IRecipient[] AdminsAsRecipient()
        {
            return CoreContext.UserManager.GetUsersByGroup(ASC.Core.Users.Constants.GroupAdmin.ID)
                .Select(u => new DirectRecipient(u.ID.ToString(), u.DisplayUserName()))
                .ToArray();
        }

        public void RegisterSendMethod()
        {
            var now = DateTime.UtcNow;
            client.RegisterSendMethod(SendMsgWhatsNew, TimeSpan.FromHours(1), new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0));
        }

        #region What's New
        private void SendMsgWhatsNew(DateTime scheduleDate)
        {
            SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);

            var products = new List<IProduct>(ProductManager.Instance.Products);
            if (products.Count == 0) return;
            products.Sort((p1, p2) => string.Compare(p1.ProductName, p2.ProductName));

            var tenants = UserActivityManager.GetChangedTenants(scheduleDate.AddDays(-1), scheduleDate);

            foreach (var tenantid in tenants)
            {
                var tenant = CoreContext.TenantManager.GetTenant(tenantid);
                if (tenant == null || tenant.Status != TenantStatus.Active) continue;
                if (!TimeToSendWhatsNew(TenantUtil.DateTimeFromUtc(tenant, scheduleDate))) continue;

                CoreContext.TenantManager.SetCurrentTenant(tenant);
                foreach (var user in CoreContext.UserManager.GetUsers())
                {
                    var recipient = new DirectRecipient(user.ID.ToString(), user.UserName);
                    if (!IsSubscribeToWhatsNew(recipient)) continue;

                    var activities = new Dictionary<string, IList<WhatsNewUserActivity>>();

                    foreach (var product in products)
                    {
                        if (product.Context.WhatsNewHandler == null || string.IsNullOrEmpty(product.ProductName)) continue;

                        if (!activities.ContainsKey(product.ProductName))
                        {
                            var whatsNewActivities = product.Context.WhatsNewHandler.GetUserActivities(user.ID, scheduleDate.AddDays(-1), scheduleDate);
                            if (whatsNewActivities != null && whatsNewActivities.Count != 0)
                            {
                                activities.Add(product.ProductName, whatsNewActivities);
                            }
                        }
                    }

                    if (0 < activities.Count)
                    {
                        client.SendNoticeAsync(
                            Constants.ActionSendWhatsNew, null, recipient, null,
                            new TagValue(Constants.TagActivities, activities),
                            new TagValue(Constants.TagDate, scheduleDate.AddDays(-1).ToString("M"))
                        );
                    }
                }
            }
        }

        private bool TimeToSendWhatsNew(DateTime currentTime)
        {
            var hourToSend = 7;
            if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings["whatsnewtime"]))
            {
                var hour = 0;
                if (int.TryParse(WebConfigurationManager.AppSettings["whatsnewtime"], out hour))
                {
                    hourToSend = hour;
                }
            }
            return currentTime.Hour == hourToSend;
        }

        /// <summary>
        /// Checks if user with the specified userID is subscribed to what's new
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public bool IsSubscribeToWhatsNew(Guid userID)
        {
            return IsSubscribeToWhatsNew(ToRecipient(userID));
        }
        /// <summary>
        /// Checks if the user with the specified userID is subscribed to what's new
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        private bool IsSubscribeToWhatsNew(IRecipient recipient)
        {
            if (recipient == null) return false;

            var subscriptionProvider = source.GetSubscriptionProvider();
            return subscriptionProvider.IsSubscribed(Constants.ActionSendWhatsNew, recipient, null);
        }

        /// <summary>
        /// Checks if user with the specified userID is subscribed to admin notifications
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public bool IsSubscribeToAdminNotify(Guid userID)
        {
            return source.GetSubscriptionProvider().IsSubscribed(Constants.ActionAdminNotify, ToRecipient(userID), null);
        }

        public void SubscribeToWhatsNew(Guid userID, bool isSubscribe)
        {
            var recipient = ToRecipient(userID);
            if (recipient != null)
            {
                if (isSubscribe)
                    source.GetSubscriptionProvider().Subscribe(Constants.ActionSendWhatsNew, null, recipient);
                else
                    source.GetSubscriptionProvider().UnSubscribe(Constants.ActionSendWhatsNew, null, recipient);
            }
        }
        public void SubscribeToAdminNotify(Guid userID, bool isSubscribe)
        {
            var recipient = source.GetRecipientsProvider().GetRecipient(userID.ToString());
            if (recipient != null)
            {
                if (isSubscribe)
                    source.GetSubscriptionProvider().Subscribe(Constants.ActionAdminNotify, null, recipient);
                else
                    source.GetSubscriptionProvider().UnSubscribe(Constants.ActionAdminNotify, null, recipient);
            }
        }

        private IRecipient ToRecipient(Guid userID)
        {
            return source.GetRecipientsProvider().GetRecipient(userID.ToString());
        }
        #endregion

        static string EMailSenderName { get { return ASC.Core.Configuration.Constants.NotifyEMailSenderSysName; } }
        static string[] AllSenderNames { get { return Array.ConvertAll(WorkContext.AvailableNotifySenders, (sndr) => sndr.ID); } }

        #region web.studio notifications


        #region admin notifications

        /// <summary>
        /// Notify about profile updated
        /// </summary>
        public void SendMsgToAdminAboutProfileUpdated()
        {
            client.SendNoticeAsync(Constants.ActionAdminNotify, null, null, new TagValue("UNDERLYING_ACTION", "self_profile_updated"));
        }
        /// <summary>
        /// Send about new user join
        /// </summary>
        /// <param name="newUserInfo"></param>
        /// <param name="password"></param>
        public void UserHasJoin()
        {
            client.SendNoticeAsync(Constants.ActionAdminNotify, null, null, new TagValue("UNDERLYING_ACTION", "user_has_join"));
        }
        #endregion


        /// <summary>
        /// notify user about his password changed
        /// </summary>
        public void UserPasswordChanged(Guid userID, string password)
        {
            UserInfo Author = null;
            UserInfo newUserInfo = CoreContext.UserManager.GetUsers(userID);

            if (CoreContext.UserManager.UserExists(SecurityContext.CurrentAccount.ID))
                Author = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            ISendInterceptor initInterceptor = null;
            if (Author != null)
            {
                initInterceptor = new InitiatorInterceptor(new[] { UserInfoAsRecipient(Author) });
                client.AddInterceptor(initInterceptor);
            }

            client.SendNoticeToAsync(
                           Constants.ActionPasswordChanged,
                           null,
                           new[] { UserInfoAsRecipient(newUserInfo) },
                           new[] { EMailSenderName },
                           null,
                           new TagValue(Constants.TagUserName, newUserInfo.DisplayUserName()),
                           new TagValue(Constants.TagUserEmail, newUserInfo.Email),
                           new TagValue(Constants.TagMyStaffLink, GetMyStaffLink()),
                           new TagValue(Constants.TagPassword, password)
                       );


            if (initInterceptor != null)
                client.RemoveInterceptor(initInterceptor.Name);
        }

        /// <summary>
        /// Notify user about his profile updated
        /// </summary>
        public void UserInfoUpdated(UserInfo prevUserInfo, UserInfo newUserInfo)
        {
            UserInfoUpdated(prevUserInfo, newUserInfo, null);
        }


        /// <summary>
        /// Notify user about his joining to portal after invite
        /// </summary>
        public void UserInfoAddedAfterInvite(UserInfo newUserInfo, string password)
        {
            if (CoreContext.UserManager.UserExists(newUserInfo.ID))
            {
                client.SendNoticeToAsync(
                            Constants.ActionYouAddedAfterInvite,
                            null,
                            new[] { UserInfoAsRecipient(newUserInfo) },
                            new[] { EMailSenderName },
                            null,
                            new TagValue(Constants.TagUserName, newUserInfo.DisplayUserName()),
                            new TagValue(Constants.TagUserEmail, newUserInfo.Email),
                            new TagValue(Constants.TagMyStaffLink, GetMyStaffLink()),
                            new TagValue(Constants.TagPassword, password)
                        );
            }
        }

        /// <summary>
        /// Notify user about his profile added
        /// </summary>
        public void UserInfoAdded(UserInfo newUserInfo, string password)
        {
            if (CoreContext.UserManager.UserExists(newUserInfo.ID))
            {
                client.SendNoticeToAsync(
                            Constants.ActionYouAdded,
                            null,
                            new[] { UserInfoAsRecipient(newUserInfo) },
                            new[] { EMailSenderName },
                            null,
                            new TagValue(Constants.TagUserName, newUserInfo.DisplayUserName()),
                            new TagValue(Constants.TagUserEmail, newUserInfo.Email),
                            new TagValue(Constants.TagMyStaffLink, GetMyStaffLink()),
                            new TagValue(Constants.TagPassword, password)
                        );
            }
        }

        /// <summary>
        /// Notify user about his profile updated
        /// </summary>
        public void UserInfoUpdated(UserInfo prevUserInfo, UserInfo newUserInfo, string password)
        {
            UserInfo Author = null;
            if (CoreContext.UserManager.UserExists(SecurityContext.CurrentAccount.ID))
                Author = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            ISendInterceptor initInterceptor = null;
            if (Author != null)
            {
                initInterceptor = new InitiatorInterceptor(new[] { UserInfoAsRecipient(Author) });
                client.AddInterceptor(initInterceptor);
            }

            client.SendNoticeToAsync(
                           Constants.ActionYourProfileUpdated,
                           null,
                           new[] { UserInfoAsRecipient(newUserInfo) },
                           AllSenderNames,
                           null,
                           new TagValue(Constants.TagProfileChanges, GetUserInfoChanges(prevUserInfo, newUserInfo)),
                           new TagValue(Constants.TagUserName, newUserInfo.DisplayUserName()),
                           new TagValue(Constants.TagMyStaffLink, GetMyStaffLink()),
                           new TagValue(Constants.TagPassword, password),
                           new TagValue(Constants.TagIsPasswordChange, password != null)
                       );

            if (initInterceptor != null)
                client.RemoveInterceptor(initInterceptor.Name);
        }

        /// <summary>
        /// Send new password to user
        /// </summary>
        public void SendUserPassword(UserInfo ui, string password)
        {
            client.SendNoticeToAsync(
                        Constants.ActionSendPassword,
                        null,
                        new[] { UserInfoAsRecipient(ui) },
                        new[] { EMailSenderName },
                        null,
                        new TagValue(Constants.TagPassword, password),
                        new TagValue(Constants.TagUserName, ui.DisplayUserName()),
                        new TagValue(Constants.TagUserEmail, ui.Email),
                        new TagValue(Constants.TagMyStaffLink, GetMyStaffLink()),
                        new TagValue(Constants.TagAuthor, (HttpContext.Current != null) ? HttpContext.Current.Request.UserHostAddress : null)
                        );
        }

        /// <summary>
        /// Send invites
        /// </summary>
        private void InviteOrJoinUsers(string[] emailList, string inviteMessage, bool join, bool withFullAccessPrivileges)
        {
            if (emailList == null)
                throw new ArgumentNullException("emailList");

            foreach (var email in emailList)
                SendInvite(new UserInfo() { Email = email }, inviteMessage, join, withFullAccessPrivileges);
        }


        /// <summary>
        /// Send invites
        /// </summary>
        public void InviteOrJoinUsers(IList<UserInfo> users, string inviteMessage, bool join, bool withFullAccessPrivileges)
        {
            if (users == null)
                throw new ArgumentNullException("users");

            foreach (var user in users)
                SendInvite(user, inviteMessage, join, withFullAccessPrivileges);
        }

        /// <summary>
        /// Send invites
        /// </summary>
        /// <param name="emailList">separator may be ' ', ',', ';', '\n\r'</param>
        public void InviteUsers(string emailList, string inviteMessage, bool join, bool withFullAccessPrivileges)
        {
            if (emailList == null) throw new ArgumentNullException("emailList");
            if (emailList == "") return;

            InviteOrJoinUsers(GetEmails(emailList), inviteMessage, join, withFullAccessPrivileges);
        }

        void SendInvite(UserInfo user, string inviteMessage, bool join, bool withFullAccessPrivileges)
        {
            var email = user.Email.ToLower();
            var validationKey = EmailValidationKeyProvider.GetEmailKey(
                    email + ConfirmType.EmpInvite.ToString().ToLower() + (withFullAccessPrivileges ? "allrights" : ""));


            string inviteUrl = String.Format("~/confirm.aspx?type={2}&email={0}&firstname={4}&lastname={5}&key={1}{3}{6}",
                                            email,																//0
                                            validationKey,													//1
                                            ConfirmType.EmpInvite.ToString().ToLower(),		//2
                                            withFullAccessPrivileges ? "&fap=1" : "",				//3
                                            HttpUtility.UrlEncode(user.FirstName),					//4
                                            HttpUtility.UrlEncode(user.LastName),					//5
                                            String.IsNullOrEmpty(user.Department) ? "" : "&deptID=" + user.Department); //6		

            client.SendNoticeToAsync(
                        join ? Constants.ActionJoinUsers : Constants.ActionInviteUsers,
                        null,
                        RecipientFromEmail(new string[] { email }),
                        new[] { EMailSenderName },
                        null,
                        new TagValue(Constants.TagUserName,
                            SecurityContext.IsAuthenticated ? DisplayUserSettings.GetFullUserName(SecurityContext.CurrentAccount.ID) : ((HttpContext.Current != null) ? HttpContext.Current.Request.UserHostAddress : null)
                            ),

                        new TagValue(Constants.TagInviteLink, CommonLinkUtility.GetFullAbsolutePath(inviteUrl)),

                        new TagValue(Constants.TagBody, inviteMessage ?? ""),

                        //User name tag
                        new TagValue(new Tag("UserDisplayName"), (user.DisplayUserName(true) ?? "").Trim())
                    );

        }

        /// <summary>
        /// Send message to user about backup complite with link
        /// </summary>
        /// <param name="user"></param>
        /// <param name="url"></param>
        /// <param name="availibleDue"></param>
        public void SendMsgBackupCompleted(UserInfo user, string url, DateTime availibleDue)
        {
            client.SendNoticeToAsync(
                        Constants.ActionBackupCreated,
                        null,
                        new[] { new DirectRecipient(user.ID.ToString(), user.DisplayUserName()) },
                        new[] { EMailSenderName },
                        null,
                        new TagValue(Constants.TagBackupUrl, url),
                        new TagValue(Constants.TagBackupHours, (int)(availibleDue - DateTime.UtcNow).TotalHours),
                        new TagValue(Constants.TagOwnerName, user.DisplayUserName())
                    );
        }
        /// <summary>
        /// Send message with portal deactivation link
        /// </summary>
        /// <param name="tOwner"></param>
        /// <param name="d_url"></param>
        /// <param name="a_url"></param>
        public void SendMsgPortalDeactivation(Tenant t, string d_url, string a_url)
        {
            var u = CoreContext.UserManager.GetUsers(t.OwnerId);
            client.SendNoticeToAsync(
                        Constants.ActionPortalDeactivate,
                        null,
                        new[] { u },
                        new[] { EMailSenderName },
                        null,
                        new TagValue(Constants.TagActivateUrl, a_url),
                        new TagValue(Constants.TagDeactivateUrl, d_url),
                        new TagValue(Constants.TagOwnerName, u.DisplayUserName(true))
                    );
        }

        /// <summary>
        /// Send message with portal deletion link
        /// </summary>
        /// <param name="tOwner"></param>
        /// <param name="url"></param>
        public void SendMsgPortalDeletion(Tenant t, string url)
        {
            var u = CoreContext.UserManager.GetUsers(t.OwnerId);
            client.SendNoticeToAsync(
                        Constants.ActionPortalDelete,
                        null,
                        new[] { u },
                        new[] { EMailSenderName },
                        null,
                        new TagValue(Constants.TagDeleteUrl, url),
                        new TagValue(Constants.TagOwnerName, u.DisplayUserName(true))
                    );
        }

        /// <summary>
        /// Send message with dns settings change.
        /// </summary>
        /// <param name="tOwner"></param>
        /// <param name="d_url"></param>
        /// <param name="a_url"></param>
        public void SendMsgDnsChange(Tenant t, string confirmDnsUpdateUrl, string portalAddress, string portalDns)
        {
            var u = CoreContext.UserManager.GetUsers(t.OwnerId);
            client.SendNoticeToAsync(
                        Constants.ActionDnsChange,
                        null,
                        new[] { u },
                        null,
                        new TagValue("ConfirmDnsUpdate", confirmDnsUpdateUrl),
                        new TagValue("PortalAddress", AddHttpToUrl(portalAddress)),
                        new TagValue("PortalDns", AddHttpToUrl(portalDns ?? string.Empty)),
                        new TagValue(Constants.TagOwnerName, u.DisplayUserName(true))
                    );
        }


        public void SendMsgConfirmChangeOwner(Tenant t, string newOwnerName, string confirmOwnerUpdateUrl)
        { 
            var u = CoreContext.UserManager.GetUsers(t.OwnerId);
            client.SendNoticeToAsync(
                        Constants.ActionConfirmOwnerChange,
                        null,
                        new[] { u },
                        new[] { EMailSenderName },
                        null,
                        new TagValue("ConfirmPortalOwnerUpdate", confirmOwnerUpdateUrl),
                        new TagValue(Constants.TagUserName, newOwnerName),
                        new TagValue(Constants.TagOwnerName, u.DisplayUserName(true))
                    );

            
        }

        private static string AddHttpToUrl(string url)
        {
            const string httpPrefix = "http://";
            if (!string.IsNullOrEmpty(url) && !url.StartsWith(httpPrefix))
            {
                url = httpPrefix + url;
            }
            return url;
        }

        #endregion

        #region  helper methods

        internal IDirectRecipient[] RecipientFromEmail(string[] emails)
        {
            return Array.ConvertAll<string, IDirectRecipient>(emails, (email) => new DirectRecipient(email, null, new[] { email }));
        }

        internal string[] GetEmails(string emailList)
        {
            if (String.IsNullOrEmpty(emailList)) throw new ArgumentNullException("emailList");

            return emailList.Split(new[] { " ", ",", ";", Environment.NewLine, "\n", "\n\r" }, StringSplitOptions.RemoveEmptyEntries);
        }

        internal IRecipient UserInfoAsRecipient(UserInfo userInfo)
        {
            return source.GetRecipientsProvider().GetRecipient(userInfo.ID.ToString());
        }

        internal string GetUserProfileLink(Guid userID)
        {

            return CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetUserProfile(userID, CommonLinkUtility.GetProductID()));
        }

        private static string GetMyStaffLink()
        {
            return CommonLinkUtility.GetMyStaff();
        }

        public class ProfileChangeItem
        {
            public ProfileChangeItem() { }
            public ProfileChangeItem(string field, string old, string new_)
            {
                Field = field;
                Old = old;
                New = new_;
            }
            public string Field { get; set; }
            public string Old { get; set; }
            public string New { get; set; }
        }
        internal static List<ProfileChangeItem> GetUserInfoChanges(UserInfo prevUserInfo, UserInfo newUserInfo)
        {
            List<ProfileChangeItem> result = new List<ProfileChangeItem>();
            if (prevUserInfo.FirstName != newUserInfo.FirstName)
                result.Add(new ProfileChangeItem(Resources.Resource.FirstName, prevUserInfo.FirstName, newUserInfo.FirstName));
            if (prevUserInfo.LastName != newUserInfo.LastName)
                result.Add(new ProfileChangeItem(Resources.Resource.LastName, prevUserInfo.LastName, newUserInfo.LastName));
            if (prevUserInfo.BirthDate != newUserInfo.BirthDate)
                result.Add(new ProfileChangeItem(
                    Resources.Resource.Birthdate,
                    prevUserInfo.BirthDate.HasValue ? prevUserInfo.BirthDate.Value.ToShortDateString() : "",
                    newUserInfo.BirthDate.HasValue ? newUserInfo.BirthDate.Value.ToShortDateString() : "")
                    );
            if (prevUserInfo.Department != newUserInfo.Department)
                result.Add(new ProfileChangeItem(CustomNamingPeople.Substitute<Resources.Resource>("Department"), prevUserInfo.Department ?? "", newUserInfo.Department ?? ""));
            if (prevUserInfo.Title != newUserInfo.Title)
                result.Add(new ProfileChangeItem(CustomNamingPeople.Substitute<Resources.Resource>("UserPost"), prevUserInfo.Title ?? "", newUserInfo.Title ?? ""));
            if (prevUserInfo.WorkFromDate != newUserInfo.WorkFromDate)
                result.Add(new ProfileChangeItem(
                    ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<Resources.Resource>("WorkFromDate"),
                    prevUserInfo.WorkFromDate.HasValue ? prevUserInfo.WorkFromDate.Value.ToShortDateString() : "",
                    newUserInfo.WorkFromDate.HasValue ? newUserInfo.WorkFromDate.Value.ToShortDateString() : "")
                    );
            if (prevUserInfo.Email != newUserInfo.Email)
                result.Add(new ProfileChangeItem(Resources.Resource.Email, prevUserInfo.Email ?? "", newUserInfo.Email ?? ""));

            return result;
        }
        #endregion
    }
}

