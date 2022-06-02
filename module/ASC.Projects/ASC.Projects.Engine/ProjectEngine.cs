using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Web.Core.Users.Activity;

namespace ASC.Projects.Engine
{
    public class ProjectEngine
    {
        private readonly EngineFactory _factory;
        private readonly IProjectDao projectDao;
        private readonly IProjectChangeRequestDao requestDao;


        public ProjectEngine(IDaoFactory daoFactory, EngineFactory factory)
        {
            _factory = factory;
            projectDao = daoFactory.GetProjectDao();
            requestDao = daoFactory.GetProjectChangeRequestDao();
        }


        public virtual List<Project> GetAll()
        {
            return projectDao.GetAll(null, 0)
                .Where(p => CanRead(p))
                .ToList();
        }

        public virtual List<Project> GetAll(ProjectStatus status, int max)
        {
            return projectDao.GetAll(status, max)
                .Where(p => CanRead(p))
                .ToList();
        }

        public virtual List<Project> GetLast(ProjectStatus status, int max)
        {
            int offset = 0;
            var lastProjects = new List<Project>();
            var projects = projectDao.GetLast(status, offset, max)
                .Where(p => CanRead(p))
                .ToList();

            lastProjects.AddRange(projects);

            if (lastProjects.Count < max)
            {
                do
                {
                    offset = offset + max;
                    projects = projectDao.GetLast(status, offset, max);

                    if (projects.Count == 0)
                        return lastProjects;
                    else projects = projects
                        .Where(p => CanRead(p))
                        .ToList();

                    lastProjects.AddRange(projects);
                }
                while (lastProjects.Count < max);
            }

            return lastProjects.Count == max ? lastProjects : lastProjects.GetRange(0, max);
        }

        public virtual List<Project> GetByParticipant(Guid participant)
        {
            return projectDao.GetByParticipiant(participant, ProjectStatus.Open)
                .Where(p => CanRead(p))
                .ToList();
        }
        public virtual List<Project> GetFollowing(Guid participant)
        {
            return projectDao.GetFollowing(participant)
                .Where(p => CanRead(p))
                .ToList();
        }
        public virtual Project GetByID(int projectID)
        {
            var project = projectDao.GetById(projectID);
            return CanRead(project) ? project : null;
        }

        public virtual List<Project> GetByID(ICollection projectIDs)
        {
            return projectDao.GetById(projectIDs)
                .Where(p => CanRead(p))
                .ToList();
        }

        public virtual bool IsExists(int projectID)
        {
            return projectDao.IsExists(projectID);
        }

        private bool CanRead(Project project)
        {
            return ProjectSecurity.CanRead(project);
        }


        public virtual int Count()
        {
            return projectDao.Count();
        }

        public virtual int GetTaskCount(int projectId, params TaskStatus[] taskStatus)
        {
            return GetTaskCount(new List<int>() { projectId }, taskStatus)[0];
        }

        public virtual List<int> GetTaskCount(List<int> projectId, params TaskStatus[] taskStatus)
        {
            return projectDao.GetTaskCount(projectId, taskStatus);
        }

        public virtual int GetMilestoneCount(int projectId, params MilestoneStatus[] milestoneStatus)
        {
            return projectDao.GetMilestoneCount(projectId, milestoneStatus);
        }

        public virtual int GetMessageCount(int projectId)
        {
            return projectDao.GetMessageCount(projectId);
        }


        public Project SaveOrUpdate(Project project, bool notifyManager)
        {
            return SaveOrUpdate(project, notifyManager, false);
        }

        public virtual Project SaveOrUpdate(Project project, bool notifyManager, bool isImport)
        {
            if (project == null) throw new ArgumentNullException("project");

            project.LastModifiedBy = SecurityContext.CurrentAccount.ID;
            project.LastModifiedOn = TenantUtil.DateTimeNow();

            if (project.ID == 0)
            {
                if (project.CreateBy == default(Guid)) project.CreateBy = SecurityContext.CurrentAccount.ID;
                if (project.CreateOn == default(DateTime)) project.CreateOn = TenantUtil.DateTimeNow();

                ProjectSecurity.DemandCreateProject();
                projectDao.Save(project);

                if (isImport)
                {
                    TimeLinePublisher.Project(project, EngineResource.ActionText_Imported, UserActivityConstants.ContentActionType, UserActivityConstants.Max);
                }
                else
                {
                    TimeLinePublisher.Project(project, EngineResource.ActionText_Create, UserActivityConstants.ContentActionType, UserActivityConstants.Max);
                }
            }
            else
            {
                ProjectSecurity.DemandEdit(project);
                projectDao.Save(project);
                TimeLinePublisher.Project(project, EngineResource.ActionText_Update, UserActivityConstants.ActivityActionType, UserActivityConstants.Max);
            }
            if (notifyManager && !_factory.DisableNotifications)
                NotifyClient.Instance.SendAboutResponsibleByProject(project.Responsible, project);
            return project;
        }

        public virtual void Delete(int projectId)
        {
            var project = GetByID(projectId);
            if (project == null) return;

            ProjectSecurity.DemandEdit(project);
            projectDao.Delete(projectId);
            TimeLinePublisher.Project(project, EngineResource.ActionText_Delete, UserActivityConstants.ActivityActionType, UserActivityConstants.Max);
        }


        public virtual List<Participant> GetTeam(int project)
        {
            return projectDao.GetTeam(project)
                .Select(id => new Participant(id))
                .ToList();
        }

