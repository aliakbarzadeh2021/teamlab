using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxPro;
using ASC.Core;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Projects.Classes;
using ASC.Projects.Core;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Configuration;
using ASC.Web.Core.Helpers;
using ASC.Web.Projects.Resources;
using System.Text;
using System.IO;

namespace ASC.Web.Projects.Controls.IssueTracker
{
    [AjaxNamespace("AjaxPro.ListIssueTrackerView")]
    public partial class ListIssueTrackerView : BaseUserControl
    {
        public Project Project { get; set; }
        public bool MakeChangeStatus { get; set; }

        public List<Issue> Items
        {
            set { rptIssuesList.DataSource = value; }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(ListIssueTrackerView));
            rptIssuesList.DataSource = Global.EngineFactory.GetIssueEngine().GetIssues(Project.ID);
            rptIssuesList.DataBind();
        }

        protected void rptIssuesList_OnItemDataBound(Object Sender, RepeaterItemEventArgs e)
        {
            if (!(e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)) return;

            var cntrlIssueItemView = (IssueItemView)e.Item.FindControl("_issueItemView");
            var issue = (Issue)e.Item.DataItem;
            cntrlIssueItemView.issueID = issue.ID;
            cntrlIssueItemView.AssignedOn = issue.AssignedOn;
            cntrlIssueItemView.Target = issue;
            cntrlIssueItemView.RowIndex = e.Item.ItemIndex;
            cntrlIssueItemView.Status = RenderStatus(cntrlIssueItemView.Target);
        }

        protected String RenderStatus(Issue issue)
        {
            var html = new StringBuilder();
            string statusTitle = null;

            switch (issue.Status)
            {
                case IssueStatus.New:
                    html.AppendFormat(@"<img src='{0}' alt='{1}' title='{1}' style='border:0px; float:left;' />",
                        WebImageSupplier.GetAbsoluteWebPath("status_open_combo.png", ProductEntryPoint.ID), ResourceEnumConverter.ConvertToString(IssueStatus.New));
                    statusTitle = ResourceEnumConverter.ConvertToString(IssueStatus.New);
                    break;

                case IssueStatus.Open:
                    html.AppendFormat(@"<img src='{0}' alt='{1}' title='{1}' style='border:0px; float:left;' />",
                        WebImageSupplier.GetAbsoluteWebPath("status_open_combo.png", ProductEntryPoint.ID), ResourceEnumConverter.ConvertToString(IssueStatus.Open));
                    statusTitle = ResourceEnumConverter.ConvertToString(IssueStatus.Open);
                    break;

                case IssueStatus.Fixed:
                    html.AppendFormat(@"<img src='{0}' alt='{1}' title='{1}' style='border:0px;  float:left;' />",
                        WebImageSupplier.GetAbsoluteWebPath("status_closed_combo.png", ProductEntryPoint.ID), ResourceEnumConverter.ConvertToString(IssueStatus.Fixed));
                    statusTitle = ResourceEnumConverter.ConvertToString(IssueStatus.Fixed);
                    break;

                case IssueStatus.Closed:
                    html.AppendFormat(@"<img src='{0}' alt='{1}' title='{1}' style='border:0px;  float:left;' />",
                        WebImageSupplier.GetAbsoluteWebPath("status_wait_combo.png", ProductEntryPoint.ID), ResourceEnumConverter.ConvertToString(IssueStatus.Closed));
                    statusTitle = ResourceEnumConverter.ConvertToString(IssueStatus.Closed);
                    break;

                case IssueStatus.Rejected:
                    html.AppendFormat(@"<img src='{0}' alt='{1}' title='{1}' style='border:0px;  float:left;' />",
                        WebImageSupplier.GetAbsoluteWebPath("status_wait_combo.png", ProductEntryPoint.ID), ResourceEnumConverter.ConvertToString(IssueStatus.Rejected));
                    statusTitle = ResourceEnumConverter.ConvertToString(IssueStatus.Rejected);
                    break;

                default:
                    statusTitle = String.Empty;
                    break;
            }

            return String.Format(@"<a  style='float:left;' href='javascript:void(0)' class='pm-issueStatus-switch' onclick='javascript:ASC.Projects.IssueTrackerPage.execShowIssueActionPanel({0}, this)'>{1}</a><div style='margin-left: 50px; line-height: 24px;' class='textBigDescribe'>{2}</div>", (int)issue.Status, html, statusTitle);
        }

