﻿using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Web.UserControls.Wiki.Data;
using ASC.Web.UserControls.Wiki.Handlers;

namespace ASC.Web.UserControls.Wiki.UC
{
    public partial class ViewFile : BaseUserControl
    {
        private string _fileName = string.Empty;
        public string FileName
        {
            get
            {
                return /*PageNameUtil.Encode*/(_fileName);
            }
            set
            {
                _fileName = /*PageNameUtil.Decode*/(value);
            }
        }


        private Files _fileInfo;
        protected Files CurrentFile
        {
            get
            {
                if (_fileInfo == null)
                {
                    if (string.IsNullOrEmpty(FileName))
                        return null;

                    _fileInfo = wikiDAO.FilesGetByName(FileName);
                }
                return _fileInfo;
            }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if(CurrentFile != null && !string.IsNullOrEmpty(CurrentFile.FileName))
                {
                    RiseWikiPageLoaded(CurrentFile);
                    RisePublishVersionInfo(CurrentFile);
                }
            }
        }


        protected string GetFileRender()
        {
            Files file = wikiDAO.FilesGetByName(_fileName);
            if(file == null)
            {
                
                RisePageEmptyEvent();
                return string.Empty;// "nonefile.png";
            }

            string ext = file.FileLocation.Split('.')[file.FileLocation.Split('.').Length - 1];
            if (!string.IsNullOrEmpty(ext) && !WikiFileHandler.ImageExtentions.Contains(ext.ToLower()))
            {
                return string.Format(@"<a class=""wikiEditButton"" href=""{0}"" title=""{1}"">{2}</a>",
                    ResolveUrl(string.Format(ImageHandlerUrlFormat, FileName)),
                    file.FileName,
                    Resources.WikiUCResource.wikiFileDownloadCaption);
            }

            return string.Format(@"<img src=""{0}"" style=""max-width:300px; max-height:200px"" />",
                ResolveUrl(string.Format(ImageHandlerUrlFormat, FileName)));
            

            
        }
    }
}