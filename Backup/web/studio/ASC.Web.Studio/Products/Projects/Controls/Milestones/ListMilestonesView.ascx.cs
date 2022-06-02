#region Import

using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Projects.Core.Domain;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Resources;
using AjaxPro;
using ASC.Web.Projects.Controls.Tasks;
using ASC.Web.Projects.Controls.Projects;

#endregion

namespace ASC.Web.Projects.Controls.Milestones
{
    public partial class ListMilestonesView : BaseUserControl
    {
        

        public String status;
        public ProjectFat ProjectFat { get; set; }

        

        #region Property

        public List<Milestone> Milestones { get; set; }

        #endregion


        #region Methods

        public String GetFirstImageUrl()
        {
            return WebImageSupplier.GetAbsoluteWebPath("expand.gif", ProductEntryPoint.ID);
        }

        public String GetLastImageUrl()
        {
            return WebImageSupplier.GetAbsoluteWebPath("collapse_right_dark.png");
        }

        protected String RenderStatus(MilestoneStatus milestoneStatus)
        {

            StringBuilder innerHTML = new StringBuilder();

            switch (milestoneStatus)
            {

                case MilestoneStatus.Open:
                    innerHTML.AppendFormat(@"<img align='absmiddle' src='{0}' alt='{1}' title='{1}' style='border:0px;' />",
                                            WebImageSupplier.GetAbsoluteWebPath("status_open_border.gif", ProductEntryPoint.ID),
                                            ResourceEnumConverter.ConvertToString(MilestoneStatus.Open));
                    break;
                case MilestoneStatus.Late:
                    innerHTML.AppendFormat(@"<img align='absmiddle' src='{0}' alt='{1}' title='{1}' style='border:0px;' />",
                                            WebImageSupplier.GetAbsoluteWebPath("status_open_border.gif", ProductEntryPoint.ID),
                                            ResourceEnumConverter.ConvertToString(MilestoneStatus.Late));
                    break;
                case MilestoneStatus.Closed:
                    innerHTML.AppendFormat(@"<img align='absmiddle' src='{0}'  alt='{1}' title='{1}' style='border:0px;' />",
                                            WebImageSupplier.GetAbsoluteWebPath("status_closed_border.gif", ProductEntryPoint.ID),
                                            ResourceEnumConverter.ConvertToString(MilestoneStatus.Closed));
                    break;
                default:
                    break;
            }

            return innerHTML.ToString();

        }

        public String GrammaticalHelperTotalCountTasks(int milestoneID)
        {
            int count = Global.EngineFactory.GetTaskEngine().GetTaskCount(milestoneID,TaskStatus.NotAccept,TaskStatus.Open,TaskStatus.Closed);

            return count + " " + GrammaticalHelper.ChooseNumeralCase(count, GrammaticalResource.TaskNominative, GrammaticalResource.TaskGenitiveSingular, GrammaticalResource.TaskGenitivePlural);
        }

        public String GrammaticalHelperActiveCountTasks(int milestoneID)
        {

            int count = Global.EngineFactory.GetTaskEngine().GetTaskCount(milestoneID, TaskStatus.NotAccept, TaskStatus.Open);

            return count + " " + GrammaticalHelper.ChooseNumeralCase(count, GrammaticalResource.TaskNotDoneNominative, GrammaticalResource.TaskNotDoneGenitiveSingular, GrammaticalResource.TaskNotDoneGenitivePlural);
        }

        public String GrammaticalHelperCountDays(Milestone milestone)
        {
            if (milestone.Status == MilestoneStatus.Closed)
                return string.Empty;

            DateTime deadline = milestone.DeadLine;
            int count = deadline.Subtract(ASC.Core.Tenants.TenantUtil.DateTimeNow()).Days;

            if (deadline.Date == ASC.Core.Tenants.TenantUtil.DateTimeNow().Date)
                return string.Format("<div class='pm-grayText' style='text-align: center; padding-bottom:3px;'>({0})</div>",ProjectsCommonResource.Today);
            
            if (deadline < ASC.Core.Tenants.TenantUtil.DateTimeNow())
                return string.Format("<div class='pm-redText' style='text-align: center; padding-bottom:3px;'>({0} {1})</div>", Math.Abs(count),
                    GrammaticalHelper.ChooseNumeralCase(Math.Abs(count), GrammaticalResource.DayNominative, GrammaticalResource.DayGenitiveSingular, GrammaticalResource.DayGenitivePlural));

            return string.Format("<div class='pm-grayText' style='text-align: center; padding-bottom:3px;'>({0} {1})</div>", count + 1,
                    GrammaticalHelper.ChooseNumeralCase(count+1, GrammaticalResource.DayNominative, GrammaticalResource.DayGenitiveSingular, GrammaticalResource.DayGenitivePlural)); 

        }

        public String GetCaption(Milestone milestone)
        {
            StringBuilder innerHTML = new StringBuilder();

            string color = "linkHeaderLight";
            
            if (milestone.Status == MilestoneStatus.Open)
            {
                if (milestone.DeadLine.AddDays(1) < ASC.Core.Tenants.TenantUtil.DateTimeNow()) color = "pm-redText " + color;
            }

            if(milestone.IsKey)
                innerHTML.AppendFormat(@"<img align='absmiddle' style='margin-right: 5px;' alt='{0}' src='{1}' title='{0}'>",
                                    MilestoneResource.RootMilestone, WebImageSupplier.GetAbsoluteWebPath("key.png", ProductEntryPoint.ID));

            innerHTML.AppendFormat(@"<a class='{3}' href='milestones.aspx?prjID={2}&id={0}'>{1}</a>",
                                    milestone.ID.ToString(),milestone.Title.HtmlEncode(),ProjectFat.Project.ID.ToString(),color);
                 
            return innerHTML.ToString();
        }

