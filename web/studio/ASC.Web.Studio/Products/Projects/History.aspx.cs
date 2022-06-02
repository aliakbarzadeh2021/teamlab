#region Import

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Web.Controls;
using ASC.Web.Core.Users.Activity;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Controls.Common;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;
using ASC.Web.Core.Users;
using ASC.Web.Core.Helpers;
using ASC.Core;
using ASC.Web.Studio.Controls.Common;
using ASC.Projects.Engine;

#endregion

namespace ASC.Web.Projects
{
    public partial class History : BasePage
    {

        #region Property

        public Project Project { get; set; }
        public static String StartDateTiks
        {
            get
            {
                string result = HttpContext.Current.Request["startDate"];
                return result == null ? "" : result;
            }
        }
        public static String FinishDateTiks
        {
            get
            {
                string result = HttpContext.Current.Request["finishDate"];
                return result == null ? "" : result;
            }
        }
        public static String TimeRange
        {
            get
            {
                string result = HttpContext.Current.Request["timeRange"];
                return result == null ? "" : result;
            }
        }
        public static String UID
        {
            get
            {
                string result = HttpContext.Current.Request["uid"];
                return result == null ? "" : result;
            }
        }
        public static String Module
        {
            get
            {
                string result = HttpContext.Current.Request["module"];
                return result == null ? "" : result;
            }
        }

        #endregion

        #region Methods

