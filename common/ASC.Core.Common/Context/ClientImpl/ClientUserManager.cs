using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Security;
using ASC.Core.Tenants;
using ASC.Core.Users;

namespace ASC.Core
{
    class ClientUserManager : IUserManagerClient, IGroupManagerClient
    {
        private readonly IUserService userService;

        private readonly IDictionary<Guid, UserInfo> systemUsers;


        public ClientUserManager(IUserService service)
        {
            this.userService = service;

            systemUsers = Configuration.Constants.SystemAccounts.ToDictionary(a => a.ID, a => new UserInfo { ID = a.ID, LastName = a.Name });
            systemUsers[Constants.LostUser.ID] = Constants.LostUser;
        }


        #region Users

        public UserInfo[] GetUsers()
        {
            return GetUsers(EmployeeStatus.Default);
        }

        public UserInfo[] GetUsers(EmployeeStatus status)
        {
            return GetUsersInternal()
                .Where(u => (u.Status & status) == u.Status)
                .ToArray();
        }

        public string[] GetUserNames(EmployeeStatus status)
        {
            return GetUsers(status)
                .Select(u => u.UserName)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
        }

        public UserInfo GetUserByUserName(string username)
        {
            return GetUsersInternal()
                .SingleOrDefault(u => string.Compare(u.UserName, username, StringComparison.CurrentCultureIgnoreCase) == 0) ?? Constants.LostUser;
        }

        public bool IsUserNameExists(string username)
        {
            return GetUserNames(EmployeeStatus.All)
                .Contains(username, StringComparer.CurrentCultureIgnoreCase);
        }

        public UserInfo GetUsers(Guid id)
        {
            if (systemUsers.ContainsKey(id)) return systemUsers[id];
            var u = userService.GetUser(CoreContext.TenantManager.GetCurrentTenant().TenantId, id);
            return u != null && !u.Removed ? u : Constants.LostUser;
        }

        public UserInfo GetUsers(int tenant, string login, string passwordHash)
        {
            var u = userService.GetUser(tenant, login, passwordHash);
            return u != null && !u.Removed ? u : Constants.LostUser;
        }

        public bool UserExists(Guid id)
        {
            return !GetUsers(id).Equals(Constants.LostUser);
        }

        public UserInfo GetUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return Constants.LostUser;

            return GetUsersInternal()
                .SingleOrDefault(u => string.Compare(u.Email, email, StringComparison.CurrentCultureIgnoreCase) == 0) ?? Constants.LostUser;
        }

        public UserInfo[] Search(string text, EmployeeStatus status)
        {
            return Search(text, status, Guid.Empty);
        }

        public UserInfo[] Search(string text, EmployeeStatus status, Guid groupId)
        {
            if (text == null || text.Trim() == string.Empty) return new UserInfo[0];

            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return new UserInfo[0];

            var users = groupId == Guid.Empty ?
                GetUsers(status) :
                GetUsersByGroup(groupId).Where(u => (u.Status & status) == status);

            var findUsers = new List<UserInfo>();
            var properties = new string[3];
            foreach (var user in users)
            {
                properties[0] = user.LastName ?? string.Empty;
                properties[1] = user.FirstName ?? string.Empty;
                properties[2] = user.Title ?? string.Empty;
                if (IsPropertiesContainsWords(properties, words))
                {
                    findUsers.Add(user);
                }
            }
            return findUsers.ToArray();
        }

        public UserInfo SaveUserInfo(UserInfo u)
        {
            if (systemUsers.ContainsKey(u.ID)) return systemUsers[u.ID];
            if (u.ID == Guid.Empty) SecurityContext.DemandPermissions(Constants.Action_AddRemoveUser);
            else SecurityContext.DemandPermissions(new SecurityObjectId<UserInfo>(u.ID), new UserSecurityProvider(), Constants.Action_EditUser);

            return userService.SaveUser(CoreContext.TenantManager.GetCurrentTenant().TenantId, u);
        }

        public void DeleteUser(Guid id)
        {
            if (systemUsers.ContainsKey(id)) return;
            SecurityContext.DemandPermissions(Constants.Action_AddRemoveUser);
            if (id == CoreContext.TenantManager.GetCurrentTenant().OwnerId)
            {
                throw new InvalidOperationException("Can not remove tenant owner.");
            }

            userService.RemoveUser(CoreContext.TenantManager.GetCurrentTenant().TenantId, id);
        }

        public void SaveUserPhoto(Guid id, Guid notused, byte[] photo)
        {
            if (systemUsers.ContainsKey(id)) return;
            SecurityContext.DemandPermissions(new SecurityObjectId<UserInfo>(id), new UserSecurityProvider(), Constants.Action_EditUser);

            userService.SetUserPhoto(CoreContext.TenantManager.GetCurrentTenant().TenantId, id, photo);
        }

