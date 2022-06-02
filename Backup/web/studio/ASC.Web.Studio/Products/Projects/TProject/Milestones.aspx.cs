using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Web.Projects.Classes;
using ASC.Web.Controls;
using ASC.Web.Projects.Resources;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects.Controls.ProjectTemplates;
using ASC.Web.Studio.Controls.Common;
using AjaxPro;
using ASC.Web.Studio.Utility;
using ASC.Web.Projects.Controls;
using ASC.Projects.Engine;

namespace ASC.Web.Projects.TProject
{
	[AjaxNamespace("MilestoneTemplates")]
	public partial class Milestones : BasePage
	{

		public TemplateProject Template { get; set; }

		public TemplateMilestone Milestone { get; set; }

		private void InitMilestone()
		{
			Template = ProjectTemplatesUtil.GetProjectTemplate();
		}

		protected override void PageLoad()
		{
			Utility.RegisterTypeForAjax(this.GetType());
			InitMilestone();

			ProjectTemplatesUtil.InitProjectTemplatesBreadcrumbs(Master, Template, MilestoneResource.Milestones, "milestones");

			SideActionsPanel.Controls.Add(new NavigationItem()
			{
				Name = MilestoneResource.CreateNewMilestone,
				URL = string.Format(ProjectTemplatesConst.PlanNewMilestonePath, "milestones", Template.Id)
			});

			var engine = Global.EngineFactory.GetTemplateEngine();

			int milestoneID;

			string actionType = UrlParameters.ActionType.ToLower();
			switch (actionType)
			{
				#region Create Milestone Mode
				case "add":
					CreateMilestonePanel.Visible = true;
					SideActionsPanel.Visible = false;
					Master.BreadCrumbs.Add(new BreadCrumb
					{
						Caption = MilestoneResource.CreateNewMilestone,
						NavigationUrl = string.Empty
					});

					Title = HeaderStringHelper.GetPageTitle(MilestoneResource.CreateNewMilestone, Master.BreadCrumbs);

					CreateMilestoneButton.ButtonText = MilestoneResource.AddThisMilestone;
					CreateMilestoneButton.AjaxRequestText = MilestoneResource.MilestoneAdded;

					Page.ClientScript.RegisterClientScriptBlock(typeof(string), "set_input_focus", "; jq(function(){jq('#MilestoneTitleInput').focus();}); ", true);
					break;
				#endregion


				#region Edit Milestone Mode
				case "edit":
					CreateMilestonePanel.Visible = true;
					SideActionsPanel.Visible = false;
					Master.BreadCrumbs.Add(new BreadCrumb
					{
						Caption = MilestoneResource.EditMilestone,
						NavigationUrl = string.Empty
					});

					Title = HeaderStringHelper.GetPageTitle(MilestoneResource.EditMilestone, Master.BreadCrumbs);

					CreateMilestoneButton.ButtonText = ProjectsCommonResource.SaveChanges;
					CreateMilestoneButton.AjaxRequestText = MilestoneResource.SaveMilestone;

					if (int.TryParse(UrlParameters.EntityID, out milestoneID))
					{
						Milestone = engine.GetTemplateMilestone(milestoneID);
						if (Milestone != null)
						{
							int week, day;
							ProjectTemplatesUtil.GetMilestoneDeadLine(Milestone.DurationInDays, out week, out day);
                            hfMilestoneTitle.Value = Milestone.Title;
                            var js = string.Format("jq(function(){{ ProjectTemplatesController.InitMilestone({0}, {1}, {2}, {3}); }});", Milestone.IsNotify.ToString().ToLower(), Milestone.IsKey.ToString().ToLower(), week, day);
							Page.ClientScript.RegisterClientScriptBlock(typeof(string), "initMilestone", js, true);
						}
					}

					break;
				#endregion

				default:
					#region Milestone Details Mode
					if (int.TryParse(UrlParameters.EntityID, out milestoneID))
					{
						MilestoneInfoPanel.Visible = true;
						Milestone = engine.GetTemplateMilestone(milestoneID);

						if (Milestone == null)
						{
							ElementNotFoundControlView(Template.Id);
						}
						else
						{
							Master.BreadCrumbs.Add(new BreadCrumb
							{
								Caption = Milestone.Title.HtmlEncode(),
								NavigationUrl = string.Empty
							});

							Title = HeaderStringHelper.GetPageTitle(Milestone.Title, Master.BreadCrumbs);

							SideActionsPanel.Controls.Add(new NavigationItem()
							{
								Name = MilestoneResource.EditMilestone,
								URL = string.Format(ProjectTemplatesConst.ItemEditPath, "milestones", Template.Id, Milestone.Id)
							});
							SideActionsPanel.Controls.Add(new NavigationItem()
							{
								Name = MilestoneResource.Delete,
								URL = "javascript:ProjectTemplatesController.DeleteMilestoneTemplate();"
							});

							Master.CommonContainerHeader = Global.RenderCommonContainerHeader(string.Format("<span class='headerBase' style='padding-right:10px'>{0}</span>", Milestone.Title.HtmlEncode()), EntityType.Milestone);

							MilestoneDetailsView(Template, Milestone);
						}
					}
					#endregion

					#region List Milestones Mode
					else
					{
						MilestonesHolder.Visible = true;
						var milestones = engine.GetTemplateMilestones(Template.Id);
						if (milestones == null || milestones.Count == 0)
						{
							EmptyScreenControlView(Template);
						}
						else
						{
							var milestonesList = (ListMilestonesTemplate)LoadControl(PathProvider.GetControlVirtualPath("ListMilestonesTemplate.ascx"));
							milestonesList.Template = Template;
							milestonesList.MilestoneTemplates = milestones;
							MilestonesHolder.Controls.Add(milestonesList);
						}
						Title = HeaderStringHelper.GetPageTitle(MilestoneResource.Milestones, Master.BreadCrumbs);
					}
					#endregion
					break;
			}

			ProjectTemplatesUtil.AddCreateProjectFromTemplateActions(SideActionsPanel, Template);

		}



