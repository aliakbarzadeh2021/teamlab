#region Import

using System;
using System.Collections.Generic;
using System.Text;
using ASC.Core;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Projects.Engine;
using ASC.Web.Controls;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Controls;
using ASC.Web.Projects.Controls.Messages;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;

#endregion

namespace ASC.Web.Projects
{
    public partial class Messages : BasePage
    {


        protected void Page_Init(object sender, EventArgs e)
        {

            InitView();

        }

        protected override void PageLoad()
        {

        }


        #region Methods

        protected void InitView()
        {

            AjaxPro.Utility.RegisterTypeForAjax(typeof(MessageActionView));

            int messageID;

            RequestContext.EnsureCurrentProduct();

            if (!ProjectSecurity.CanReadMessages(RequestContext.GetCurrentProject()))
            {
                Response.Redirect(ProjectsCommonResource.StartURL);
            }

            var projectFat = RequestContext.GetCurrentProjectFat();

            if (Int32.TryParse(UrlParameters.EntityID, out messageID))
            {

                Message message = Global.EngineFactory.GetMessageEngine().GetByID(messageID);
                if (message == null || message.Project.ID != projectFat.Project.ID)
                    ElementNotFoundControlView(projectFat.Project.ID);
                else
                {
                    if (String.Compare(UrlParameters.ActionType, "edit", true) == 0)
                    {
                        if (ProjectSecurity.CanEdit(message))
                        {
                            MessageActionView(projectFat, message);
                        }
                        else
                        {
                            Response.Redirect(ProjectsCommonResource.StartURL);
                        }
                    }
                    else
                        MessageDetailsView(projectFat, message);
                }
            }
            else
            {
                if (String.Compare(UrlParameters.ActionType, "add", true) == 0)
                {
                    if (ProjectSecurity.CanCreateMessage(projectFat.Project))
                    {
                        MessageActionView(projectFat, null);
                    }
                    else
                    {
                        Response.Redirect(ProjectsCommonResource.StartURL);
                    }
                }
                else
                    ListMessageView(projectFat);

            }


            if (SideActionsPanel.Controls.Count == 0)
                SideActionsPanel.Visible = false;


            Title = HeaderStringHelper.GetPageTitle(MessageResource.Messages, Master.BreadCrumbs);
        }

        public void ListMessageView(ProjectFat projectFat)
        {
            InitBaseBreadCrumbs(projectFat);

            var messages = Global.EngineFactory.GetMessageEngine().GetByProject(projectFat.Project.ID);
            if (messages.Count != 0)
            {

                ListMessageView oListMessageView = (ListMessageView)LoadControl(PathProvider.GetControlVirtualPath("ListMessageView.ascx"));
                oListMessageView.Messages = messages;
                _content.Controls.Add(oListMessageView);

                if (ProjectSecurity.CanCreateMessage(projectFat.Project))
                    SideActionsPanel.Controls.Add(new NavigationItem
                    {
                        Name = MessageResource.CreateMessage,
                        URL = String.Format("messages.aspx?prjID={0}&action=add", projectFat.Project.ID)

                    });
            }
            else
            {

                EmptyScreenControl emptyScreenControl = new EmptyScreenControl();

                if (ProjectSecurity.CanCreateMessage(projectFat.Project))
                {
                    emptyScreenControl.HeaderContent = String.Format("<a href='{1}'>{0}</a>", MessageResource.EmptyScreenHeaderContent, String.Format("messages.aspx?prjID={0}&action=add", projectFat.Project.ID));
                }
                emptyScreenControl.HeaderDescribe = MessageResource.EmptyScreenHeaderDescribe;

                _content.Controls.Add(emptyScreenControl);

            }

        }

        public void MessageDetailsView(ProjectFat projectFat, Message message)
        {
            MessageDetailsView oMessageDetailsView = (MessageDetailsView)LoadControl(PathProvider.GetControlVirtualPath("MessageDetailsView.ascx"));
            oMessageDetailsView.ProjectFat = projectFat;
            oMessageDetailsView.Message = message;
            _content.Controls.Add(oMessageDetailsView);

            InitBaseBreadCrumbs(projectFat);

            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = oMessageDetailsView.Message.Title.HtmlEncode(),
                NavigationUrl = ""
            });


            if (ProjectSecurity.CanCreateMessage(message.Project))
                SideActionsPanel.Controls.Add(new NavigationItem
                {
                    Name = MessageResource.CreateMessage,
                    URL = "messages.aspx?prjID=" + projectFat.Project.ID + "&action=add"

                });

            if (ProjectSecurity.CanEdit(message))
            {
                SideActionsPanel.Controls.Add(new NavigationItem
                {
                    Name = MessageResource.EditMessage,
                    URL = String.Format("messages.aspx?prjID={0}&id={1}&action=edit", projectFat.Project.ID, message.ID)

                });
                SideActionsPanel.Controls.Add(new NavigationItem
                {
                    Name = MessageResource.DeleteMessage,
                    URL = String.Format("javascript:ASC.Projects.Messages.deleteMessage({0},0)", message.ID)

                });
            }

            if (SecurityContext.IsAuthenticated)
            {
                var objectID = String.Format("{0}_{1}", message.UniqID, message.Project.ID);
                var objects = new List<String>(NotifySource.Instance.GetSubscriptionProvider().GetSubscriptions(
                  NotifyConstants.Event_NewCommentForMessage,
                  NotifySource.Instance.GetRecipientsProvider().GetRecipient(
                      ASC.Core.SecurityContext.CurrentAccount.ID.ToString())
                  ));

                var subscribed = !String.IsNullOrEmpty(objects.Find(item => String.Compare(item, objectID, true) == 0));
                var subscribeLinkHtml = new StringBuilder();

                subscribeLinkHtml.Append("<div id='message_subscriber'>");
                subscribeLinkHtml.AppendFormat("<a  href='javascript:void(0)' onclick='javascript:ASC.Projects.Messages.changeSubscribe({1})' class=\"linkAction\">{0}</a>",
                      subscribed ? ProjectsCommonResource.UnSubscribeOnNewComment : ProjectsCommonResource.SubscribeOnNewComment,
                      message.ID
                    );
                subscribeLinkHtml.Append("</div>");
                SideActionsPanel.Controls.Add(new HtmlMenuItem(subscribeLinkHtml.ToString()));
            }
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
                NavigationUrl = "projects.aspx?prjID=" + projectFat.Project.ID

            });

            Master.BreadCrumbs.Add(new BreadCrumb
            {
                Caption = MessageResource.Messages,
                NavigationUrl = "messages.aspx?prjID=" + projectFat.Project.ID

            });
        }

        public void MessageActionView(ProjectFat projectFat, Message message)
        {

            MessageActionView oMessageActionView = (MessageActionView)LoadControl(PathProvider.GetControlVirtualPath("MessageActionView.ascx"));

            oMessageActionView.ProjectFat = projectFat;
            oMessageActionView.Message = message;

            _content.Controls.Add(oMessageActionView);

            if (message == null)
            {

                Master.BreadCrumbs.Add(new BreadCrumb
                                           {
                                               Caption = MessageResource.NewMessage

                                           });


                SideActionsPanel.Visible = false;

            }
            else
            {

                SideActionsPanel.Controls.Add(new NavigationItem
                    {
                        Name = MessageResource.DeleteMessage,
                        URL = String.Format("javascript:ASC.Projects.Messages.deleteMessage({0},0)", message.ID)

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

            SideActionsPanel.Visible = false;

        }

        #endregion

    }
}