        public byte[] GetUserPhoto(Guid id, Guid notused)
        {
            if (systemUsers.ContainsKey(id)) return null;
            return userService.GetUserPhoto(CoreContext.TenantManager.GetCurrentTenant().TenantId, id);
        }

        public GroupInfo[] GetUserGroups(Guid id)
        {
            return GetUserGroups(id, Guid.Empty);
        }

        public GroupInfo[] GetUserGroups(Guid id, Guid categoryID)
        {
            return GetUserGroups(id, IncludeType.Distinct, categoryID);
        }

        public GroupInfo[] GetUserGroups(Guid userID, IncludeType includeType)
        {
            return GetUserGroups(userID, includeType, null);
        }

        private GroupInfo[] GetUserGroups(Guid userID, IncludeType includeType, Guid? categoryId)
        {
            var result = new List<GroupInfo>();
            var distinctUserGroups = new List<GroupInfo>();

            var refs = GetRefsInternal();
            foreach (var g in GetGroupsInternal().Where(g => !categoryId.HasValue || g.CategoryID == categoryId))
            {
                if (IsUserInGroupInternal(userID, g.ID, refs)) distinctUserGroups.Add(g);
            }

            if (IncludeType.Distinct == (includeType & IncludeType.Distinct))
            {
                result.AddRange(distinctUserGroups);
            }

            if (IncludeType.InParent == (includeType & IncludeType.InParent))
            {
                foreach (var group in distinctUserGroups)
                {
                    var current = group.Parent;
                    while (current != null)
                    {
                        if (!result.Contains(current)) result.Add(current);
                        current = current.Parent;
                    }
                }
            }

            if (IncludeType.InChild == (includeType & IncludeType.InChild))
            {
                distinctUserGroups.ForEach(g => RecursiveAddChildGroups(g, distinctUserGroups));
            }

            return result.ToArray();
        }

        public bool IsUserInGroup(Guid userId, Guid groupId)
        {
            return IsUserInGroupInternal(userId, groupId, GetRefsInternal());
        }

        public UserInfo[] GetUsersByGroup(Guid groupId)
        {
            var group = GetGroupInfo(groupId);
            if (Constants.LostGroupInfo.Equals(group)) return new UserInfo[0];

            var refs = GetRefsInternal();
            return GetUsers().Where(u => IsUserInGroupInternal(u.ID, groupId, refs)).ToArray();
        }

        public void AddUserIntoGroup(Guid userId, Guid groupId)
        {
            if (Constants.LostUser.ID == userId || Constants.LostGroupInfo.ID == groupId)
            {
                return;
            }
            SecurityContext.DemandPermissions(new[] { Constants.Action_EditGroups, Constants.Action_EditAz });

            userService.SaveUserGroupRef(
                CoreContext.TenantManager.GetCurrentTenant().TenantId,
                new UserGroupRef(userId, groupId, UserGroupRefType.Contains));
        }

        public void RemoveUserFromGroup(Guid userId, Guid groupId)
        {
            if (Constants.LostUser.ID == userId || Constants.LostGroupInfo.ID == groupId) return;
            SecurityContext.DemandPermissions(new[] { Constants.Action_EditGroups, Constants.Action_EditAz });

            userService.RemoveUserGroupRef(CoreContext.TenantManager.GetCurrentTenant().TenantId, userId, groupId, UserGroupRefType.Contains);
        }

        #endregion Users


        #region Company

        public GroupInfo[] GetDepartments()
        {
            return CoreContext.GroupManager.GetGroups();
        }

        public Guid GetDepartmentManager(Guid deparmentID)
        {
            return GetRefsInternal()
                .Values
                .Where(r => r.RefType == UserGroupRefType.Manager && r.GroupId == deparmentID && !r.Removed)
                .Select(r => r.UserId)
                .SingleOrDefault();
        }

        public void SetDepartmentManager(Guid deparmentID, Guid userID)
        {
            var managerId = GetDepartmentManager(deparmentID);
            if (managerId != Guid.Empty)
            {
                userService.RemoveUserGroupRef(
                    CoreContext.TenantManager.GetCurrentTenant().TenantId,
                    managerId, deparmentID, UserGroupRefType.Manager);
            }
            if (userID != Guid.Empty)
            {
                userService.SaveUserGroupRef(
                    CoreContext.TenantManager.GetCurrentTenant().TenantId,
                    new UserGroupRef(userID, deparmentID, UserGroupRefType.Manager));
            }
        }

        public UserInfo GetCompanyCEO()
        {
            var id = GetDepartmentManager(Guid.Empty);
            return id != Guid.Empty ? GetUsers(id) : null;
        }

        public void SetCompanyCEO(Guid userId)
        {
            SetDepartmentManager(Guid.Empty, userId);
        }

        #endregion Company


        #region Groups

        public GroupInfo[] GetGroups()
        {
            return GetGroups(Guid.Empty);
        }