		protected string DayName(int day)
		{
			return System.Globalization.DateTimeFormatInfo.CurrentInfo.GetDayName((DayOfWeek)day);
		}

		protected int FirstDay
		{
			get
			{
				return (int)System.Globalization.DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek;
			}
		}

		protected string MilestonesPath
		{
			get
			{
				return string.Format(ProjectTemplatesConst.TemplatePath, "milestones", Template.Id);
			}
		}

		[AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
		public string CreateMilestone(string title, int projectID, bool notify, bool isKey, int milestoneID, int week, int dayOfWeek)
		{
            ProjectSecurity.DemandAuthentication();

			try
			{
				var engine = Global.EngineFactory.GetTemplateEngine();
				TemplateMilestone milestone = null;
				if (milestoneID > 0)
				{
					milestone = engine.GetTemplateMilestone(milestoneID);
				}
				if (milestone == null)
				{
					milestone = new TemplateMilestone(projectID, title);
				}
				milestone.Title = title;
				milestone.ProjectId = projectID;
				milestone.IsNotify = notify;
				milestone.IsKey = isKey;
				milestone.DurationInDays = week * 10 + dayOfWeek;

				engine.SaveTemplateMilestone(milestone);
			}
			catch (Exception e)
			{
				return e.Message;
			}
			return string.Empty;
		}

		[AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
		public string DeleteMilestoneTemplate(int milestoneID, int projectID)
		{
            ProjectSecurity.DemandAuthentication();

			var engine = Global.EngineFactory.GetTemplateEngine();
			engine.RemoveTemplateMilestone(milestoneID);
			return string.Format(ProjectTemplatesConst.TemplatePath, "milestones", projectID);
		}




		protected void MilestoneDetailsView(TemplateProject template, TemplateMilestone milestone)
		{
			MilestoneDetailsTemplateView cntrlMilestoneDetailsView = (MilestoneDetailsTemplateView)LoadControl(PathProvider.GetControlVirtualPath("MilestoneDetailsTemplateView.ascx"));
			cntrlMilestoneDetailsView.Milestone = milestone;
			cntrlMilestoneDetailsView.Template = template;
			MilestoneInfoPanel.Controls.Add(cntrlMilestoneDetailsView);

			SideActionsPanel.Controls.Add(new NavigationItem
			{
				Name = TaskResource.AddNewTask,
				URL = String.Format(
					"javascript:ASC.Projects.TaskActionPage.init(-1,{0}, null); ASC.Projects.TaskActionPage.show()", milestone.Id)

			});
		}

		protected void EmptyScreenControlView(TemplateProject template)
		{
			if (ProjectTemplatesUtil.CheckEditPermission(template))
			{
				EmptyScreenControl emptyScreenControl = new EmptyScreenControl();
				emptyScreenControl.HeaderContent = String.Format("<a href='{1}'>{0}</a>", MilestoneResource.EmptyScreenMilestoneTemplate, String.Format("milestones.aspx?prjID={0}&action=add", template.Id));
				emptyScreenControl.HeaderDescribe = MilestoneResource.EmptyScreenMilestoneTemplateDescription;
				MilestonesHolder.Controls.Add(emptyScreenControl);
			}
			else
			{
				MilestonesHolder.Controls.Add(new NotFoundControl());
			}
		}

		protected void ElementNotFoundControlView(int prjID)
		{
			MilestoneInfoPanel.Controls.Add(new ElementNotFoundControl
			{
				Header = MilestoneResource.MilestoneNotFound_Header,
				Body = MilestoneResource.MilestoneNotFound_Body,
				RedirectURL = String.Format("milestones.aspx?prjID={0}", prjID),
				RedirectTitle = MilestoneResource.MilestoneNotFound_RedirectTitle
			});
		}
	}
}
