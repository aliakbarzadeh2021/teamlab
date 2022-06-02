#region Import

using System;
using System.Collections.Generic;
using ASC.Core;
using ASC.Projects.Core;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Controls;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Controls;
using ASC.Web.Projects.Controls.Milestones;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;
using ASC.Projects.Core.Services.NotifyService;
using System.Text;

#endregion

namespace ASC.Web.Projects
{
    public partial class Milestones : BasePage
    {
        protected override void PageLoad()
        {
            RequestContext.EnsureCurrentProduct();


            int milestoneID;
            string actionType = UrlParameters.ActionType;

            var projectFat = RequestContext.GetCurrentProjectFat();

            if (!ProjectSecurity.CanReadMilestones(projectFat.Project))
            {
                Response.Redirect(ProjectsCommonResource.StartURL);
            }

            if (String.Compare(actionType, "add", true) == 0)
            {
                if (!ProjectSecurity.CanCreateMilestone(projectFat.Project))
                    Response.Redirect("milestones.aspx?prjID=" + projectFat.Project.ID);
                else
                    MilestoneActionView(actionType, projectFat, 0);
            }
            else
            {
                if (Int32.TryParse(UrlParameters.EntityID, out milestoneID))
                {
                    var milestone = projectFat.GetMilestone(milestoneID);
                    if (ProjectSecurity.CanRead(milestone))
                    {
                        if (String.Compare(actionType, "edit", true) == 0)
                        {
                            if (!ProjectSecurity.CanEdit(milestone))
                                Response.Redirect("milestones.aspx?prjID=" + projectFat.Project.ID);
                            else
                                MilestoneActionView(actionType, projectFat, milestoneID);
                        }
                        else
                            MilestoneDetailsView(projectFat, milestone);
                    }
                    else
                        ElementNotFoundControlView(projectFat.Project.ID);
                }
                else
                {
                    var milestones = projectFat.GetMilestones();
                    if (milestones.Count != 0)
                        ListMilestoneView(projectFat, milestones);
                    else
                        EmptyScreenControlView(projectFat.Project);
                }
            }




            Title = HeaderStringHelper.GetPageTitle(MilestoneResource.Milestones, Master.BreadCrumbs);
        }

        #region Methods

        protected void ListMilestoneView(ProjectFat projectFat, List<Milestone> milestones)
        {
            if (milestones != null && 0 < milestones.Count)
            {
                milestones.ForEach(m => ProjectSecurity.DemandRead(m));
            }
            var listMilestones = milestones;
            var milestonesView = (ListMilestonesView)LoadControl(PathProvider.GetControlVirtualPath("ListMilestonesView.ascx"));
            milestonesView.status = "Active";
            milestonesView.ProjectFat = projectFat;

            milestonesView.Milestones = listMilestones.FindAll(item => item.Status != MilestoneStatus.Closed && item.Status != MilestoneStatus.Disable);
            if (milestonesView.Milestones.Count != 0)
            {
                active_content.Controls.Add(milestonesView);
            }

            milestonesView = (ListMilestonesView)LoadControl(PathProvider.GetControlVirtualPath("ListMilestonesView.ascx"));
            milestonesView.status = "Closed";
            milestonesView.ProjectFat = projectFat;

            milestonesView.Milestones = listMilestones.FindAll(item => item.Status == MilestoneStatus.Closed && item.Status != MilestoneStatus.Disable);
            if (milestonesView.Milestones.Count != 0)
            {
                closed_content.Controls.Add(milestonesView);
            }

            if (ProjectSecurity.CanCreateMilestone(projectFat.Project))
            {
                SideActionsPanel.Controls.Add(new NavigationItem
                {
                    Name = MilestoneResource.CreateNewMilestone,
                    URL = "milestones.aspx?prjID=" + projectFat.Project.ID + "&action=add"
                });
            }

            Master.BreadCrumbs.Add(new BreadCrumb(ProjectResource.Projects, "projects.aspx"));
            Master.BreadCrumbs.Add(new BreadCrumb(projectFat.Project.HtmlTitle.HtmlEncode(), "projects.aspx?prjID=" + projectFat.Project.ID));
            Master.BreadCrumbs.Add(new BreadCrumb(MilestoneResource.Milestones));

            SideActionsPanel.Visible = SideActionsPanel.Controls.Count != 0;
            SideNavigatorPanel.Visible = false;
        }