        public GroupInfo[] GetGroups(Guid categoryID)
        {
            return GetGroupsInternal()
                .Where(g => g.Parent == null && g.CategoryID == categoryID)
                .ToArray();
        }

        public GroupInfo GetGroupInfo(Guid groupID)
        {
            return GetGroupsInternal()
                .SingleOrDefault(g => g.ID == groupID) ?? Constants.LostGroupInfo;
        }

        public GroupInfo SaveGroupInfo(GroupInfo g)
        {
            if (Constants.LostGroupInfo.Equals(g)) return Constants.LostGroupInfo;
            if (Constants.BuildinGroups.Any(b => b.ID == g.ID)) return Constants.BuildinGroups.Single(b => b.ID == g.ID);
            SecurityContext.DemandPermissions(new[] { Constants.Action_EditGroups, Constants.Action_EditAz });

            var newGroup = userService.SaveGroup(CoreContext.TenantManager.GetCurrentTenant().TenantId, ToGroup(g));
            return GetGroupInfo(newGroup.Id);
        }

        public void DeleteGroup(Guid id)
        {
            if (Constants.LostGroupInfo.Equals(id)) return;
            if (Constants.BuildinGroups.Any(b => b.ID == id)) return;
            SecurityContext.DemandPermissions(new[] { Constants.Action_EditGroups, Constants.Action_EditAz });

            userService.RemoveGroup(CoreContext.TenantManager.GetCurrentTenant().TenantId, id);
        }

        #endregion Groups


        private void RecursiveAddChildGroups(GroupInfo parent, List<GroupInfo> result)
        {
            if (parent == null || parent.Descendants == null) return;
            foreach (var group in parent.Descendants)
            {
                if (!result.Contains(group)) result.Add(group);
                RecursiveAddChildGroups(group, result);
            }
        }

        private bool IsPropertiesContainsWords(IEnumerable<string> properties, IEnumerable<string> words)
        {
            foreach (var w in words)
            {
                var find = false;
                foreach (var p in properties)
                {
                    find = (3 <= w.Length) && (0 <= p.IndexOf(w, StringComparison.CurrentCultureIgnoreCase));
                    if (find) break;
                }
                if (!find) return false;
            }
            return true;
        }


        private IEnumerable<UserInfo> GetUsersInternal()
        {
            return userService.GetUsers(CoreContext.TenantManager.GetCurrentTenant().TenantId, default(DateTime))
                .Values
                .Where(u => !u.Removed);
        }

        private IEnumerable<GroupInfo> GetGroupsInternal()
        {
            var groupsInfo = new Dictionary<Guid, GroupInfo>();
            var groups = userService.GetGroups(CoreContext.TenantManager.GetCurrentTenant().TenantId, default(DateTime));
            foreach (var g in groups.Values.Where(g => !g.Removed).OrderBy(g => g.ParentId))
            {
                var gi = new GroupInfo
                {
                    ID = g.Id,
                    Name = g.Name,
                    CategoryID = g.CategoryId,
                };
                if (g.ParentId != Guid.Empty && groupsInfo.ContainsKey(g.ParentId))
                {
                    groupsInfo[g.ParentId].AddDescendant(gi);
                }
                groupsInfo[gi.ID] = gi;
            }
            return groupsInfo.Values.Concat(Constants.BuildinGroups);
        }

        private IDictionary<string, UserGroupRef> GetRefsInternal()
        {
            return userService.GetUserGroupRefs(CoreContext.TenantManager.GetCurrentTenant().TenantId, default(DateTime));
        }

        private bool IsUserInGroupInternal(Guid userId, Guid groupId, IDictionary<string, UserGroupRef> refs)
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant();

            if (groupId == Constants.GroupEveryone.ID)
            {
                return true;
            }
            if (groupId == Constants.GroupAdmin.ID && tenant.OwnerId == userId)
            {
                return true;
            }

            UserGroupRef r = null;
            if (groupId == Constants.GroupUser.ID || groupId == Constants.GroupVisitor.ID)
            {
                var visitor = refs.TryGetValue(UserGroupRef.CreateKey(tenant.TenantId, userId, Constants.GroupVisitor.ID, UserGroupRefType.Contains), out r) && !r.Removed;
                if (groupId == Constants.GroupVisitor.ID) return visitor;
                return !visitor;
            }
            return refs.TryGetValue(UserGroupRef.CreateKey(tenant.TenantId, userId, groupId, UserGroupRefType.Contains), out r) && !r.Removed;
        }

        private Group ToGroup(GroupInfo g)
        {
            if (g == null) return null;
            return new Group
            {
                Id = g.ID,
                Name = g.Name,
                ParentId = g.Parent != null ? g.Parent.ID : Guid.Empty,
                CategoryId = g.CategoryID,
            };
        }
    }
}