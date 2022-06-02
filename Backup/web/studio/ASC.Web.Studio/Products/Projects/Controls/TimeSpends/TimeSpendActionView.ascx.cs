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
using System.Globalization;
using System.Linq;
using ASC.Web.Core.Users;
using ASC.Projects.Engine;

#endregion

namespace ASC.Web.Projects.Controls.TimeSpends
{
    [AjaxNamespace("AjaxPro.TimeSpendActionView")]
    public partial class TimeSpendActionView : BaseUserControl
    {

        #region Properies

        ProjectFat ProjectFat { get; set; }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(TimeSpendActionView));

            _timetrackingContainer.Options.IsPopup = true;
        }

        #endregion

        #region Methods

        [AjaxMethod]
        public string Save(int taskID, String datetime, Guid person, String hours, String note, int prjID)
        {
            ProjectSecurity.DemandAuthentication();

            float res;
            if (!float.TryParse(hours, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out res)) res = (float)Convert.ToDouble(hours);

            var item = new TimeSpend
            {
                Hours = res,
                Date = DateTime.Parse(datetime),
                Note = note,
                Project = prjID
            };

            item.RelativeTask = taskID;

            item.Person = person;

            Global.EngineFactory.GetTimeTrackingEngine().SaveOrUpdate(item);

            double count = 0;

            List<TimeSpend> list = Global.EngineFactory.GetTimeTrackingEngine().GetByTask(taskID);

            foreach (TimeSpend ts in list)
            {
                count += ts.Hours;
            }

            return count.ToString("N2");

        }

        [AjaxMethod]
        public string GetTaskTitle(int taskID)
        {
            ProjectSecurity.DemandAuthentication();

            return Global.EngineFactory.GetTaskEngine().GetByID(taskID).Title.HtmlEncode();
        }

        [AjaxMethod]
        public object InitPopUpContainer(int prjID, int taskID)
        {
            ProjectSecurity.DemandAuthentication();

            ProjectFat = new ProjectFat(Global.EngineFactory.GetProjectEngine().GetByID(prjID));
            
            var count = 0.0;
            foreach (var ts in Global.EngineFactory.GetTimeTrackingEngine().GetByTask(taskID))
            {
                count += ts.Hours;
            }

            var task = Global.EngineFactory.GetTaskEngine().GetByID(taskID);

            var sb = new StringBuilder();

            var team = ProjectFat.GetTeam().Select(p => p.UserInfo).OrderBy(u => u, UserInfoComparer.Default).ToList();

            foreach (var u in team)
            {
                sb.AppendFormat("<option value='{0}'>{1}</option>", u.ID, u.DisplayUserName());
            }

            return new
            {
                ImgClockActive = WebImageSupplier.GetAbsoluteWebPath("clock_active.png", ProductEntryPoint.ID),
                CountHours = count.ToString("N2"),
                ResponsibleID = task.Responsible.ToString(),
                DateTimeNow = ASC.Core.Tenants.TenantUtil.DateTimeNow().ToString(DateTimeExtension.DateFormatPattern),
                HTML = sb.ToString()
            };
        }

        

        #endregion

    }
}