        [AjaxMethod]
        public NameValueCollection SaveOrUpdateIssue(int issueID, int projectID, String title, String detectedInVersion, int priority, String description, Guid assignedOn, String correctedInVersion, bool notifyAssignedOn)
        {
            ProjectSecurity.DemandAuthentication();

            var issue = new Issue()
            {
                ProjectID = projectID,
                Title = title,
                Description = description,
                AssignedOn = assignedOn,
                DetectedInVersion = detectedInVersion,
                CorrectedInVersion = correctedInVersion,
                Priority = (IssuePriority)priority,
                Status = IssueStatus.New,
            };

            Global.EngineFactory.GetIssueEngine().SaveIssue(issue);

            var page = new Page();
            var cntrlIssueItemView = (IssueItemView)LoadControl(PathProvider.GetControlVirtualPath("IssueItemView.ascx"));
            cntrlIssueItemView.AssignedOn = issue.AssignedOn;
            cntrlIssueItemView.Target = issue;
            cntrlIssueItemView.RowIndex = 0;
            cntrlIssueItemView.Status = RenderStatus(issue);
            cntrlIssueItemView.MultiSelect = true;

            page.Controls.Add(cntrlIssueItemView);

            var output = string.Empty;
            using (var writer = new StringWriter())
            {
                HttpContext.Current.Server.Execute(page, writer, false);
                output = writer.ToString();
            }

            return new NameValueCollection
            {
                {"ID", issue.ID.ToString()},
                {"HTML", output}, 
                {"InfoBlockText", String.Empty}
            };
        }

