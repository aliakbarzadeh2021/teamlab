using System;
using System.Collections.Generic;
using System.Text;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Web.Controls;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Controls;
using ASC.Web.Projects.Controls.Messages;
using ASC.Web.Projects.Controls.ProjectTemplates;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;
using ASC.Web.Projects.Controls.ProjectTemplates.Messages;

namespace ASC.Web.Projects.TProject
{
	public partial class Messages : BasePage
	{

		#region Init template
		protected TemplateProject Template { get; set; }

		private void InitProjectTemplate()
		{
			Template = ProjectTemplatesUtil.GetProjectTemplate();
		}
		#endregion

		protected void Page_Init(object sender, EventArgs e)
		{
			InitProjectTemplate();
			ProjectTemplatesUtil.InitProjectTemplatesBreadcrumbs(Master, Template, Resources.MessageResource.Messages, "Messages");
			Title = HeaderStringHelper.GetPageTitle(Resources.MessageResource.Messages, Master.BreadCrumbs);

			AjaxPro.Utility.RegisterTypeForAjax(typeof(MessageViewTemplate));

			int messageID;

			if (Int32.TryParse(UrlParameters.EntityID, out messageID))
			{
				TemplateMessage message = Global.EngineFactory.GetTemplateEngine().GetTemplateMessage(messageID);
				if (message == null || message.ProjectId != Template.Id)
					ElementNotFoundControlView(Template.Id);
				else
				{
					if (String.Compare(UrlParameters.ActionType, "edit", true) == 0)
						MessageActionView(message, Template);
					else
						MessageDetailsView(message);
				}
			}
			else
			{
				if (String.Compare(UrlParameters.ActionType, "add", true) == 0)
					MessageActionView(null, Template);
				else
					ListMessageView();
			}

			ProjectTemplatesUtil.AddCreateProjectFromTemplateActions(SideActionsPanel, Template);
		}

		protected override void PageLoad()
		{

		}

		#region Methods


		public void ListMessageView()
		{
			var messages = Global.EngineFactory.GetTemplateEngine().GetTemplateMessages(Template.Id);
			if (messages.Count != 0)
			{
				ListMessageTemplateView oListMessageView = (ListMessageTemplateView)LoadControl(PathProvider.GetControlVirtualPath("ListMessageTemplateView.ascx"));
				oListMessageView.Messages = messages;
				_content.Controls.Add(oListMessageView);
			}
			else
			{
				if (ProjectTemplatesUtil.CheckEditPermission(Template))
				{
					EmptyScreenControl emptyScreenControl = new EmptyScreenControl();
					emptyScreenControl.HeaderContent = String.Format("<a href='{1}'>{0}</a>", MessageResource.EmptyScreenMessageTemplate, String.Format("messages.aspx?prjID={0}&action=add", Template.Id));
					emptyScreenControl.HeaderDescribe = MessageResource.EmptyScreenMessageTemplateDescription;
					_content.Controls.Add(emptyScreenControl);
				}
				else
				{
					_content.Controls.Add(new NotFoundControl());
				}
			}

			SideActionsPanel.Controls.Add(new NavigationItem
			{
				Name = MessageResource.CreateMessage,
				URL = String.Format("messages.aspx?prjID={0}&action=add", Template.Id)
			});
		}

		public void MessageDetailsView(TemplateMessage message)
		{
			MessageTemplateDetails oMessageDetailsView = (MessageTemplateDetails)LoadControl(PathProvider.GetControlVirtualPath("MessageTemplateDetails.ascx"));
			oMessageDetailsView.Message = message;
			oMessageDetailsView.Template = Template;
			_content.Controls.Add(oMessageDetailsView);

			Master.BreadCrumbs.Add(new BreadCrumb
			{
				Caption = oMessageDetailsView.Message.Title.HtmlEncode(),
				NavigationUrl = ""
			});

			SideActionsPanel.Controls.Add(new NavigationItem
			{
				Name = MessageResource.CreateMessage,
				URL = "messages.aspx?prjID=" + Template.Id + "&action=add"
			});

			SideActionsPanel.Controls.Add(new NavigationItem
			{
				Name = MessageResource.EditMessage,
				URL = String.Format("messages.aspx?prjID={0}&id={1}&action=edit", Template.Id, message.Id)
			});

			SideActionsPanel.Controls.Add(new NavigationItem
			{
				Name = MessageResource.DeleteMessage,
				URL = String.Format("javascript:ASC.Projects.Messages.deleteMessage({0},0)", message.Id)
			});

			Master.CommonContainerHeader = Global.RenderCommonContainerHeader(oMessageDetailsView.Message.Title.HtmlEncode(), EntityType.Message);
		}


		protected void InitBaseBreadCrumbs(ProjectFat projectFat)
		{
			Master.BreadCrumbs.Add(new BreadCrumb
			  {
				  Caption = ProjectResource.Projects,
				  NavigationUrl = "projects.aspx"
			  });

			Master.BreadCrumbs.Add(new BreadCrumb
			{
				Caption = projectFat.Project.HtmlTitle.HtmlEncode(),
				NavigationUrl = "projects.aspx?prjID=" + Template.Id
			});

			Master.BreadCrumbs.Add(new BreadCrumb
			{
				Caption = MessageResource.Messages,
				NavigationUrl = "messages.aspx?prjID=" + Template.Id
			});
		}

		public void MessageActionView(TemplateMessage message, TemplateProject template)
		{
			MessageViewTemplate oMessageActionView = (MessageViewTemplate)LoadControl(PathProvider.GetControlVirtualPath("MessageViewTemplate.ascx"));

			oMessageActionView.Message = message;
			oMessageActionView.Template = template;

			_content.Controls.Add(oMessageActionView);

			if (message == null)
			{
				Master.BreadCrumbs.Add(new BreadCrumb() {  Caption = MessageResource.NewMessage  });
				SideActionsPanel.Visible = false;
			}
			else
			{
				SideActionsPanel.Controls.Add(new NavigationItem
					{
						Name = MessageResource.DeleteMessage,
						URL = String.Format("javascript:ASC.Projects.Messages.deleteMessage({0},0)", message.Id)
					});

				Master.BreadCrumbs.Add(new BreadCrumb
				{
					Caption = String.Format("{0} «{1}»", MessageResource.EditMessage, message.Title.HtmlEncode())
				});
			}
		}

		protected void ElementNotFoundControlView(int prjID)
		{
			_content.Controls.Add(new ElementNotFoundControl
			{
				Header = MessageResource.MessageNotFound_Header,
				Body = MessageResource.MessageNotFound_Body,
				RedirectURL = String.Format("messages.aspx?prjID={0}", prjID),
				RedirectTitle = MessageResource.MessageNotFound_RedirectTitle
			});
		}

		#endregion
	}
}
