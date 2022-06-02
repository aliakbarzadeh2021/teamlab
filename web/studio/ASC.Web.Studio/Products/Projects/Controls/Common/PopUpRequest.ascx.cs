#region Import

using System;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects.Classes;
using System.Text;
using System.Collections.Generic;
using ASC.Web.Projects.Resources;
using ASC.Core;
using ASC.Web.Projects.Configuration;
using ASC.Projects.Engine;


#endregion

namespace ASC.Web.Projects.Controls.Common
{
    public partial class PopUpRequest : BaseUserControl
    {
        protected bool _securityEnable;

        #region Properties

        public ProjectChangeRequest request { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }
        public bool HasTemplate { get; private set; }
        public string TemplateTitle { get; private set; }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            _securityEnable = ProjectSecurity.SecurityEnabled(request != null ? request.ProjectID : 0);

            Title = request.Title.HtmlEncode();
            Description = request.Description;
            Tags = string.Join(", ", Global.EngineFactory.GetTagEngine().GetProjectRequestTags(request.ID));

            if (request != null && request.TemplateId > 0 && request.RequestType == ProjectRequestType.Create)
            {
                var template = Global.EngineFactory.GetTemplateEngine().GetTemplateProject(request.TemplateId);
                if (template != null)
                {
                    HasTemplate = true;
                    TemplateTitle = template.Title;
                }
            }
        }

        #endregion

        #region Methods

        public bool IsEditProject()
        {
            return request.RequestType == ProjectRequestType.Edit;
        }
        public int GetProjectStatus()
        {
            if (request.Status == ProjectStatus.Open) return 0;
            else return 1;
        }
        public string[] ProjectDescribe()
        {
            string[] describe = new string[4];

            if (request.RequestType == ProjectRequestType.Edit)
            {
                Project project = Global.EngineFactory.GetProjectEngine().GetByID(request.ProjectID);

                describe[0] = project.Title;
                describe[1] = project.Description;
                describe[2] = Global.EngineFactory.GetParticipantEngine().GetByID(project.Responsible).UserInfo.DisplayUserName();
                describe[3] = project.Status == ProjectStatus.Open ? ProjectResource.ActiveProject : ProjectResource.ClosedProject;

                return describe;
            }

            return describe;
        }
        public string[] RequestDescribe()
        {
            string[] describe = new string[2];

            describe[0] = request.Title;
            describe[1] = request.Description;

            return describe;
        }
        public string GetRequestID()
        {
            return request.ID.ToString();
        }
        public bool IsHidden()
        {
            return request.Private;
        }

        public string UsersByDepartments()
        {
            var sb = new StringBuilder();
            var notAssignedWithDepartmentUsers = new List<UserInfo>();

            foreach (var userInfo in CoreContext.UserManager.GetUsers())
            {
                if (string.IsNullOrEmpty(userInfo.Department))
                {
                    notAssignedWithDepartmentUsers.Add(userInfo);
                }
            }

            notAssignedWithDepartmentUsers.Sort((x, y) => string.Compare(x.DisplayUserName(), y.DisplayUserName()));
            foreach (var userInfo in notAssignedWithDepartmentUsers)
            {
                if (userInfo.ID == request.Responsible)
                    sb.AppendFormat("<option value='{0}' selected='selected'>{1}</option>", userInfo.ID, userInfo.DisplayUserName(true));
                else
                    sb.AppendFormat("<option value='{0}'>{1}</option>", userInfo.ID, userInfo.DisplayUserName(true));
            }

            foreach (var group in CoreContext.GroupManager.GetGroups().OrderBy(g => g.Name))
            {
                sb.AppendFormat("<optgroup label=\"{0}\">", group.Name.HtmlEncode());
                foreach (var userInfo in CoreContext.UserManager.GetUsersByGroup(group.ID).OrderBy(u => u.DisplayUserName()))
                {
                    if (userInfo.ID == request.Responsible)
                        sb.AppendFormat("<option value='{0}' selected='selected'>{1}</option>", userInfo.ID, userInfo.DisplayUserName(true));
                    else
                        sb.AppendFormat("<option value='{0}'>{1}</option>", userInfo.ID, userInfo.DisplayUserName(true));
                }
            }

            return sb.ToString();
        }
        public string RenderActionTitle()
        {
            var action = string.Empty;
            if (request.RequestType == ProjectRequestType.Create) action = RequestResource.HasAddedTheNewProject;
            if (request.RequestType == ProjectRequestType.Edit) action = RequestResource.HasEditedTheProject;
            if (request.RequestType == ProjectRequestType.Remove) action = RequestResource.HasRemovedTheProject;

            return string.Format("{0}<span class='splitter'><!– –></span><b>{1}</b>",
                StudioUserInfoExtension.RenderProfileLink(CoreContext.UserManager.GetUsers(request.CreateBy), ProductEntryPoint.ID),
                action); 
        }

        #endregion
    }
}