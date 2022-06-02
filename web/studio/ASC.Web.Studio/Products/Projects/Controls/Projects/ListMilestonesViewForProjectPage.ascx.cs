#region Import

using System;
using System.Collections.Generic;
using System.Text;
using ASC.Projects.Core.Domain;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Resources;
#endregion

namespace ASC.Web.Projects.Controls.Projects
{
    public partial class ListMilestonesViewForProjectPage : BaseUserControl
    {
        #region Members

        public int ProjectID { get; set; }

        #endregion

        #region Property

        public List<Milestone> Milestones { get; set; }

        #endregion


        #region Methods

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

        public String GrammaticalHelperTotalCountTasks(Milestone milestone)
        {
            int count = 0;
            foreach (Task task in Global.EngineFactory.GetTaskEngine().GetMilestoneTasks(milestone.ID))
            {
                if (task.Status != TaskStatus.Disable)
                {
                    count++;
                }
            }

            return count + " " + GrammaticalHelper.ChooseNumeralCase(count, GrammaticalResource.TaskNominative, GrammaticalResource.TaskGenitiveSingular, GrammaticalResource.TaskGenitivePlural);
        }

        public String GrammaticalHelperActiveCountTasks(Milestone milestone)
        {
            int count = 0;
            foreach (Task task in Global.EngineFactory.GetTaskEngine().GetMilestoneTasks(milestone.ID))
                if (task.Status == TaskStatus.Open)
                    count++;

            return count + " " + GrammaticalHelper.ChooseNumeralCase(count, GrammaticalResource.TaskNotDoneNominative, GrammaticalResource.TaskNotDoneGenitiveSingular, GrammaticalResource.TaskNotDoneGenitivePlural);
        }

        public String GetCaption(Milestone milestone)
        {
            StringBuilder innerHTML = new StringBuilder();

            string color = "";

            if (milestone.Status == MilestoneStatus.Closed) color = "pm-greenText";
            if (milestone.Status == MilestoneStatus.Open)
            {
                color = "pm-blueText";
                if (milestone.DeadLine.AddDays(1) < ASC.Core.Tenants.TenantUtil.DateTimeNow()) color = "pm-redText";
            }

            innerHTML.AppendFormat(@"<span class='headerBase pm-grayText' style='padding-right:10px;'>{0}</span>", DateTimeExtension.ToShortDayMonth(milestone.DeadLine));

            innerHTML.AppendFormat(@"<a class='{3}' href='milestones.aspx?prjID={2}&id={0}' style='font-family:Arial,sans-serif;font-size:16px;font-weight:bolder;text-decoration:underline;'>{1}</a>",
                                    milestone.ID.ToString(), milestone.Title, ProjectID.ToString(), color);

            return innerHTML.ToString();
        }

        public String GetStatus(Milestone milestone)
        {
            StringBuilder innerHTML = new StringBuilder();

            innerHTML.AppendFormat(@"<span class='headerColumn' style='margin-left:50px;padding-right:10px;'>{0}:</span>", MilestoneResource.Status);
            innerHTML.AppendLine(RenderStatus(milestone.Status));

            if (milestone.Status == MilestoneStatus.Open)
                innerHTML.AppendFormat(@"<span style='padding-left:5px' >{0}</span>", ResourceEnumConverter.ConvertToString(MilestoneStatus.Open));
            if (milestone.Status == MilestoneStatus.Closed)
                innerHTML.AppendFormat(@"<span style='padding-left:5px' >{0}</span>", ResourceEnumConverter.ConvertToString(MilestoneStatus.Closed));

            return innerHTML.ToString();
        }

        public String GetTotalTasksCount(Milestone milestone)
        {
            StringBuilder innerHTML = new StringBuilder();
            innerHTML.AppendLine("<div style='text-align:left'>");
            innerHTML.AppendFormat(@"<span class='headerColumn' style='margin-left:50px;padding-right:10px;'>{0}:</span>", MilestoneResource.Total);
            innerHTML.AppendFormat(@"<span>{0}</span>", GrammaticalHelperTotalCountTasks(milestone));
            innerHTML.AppendLine("</div>");
            return innerHTML.ToString();

        }

        public String GetProgress(Milestone milestone)
        {
            StringBuilder innerHTML = new StringBuilder();
            innerHTML.AppendLine("<div style='text-align:left'>");

            innerHTML.AppendFormat(@"<span class='headerColumn' style='padding-right:10px;'>{0}:</span>", MilestoneResource.Progress);
            innerHTML.AppendFormat(@"<span>{0}</span>", GrammaticalHelperActiveCountTasks(milestone));

            innerHTML.AppendLine("</div>");
            return innerHTML.ToString();
        }

        #endregion


        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            MilestonesRepeater.DataSource = Milestones;
            MilestonesRepeater.DataBind();
        }

        #endregion

    }
}