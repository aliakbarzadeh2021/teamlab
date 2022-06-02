﻿using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Web.UserControls.Wiki;


namespace ASC.Web.UserControls.Wiki.UC
{
    public partial class ListPages : BaseUserControl
    {

        public string mainPath
        {
            get
            {
                if(ViewState["mainPath"] == null)
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
            if(!Page.IsPostBack)
            {
                BindListPages();
            }
            
        }

        private void BindListPages()
        {
            rptListPages.DataSource = wikiDAO.PagesGetAll();
            rptListPages.DataBind();
        }

        protected string GetPageLink(string PageName)
        {
            return ActionHelper.GetViewPagePath(mainPath, PageName);
        }
    }
}
