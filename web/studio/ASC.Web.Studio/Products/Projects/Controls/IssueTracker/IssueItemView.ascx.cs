using System;
using System.Text;
using System.Web;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Resources;
using System.Collections.Generic;
using ASC.Web.Projects.Classes;

namespace ASC.Web.Projects.Controls.IssueTracker
{
    public partial class IssueItemView : BaseUserControl
    {
        public String Title
        { get; set; }

        public int issueID
        { get; set; }

        public Guid AssignedOn
        { get; set; }

        public String Status
        { get; set; }

        public int RowIndex
        { get; set; }

        public Issue Target
        { get; set; }

        public bool MultiSelect
        { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected String RenderHighlightRow(Issue issue, int rowIndex)
        {
            String issueStatusClass = String.Empty;

            switch(issue.Status)
            {
                case IssueStatus.New:
                    issueStatusClass = "pm-issue-new";
                    break;
                case IssueStatus.Open:
                    issueStatusClass = "pm-issue-open";
                    break;
                case IssueStatus.Closed:
                    issueStatusClass = "pm-issue-closed";
                    break;
                case IssueStatus.Fixed:
                    issueStatusClass = "pm-issue-fixed";
                    break;
                case IssueStatus.Rejected:
                    issueStatusClass = "pm-issue-rejected";
                    break;
            }

            return String.Format("class='{0}'", issueStatusClass);
        }

        protected String RenderTitle(Issue issue)
        {

            StringBuilder innerHTML = new StringBuilder();

            switch (issue.Priority)
            {

                case IssuePriority.High:
                    innerHTML.AppendFormat("<img title='{0}' src='{1}' align='top' alt='{0}' style='margin-right:5px;margin-top:1px; ' />", IssueTrackerResource.HighPriority_Title, WebImageSupplier.GetAbsoluteWebPath("prior_hi.png", ProductEntryPoint.ID));
                    break;
                case IssuePriority.Low:
                    innerHTML.AppendFormat("<img title='{0}' src='{1}' align='top' alt='{0}' style='margin-right:5px;margin-top:1px; ' />", IssueTrackerResource.LowPriority_Title, WebImageSupplier.GetAbsoluteWebPath("prior_lo.png", ProductEntryPoint.ID));
                    break;
            }


            innerHTML.AppendFormat("<a class='linkHeaderLightMedium' title=\"{3}\" href='issueTracker.aspx?prjID={0}&id={1}'>{2}</a>",
                issue.ProjectID,
                issue.ID,
                issue.Title,
                issue.Description.ReplaceSingleQuote().Replace("\n", "<br/>")
                );

            return innerHTML.ToString();
        }
    }
}