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
using ASC.Core;
using ASC.Web.Projects.Configuration;
using ASC.Web.Studio.Shared.Extensions;
using ASC.Core.Users;
#endregion

namespace ASC.Web.Projects.Controls.Milestones
{
    public partial class ListMilestoneView : BaseUserControl
    {

        #region Members
        public MilestoneStatus? MilestoneStatus1 { get; set; }
        public int ProjectID { get; set; }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            DataRow row;
            DataTable Table = MakeTable();

            List<Milestone> milestones = new List<Milestone>(Page.EngineFactory.GetProjectEngine().GetByID(ProjectID).Milestones);
            List<Milestone> activeMilestones = milestones.FindAll(new Predicate<Milestone>(item => item.Status.ToString() != "Closed"));
            foreach (Milestone milestone in activeMilestones)
            {
                row = Table.NewRow();
                row = InitDataRow(row, milestone);
                Table.Rows.Add(row);
            }
            MilestonesRepeater.DataSource = Table;
            MilestonesRepeater.DataBind();
        }

        #region Methods


        public DataTable MakeTable()
        {
            DataTable Table = new DataTable();

            DataColumn MilestoneIDColumn = new DataColumn();
            MilestoneIDColumn.DataType = System.Type.GetType("System.Int32");
            MilestoneIDColumn.ColumnName = "MilestoneID";
            Table.Columns.Add(MilestoneIDColumn);

            DataColumn MilestoneTitleColumn = new DataColumn();
            MilestoneTitleColumn.DataType = System.Type.GetType("System.String");
            MilestoneTitleColumn.ColumnName = "MilestoneTitle";
            Table.Columns.Add(MilestoneTitleColumn);

            DataColumn MilestoneStatusColumn = new DataColumn();
            MilestoneStatusColumn.DataType = System.Type.GetType("System.String");
            MilestoneStatusColumn.ColumnName = "MilestoneStatus";
            Table.Columns.Add(MilestoneStatusColumn);

            DataColumn TaskCountColumn = new DataColumn();
            TaskCountColumn.DataType = System.Type.GetType("System.Int32");
            TaskCountColumn.ColumnName = "TaskCount";
            Table.Columns.Add(TaskCountColumn);

            DataColumn ActiveTasksCountColumn = new DataColumn();
            ActiveTasksCountColumn.DataType = System.Type.GetType("System.Int32");
            ActiveTasksCountColumn.ColumnName = "ActiveTasksCount";
            Table.Columns.Add(ActiveTasksCountColumn);

            DataColumn DeadLineColumn = new DataColumn();
            DeadLineColumn.DataType = System.Type.GetType("System.String");
            DeadLineColumn.ColumnName = "DeadLine";
            Table.Columns.Add(DeadLineColumn);

            DataColumn TitleColorColumn = new DataColumn();
            TitleColorColumn.DataType = System.Type.GetType("System.String");
            TitleColorColumn.ColumnName = "TitleColor";
            Table.Columns.Add(TitleColorColumn);

            return Table;
        }

        public DataRow InitDataRow(DataRow row, Milestone milestone)
        {
            row["MilestoneID"] = milestone.ID;
            row["MilestoneTitle"] = milestone.Title;
            row["MilestoneStatus"] = milestone.Status.ToString();
            row["TaskCount"] = milestone.Tasks.Count;
            List<Task> Tasks = new List<Task>(milestone.Tasks);
            List<Task> activeTasks = Tasks.FindAll(new Predicate<Task>(item => item.Status.ToString() != "Closed"));
            row["ActiveTasksCount"] = activeTasks.Count;
            row["DeadLine"] = milestone.DeadLine.ToString("dd.MM");
            if(milestone.Status.ToString()=="Late")
                row["TitleColor"] = "color:Red;";
            else row["TitleColor"] = "";

            return row;
        }

        #endregion
    }
}