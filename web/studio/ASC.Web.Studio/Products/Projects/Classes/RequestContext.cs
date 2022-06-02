using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Collections;
using ASC.Web.Projects.Classes;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects.Resources;
using System.Collections.Generic;
using log4net.Core;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Users;
using ASC.Projects.Engine;

namespace ASC.Web.Projects.Classes
{

    public class RequestContext
    {
        static ProjectFat _projectctx { get { return Hash["_projectctx"] as ProjectFat; } set { Hash["_projectctx"] = value; } }
        static int? _projectId { get { return Hash["_projectId"] as int?; } set { Hash["_projectId"] = value; } }
        static int? _projectsCount { get { return Hash["_projectsCount"] as int?; } set { Hash["_projectsCount"] = value; } }

        static List<Project> _userProjects { get { return Hash["_userProjects"] as List<Project>; } set { Hash["_userProjects"] = value; } }
        static List<Project> _userFollowingProjects { get { return Hash["_userFollowingProjects"] as List<Project>; } set { Hash["_userFollowingProjects"] = value; } }

        #region project

        static public bool IsInConcreteProject()
        {
            return !String.IsNullOrEmpty(UrlParameters.ProjectID);
        }

        /// <summary>
        /// Ensure, that product exists in url and db. Redirect to default otherwise.
        /// </summary>
        /// <returns></returns>
        static public Project EnsureCurrentProduct()
        {
            if (HttpContext.Current == null) throw new ApplicationException("Not in http request");

            var project = GetCurrentProject(false);

            if (project == null)
                HttpContext.Current.Response.Redirect(ProjectsCommonResource.StartURL, true);

            return project;
        }

        static Project GetCurrentProject(bool isthrow)
        {

            if (_projectctx == null)
            {
                var project = Global.EngineFactory.GetProjectEngine().GetByID(GetCurrentProjectId(isthrow));

                if (project == null)
                {
                    if (isthrow) throw new ApplicationException("ProjectFat not finded");
                }
                else
                    _projectctx = new ProjectFat(project);
            }

            return _projectctx != null ? _projectctx.Project : null;
        }
        /// <summary>
        /// Throwing exception if product not founded by url parameter
        /// </summary>
        /// <returns></returns>
        static public Project GetCurrentProject()
        {
            return GetCurrentProject(true);
        }
        static int GetCurrentProjectId(bool isthrow)
        {
            if (!_projectId.HasValue)
            {
                int pid;
                if (!Int32.TryParse(UrlParameters.ProjectID, out pid))
                {
                    if (isthrow)
                        throw new ApplicationException("ProjectFat Id parameter invalid");
                }
                else
                    _projectId = pid;
            }
            return _projectId.HasValue ? _projectId.Value : -1;
        }
        static public int GetCurrentProjectId()
        {
            return GetCurrentProjectId(true);
        }

        public static ProjectFat GetCurrentProjectFat()
        {
            if (_projectctx == null) EnsureCurrentProduct();
            return _projectctx;
        }
        #endregion

        #region projects
        public static bool HasAnyProjects()
        {
            if (
                _projectsCount.HasValue && _projectsCount.Value > 0
                ||
                _userProjects != null && _userProjects.Count > 0
                ||
                _userFollowingProjects != null && _userFollowingProjects.Count > 0)
                return true;

            return GetProjectsCount() > 0;
        }

        public static int GetProjectsCount()
        {
            if (!_projectsCount.HasValue)
                _projectsCount = Global.EngineFactory.GetProjectEngine().Count();
            return _projectsCount.Value;
        }

        public static List<Project> GetCurrentUserProjects()
        {
            if (_userProjects == null)
                _userProjects = Global.EngineFactory.GetProjectEngine().GetByParticipant(ASC.Core.SecurityContext.CurrentAccount.ID);
            return _userProjects;
        }
        public static List<Project> GetCurrentUserFollowingProjects()
        {
            if (_userFollowingProjects == null)
                _userFollowingProjects = Global.EngineFactory.GetProjectEngine().GetFollowing(ASC.Core.SecurityContext.CurrentAccount.ID);
            return _userFollowingProjects;
        }
        #endregion




        #region internal

        const string storageKey = "PROJECT_REQ_CTX";
        static Hashtable Hash
        {
            get
            {
                if (HttpContext.Current == null) throw new ApplicationException("Not in http request");

                var hash = (Hashtable)HttpContext.Current.Items[storageKey];
                if (hash == null)
                {
                    hash = new Hashtable();
                    HttpContext.Current.Items[storageKey] = hash;
                }
                return hash;

            }
        }
        #endregion

    }

    /// <summary>
    /// Wrapper for project with instance cache some info
    /// </summary>
    public class ProjectFat
    {
        Project _project;

        internal ProjectFat(Project project)
        {
            _project = project;
            Responsible = Global.EngineFactory.GetParticipantEngine().GetByID(_project.Responsible).UserInfo;
        }

        public Project Project { get { return _project; } }

        List<Participant> _team = null;
        public List<Participant> GetTeam()
        {
            if (_team == null)
                _team = Global.EngineFactory.GetProjectEngine().GetTeam(Project.ID)
                    .OrderBy(p => p.UserInfo, UserInfoComparer.Default)
                    .ToList();

            return _team;
        }
        public List<Participant> GetActiveTeam()
        {
            var team = GetTeam();

            if (ProjectSecurity.CanEditTeam(Project))
            {
                var engine = Global.EngineFactory.GetProjectEngine();
                var deleted = team.FindAll(u => u.UserInfo.Status != EmployeeStatus.Active || !CoreContext.UserManager.UserExists(u.ID));
                foreach (var d in deleted)
                {
                    engine.RemoveFromTeam(Project, d, true);
                }
            }

            var active = team.FindAll(u => u.UserInfo.Status != EmployeeStatus.Terminated && CoreContext.UserManager.UserExists(u.ID));
            return active.OrderBy(u => u.UserInfo, UserInfoComparer.Default).ToList();
        }
        /// <summary>
        /// Is current user responsible for this project
        /// </summary>
        /// <returns></returns>
        public bool IsResponsible()
        {
            return Responsible.ID == ASC.Core.SecurityContext.CurrentAccount.ID;
        }

        List<Milestone> _milestones = null;
        public List<Milestone> GetMilestones()
        {
            if (_milestones == null)
                _milestones = Global.EngineFactory.GetMilestoneEngine().GetByProject(Project.ID);

            return _milestones;
        }
        public List<Milestone> GetMilestones(MilestoneStatus status)
        {
            return GetMilestones().FindAll(mil => mil.Status == status);
        }
        public Milestone GetMilestone(int id)
        {
            return GetMilestones().Find(mile => mile.ID == id);
        }

        public UserInfo Responsible { get; private set; }
    }


}
