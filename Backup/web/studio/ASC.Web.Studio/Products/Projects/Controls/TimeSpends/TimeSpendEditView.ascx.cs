#region Import

using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Core.Users;
using ASC.Projects.Core;
using ASC.Projects.Core.Domain;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Resources;
using AjaxPro;
using ASC.Web.Projects.Configuration;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Utility;
using System.Globalization;
using System.Web;
using ASC.Web.Controls;
using ASC.Projects.Engine;

#endregion

namespace ASC.Web.Projects.Controls.TimeSpends
{
    [AjaxNamespace("AjaxPro.TimeSpendEditView")]
    public partial class TimeSpendEditView : BaseUserControl
    {

        #region Properies

        Project Project { get; set; }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(TimeSpendEditView));

            _timetrackingContainer.Options.IsPopup = true;

            int projectID;

            if (int.TryParse(UrlParameters.ProjectID, out projectID))
            {
                Project = Global.EngineFactory.GetProjectEngine().GetByID(projectID);
                Init_ddlPerson();
            }
        }

        #endregion

        #region Methods

        [AjaxMethod]
        public string Save(string date,string personID,string hours,string note,int id,int prjID)
        {
            ProjectSecurity.DemandAuthentication();

            TimeSpend ts = Global.EngineFactory.GetTimeTrackingEngine().GetByID(id);

            ts.Date = DateTime.Parse(date);
            ts.Person = new Guid(personID);
            
            float  res;
            if (!float.TryParse(hours.Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out res)) res = (float)Convert.ToDouble(hours.Trim());
            ts.Hours = res;

            ts.Note = note;
            ts.Project = prjID;

            Global.EngineFactory.GetTimeTrackingEngine().SaveOrUpdate(ts);

            var list = Global.EngineFactory.GetTimeTrackingEngine().GetByProject(prjID);

            double totalHoursCount = 0;

            foreach (TimeSpend timeSpend in list)
            {
                totalHoursCount += timeSpend.Hours;
            }

            return StudioUserInfoExtension.RenderProfileLink(Global.EngineFactory.GetParticipantEngine().GetByID(ts.Person).UserInfo, ProductEntryPoint.ID) + "||" + ts.Hours.ToString("N2") + "||" + totalHoursCount.ToString("N2");

        }

        [AjaxMethod]
        public string InitPopUpContainer(int id)
        {
            ProjectSecurity.DemandAuthentication();
            
            StringBuilder sb = new StringBuilder();

            TimeSpend ts = Global.EngineFactory.GetTimeTrackingEngine().GetByID(id);

            sb.AppendFormat("{0}||{1}||{2}", ts.Hours.ToString("N2"), ts.Date.ToString(DateTimeExtension.DateFormatPattern), ts.Person.ToString());

            
            if (ts.RelativeTask != 0)
            {
                double count = 0;
                List<TimeSpend> list = Global.EngineFactory.GetTimeTrackingEngine().GetByTask(ts.RelativeTask);

                foreach (TimeSpend item in list)
                {
                    count += item.Hours;
                }

                sb.AppendFormat("||{0}", count.ToString("N2"));
            }

            return sb.ToString();
        }

        public void Init_ddlPerson()
        {

            List<Participant> team = Global.EngineFactory.GetProjectEngine().GetTeam(Project.ID);

            team.Sort((oX, oY) =>
            {
                return String.Compare(oX.UserInfo.DisplayUserName(), oY.UserInfo.DisplayUserName());
            });

            foreach (Participant prt in team)
            {
                ListItem listItem = new ListItem(prt.UserInfo.DisplayUserName(), prt.UserInfo.ID.ToString());
                editLogPanel_ddlPerson.Items.Add(listItem);
            }
        }

        [AjaxMethod]
        public string GetNote(int id)
        {
            ProjectSecurity.DemandAuthentication();
            
            TimeSpend timeSpend = Global.EngineFactory.GetTimeTrackingEngine().GetByID(id);

            if (timeSpend.RelativeTask != 0)
            {
                var task = Global.EngineFactory.GetTaskEngine().GetByID(timeSpend.RelativeTask);
                if (timeSpend.Note != string.Empty)
                    return string.Format("<a href=\"tasks.aspx?prjID={0}&id={1}\">{2}</a><br/>{3}",
                        timeSpend.Project, timeSpend.RelativeTask, HtmlUtility.GetText(task.Title, 45), timeSpend.Note.HtmlEncode());
                else
                    return string.Format("<a href=\"tasks.aspx?prjID={0}&id={1}\">{2}</a>",
                        timeSpend.Project, timeSpend.RelativeTask, HtmlUtility.GetText(task.Title, 45));
            }
            else return timeSpend.Note.HtmlEncode();
        }

        [AjaxMethod]
        public string GetTitle(int id)
        {
            ProjectSecurity.DemandAuthentication();
            
            TimeSpend timeSpend = Global.EngineFactory.GetTimeTrackingEngine().GetByID(id);
            return string.Format("{0} {1}",
                timeSpend.RelativeTask != 0 ? TaskResource.Task + ": " + Global.EngineFactory.GetTaskEngine().GetByID(timeSpend.RelativeTask).Title + "; " : "",
                timeSpend.Note == string.Empty ? "" : ProjectResource.ProjectDescription + ": " + timeSpend.Note.HtmlEncode());
        }

        [AjaxMethod]
        public string GetTimeSpendNote(int id)
        {
            ProjectSecurity.DemandAuthentication();

            TimeSpend timeSpend = Global.EngineFactory.GetTimeTrackingEngine().GetByID(id);

            return timeSpend.Note.HtmlEncode().Replace("'", "&#039;");
        }

        [AjaxMethod]
        public string GetTimeSpendRelativeTaskTitle(int id)
        {
            ProjectSecurity.DemandAuthentication();

            TimeSpend timeSpend = Global.EngineFactory.GetTimeTrackingEngine().GetByID(id);

            if (timeSpend.RelativeTask != 0)
            {
                return Global.EngineFactory.GetTaskEngine().GetByID(timeSpend.RelativeTask).Title.HtmlEncode();
            }

            return string.Empty;
        }

        #endregion
    }
}