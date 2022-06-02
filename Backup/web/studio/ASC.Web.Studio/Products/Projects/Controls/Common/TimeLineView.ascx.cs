#region Import

using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Core.Users.Activity;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Core.Users;
using ASC.Web.Projects.Resources;
using  ASC.Core.Users;

#endregion



namespace ASC.Web.Projects.Controls.Common
{

    public class TimelineWrapperHTML
    {

        #region Members
        
        private readonly UserActivity _userContentActivity;
        private readonly EntityType _timeLineType;
        private readonly bool _viewProject;

        #endregion


        public TimelineWrapperHTML(UserActivity userActivity, bool viewProject)
        {
            _viewProject = viewProject;

            var AdditionalDataParts = userActivity.AdditionalData.Split(new[] { '|' });
            _userContentActivity = userActivity;

            _timeLineType = (EntityType)Enum.Parse(typeof(EntityType), AdditionalDataParts[0]);
            
        }

        public String Message
        {
            get
            {
                return String.Format("<a href='{1}' style='padding-right:10px'>{2}</a> {0}", _userContentActivity.ActionText, _userContentActivity.URL, _userContentActivity.Title.HtmlEncode());
            }
        }

        public DateTime Date
        {

            get
            {

                return _userContentActivity.Date;

            }

        }

        public String EntityPlate
        {
            get
            {

                return Global.RenderEntityPlate(_timeLineType,true);
               
            }

        }

        public String Author
        {

            get
            {
                return  
                    ASC.Core.CoreContext.UserManager.GetUsers(_userContentActivity.UserID).RenderProfileLink(
                        ProductEntryPoint.ID);
            }

        }
        
    }

    public partial class TimeLineView : BaseUserControl
    {

        #region Members

        private DateTime _activityDate = DateTime.MinValue;

        #endregion

        #region Property

        public List<UserActivity> Activities { get; set; }

        public bool GroupByDate { get; set; }

        public bool RenderHeader { get; set; }

        public bool ShowActivityDate { get; set; }

        #endregion

        #region Methods

        protected void rptContent_OnItemDataBound(Object Sender, RepeaterItemEventArgs e)
        {
            if (!(e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)) return;
            if (!GroupByDate) return;

            TimelineWrapperHTML timelineWrapperHTML = (TimelineWrapperHTML)e.Item.DataItem;

            if (DateTime.Compare(timelineWrapperHTML.Date.Date, _activityDate.Date) == 0) return;

            _activityDate = timelineWrapperHTML.Date;

            String displayDate = _activityDate.ToString("dddd, dd MMMM yyyy").ToUpper();

            if (_activityDate.Date == ASC.Core.Tenants.TenantUtil.DateTimeNow().Date)
                displayDate = ProjectsCommonResource.Today;

            if (_activityDate.Date == ASC.Core.Tenants.TenantUtil.DateTimeNow().AddDays(-1).Date)
                displayDate = ProjectsCommonResource.Yesterday;
            
            Literal ltlDate = (Literal)e.Item.FindControl("_ltlDate");

            ltlDate.Text = String.Format(@"<tr>
                                               <td  style='padding:15px 0px 15px; margin:0px;' colspan='4'>
                                                    <span class='headerBase'>{0}</span>
                                               </td>
                                          </tr>", displayDate);

        }

        public string DateToShortString(DateTime date)
        {
            return date.ToShortString();
        }

        public string GetDate(DateTime date)
        {
            return date.ToString(DateTimeExtension.DateFormatPattern);
        }

        public string GetTime(DateTime date)
        {
            if (ShowActivityDate)
                return date.ToString(DateTimeExtension.DateFormatPattern) +" "+ date.ToString("HH:mm");
            return date.ToString("HH:mm");
        }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {

            List<TimelineWrapperHTML> content = new List<TimelineWrapperHTML>();

            Activities.ForEach(activity => content.Add(new TimelineWrapperHTML(activity, false)));

            rptContainer.DataSource = content;
            rptContainer.DataBind();

        }

        #endregion

    }
}