        protected String RenderDeadline(Milestone milestone)
        {
            if (milestone == null) return String.Empty;

            StringBuilder innerHTML = new StringBuilder();

            string color = string.Empty;
            string label;
          
            if(status == "Active")
            {
                label = String.Format(MilestoneResource.MilestoneStatusLabel_Active, "<strong>", "</strong>");
            }
            else
            {
             
                label = String.Format(MilestoneResource.MilestoneStatusLabel_Closed, "<strong>", "</strong>");
            }

            if (milestone.Status == MilestoneStatus.Open)
            {
                if (milestone.DeadLine.AddDays(1) < ASC.Core.Tenants.TenantUtil.DateTimeNow()) 
                {
                    color = "pm-redText";
                    label = String.Format(MilestoneResource.MilestoneStatusLabel_Late, "<strong>", "</strong>");
                }
            }

            innerHTML.AppendFormat("<div class='{1}' style='white-space: nowrap;'>{0}</div>", label,color);

            innerHTML.AppendFormat("<div class='{1}' style='white-space: nowrap;'>{0}</div>", milestone.DeadLine.ToString(DateTimeExtension.DateFormatPattern), color);

            return innerHTML.ToString();

        }

        protected String RenderDeadlineImage(Milestone milestone)
        {
            if (milestone == null) return String.Empty;

            StringBuilder innerHTML = new StringBuilder();

            string src = string.Empty;
            string alt = string.Empty;
            string title = string.Empty;

            if (status == "Active")
            {
                src = WebImageSupplier.GetAbsoluteWebPath("milestone_status_active_24.png", ProductEntryPoint.ID);
                alt = MilestoneResource.OpenMilestone;
                title = MilestoneResource.OpenMilestone;
            }
            else
            {
                src = WebImageSupplier.GetAbsoluteWebPath("milestone_status_completed_24.png", ProductEntryPoint.ID);
                alt = MilestoneResource.ClosedMilestone;
                title = MilestoneResource.ClosedMilestone;
            }

            if (milestone.Status == MilestoneStatus.Open)
            {
                if (milestone.DeadLine.AddDays(1) < ASC.Core.Tenants.TenantUtil.DateTimeNow())
                {
                    src = WebImageSupplier.GetAbsoluteWebPath("milestone_status_late_24.png", ProductEntryPoint.ID);
                    alt = MilestoneResource.LateMilestone;
                    title = MilestoneResource.LateMilestone;
                }
            }

            innerHTML.AppendFormat("<img src='{0}' alt='{1}' title='{2}'/>", src, alt, title);

            return innerHTML.ToString();

        }

        public String GetStatus(Milestone milestone)
        {
            StringBuilder innerHTML = new StringBuilder();

            innerHTML.AppendFormat(@"<span class='headerColumn' style='padding-right:10px;'>{0}:</span>", MilestoneResource.Status);
            innerHTML.AppendLine( RenderStatus(milestone.Status));

            if(milestone.Status==MilestoneStatus.Open)
                innerHTML.AppendFormat(@"<span style='padding-left:5px' >{0}</span>", ResourceEnumConverter.ConvertToString(MilestoneStatus.Open));
            if(milestone.Status==MilestoneStatus.Closed)
                innerHTML.AppendFormat(@"<span style='padding-left:5px' >{0}</span>", ResourceEnumConverter.ConvertToString(MilestoneStatus.Closed));

            return innerHTML.ToString();
        }

        public String GetTotalTasksCount(int milestoneID)
        {
            StringBuilder innerHTML = new StringBuilder();

            innerHTML.AppendFormat("<a class='linkHeaderLightMedium' href='milestones.aspx?prjID={0}&id={1}'>{2}</a>",
                ProjectFat.Project.ID,
                milestoneID,
                Global.EngineFactory.GetTaskEngine().GetTaskCount(milestoneID, TaskStatus.NotAccept, TaskStatus.Open, TaskStatus.Unclassified, TaskStatus.Closed));

            return innerHTML.ToString();
                    
        }

        public String GetProgress(int milestoneID)
        {
            StringBuilder innerHTML = new StringBuilder();

            innerHTML.AppendFormat("<a class='linkHeaderLightMedium' href='milestones.aspx?prjID={0}&id={1}'>{2}</a>",
                ProjectFat.Project.ID,
                milestoneID,
                Global.EngineFactory.GetTaskEngine().GetTaskCount(milestoneID, TaskStatus.NotAccept, TaskStatus.Open));

            return innerHTML.ToString();
        }

        public String GetTitle()
        {
            if (status == "Active")
            {
                return MilestoneResource.Active;
            }
            else return MilestoneResource.Closed;
        }

        #endregion


        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(TaskBlockView));
            MilestonesRepeater.DataSource = Milestones;
            MilestonesRepeater.DataBind();
        }

        #endregion
    }
}