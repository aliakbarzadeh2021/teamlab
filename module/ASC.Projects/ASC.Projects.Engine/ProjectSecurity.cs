using System;
using ASC.Core;
using ASC.Files.Core;
using ASC.Projects.Core.Domain;
using ASC.Projects.Data;

namespace ASC.Projects.Engine
{
    public class ProjectSecurity
    {
        public static bool CanReadMessages(Project project)
        {
            return GetTeamSecurity(project, SecurityContext.CurrentAccount.ID, ProjectTeamSecurity.Messages);
        }

        public static bool CanReadFiles(Project project)
        {
            return GetTeamSecurity(project, SecurityContext.CurrentAccount.ID, ProjectTeamSecurity.Files);
        }

        public static bool CanReadTasks(Project project)
        {
            return GetTeamSecurity(project, SecurityContext.CurrentAccount.ID, ProjectTeamSecurity.Tasks);
        }

        public static bool CanReadMilestones(Project project)
        {
            var s = GetTeamSecurity(project, SecurityContext.CurrentAccount.ID, ProjectTeamSecurity.Milestone);
            return s || GetFactory().GetMilestoneDao().CanReadMilestones(project.ID, SecurityContext.CurrentAccount.ID);
        }

        public static bool CanRead(Project project)
        {
            if (project == null) return false;
            return !project.Private || IsInTeam(project);
        }

        public static bool CanRead(Task task)
        {
            if (task == null || !CanRead(task.Project)) return false;
            if (task.Responsible == SecurityContext.CurrentAccount.ID) return true;
            if (!GetTeamSecurity(task.Project, SecurityContext.CurrentAccount.ID, ProjectTeamSecurity.Tasks)) return false;
            if (task.Milestone != 0 && !GetTeamSecurity(task.Project, SecurityContext.CurrentAccount.ID, ProjectTeamSecurity.Milestone))
            {
                var m = GetFactory().GetMilestoneDao().GetById(task.Milestone);
                if (!CanRead(m)) return false;
            }
            return true;
        }

        public static bool CanRead(Milestone milestone)
        {
            if (milestone == null || !CanRead(milestone.Project)) return false;
            if (milestone.CurrentUserHasTasks) return true;
            var s = GetTeamSecurity(milestone.Project, SecurityContext.CurrentAccount.ID, ProjectTeamSecurity.Milestone);
            return s;
        }


        public static bool CanCreateProject()
        {
            return IsAdmin;
        }

        public static bool CanCreateMilestone(Project project)
        {
            return IsProjectManager(project);
        }

        public static bool CanCreateMessage(Project project)
        {
            if (!SecurityContext.CurrentAccount.IsAuthenticated) return false;
            return true;
        }

        public static bool CanCreateTask(Project project)
        {
            return CanCreateTask(project, true);
        }

        public static bool CanCreateTask(Project project, bool forAll)
        {
            if (!SecurityContext.CurrentAccount.IsAuthenticated) return false;
            return forAll ? CanReadTasks(project) : IsInTeam(project);
        }

        public static bool CanCreateComment()
        {
            return SecurityContext.IsAuthenticated;
        }


        public static bool CanEdit(Project project)
        {
            return IsAdmin;
        }

        public static bool CanEdit(Milestone milestone)
        {
            if (milestone == null) return false;
            return IsProjectManager(milestone.Project) || milestone.CreateBy == SecurityContext.CurrentAccount.ID;
        }

        public static bool CanEdit(Message message)
        {
            if (message == null) return false;
            return IsProjectManager(message.Project) || message.CreateBy == SecurityContext.CurrentAccount.ID;
        }

        public static bool CanEdit(Task task)
        {
            if (task == null) return false;
            return IsProjectManager(task.Project) || task.CreateBy == SecurityContext.CurrentAccount.ID || task.Responsible == SecurityContext.CurrentAccount.ID ||
                (IsInTeam(task.Project) && task.Responsible == Guid.Empty);
        }

        public static bool CanEditTeam(Project project)
        {
            return IsProjectManager(project);
        }

        public static bool CanEditComment(Project project, Comment comment)
        {
            if (project == null || comment == null) return false;
            return comment.CreateBy == SecurityContext.CurrentAccount.ID || IsAdmin || project.Responsible == SecurityContext.CurrentAccount.ID;
        }


        public static void DemandCreateProject()
        {
            if (!CanCreateProject()) throw CreateSecurityException();
        }

        public static void DemandCreateMessage(Project project)
        {
            if (!CanCreateMessage(project)) throw CreateSecurityException();
        }

        public static void DemandCreateMilestone(Project project)
        {
            if (!CanCreateMilestone(project)) throw CreateSecurityException();
        }

        public static void DemandCreateTask(Project project)
        {
            if (!CanCreateTask(project, false)) throw CreateSecurityException();
        }

        public static void DemandRead(Milestone milestone)
        {
            if (!CanRead(milestone != null ? milestone.Project : null)) throw CreateSecurityException();
        }

        public static void DemandEdit(Project project)
        {
            if (!CanEdit(project)) throw CreateSecurityException();
        }

        public static void DemandEdit(Message message)
        {
            if (!CanEdit(message)) throw CreateSecurityException();
        }

        public static void DemandEdit(Milestone milestone)
        {
            if (!CanEdit(milestone)) throw CreateSecurityException();
        }

        public static void DemandEdit(Task task)
        {
            if (!CanEdit(task)) throw CreateSecurityException();
        }

        public static void DemandEditTeam(Project project)
        {
            if (!CanEditTeam(project)) throw CreateSecurityException();
        }

        public static bool IsAdministrator(Guid userId)
        {
            return CoreContext.UserManager.IsUserInGroup(userId, ASC.Core.Users.Constants.GroupAdmin.ID);
        }


        public static void DemandAuthentication()
        {
            if (!CoreContext.TenantManager.GetCurrentTenant().Public && !SecurityContext.CurrentAccount.IsAuthenticated)
            {
                throw CreateSecurityException();
            }
        }

        public static Exception CreateSecurityException()
        {
            throw new System.Security.SecurityException("Access denied.");
        }


        public static bool SecurityEnabled(int projectId)
        {
            var securityEnable = CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId).SecurityEnable;
            if (!securityEnable)
            {
                securityEnable = GetFactory().GetProjectDao().SecurityEnable(projectId);
            }
            return securityEnable;
        }


        private static bool IsAdmin
        {
            get { return CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, ASC.Core.Users.Constants.GroupAdmin.ID); }
        }

        private static ASC.Projects.Core.DataInterfaces.IDaoFactory GetFactory()
        {
            return new DaoFactory("projects", CoreContext.TenantManager.GetCurrentTenant().TenantId);
        }

        private static bool IsProjectManager(Project project)
        {
            return IsProjectManager(project, SecurityContext.CurrentAccount.ID);
        }

        private static bool IsProjectManager(Project project, Guid userId)
        {
            return IsAdmin || (project != null && (project.Responsible == userId || project.CreateBy == userId));
        }

        private static bool IsInTeam(Project project)
        {
            return IsInTeam(project, SecurityContext.CurrentAccount.ID);
        }

        private static bool IsInTeam(Project project, Guid userId)
        {
            return IsAdmin || (project != null && GetFactory().GetProjectDao().IsInTeam(project.ID, userId));
        }

        private static bool GetTeamSecurity(Project project, Guid userId, ProjectTeamSecurity security)
        {
            if (IsAdmin || project == null || !project.Private || IsProjectManager(project, userId)) return true;
            var s = GetFactory().GetProjectDao().GetTeamSecurity(project.ID, userId);
            return (s & security) != security;
        }
    }
}