        protected void InitView()
        {

            int projectID;

            if (!int.TryParse(UrlParameters.ProjectID, out projectID))
                Response.Redirect(ProjectsCommonResource.StartURL);

            Project = Global.EngineFactory.GetProjectEngine().GetByID(projectID);
            if (Project == null) Response.Redirect(ProjectsCommonResource.StartURL);


            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = ProjectResource.Projects,
                NavigationUrl = "projects.aspx"
            });


            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = Project.HtmlTitle.HtmlEncode(),
                NavigationUrl = "projects.aspx?prjID=" + Project.ID
            });

            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = ProjectsCommonResource.History,
                NavigationUrl = "history.aspx?prjID=" + Project.ID
            });

            Title = HeaderStringHelper.GetPageTitle(ProjectsCommonResource.History, Master.BreadCrumbs);

            ApplyFilterButton.Text = ReportResource.ApplyFilter;

            if (!IsPostBack)
            {
                InitDateTbx();
                InitUsersDdl();
                InitModuleDdl();
                if (TimeRange != "")
                    hiddenCurrentTimeRange.Value = TimeRange;
                else hiddenCurrentTimeRange.Value = "2";
            }

        }

        public void InitModuleDdl()
        {
            string module = Module;
            ListItem listItem = new ListItem(ReportResource.AnyType, "0");
            if (module == "0")
                listItem.Selected = true;
            ddlModule.Items.Add(listItem);

            //listItem = new ListItem(ResourceEnumConverter.ConvertToString(EntityType.File), "1");
            //if (module == "1")
            //    listItem.Selected = true;
            //ddlModule.Items.Add(listItem);

            if (ProjectSecurity.CanReadMessages(Project))
            {
                listItem = new ListItem(ResourceEnumConverter.ConvertToString(EntityType.Message), "2");
                if (module == "2")
                    listItem.Selected = true;
                ddlModule.Items.Add(listItem);
            }

            if (ProjectSecurity.CanReadMilestones(Project))
            {
                listItem = new ListItem(ResourceEnumConverter.ConvertToString(EntityType.Milestone), "3");
                if (module == "3")
                    listItem.Selected = true;
                ddlModule.Items.Add(listItem);
            }

            listItem = new ListItem(ResourceEnumConverter.ConvertToString(EntityType.Project), "4");
            if (module == "4")
                listItem.Selected = true;
            ddlModule.Items.Add(listItem);

            listItem = new ListItem(ResourceEnumConverter.ConvertToString(EntityType.Task), "5");
            if (module == "5")
                listItem.Selected = true;
            ddlModule.Items.Add(listItem);

            listItem = new ListItem(ResourceEnumConverter.ConvertToString(EntityType.Team), "6");
            if (module == "6")
                listItem.Selected = true;
            ddlModule.Items.Add(listItem);

            listItem = new ListItem(ResourceEnumConverter.ConvertToString(EntityType.Comment), "7");
            if (module == "7")
                listItem.Selected = true;
            ddlModule.Items.Add(listItem);
        }

        public void InitUsersDdl()
        {
            List<Participant> users = new List<Participant>();
            foreach (Participant prt in Global.EngineFactory.GetProjectEngine().GetTeam(Project.ID))
            {
                if (ASC.Core.CoreContext.UserManager.UserExists(prt.ID) && (CoreContext.UserManager.GetUsers(prt.ID).Status != EmployeeStatus.Terminated))
                {
                    users.Add(prt);
                }
            }

            users.Sort((x, y) => UserInfoComparer.Default.Compare(x.UserInfo, y.UserInfo));

            ddlUsers.Items.Add(new ListItem(ReportResource.AllPeopleInProject, "all"));
            foreach (Participant prt in users)
            {
                ListItem listItem = new ListItem(prt.UserInfo.DisplayUserName(), prt.UserInfo.ID.ToString());

                if (UID == prt.UserInfo.ID.ToString())
                    listItem.Selected = true;

                ddlUsers.Items.Add(listItem);
            }
        }

        public void InitDateTbx()
        {
            if (StartDateTiks != "")
            {
                tbxStartDate.Text = new DateTime(Convert.ToInt64(StartDateTiks)).ToString(DateTimeExtension.DateFormatPattern);
            }
            else
            {
                tbxStartDate.Text = ASC.Core.Tenants.TenantUtil.DateTimeNow().AddDays(1 - (int)ASC.Core.Tenants.TenantUtil.DateTimeNow().DayOfWeek).ToString(DateTimeExtension.DateFormatPattern);
            }
            if (FinishDateTiks != "")
            {
                tbxFinishDate.Text = new DateTime(Convert.ToInt64(FinishDateTiks)).ToString(DateTimeExtension.DateFormatPattern);
            }
            else
            {
                tbxFinishDate.Text = ASC.Core.Tenants.TenantUtil.DateTimeNow().AddDays(7 - (int)ASC.Core.Tenants.TenantUtil.DateTimeNow().DayOfWeek).ToString(DateTimeExtension.DateFormatPattern);
            }
        }

        public List<UserActivity> ApplyModuleFilter(List<UserActivity> activity)
        {
            List<UserActivity> act = new List<UserActivity>();
            switch (ddlModule.SelectedValue)
            {

                case "0":
                    return activity;

                case "1":
                    foreach (UserActivity ua in activity)
                    {
                        string[] AdditionalDataParts = ua.AdditionalData.Split(new[] { '|' });
                        if (AdditionalDataParts[0] == EntityType.File.ToString())
                            act.Add(ua);
                    }
                    return act;

                case "2":
                    foreach (UserActivity ua in activity)
                    {
                        string[] AdditionalDataParts = ua.AdditionalData.Split(new[] { '|' });
                        if (AdditionalDataParts[0] == EntityType.Message.ToString())
                            act.Add(ua);
                    }
                    return act;

                case "3":
                    foreach (UserActivity ua in activity)
                    {
                        string[] AdditionalDataParts = ua.AdditionalData.Split(new[] { '|' });
                        if (AdditionalDataParts[0] == EntityType.Milestone.ToString())
                            act.Add(ua);
                    }
                    return act;

                case "4":
                    foreach (UserActivity ua in activity)
                    {
                        string[] AdditionalDataParts = ua.AdditionalData.Split(new[] { '|' });
                        if (AdditionalDataParts[0] == EntityType.Project.ToString())
                            act.Add(ua);
                    }
                    return act;

                case "5":
                    foreach (UserActivity ua in activity)
                    {
                        string[] AdditionalDataParts = ua.AdditionalData.Split(new[] { '|' });
                        if (AdditionalDataParts[0] == EntityType.Task.ToString())
                            act.Add(ua);
                    }
                    return act;

                case "6":
                    foreach (UserActivity ua in activity)
                    {
                        string[] AdditionalDataParts = ua.AdditionalData.Split(new[] { '|' });
                        if (AdditionalDataParts[0] == EntityType.Team.ToString())
                            act.Add(ua);
                    }
                    return act;
                case "7":
                    foreach (UserActivity ua in activity)
                    {
                        string[] AdditionalDataParts = ua.AdditionalData.Split(new[] { '|' });
                        if (AdditionalDataParts[0] == EntityType.Comment.ToString())
                            act.Add(ua);
                    }
                    return act;

            }
            return activity;
        }

        public int isPostBack()
        {
            if (IsPostBack) return 1;
            else return 0;
        }

        #endregion

        #region Events

        protected override void PageLoad()
        {
            InitView();

            DateTime startDate = new DateTime(); DateTime finishDate = new DateTime();

            try
            {
                startDate = DateTime.Parse(tbxStartDate.Text);
                finishDate = DateTime.Parse(tbxFinishDate.Text);
            }
            catch
            {
                throw new Exception(ProjectsJSResource.IncorrectDate);
            }

            int pageNumber = 1; if (!IsPostBack) pageNumber = UrlParameters.PageNumber;

            Guid uid = new Guid();
            if (ddlUsers.SelectedValue != "all")
            {
                uid = new Guid(ddlUsers.SelectedValue);
            }

            var activities = UserActivityManager.GetUserActivities(
                TenantProvider.CurrentTenantID,
                uid,
                ProductEntryPoint.ID,
                new[] { ProductEntryPoint.ID },
                UserActivityConstants.AllActionType,
                new[] { Project.ID.ToString() },
                startDate, finishDate.AddDays(1));

            var activitiesByProject = ApplyModuleFilter(activities);

            int factor = 3;

            int totalCount = activitiesByProject.Count;
            int requireItemCount = pageNumber * Global.EntryCountOnPage * factor;

            TimeLineView timeLineView = (TimeLineView)LoadControl(PathProvider.GetControlVirtualPath("TimeLineView.ascx"));

            int index = (pageNumber - 1) * Global.EntryCountOnPage * factor;
            int count = (requireItemCount > activitiesByProject.Count
                            ? activitiesByProject.Count
                            : requireItemCount) - index;

            timeLineView.Activities = activitiesByProject.GetRange(index, count);
            timeLineView.GroupByDate = true;
            timeLineView.RenderHeader = false;

            _content.Controls.Add(timeLineView);

            string period = string.Format("&startDate={0}&finishDate={1}", startDate.Ticks, finishDate.Ticks);
            string hidden = !IsPostBack ? TimeRange : hiddenCurrentTimeRange.Value;
            string user = string.Format("&uid={0}", ddlUsers.SelectedValue);
            string module = string.Format("&module={0}", ddlModule.SelectedValue);

            if (activitiesByProject.Count > 0)
            {
                _content.Controls.Add(new PageNavigator
                {
                    CurrentPageNumber = pageNumber,
                    VisibleOnePage = false,
                    ParamName = UrlConstant.PageNumber,
                    EntryCount = totalCount,
                    VisiblePageCount = Global.VisiblePageCount,
                    EntryCountOnPage = Global.EntryCountOnPage * factor,
                    PageUrl = String.Concat(PathProvider.BaseAbsolutePath, "history.aspx?", "prjID=", Project.ID) + period + "&timeRange=" + hidden + user + module
                });
            }
            else
            {
                EmptyScreenContainer.Controls.Add(new NotFoundControl());
                EmptyScreenContainer.Visible = true;
            }


        }

        protected void ApplyFilterButton_Click(object sender, EventArgs e)
        {
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            Page.EnableViewState = true;
        }

        #endregion
    }
}
