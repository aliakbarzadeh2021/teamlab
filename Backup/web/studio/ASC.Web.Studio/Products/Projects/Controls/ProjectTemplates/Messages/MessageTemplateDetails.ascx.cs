using System;
using System.Web.UI;
using AjaxPro;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Web.Controls;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;

namespace ASC.Web.Projects.Controls.ProjectTemplates.Messages
{
    [AjaxNamespace("AjaxPro.MessageDetailsView")]
    public partial class MessageTemplateDetails : BaseUserControl
    {
        public TemplateMessage Message { get; set; }
        public TemplateProject Template { get; set; }


        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(MessageViewTemplate));
            AjaxPro.Utility.RegisterTypeForAjax(typeof(MessageTemplateDetails));
            Global.EngineFactory.GetParticipantEngine().Read(Page.Participant.ID, Message.Id.ToString(), TenantUtil.DateTimeNow());
        }


        public string GetAvatarURL()
        {
            return Global.EngineFactory.GetParticipantEngine().GetByID(Message.CreateBy).UserInfo.GetBigPhotoURL();
        }
        public string GetMessageDate()
        {
            return Message.CreateOn.ToShortString();
        }
        public string GetMessageText()
        {
            return HtmlUtility.GetFull(Message.Text, ProductEntryPoint.ID);
        }
    }
}