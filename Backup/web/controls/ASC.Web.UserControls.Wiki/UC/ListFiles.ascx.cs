using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ASC.Web.UserControls.Wiki.UC
{
    public partial class ListFiles : BaseUserControl
    {
        public string mainPath
        {
            get
            {
                if (ViewState["mainPath"] == null)
                {
                    return string.Empty;
                }

                return ViewState["mainPath"].ToString();
            }
            set
            {
                ViewState["mainPath"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                BindListFiles();
            }
        }

        protected void cmdDeleteFile_Click(object sender, EventArgs e)
        {
            string fileName = (sender as LinkButton).CommandName;
            wikiDAO.FilesDelete(fileName);
            BindListFiles();
        }

        private void BindListFiles()
        {
            rptListFiles.DataSource = wikiDAO.FilesGetAll();
            rptListFiles.DataBind();
        }

        protected string GetFileLink(string FileName)
        {
            return ActionHelper.GetViewFilePath(mainPath, FileName);
        }


    }
}