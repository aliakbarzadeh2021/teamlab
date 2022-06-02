#region Import

using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Projects.Configuration;
using ASC.Web.Projects.Classes;
using ASC.Projects.Core.Domain;
using System.Collections.Generic;
using System.Text;
using ASC.Core.Users;
using AjaxPro;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;
using ASC.Web.Controls;
using ASC.Core;
using ASC.Projects.Engine;

#endregion

namespace ASC.Web.Projects.Controls.Common
{
    [AjaxNamespace("AjaxPro.Request")]
    public partial class Request : BaseUserControl
    {
        #region Properties

        public int RequestsCount { get; set; }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(Request));
            AjaxPro.Utility.RegisterTypeForAjax(typeof(ProjectSettings));

            var requests = Global.EngineFactory.GetProjectEngine().GetRequests();
            requests.Sort((x, y) => DateTime.Compare(x.CreateOn, y.CreateOn));

            RequestsCount = requests.Count;

            DataRow row;
            DataTable table = MakeTable();
            int number = 0;

            foreach (ProjectChangeRequest request in requests)
            {
                row = table.NewRow();
                row = InitDataRow(row, request, number);
                table.Rows.Add(row);
                number++;
            }

            rptContent.DataSource = table;
            rptContent.DataBind();

            if (requests.Count != 0)
            {
                PopUpRequest cntrlPopUpRequest = (PopUpRequest)LoadControl(PathProvider.GetControlVirtualPath("PopUpRequest.ascx"));
                cntrlPopUpRequest.request = requests[0];
                phContent.Controls.Add(cntrlPopUpRequest);
            }
        }

        #endregion

        #region Methods

        public DataTable MakeTable()
        {
            DataTable Table = new DataTable();

            DataColumn Number = new DataColumn();
            Number.DataType = System.Type.GetType("System.Int32");
            Number.ColumnName = "Number";
            Table.Columns.Add(Number);

            DataColumn RequestID = new DataColumn();
            RequestID.DataType = System.Type.GetType("System.Int32");
            RequestID.ColumnName = "RequestID";
            Table.Columns.Add(RequestID);

            DataColumn ProjectID = new DataColumn();
            ProjectID.DataType = System.Type.GetType("System.Int32");
            ProjectID.ColumnName = "ProjectID";
            Table.Columns.Add(ProjectID);

            DataColumn Title = new DataColumn();
            Title.DataType = System.Type.GetType("System.String");
            Title.ColumnName = "Title";
            Table.Columns.Add(Title);

            DataColumn Description = new DataColumn();
            Description.DataType = System.Type.GetType("System.String");
            Description.ColumnName = "Description";
            Table.Columns.Add(Description);

            DataColumn Action = new DataColumn();
            Action.DataType = System.Type.GetType("System.String");
            Action.ColumnName = "Action";
            Table.Columns.Add(Action);

            DataColumn CreateBy = new DataColumn();
            CreateBy.DataType = typeof(UserInfo);
            CreateBy.ColumnName = "CreateBy";
            Table.Columns.Add(CreateBy);

            DataColumn Responsible = new DataColumn();
            Responsible.DataType = typeof(UserInfo);
            Responsible.ColumnName = "Responsible";
            Table.Columns.Add(Responsible);

            return Table;
        }

        public DataRow InitDataRow(DataRow row, ProjectChangeRequest request, int number)
        {
            row["Number"] = number;
            row["RequestID"] = request.ID;
            row["ProjectID"] = request.ProjectID;
            row["Title"] = request.Title.Trim().HtmlEncode();
            row["Description"] = request.Description.Trim().HtmlEncode();
            row["CreateBy"] = Global.EngineFactory.GetParticipantEngine().GetByID(request.CreateBy).UserInfo;
            row["Responsible"] = Global.EngineFactory.GetParticipantEngine().GetByID(request.Responsible).UserInfo;
            if (request.RequestType == ProjectRequestType.Create) row["Action"] = RequestResource.HasAddedTheNewProject;
            if (request.RequestType == ProjectRequestType.Edit) row["Action"] = RequestResource.HasEditedTheProject;
            if (request.RequestType == ProjectRequestType.Remove) row["Action"] = RequestResource.HasRemovedTheProject;
            return row;
        }

        public string GrammaticalHelperCountRequests()
        {
            return RequestsCount + " " + GrammaticalHelper.ChooseNumeralCase(RequestsCount, GrammaticalResource.NewRequestNominative, GrammaticalResource.NewRequestGenitiveSingular, GrammaticalResource.NewRequestGenitivePlural);
        }

        public String GetPrevImageUrl()
        {
            return WebImageSupplier.GetAbsoluteWebPath("left.gif", ProductEntryPoint.ID);
        }
        public String GetPrevDisableImageUrl()
        {
            return WebImageSupplier.GetAbsoluteWebPath("left_s.gif", ProductEntryPoint.ID);
        }

        public String GetNextImageUrl()
        {
            return WebImageSupplier.GetAbsoluteWebPath("right.gif", ProductEntryPoint.ID);
        }
        public String GetNextDisableImageUrl()
        {
            return WebImageSupplier.GetAbsoluteWebPath("right_s.gif", ProductEntryPoint.ID);
        }

        public String GetFirstImageUrl()
        {
            if (RequestsCount > 1)
                return GetNextImageUrl();
            else return GetNextDisableImageUrl();
        }

        public string GetDescription(string description)
        {
            if (string.IsNullOrEmpty(description)) return string.Empty;
            return description.Length <= 100 ? description : description.Remove(97) + "...";
        }

        #endregion

        #region AjaxMethods

        [AjaxMethod]
        public AjaxPopUpRequest GetPopUpRequest(string requestID)
        {
            ProjectSecurity.DemandCreateProject();

            ProjectChangeRequest request = Global.EngineFactory.GetProjectEngine().GetRequest(Convert.ToInt32(requestID));

            if (request == null) return new AjaxPopUpRequest() { Error = RequestResource.RequestNotFound, Request = string.Empty };

            Page page = new Page();

            PopUpRequest cntrlPopUpRequest = (PopUpRequest)LoadControl(PathProvider.GetControlVirtualPath("PopUpRequest.ascx"));

            cntrlPopUpRequest.request = request;

            page.Controls.Add(cntrlPopUpRequest);

            var writer = new System.IO.StringWriter();

            HttpContext.Current.Server.Execute(page, writer, false);

            string output = writer.ToString();

            writer.Close();

            return new AjaxPopUpRequest() { Error = string.Empty, Request = output };

        }

        [AjaxMethod]
        public int AcceptRequest(string requestID, string title, string description, string leaderID, string tags, bool isHidden, string status)
        {
            ProjectSecurity.DemandCreateProject();

            ProjectChangeRequest request = Global.EngineFactory.GetProjectEngine().GetRequest(Convert.ToInt32(requestID));
            request.Title = title;
            request.Status = status == "open" ? ProjectStatus.Open : ProjectStatus.Closed;
            request.Private = isHidden;
            request.Responsible = new Guid(leaderID);
            request.Description = description;

            Project project = Global.EngineFactory.GetProjectEngine().AcceptRequest(request);
            Global.EngineFactory.GetTagEngine().SetProjectTags(project.ID, tags);

            return project.ID;
        }

        [AjaxMethod]
        public void RejectRequest(string requestID)
        {
            ProjectSecurity.DemandCreateProject();

            ProjectChangeRequest request = Global.EngineFactory.GetProjectEngine().GetRequest(Convert.ToInt32(requestID));
            Global.EngineFactory.GetProjectEngine().RejectRequest(request);
        }

        public class AjaxPopUpRequest
        {
            public string Request { get; set; }
            public string Error { get; set; }
        }

        #endregion
    }
}