        protected void MilestoneDetailsView(ProjectFat projectFat, Milestone milestone)
        {
            var cntrlMilestoneDetailsView = (MilestoneDetailsView)LoadControl(PathProvider.GetControlVirtualPath("MilestoneDetailsView.ascx"));

            cntrlMilestoneDetailsView.Milestone = milestone;
            cntrlMilestoneDetailsView.ProjectFat = projectFat;

            content.Controls.Add(cntrlMilestoneDetailsView);

            SideNavigatorPanel.Visible = false;

            if (ProjectSecurity.CanCreateMilestone(projectFat.Project))
                SideActionsPanel.Controls.Add(new NavigationItem
                {
                    Name = MilestoneResource.CreateNewMilestone,
                    URL = "milestones.aspx?prjID=" + projectFat.Project.ID + "&action=add"

                });


            MilestoneStatus status = milestone.Status;

            if (status != MilestoneStatus.Disable)
            {
                if (ProjectSecurity.CanEdit(milestone))
                {

                    if (status != MilestoneStatus.Closed)
                    {
                        String nameAction = MilestoneResource.FinishMilestone.ToLower();
                        nameAction = Char.ToUpper(nameAction[0]) + nameAction.Substring(1);

                        SideActionsPanel.Controls.Add(new NavigationItem
                        {
                            Name = nameAction,
                            URL = "javascript:ASC.Projects.Milestones.finishMilestone();"
                        });

                    }
                    if (status == MilestoneStatus.Closed)
                    {
                        String nameAction = MilestoneResource.ResumeMilestone.ToLower();
                        nameAction = Char.ToUpper(nameAction[0]) + nameAction.Substring(1);

                        SideActionsPanel.Controls.Add(new NavigationItem
                        {
                            Name = nameAction,
                            URL = "javascript:ASC.Projects.Milestones.resumeMilestone();"
                        });
                    }

                }
            }


            if (ProjectSecurity.CanCreateTask(milestone.Project, false) && milestone.Status != MilestoneStatus.Closed)
                SideActionsPanel.Controls.Add(new NavigationItem
                                                  {
                                                      Name = TaskResource.AddTask,
                                                      URL = String.Format(
                                                          "javascript:ASC.Projects.TaskActionPage.init(-1,{0}, null); ASC.Projects.TaskActionPage.show()", milestone.ID)

                                                  });

            if (ProjectSecurity.CanEdit(milestone))
            {
                if (milestone.Status != MilestoneStatus.Closed)
                {
                    SideActionsPanel.Controls.Add(new NavigationItem
                    {
                        Name = MilestoneResource.EditMilestone,
                        URL = "milestones.aspx?prjID=" + projectFat.Project.ID + "&ID=" + milestone.ID + "&action=edit"
                    });
                }

                SideActionsPanel.Controls.Add(new NavigationItem
                {
                    Name = MilestoneResource.Delete,
                    URL = "javascript:ASC.Projects.Milestones.deleteMilestone();"
                });
            }

            if (SecurityContext.IsAuthenticated)
            {
                //add subscribe to milestone block
                var objectID = String.Format("{0}_{1}", milestone.UniqID, projectFat.Project.ID);
                var recipient = NotifySource.Instance.GetRecipientsProvider().GetRecipient(SecurityContext.CurrentAccount.ID.ToString());
                var objects = recipient != null ?
                    new List<string>(NotifySource.Instance.GetSubscriptionProvider().GetSubscriptions(NotifyConstants.Event_NewCommentForMilestone, recipient)) :
                    new List<string>();

                var subscribed = !String.IsNullOrEmpty(objects.Find(item => String.Compare(item, objectID, true) == 0));

                var innerHTML = new StringBuilder("<div id='milestone_subscriber'>");
                innerHTML.AppendFormat("<a  href='javascript:void(0)' onclick='javascript:ASC.Projects.Milestones.changeSubscribe({1})' class=\"linkAction\">{0}</a>",
                      subscribed ? ProjectsCommonResource.UnSubscribeOnNewComment : ProjectsCommonResource.SubscribeOnNewComment,
                      milestone.ID);
                innerHTML.Append("</div>");
                SideActionsPanel.Controls.Add(new HtmlMenuItem(innerHTML.ToString()));
            }
            if (SideActionsPanel.Controls.Count == 0)
            {
                SideActionsPanel.Visible = false;
            }
            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = ProjectResource.Projects,
                NavigationUrl = "projects.aspx"

            });
            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = projectFat.Project.HtmlTitle.HtmlEncode(),
                NavigationUrl = "projects.aspx?prjID=" + projectFat.Project.ID

            });
            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = MilestoneResource.Milestones,
                NavigationUrl = "milestones.aspx?prjID=" + projectFat.Project.ID

            });
            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = milestone.Title.HtmlEncode(),
                NavigationUrl = ""

            });
            Master.CommonContainerHeader = Global.RenderCommonContainerHeader(
                string.Format("<span class='headerBase' style='padding-right:10px'>{0}</span>", milestone.DeadLine.ToString(DateTimeExtension.DateFormatPattern)) +
                milestone.Title.HtmlEncode(), EntityType.Milestone);
            Title = HeaderStringHelper.GetPageTitle(MilestoneResource.Milestone, Master.BreadCrumbs);
        }

        protected void MilestoneActionView(string actionType, ProjectFat projectFat, int milestoneID)
        {
            MilestoneActionView cntrlMilestoneActionView = (MilestoneActionView)LoadControl(PathProvider.GetControlVirtualPath("MilestoneActionView.ascx"));

            cntrlMilestoneActionView.ActionType = actionType;
            cntrlMilestoneActionView.ProjectFat = projectFat;
            if (milestoneID != 0)
                cntrlMilestoneActionView.Milestone = projectFat.GetMilestone(milestoneID);
            else
                cntrlMilestoneActionView.Milestone = new Milestone();
            content.Controls.Add(cntrlMilestoneActionView);

            SideNavigatorPanel.Visible = false;
            SideActionsPanel.Visible = false;


            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = ProjectResource.Projects,
                NavigationUrl = "projects.aspx"

            });
            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = projectFat.Project.HtmlTitle.HtmlEncode(),
                NavigationUrl = "projects.aspx?prjID=" + projectFat.Project.ID.ToString()

            });
            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = MilestoneResource.Milestones,
                NavigationUrl = "milestones.aspx?prjID=" + projectFat.Project.ID.ToString()

            });
            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = actionType == "add" ? MilestoneResource.CreateNewMilestone : String.Format("{0} «{1}»", MilestoneResource.EditMilestone, Global.EngineFactory.GetMilestoneEngine().GetByID(milestoneID).Title.HtmlEncode()),
                NavigationUrl = ""

            });

            Title = HeaderStringHelper.GetPageTitle(MilestoneResource.CreateNewMilestone, Master.BreadCrumbs);
        }

        protected void EmptyScreenControlView(Project project)
        {
            var emptyScreenControl = new EmptyScreenControl();
            if (!ProjectSecurity.CanCreateMilestone(project))
            {
                SideActionsPanel.Visible = false;
                emptyScreenControl.HeaderContent = MilestoneResource.NoOneMilestoneInProject;
                emptyScreenControl.HeaderDescribe = string.Empty;
            }
            else
            {
                SideActionsPanel.Controls.Add(new NavigationItem
                {
                    Name = MilestoneResource.CreateNewMilestone,
                    URL = "milestones.aspx?prjID=" + project.ID + "&action=add"
                });
                emptyScreenControl.HeaderContent = String.Format("<a href='{1}'>{0}</a>", MilestoneResource.EmptyScreenHeaderContent, String.Format("milestones.aspx?prjID={0}&action=add", project.ID));
                emptyScreenControl.HeaderDescribe = MilestoneResource.EmptyScreenHeaderDescribe;
            }

            content.Controls.Add(emptyScreenControl);

            SideNavigatorPanel.Visible = false;

            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = ProjectResource.Dashboard,
                NavigationUrl = "./"

            });
            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = ProjectResource.Projects,
                NavigationUrl = "projects.aspx"

            });
            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = project.HtmlTitle.HtmlEncode(),
                NavigationUrl = "projects.aspx?PrjID=" + project.ID

            });
            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = MilestoneResource.Milestones,
                NavigationUrl = ""

            });

        }

        protected void ElementNotFoundControlView(int prjID)
        {

            content.Controls.Add(new ElementNotFoundControl
            {
                Header = MilestoneResource.MilestoneNotFound_Header,
                Body = MilestoneResource.MilestoneNotFound_Body,
                RedirectURL = String.Format("milestones.aspx?prjID={0}", prjID),
                RedirectTitle = MilestoneResource.MilestoneNotFound_RedirectTitle
            });
            SideNavigatorPanel.Visible = false;
            SideActionsPanel.Visible = false;

        }

        #endregion
    }
}