        public virtual bool IsInTeam(int project, Guid participant)
        {
            return projectDao.IsInTeam(project, participant);
        }

        public virtual void AddToTeam(Project project, Participant participant, bool sendNotification)
        {
            if (project == null) throw new ArgumentNullException("project");
            if (participant == null) throw new ArgumentNullException("participant");

            ProjectSecurity.DemandEditTeam(project);
            projectDao.AddToTeam(project.ID, participant.ID);
            TimeLinePublisher.Team(project, participant.UserInfo, EngineResource.ActionText_AddToTeam);

            if (!_factory.DisableNotifications && sendNotification)
                NotifyClient.Instance.SendInvaiteToProjectTeam(participant.ID, project);
        }

        public virtual void RemoveFromTeam(Project project, Participant participant, bool sendNotification)
        {
            if (project == null) throw new ArgumentNullException("project");
            if (participant == null) throw new ArgumentNullException("participant");

            ProjectSecurity.DemandEditTeam(project);
            projectDao.RemoveFromTeam(project.ID, participant.ID);
            TimeLinePublisher.Team(project, participant.UserInfo, EngineResource.ActionText_DeletedFromTeam);

            if (!_factory.DisableNotifications && sendNotification)
                NotifyClient.Instance.SendRemovingFromProjectTeam(participant.ID, project);
        }

        public virtual void SetTeamSecurity(Project project, Participant participant, ProjectTeamSecurity teamSecurity, bool visible)
        {
            if (project == null) throw new ArgumentNullException("project");
            if (participant == null) throw new ArgumentNullException("participant");

            ProjectSecurity.DemandEditTeam(project);

            var security = projectDao.GetTeamSecurity(project.ID, participant.ID);
            if (visible)
            {
                if (security != ProjectTeamSecurity.None) security ^= teamSecurity;
            }
            else
            {
                security |= teamSecurity;
            }
            projectDao.SetTeamSecurity(project.ID, participant.ID, security);
            TimeLinePublisher.TeamSecurity(project,participant);
        }

        public virtual bool GetTeamSecurity(Project project, Participant participant, ProjectTeamSecurity teamSecurity)
        {
            if (project == null) throw new ArgumentNullException("project");
            if (participant == null) throw new ArgumentNullException("participant");

            var security = projectDao.GetTeamSecurity(project.ID, participant.ID);
            return (security & teamSecurity) != teamSecurity;
        }


        public virtual ProjectChangeRequest GetRequest(int id)
        {
            return requestDao.GetById(id);
        }

        public virtual List<ProjectChangeRequest> GetRequests()
        {
            return requestDao.GetAll();
        }

        public virtual void SendRequest(ProjectChangeRequest request)
        {
            NotifyChangeRequest(request);

            if (request.CreateBy == default(Guid)) request.CreateBy = SecurityContext.CurrentAccount.ID;
            if (request.CreateOn == default(DateTime)) request.CreateOn = TenantUtil.DateTimeNow();

            requestDao.Save(request);
        }

        private void NotifyChangeRequest(ProjectChangeRequest request)
        {
            if (_factory.DisableNotifications) return;

            if (request.RequestType == ProjectRequestType.Create)
            {
                NotifyClient.Instance.SendProjectCreateRequest(request);
            }
            else
            {
                var project = projectDao.GetById(request.ProjectID);
                if (project != null)
                {
                    if (request.RequestType == ProjectRequestType.Remove)
                        NotifyClient.Instance.SendProjectRemoveRequest(request);
                    if (request.RequestType == ProjectRequestType.Edit)
                        NotifyClient.Instance.SendProjectEditRequest(project, request);
                }
            }
        }

        public virtual Project AcceptRequest(ProjectChangeRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");

            Project project = null;
            if (request.RequestType == ProjectRequestType.Create)
            {
                if (0 < request.TemplateId)
                {
                    project = _factory.GetTemplateEngine().CreateProjectFromTemplate(request.TemplateId, request.Title, request.Responsible, request.Description, string.Empty, request.Private);
                }
                else
                {
                    project = new Project();
                    project.Title = request.Title;
                    project.Description = request.Description;
                    project.Responsible = request.Responsible;
                    project.Status = request.Status;
                    project.Private = request.Private;

                    project = SaveOrUpdate(project, true);
                    if (!IsInTeam(project.ID, request.Responsible))
                        AddToTeam(project, new Participant(request.Responsible), true);
                }
            }
            else if (request.RequestType == ProjectRequestType.Edit)
            {
                project = projectDao.GetById(request.ProjectID);
                project.Title = request.Title;
                project.Description = request.Description;
                project.Responsible = request.Responsible;
                project.Status = request.Status;
                project.Private = request.Private;

                project = SaveOrUpdate(project, true);
                if (!IsInTeam(project.ID, request.Responsible))
                    AddToTeam(project, new Participant(request.Responsible), true);
            }
            else if (request.RequestType == ProjectRequestType.Remove)
            {
                project = GetByID(request.ProjectID);
                if (project != null)
                {
                    ProjectSecurity.DemandEdit(project);
                    projectDao.Delete(project.ID);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("request.RequestType");
            }

            requestDao.Delete(request.ID);

            request.ProjectID = project.ID;
            if (!_factory.DisableNotifications)
            {
                NotifyClient.Instance.SendProjectAcceptRequest(request);
            }

            return project;
        }

        public virtual void RejectRequest(ProjectChangeRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            requestDao.Delete(request.ID);

            if (!_factory.DisableNotifications)
                NotifyClient.Instance.SendProjectRejectRequest(request);
        }
    }
}
