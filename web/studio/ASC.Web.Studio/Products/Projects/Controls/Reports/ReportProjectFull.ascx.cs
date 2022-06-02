#region Import

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using ASC.Projects.Core.Domain;
using System.Text;
using ASC.Core.Users;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Resources;
using ASC.Projects.Common;
using ASC.Core;
using ASC.Web.Core.Users.Activity;
using ASC.Web.Studio.Utility;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Utility.Skins;

#endregion

namespace ASC.Web.Projects.Controls.Reports
{
    public partial class ReportProjectFull : BaseUserControl
    {

        #region Properties

        public String selectedUser
        {
            get { return HiddenFieldForUser.Value; }
            set { HiddenFieldForUser.Value = value; }
        }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            _rblProjectsStatus.Items[0].Text = ResourceEnumConverter.ConvertToString(ProjectStatus.Open);
            _rblProjectsStatus.Items[1].Text = ResourceEnumConverter.ConvertToString(ProjectStatus.Closed);
            _rblProjectsStatus.Items[2].Text = ReportResource.Any;
        }

        public void ButtonReportGenerate_Click(Object sender, EventArgs e)
        {
            List<WrapperReportProjectFull> list = GetListWrapper();

            Init_lblReportInfo(list.Count);

            if (list.Count > 0)
                tableRpt.DataSource = list;
            else
                tableRpt.DataSource = null;

            tableRpt.DataBind();
        }

        #endregion

        #region Methods

        public String GetFirstImageUrl()
        {
            return WebImageSupplier.GetAbsoluteWebPath("collapce.gif", ProductEntryPoint.ID);
        }

        public String GetLastImageUrl()
        {
            return WebImageSupplier.GetAbsoluteWebPath("collapse_right_dark.png");
        }

        public string InitUsersDdl()
        {
            StringBuilder sb = new StringBuilder();

            List<UserInfo> userList = new List<UserInfo>(ASC.Core.CoreContext.UserManager.GetUsers());
            List<UserInfo> notAssignedWithDepartmentUsers = new List<UserInfo>();

            foreach (UserInfo userInfo in userList)
            {
                if (userInfo.Department == string.Empty || userInfo.Department == null)
                {
                    notAssignedWithDepartmentUsers.Add(userInfo);
                }
            }

            notAssignedWithDepartmentUsers.Sort((oX, oY) =>
            {
                return String.Compare(oX.DisplayUserName(), oY.DisplayUserName());
            });

            List<Group> groups = ASC.Core.CoreContext.UserManager.GetDepartmentsWithUsers().Groups;

            groups.Sort((oX, oY) =>
            {
                return String.Compare(oX.GroupInfo.Name, oY.GroupInfo.Name);
            });

            sb.Append("<option value='-1' id='ddlUser-1'></option>");

            foreach (UserInfo userInfo in notAssignedWithDepartmentUsers)
            {
                sb.AppendFormat("<option value='{0}' id='ddlUser{0}'>{1}</option>", userInfo.ID, userInfo.DisplayUserName());
            }

            foreach (Group group in groups)
            {
                sb.AppendFormat("<optgroup label='{0}'>", group.GroupInfo.Name);
                List<UserInfo> users = group.Users;
                users.Sort((oX, oY) =>
                {
                    return String.Compare(oX.DisplayUserName(), oY.DisplayUserName());
                });
                foreach (UserInfo userInfo in users)
                {
                    sb.AppendFormat("<option value='{0}' id='ddlUser{0}'>{1}</option>", userInfo.ID, userInfo.DisplayUserName());
                }

            }

            return sb.ToString();
        }
        
        public void Init_lblReportInfo(int count)
        {
            string projects = string.Empty;
            string projectsStatus = string.Format("{1}: {0}", _rblProjectsStatus.SelectedItem.Text , ReportResource.ProjectsStatus);

            projects = string.Format("{0}: {1}, ",
                    ProjectResource.Projects,
                    ReportResource.Any);

            _lblReportInfo.Text = string.Format(ReportResource.ReportGenerated2,
                    projects,
                    projectsStatus,
                    string.Empty
                    );

            if (count == 0)
            {

                EmptyScreenControl emptyScreenControl = new EmptyScreenControl();

                emptyScreenControl.HeaderDescribe = CommonResource.NoData;

                emptyScreenControlPh.Controls.Add(emptyScreenControl);
            }
        }

        public List<WrapperReportProjectFull> GetListWrapper()
        {
            List<WrapperReportProjectFull> list = new List<WrapperReportProjectFull>();
            List<UserActivity> activities = UserActivityManager.GetUserActivities(TenantProvider.CurrentTenantID, null, ProductEntryPoint.ID, ProductEntryPoint.ID, null, null, null);
            List<UserActivity> activitiesByProject = new List<UserActivity>();
            
           // Global.EngineFactory.GetProjectEngine().BuildProjectListReport(nu);
           
            ArrayList projectCollection;

            switch (_rblProjectsStatus.SelectedItem.Value)
            {
                case "0":
                    projectCollection = ArrayList.Adapter(Global.EngineFactory.GetProjectEngine().BuildProjectListReport(ProjectStatus.Open, GetUsersID()));
                    break;
                case "1":
                    projectCollection = ArrayList.Adapter(Global.EngineFactory.GetProjectEngine().BuildProjectListReport(ProjectStatus.Closed, GetUsersID()));
                    break;
                case "2":
                    projectCollection = ArrayList.Adapter(Global.EngineFactory.GetProjectEngine().BuildProjectListReport(null, GetUsersID()));
                    break;
                default: projectCollection = new ArrayList(); break;   
            }

            foreach (var collectionItem in projectCollection)
                    {
                       var columns = (object[])collectionItem;

                       activitiesByProject = activities.FindAll(item => item.ContainerID == columns[0].ToString()); 

                       list.Add(new WrapperReportProjectFull
                        {
                            project_id = columns[0],
                            project_title = columns[1],
                            project_leader_id = columns[2],
                            project_status = columns[3],
                            milestones_count = columns[4],
                            tasks_count = columns[5],
                            participants_count = columns[6],
                            actionsCount = activitiesByProject.Count
                        });
                    }

            return list;


        }

        public Guid[] GetUsersID()
        {
            Guid[] users = new Guid[1];

            if (selectedUser != "-1")
            {
                users[0]=new Guid(selectedUser);
                return users;
            }

            return null;
        }

        #endregion
    }

    public class WrapperReportProjectFull
    {
        public object project_id;
        public object project_title;
        public object project_leader_id;
        public object project_status;
        public object milestones_count;
        public object tasks_count;
        public object participants_count;
        public int actionsCount;
    }
}