        [AjaxMethod]
        public String IssueActionPanelHTML(int issueID, int issueStatus)
        {
            ProjectSecurity.DemandAuthentication();

            var status = (IssueStatus)issueStatus;
            var innerHTML = new StringBuilder("<dl class='cornerAll pm-issueStatus pm-flexible'>");
            switch (status)
            {
                case IssueStatus.New:
                    innerHTML.AppendFormat(
                                       @"<dt>
                                           <img src='{0}' title='{2}' alt='{2}' />
                                      </dt>
                                      <dd  class='textMediumDescribe' onclick='javascript:ASC.Projects.IssueTrackerPage.execIssueChangeStatus({4},{3})'>
                                            {1}
                                      </dd>",
                          WebImageSupplier.GetAbsoluteWebPath("status_open.png", ProductEntryPoint.ID), IssueTrackerResource.IssueOpen.ToLower(), IssueTrackerResource.IssueOpen, (int)IssueStatus.Open, issueID);

                    innerHTML.AppendFormat(
                         @"<dt>
                                <img src='{0}' title='{2}' alt='{2}' />
                          </dt>
                          <dd  class='textMediumDescribe' onclick='javascript:ASC.Projects.IssueTrackerPage.execIssueChangeStatus({4},{3})'>
                                {1}
                          </dd>",
                           WebImageSupplier.GetAbsoluteWebPath("status_wait.png", ProductEntryPoint.ID), IssueTrackerResource.IssueRejected.ToLower(), IssueTrackerResource.IssueRejected, (int)IssueStatus.Rejected, issueID);

                    break;
                case IssueStatus.Open:
                    innerHTML.AppendFormat(
                                       @"<dt>
                                           <img src='{0}' title='{2}' alt='{2}' />
                                      </dt>
                                      <dd  class='textMediumDescribe'  onclick='javascript:ASC.Projects.IssueTrackerPage.execIssueChangeStatus({4},{3})'>
                                            {1}
                                      </dd>",
                          WebImageSupplier.GetAbsoluteWebPath("status_closed.png", ProductEntryPoint.ID), IssueTrackerResource.IssueFixed.ToLower(), IssueTrackerResource.IssueFixed, (int)IssueStatus.Fixed, issueID);

                    break;
                case IssueStatus.Fixed:
                    innerHTML.AppendFormat(
                                       @"<dt>
                                           <img src='{0}' title='{2}' alt='{2}' />
                                      </dt>
                                      <dd  class='textMediumDescribe' onclick='javascript:ASC.Projects.IssueTrackerPage.execIssueChangeStatus({4},{3})'>
                                            {1}
                                      </dd>",
                          WebImageSupplier.GetAbsoluteWebPath("status_open.png", ProductEntryPoint.ID), IssueTrackerResource.IssueOpen.ToLower(), IssueTrackerResource.IssueOpen, (int)IssueStatus.Open, issueID);

                    innerHTML.AppendFormat(
                         @"<dt>
                                <img src='{0}' title='{2}' alt='{2}' />
                          </dt>
                          <dd  class='textMediumDescribe'  onclick='javascript:ASC.Projects.IssueTrackerPage.execIssueChangeStatus({4},{3})'>
                                {1}
                          </dd>",
                          WebImageSupplier.GetAbsoluteWebPath("status_wait.png", ProductEntryPoint.ID), IssueTrackerResource.IssueClosed.ToLower(), IssueTrackerResource.IssueClosed, (int)IssueStatus.Closed, issueID);

                    break;
                case IssueStatus.Closed:
                    innerHTML.AppendFormat(
                                       @"<dt>
                                           <img src='{0}' title='{2}' alt='{2}' />
                                      </dt>
                                      <dd  class='textMediumDescribe' onclick='javascript:ASC.Projects.IssueTrackerPage.execIssueChangeStatus({4},{3})'>
                                            {1}
                                      </dd>",
                          WebImageSupplier.GetAbsoluteWebPath("status_open.png", ProductEntryPoint.ID), IssueTrackerResource.IssueOpen.ToLower(), IssueTrackerResource.IssueOpen, (int)IssueStatus.Open, issueID);

                    break;
                case IssueStatus.Rejected:
                    innerHTML.AppendFormat(
                                       @"<dt>
                                           <img src='{0}' title='{2}' alt='{2}' />
                                      </dt>
                                      <dd  class='textMediumDescribe' onclick='javascript:ASC.Projects.IssueTrackerPage.execIssueChangeStatus({4},{3})'>
                                            {1}
                                      </dd>",
                          WebImageSupplier.GetAbsoluteWebPath("status_open.png", ProductEntryPoint.ID), IssueTrackerResource.IssueOpen.ToLower(), IssueTrackerResource.IssueOpen, (int)IssueStatus.Open, issueID);

                    innerHTML.AppendFormat(
                         @"<dt>
                                <img src='{0}' title='{2}' alt='{2}' />
                          </dt>
                          <dd  class='textMediumDescribe'  onclick='javascript:ASC.Projects.IssueTrackerPage.execIssueChangeStatus({4},{3})'>
                                {1}
                          </dd>",
                          WebImageSupplier.GetAbsoluteWebPath("status_wait.png", ProductEntryPoint.ID), IssueTrackerResource.IssueClosed.ToLower(), IssueTrackerResource.IssueClosed, (int)IssueStatus.Closed, issueID);

                    break;
            }


            innerHTML.AppendLine("</dl>");

            return innerHTML.ToString();

        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public String IssueChangeStatus(int issueID, int newStatus)
        {
            ProjectSecurity.DemandAuthentication();

            var issue = Global.EngineFactory.GetIssueEngine().GetIssue(issueID);
            if (issue != null)
            {
                issue.Status = (IssueStatus)newStatus;
                Global.EngineFactory.GetIssueEngine().SaveIssue(issue);
                MakeChangeStatus = true;
                return RenderStatus(issue);
            }
            return string.Empty;
        }
    }
}