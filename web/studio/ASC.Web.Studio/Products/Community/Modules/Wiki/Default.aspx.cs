using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using AjaxPro;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Notify.Patterns;
using ASC.Web.Community.Wiki.Common;
using ASC.Web.Community.Wiki.Resources;
using ASC.Web.Controls;
using ASC.Web.Controls.CommentInfoHelper;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Wiki;
using ASC.Web.UserControls.Wiki.Data;
using ASC.Web.UserControls.Wiki.Handlers;
using ASC.Web.UserControls.Wiki.UC;

namespace ASC.Web.Community.Wiki
{


    [AjaxNamespace("_Default")]
    public partial class _Default : WikiBasePage, IContextInitializer
    {


        public delegate CommentInfo GetCommentInfoHandler(WikiComments comment);


        bool isEmptyPage = false;


        protected int Version
        {
            get
            {
                int result;
                if (Request["ver"] == null || !int.TryParse(Request["ver"], out result))
                    return 0;

                return result;
            }
        }
        protected bool m_IsCategory
        {
            get
            {
                return Action == ActionOnPage.CategoryView || Action == ActionOnPage.CategoryEdit;
            }

        }

        private string _categoryName = null;
        protected string m_categoryName
        {
            get
            {
                if (_categoryName == null)
                {
                    _categoryName = string.Empty;
                    if (m_IsCategory)
                    {
                        _categoryName = PageNameUtil.Decode(WikiPage).Split(':')[1].Trim();

                    }
                }

                return _categoryName;
            }
        }

        protected string PrintPageName
        {
            get
            {
                string pageName = PageNameUtil.Decode(WikiPage);
                if (string.IsNullOrEmpty(pageName))
                {
                    pageName = WikiResource.MainWikiCaption;
                }
                return pageName;
            }
        }


        protected string PrintPageNameEncoded
        {
            get { return HttpUtility.HtmlEncode(PrintPageName); }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            wikiViewPage.TenantId = wikiViewFile.TenantId = wikiEditFile.TenantId = wikiEditPage.TenantId = TenantId;
        }

        private void CheckSpetialSymbols()
        {
            string spetialName = PrintPageName;
            if (!spetialName.Contains(":"))
                return;

            string spetial = spetialName.Split(':')[0];
            spetialName = spetialName.Split(':')[1];

            /*if (spetial.Equals(ASC.Web.UserControls.Wiki.Resources.WikiResource.wikiCategoryKeyCaption, StringComparison.InvariantCultureIgnoreCase))
            {
                Response.RedirectLC(string.Format("ListPages.aspx?cat={0}", spetialName.Trim()), this);
            }
            else*/
            if (spetial.Equals(ASC.Web.UserControls.Wiki.Constants.WikiInternalKeyCaption, StringComparison.InvariantCultureIgnoreCase))
            {
                spetialName = spetialName.Trim();
                string anchors = spetialName;
                if (spetialName.Contains("#"))
                {
                    spetialName = spetialName.Split('#')[0];
                    anchors = anchors.Remove(0, spetialName.Length).TrimStart('#');

                }
                else
                {
                    anchors = string.Empty;
                }

                if (spetialName.Equals(ASC.Web.UserControls.Wiki.Constants.WikiInternalIndexKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    Response.RedirectLC("ListPages.aspx", this);
                }
                else if (spetialName.Equals(ASC.Web.UserControls.Wiki.Constants.WikiInternalCategoriesKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    Response.RedirectLC("ListCategories.aspx", this);
                }
                else if (spetialName.Equals(ASC.Web.UserControls.Wiki.Constants.WikiInternalFilesKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    Response.RedirectLC("ListFiles.aspx", this);
                }



                else if (spetialName.Equals(ASC.Web.UserControls.Wiki.Constants.WikiInternalHomeKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (string.IsNullOrEmpty(anchors))
                    {
                        Response.RedirectLC("Default.aspx", this);
                    }
                    else
                    {
                        Response.RedirectLC(string.Format(@"Default.aspx?page=#{0}", anchors), this);
                    }

                }
                else if (spetialName.Equals(ASC.Web.UserControls.Wiki.Constants.WikiInternalNewPagesKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    Response.RedirectLC("ListPages.aspx?n=", this);
                }
                else if (spetialName.Equals(ASC.Web.UserControls.Wiki.Constants.WikiInternalRecentlyKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    Response.RedirectLC("ListPages.aspx?f=", this);
                }
                else if (spetialName.Equals(ASC.Web.UserControls.Wiki.Constants.WikiInternalHelpKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (string.IsNullOrEmpty(anchors))
                    {
                        Response.RedirectLC(string.Format(@"Default.aspx?page={0}", ASC.Web.UserControls.Wiki.Resources.WikiUCResource.HelpPageCaption), this);
                    }
                    else
                    {
                        Response.RedirectLC(string.Format(@"Default.aspx?page={0}#{1}", ASC.Web.UserControls.Wiki.Resources.WikiUCResource.HelpPageCaption, anchors), this);
                    }

                }


                else
                {
                    return;
                }

            }

        }

        protected void wikiEditPage_SetNewFCKMode(bool isWysiwygDefault)
        {
            WikiModuleSettings.SetIsWysiwygDefault(isWysiwygDefault, SecurityContext.CurrentAccount.ID);
        }


        protected string wikiEditPage_GetUserFriendlySizeFormat(long size)
        {
            return GetFileLengthToString(size);
        }

        protected void cmdDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(WikiPage) || IsFile)
                return;
            string pageName = PageNameUtil.Decode(WikiPage);
            foreach (var cat in CategoriesProvider.CategoriesSelectCategoriesWillBeDeletedAtAllByPageNam(pageName, TenantId))
            {
                WikiNotifySource.Instance.GetSubscriptionProvider().UnSubscribe(ASC.Web.Community.Wiki.Common.Constants.AddPageToCat, cat.CategoryName);
            }

            CategoriesProvider.CategoryDeleteAllByPageName(pageName, TenantId);

            WikiNotifySource.Instance.GetSubscriptionProvider().UnSubscribe(ASC.Web.Community.Wiki.Common.Constants.EditPage, pageName);

            foreach (var comment in CommentsProvider.GetAllComments(pageName, TenantId))
            {
                CommonControlsConfigurer.FCKUploadsRemoveForItem("wiki_comments", comment.Id.ToString());
            }
            PagesProvider.PagesDelete(pageName, TenantId);

            Response.RedirectLC("Default.aspx", this);
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            (Master as WikiMaster).GetNavigateActionsVisible += new WikiMaster.GetNavigateActionsVisibleHandle(_Default_GetNavigateActionsVisible);
            (Master as WikiMaster).GetDelUniqId += new WikiMaster.GetDelUniqIdHandle(_Default_GetDelUniqId);



            Utility.RegisterTypeForAjax(typeof(_Default), Page);
            LoadViews();

            if (!IsPostBack)
            {

                if (IsFile)
                {
                    Response.RedirectLC(string.Format(WikiSection.Section.ImageHangler.UrlFormat, WikiPage, TenantId), this);
                }

                pCredits.Visible = Action.Equals(ActionOnPage.View) || Action.Equals(ActionOnPage.CategoryView);


                commentList.FCKBasePath = CommonControlsConfigurer.FCKEditorBasePath;

                CheckSpetialSymbols();

                wikiEditPage.mainPath = this.ResolveUrlLC("Default.aspx");
                InitEditsLink();

                string mainStudioCss = WebSkin.GetUserSkin().BaseCSSFileAbsoluteWebPath; ;


                wikiEditPage.EditorAreaCSS = mainStudioCss;
                wikiEditPage.CanUploadFiles = SecurityContext.CheckPermissions(ASC.Web.Community.Wiki.Common.Constants.Action_UploadFile) && !ASC.Web.Core.Mobile.MobileDetector.IsRequestMatchesMobile(Context);
                wikiEditPage.MainCssFile = mainStudioCss;
                wikiEditPage.AjaxProgressLoaderGif = ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif");
                wikiEditPage.PleaseWaitMessage = WikiResource.PleaseWaitMessage;


                if (Action == ActionOnPage.CategoryView)
                {
                    BindPagesByCategory();
                }
            }
        }

        //protected void cmdInternal_Click(object sende, EventArgs e)
        //{
        //    List<Pages> pages = PagesProvider.PagesGetAll(TenantId, true);
        //    foreach (Pages p in pages)
        //    {
        //        PagesProvider.UpdateCategoriesByPageContent(p, TenantId);
        //    }

        //}

        //UpdateEditDeleteVisible(_wikiObjOwner);

        private IWikiObjectOwner _wikiObjOwner = null;
        protected void wikiViewPage_WikiPageLoaded(bool isNew, IWikiObjectOwner owner)
        {
            _wikiObjOwner = owner;
            wikiViewPage.CanEditPage = SecurityContext.CheckPermissions(new WikiObjectsSecurityObject(_wikiObjOwner), ASC.Web.Community.Wiki.Common.Constants.Action_EditPage);
            UpdateEditDeleteVisible(owner);
            WikiMaster.UpdateNavigationItems();
        }

        protected void wikiEditPage_WikiPageLoaded(bool isNew, IWikiObjectOwner owner)
        {
            if (!isNew)
            {
                _wikiObjOwner = owner;
            }

            if ((isNew && !SecurityContext.CheckPermissions(ASC.Web.Community.Wiki.Common.Constants.Action_AddPage))
                ||
                (!isNew && !(SecurityContext.CheckPermissions(new WikiObjectsSecurityObject(owner), ASC.Web.Community.Wiki.Common.Constants.Action_EditPage))))
            {
                Response.RedirectLC("Default.aspx", this);
            }
        }

        protected void wikiEditPage_SaveNewCategoriesAdded(object sender, List<string> categories, string pageName)
        {
            string authorId = SecurityContext.CurrentAccount.ID.ToString();
            foreach (string catName in categories)
            {
                WikiNotifyClient.SendNoticeAsync(
                                           authorId,
                                           ASC.Web.Community.Wiki.Common.Constants.AddPageToCat,
                                           catName,
                                           null,
                                           GetListOfTagValForCategoryNotify(catName, pageName));
            }
        }

        string _Default_GetDelUniqId()
        {
            return cmdDelete.UniqueID;
        }

        WikiNavigationActionVisible _Default_GetNavigateActionsVisible()
        {
            WikiNavigationActionVisible result = WikiNavigationActionVisible.None;
            if ((Action.Equals(ActionOnPage.View) || Action.Equals(ActionOnPage.CategoryView)) && !IsFile)
            {
                result = WikiNavigationActionVisible.AddNewPage |
                         WikiNavigationActionVisible.ShowVersions | WikiNavigationActionVisible.PrintPage |
                         WikiNavigationActionVisible.SubscriptionOnNewPage | WikiNavigationActionVisible.SubscriptionThePage;
                if (_wikiObjOwner != null && SecurityContext.CheckPermissions(new WikiObjectsSecurityObject(_wikiObjOwner), ASC.Web.Community.Wiki.Common.Constants.Action_EditPage))
                {
                    result |= WikiNavigationActionVisible.EditThePage;
                }
                if (!string.IsNullOrEmpty(WikiPage) && _wikiObjOwner != null && SecurityContext.CheckPermissions(new WikiObjectsSecurityObject(_wikiObjOwner), ASC.Web.Community.Wiki.Common.Constants.Action_RemovePage))
                {
                    result |= WikiNavigationActionVisible.DeleteThePage;
                }

                if (pPageIsNotExists.Visible)
                {
                    result = WikiNavigationActionVisible.CreateThePage;
                }
            }
            else if (Action.Equals(ActionOnPage.AddNew))
            {
                result = WikiNavigationActionVisible.AddNewPage;
            }
            else if ((Action.Equals(ActionOnPage.Edit) || Action.Equals(ActionOnPage.CategoryEdit)) && !IsFile)
            {
                result = WikiNavigationActionVisible.AddNewPage;
            }

            if (Action.Equals(ActionOnPage.CategoryView) || Action.Equals(ActionOnPage.CategoryEdit))
            {
                result |= WikiNavigationActionVisible.SubscriptionOnCategory;
            }

            return result;
        }

        internal string GetCategoryName()
        {
            return m_categoryName;
        }

        protected void OnPageEmpty(object sender, EventArgs e)
        {
            string pageName = PageNameUtil.Decode(WikiPage);

            wikiViewPage.Visible = false;
            wikiEditPage.Visible = false;
            wikiViewFile.Visible = false;
            wikiEditFile.Visible = false;
            pPageIsNotExists.Visible = true;

            if (!(Action.Equals(ActionOnPage.CategoryView) || Action.Equals(ActionOnPage.CategoryEdit)))
            {
                if (IsFile)
                {
                    txtPageEmptyLabel.Text = PrepereEmptyString(WikiResource.MainWikiFileIsNotExists, true, false);
                }
                else
                {
                    if (PagesProvider.PagesSearchByName(pageName, TenantId).Count > 0)
                    {
                        txtPageEmptyLabel.Text = PrepereEmptyString(WikiResource.MainWikiPageIsNotExists, false, true);
                    }
                    else
                    {
                        txtPageEmptyLabel.Text = PrepereEmptyString(WikiResource.MainWikiPageIsNotExists, false, false);
                    }


                }

                PageHeaderText = pageName;



            }

            isEmptyPage = true;
            InitEditsLink();
            WikiMaster.UpdateNavigationItems();
        }

        private string PrepereEmptyString(string format, bool isFile, bool isSearchResultExists)
        {
            commentList.Visible = false;
            RegexOptions mainOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
            Regex rxLinkCreatePlace = new Regex(@"{0([\s\S]+?)}", mainOptions);
            Regex rxLinkSearchResult = new Regex(@"{1([\s\S]+?)}", mainOptions);
            Regex rxSearchResultParth = new Regex(@"\[\[([\s\S]+?)\]\]", mainOptions);
            string result = format;

            foreach (Match match in rxLinkCreatePlace.Matches(format))
            {
                if (isFile)
                {
                    if (SecurityContext.CheckPermissions(ASC.Web.Community.Wiki.Common.Constants.Action_UploadFile) && !ASC.Web.Core.Mobile.MobileDetector.IsRequestMatchesMobile(Context))
                    {
                        result = result.Replace(match.Value, string.Format(@"<a href=""{0}"">{1}</a>", ActionHelper.GetEditFilePath(this.ResolveUrlLC("Default.aspx"), PageNameUtil.Decode(WikiPage)), match.Groups[1].Value));
                    }
                    else
                    {

                    }
                }
                else
                {
                    if (SecurityContext.CheckPermissions(ASC.Web.Community.Wiki.Common.Constants.Action_AddPage))
                    {
                        result = result.Replace(match.Value, string.Format(@"<a href=""{0}"">{1}</a>", ActionHelper.GetEditPagePath(this.ResolveUrlLC("Default.aspx"), PageNameUtil.Decode(WikiPage)), match.Groups[1].Value));
                    }
                    else
                    {
                        result = result.Replace(match.Value, match.Groups[1].Value);
                    }
                }

            }

            if (isSearchResultExists && !isFile)
            {
                result = rxSearchResultParth.Replace(result, SearchResultParthMatchEvaluator);

                foreach (Match match in rxLinkSearchResult.Matches(format))
                {
                    result = result.Replace(match.Value, string.Format(@"<a href=""{0}"">{1}</a>", this.ResolveUrlLC(string.Format("Search.aspx?Search={0}&pn=", HttpUtility.UrlEncode(PageNameUtil.Decode(WikiPage)))), match.Groups[1].Value));
                }
            }
            else
            {
                result = rxSearchResultParth.Replace(result, string.Empty);
            }

            return result;
        }

        private string SearchResultParthMatchEvaluator(Match match)
        {
            return match.Groups[1].Value;
        }

        private string GetAbsolutePath(string relative)
        {
            return string.Format(@"{0}://{1}{2}{3}",
                                    Request.Url.Scheme,
                                    Request.Url.Host,
                                    (Request.Url.Port != 80 ? string.Format(":{0}", Request.Url.Port) : string.Empty),
                                    this.ResolveUrlLC(relative));
        }

        private void LoadViews()
        {


            wikiEditPage.AlaxUploaderPath = GetAbsolutePath("~/js/ajaxupload.3.5.js");
            wikiEditPage.JQPath = GetAbsolutePath("~/js/auto/jquery_full.js");

            wikiEditPage.CurrentUserId = SecurityContext.CurrentAccount.ID;
            wikiViewPage.Visible = false;
            wikiEditPage.Visible = false;
            wikiViewFile.Visible = false;
            wikiEditFile.Visible = false;
            pPageIsNotExists.Visible = false;
            pView.Visible = false;
            PrintHeader.Visible = false;
            phCategoryResult.Visible = Action == ActionOnPage.CategoryView;


            string pageName = PrintPageName;



            switch (Action)
            {
                case ActionOnPage.AddNew:
                    pageName = WikiResource.MainWikiAddNewPage;
                    wikiEditPage.IsWysiwygDefault = WikiModuleSettings.GetIsWysiwygDefault(SecurityContext.CurrentAccount.ID);
                    wikiEditPage.Visible = true;
                    wikiEditPage.IsNew = true;
                    PageHeaderText = pageName;
                    break;
                case ActionOnPage.AddNewFile:
                    pageName = WikiResource.MainWikiAddNewFile;
                    wikiEditFile.Visible = true;
                    PageHeaderText = pageName;
                    break;
                case ActionOnPage.Edit:
                case ActionOnPage.CategoryEdit:
                    if (IsFile)
                    {
                        wikiEditFile.FileName = WikiPage;
                        wikiEditFile.Visible = true;
                        BreadCrumb.Clear();
                        BreadCrumb.Add(new ASC.Web.Controls.BreadCrumb() { Caption = pageName, NavigationUrl = ActionHelper.GetViewFilePath(this.ResolveUrlLC("Default.aspx"), PageNameUtil.Decode(WikiPage)) });
                        BreadCrumb.Add(new ASC.Web.Controls.BreadCrumb() { Caption = WikiResource.MainWikiEditFile });
                    }
                    else
                    {
                        wikiEditPage.PageName = WikiPage;
                        wikiEditPage.IsWysiwygDefault = WikiModuleSettings.GetIsWysiwygDefault(SecurityContext.CurrentAccount.ID);
                        wikiEditPage.Visible = true;
                        if (m_IsCategory)
                            wikiEditPage.IsSpecialName = true;

                        BreadCrumb.Clear();
                        if (m_IsCategory)
                        {
                            BreadCrumb.Add(new BreadCrumb() { Caption = WikiResource.menu_ListCategories, NavigationUrl = this.ResolveUrlLC("ListCategories.aspx") });
                            BreadCrumb.Add(new BreadCrumb() { Caption = string.Format(WikiResource.menu_ListPagesCatFormat, m_categoryName), NavigationUrl = ActionHelper.GetViewPagePath(this.ResolveUrlLC("Default.aspx"), m_categoryName, ASC.Web.UserControls.Wiki.Constants.WikiCategoryKeyCaption) });
                        }
                        else
                        {
                            BreadCrumb.Add(new ASC.Web.Controls.BreadCrumb() { Caption = pageName, NavigationUrl = ActionHelper.GetViewPagePath(this.ResolveUrlLC("Default.aspx"), PageNameUtil.Decode(WikiPage)) });
                        }
                        BreadCrumb.Add(new ASC.Web.Controls.BreadCrumb() { Caption = WikiResource.MainWikiEditPage });
                        break;
                    }
                    break;
                case ActionOnPage.View:
                case ActionOnPage.CategoryView:
                    pView.Visible = true;
                    if (IsFile)
                    {
                        wikiViewFile.FileName = WikiPage;
                        PageHeaderText = pageName;
                        wikiViewFile.Visible = true;
                    }
                    else
                    {
                        PrintHeader.Visible = true;
                        wikiViewPage.PageName = WikiPage;
                        wikiViewPage.Version = Version;
                        if (Version == 0)
                        {
                            if (m_IsCategory)
                            {
                                BreadCrumb.Add(new BreadCrumb() { Caption = WikiResource.menu_ListCategories, NavigationUrl = this.ResolveUrlLC("ListCategories.aspx") });
                                BreadCrumb.Add(new BreadCrumb() { Caption = string.Format(WikiResource.menu_ListPagesCatFormat, m_categoryName) });
                            }
                            else
                            {
                                PageHeaderText = pageName;
                            }
                        }
                        else
                        {
                            BreadCrumb.Clear();
                            if (m_IsCategory)
                            {
                                BreadCrumb.Add(new BreadCrumb() { Caption = WikiResource.menu_ListCategories, NavigationUrl = this.ResolveUrlLC("ListCategories.aspx") });
                                BreadCrumb.Add(new BreadCrumb() { Caption = string.Format(WikiResource.menu_ListPagesCatFormat, m_categoryName), NavigationUrl = ActionHelper.GetViewPagePath(this.ResolveUrlLC("Default.aspx"), m_categoryName, ASC.Web.UserControls.Wiki.Constants.WikiCategoryKeyCaption) });
                            }
                            else
                            {
                                BreadCrumb.Add(new ASC.Web.Controls.BreadCrumb() { Caption = pageName, NavigationUrl = ActionHelper.GetViewPagePath(this.ResolveUrlLC("Default.aspx"), PageNameUtil.Decode(WikiPage)) });
                            }

                            BreadCrumb.Add(new ASC.Web.Controls.BreadCrumb() { Caption = WikiResource.wikiHistoryCaption, NavigationUrl = ActionHelper.GetViewPagePath(this.ResolveUrlLC("PageHistoryList.aspx"), PageNameUtil.Decode(WikiPage)) });
                            BreadCrumb.Add(new ASC.Web.Controls.BreadCrumb() { Caption = string.Format("{0}{1}", WikiResource.wikiVersionCaption, Version) });
                        }
                        wikiViewPage.Visible = true;
                    }
                    InitCommentsView();
                    break;

            }


        }

        protected void BindPagesByCategory()
        {
            if (Action != ActionOnPage.CategoryView || string.IsNullOrEmpty(m_categoryName))
                return;

            List<Pages> result = PagesProvider.PagesGetAllByCategoryName(m_categoryName, TenantId);

            result.RemoveAll(pemp => string.IsNullOrEmpty(pemp.PageName));

            string firstLetter;
            List<string> letters = new List<string>(WikiResource.wikiCategoryAlfaList.Split(','));


            string otherSymbol = string.Empty;
            if (letters.Count > 0)
            {
                otherSymbol = letters[0];
                letters.Remove(otherSymbol);
            }

            List<PageDictionary> dictList = new List<PageDictionary>();
            PageDictionary pageDic;
            foreach (Pages page in result)
            {

                firstLetter = new string(page.PageName[0], 1);

                if (!letters.Exists(lt => lt.Equals(firstLetter, StringComparison.InvariantCultureIgnoreCase)))
                {
                    firstLetter = otherSymbol;
                }

                if (!dictList.Exists(dl => dl.HeadName.Equals(firstLetter, StringComparison.InvariantCultureIgnoreCase)))
                {
                    pageDic = new PageDictionary();
                    pageDic.HeadName = firstLetter;
                    pageDic.Pages.Add(page);
                    dictList.Add(pageDic);
                }
                else
                {
                    pageDic = dictList.Find(dl => dl.HeadName.Equals(firstLetter, StringComparison.InvariantCultureIgnoreCase));
                    pageDic.Pages.Add(page);
                }
            }

            dictList.Sort(SortPageDict);


            int countAll = dictList.Count * 3 + result.Count; //1 letter is like 2 links to category
            int perColumn = (int)(Math.Round((decimal)countAll / 3));

            List<List<PageDictionary>> mainDictList = new List<List<PageDictionary>>();

            int index = 0, lastIndex = 0, count = 0;

            PageDictionary p;
            for (int i = 0; i < dictList.Count; i++)
            {
                p = dictList[i];

                count += 3;
                count += p.Pages.Count;
                index++;
                if (count >= perColumn || i == dictList.Count - 1)
                {
                    count = count - perColumn;
                    mainDictList.Add(dictList.GetRange(lastIndex, index - lastIndex));
                    lastIndex = index;
                }

            }

            rptCategoryPageList.DataSource = mainDictList;
            rptCategoryPageList.DataBind();
        }


        private int SortPageDict(PageDictionary cd1, PageDictionary cd2)
        {
            return cd1.HeadName.CompareTo(cd2.HeadName);
        }

        protected void On_PublishVersionInfo(object sender, VersionEventArgs e)
        {
            if (!e.UserID.Equals(Guid.Empty))
            {
                litAuthorInfo.Text = GetPageInfo(PageNameUtil.Decode(WikiPage), e.UserID, e.Date);
            }
            else
            {
                litAuthorInfo.Text = string.Empty;
            }

            hlVersionPage.Text = string.Format(WikiResource.cmdVersionTemplate, e.Version);
            hlVersionPage.NavigateUrl = ActionHelper.GetViewPagePath(this.ResolveUrlLC("PageHistoryList.aspx"), PageNameUtil.Decode(WikiPage));
            hlVersionPage.Visible = Action.Equals(ActionOnPage.View) || Action.Equals(ActionOnPage.CategoryView);
            litVersionSeparator.Visible = hlEditPage.Visible;

        }
        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            if (!IsFile)
            {
                Response.RedirectLC(ActionHelper.GetViewPagePath(this.ResolveUrlLC("Default.aspx"), PageNameUtil.Decode(WikiPage)), this);
            }
            else
            {
                Response.RedirectLC(ActionHelper.GetViewFilePath(this.ResolveUrlLC("Default.aspx"), PageNameUtil.Decode(WikiPage)), this);
            }
        }

        protected void cmdSave_Click(object sender, EventArgs e)
        {
            SaveResult result;
            string pageName;
            if (IsFile || Action.Equals(ActionOnPage.AddNewFile))
            {
                result = wikiEditFile.Save(SecurityContext.CurrentAccount.ID, out pageName);
            }
            else
            {
                result = wikiEditPage.Save(SecurityContext.CurrentAccount.ID, out pageName);
            }

            PrintResultBySave(result, pageName);
            if (result == SaveResult.OkPageRename)
            {
                //Redirect
                Response.RedirectLC(ActionHelper.GetViewPagePath(this.ResolveUrlLC("Default.aspx"), PageNameUtil.Decode(pageName)), this);
            }
        }

        private void PrintInfoMessage(string info, InfoType type)
        {
            WikiMaster.PrintInfoMessage(info, type);
        }

        private void PrintResultBySave(SaveResult result, string pageName)
        {
            InfoType infoType = InfoType.Info;
            string authorId = SecurityContext.CurrentAccount.ID.ToString();
            if (!result.Equals(SaveResult.Ok) && !result.Equals(SaveResult.NoChanges))
            {
                infoType = InfoType.Alert;
            }


            switch (result)
            {
                case SaveResult.SectionUpdate:
                    WikiNotifyClient.SendNoticeAsync(
                            authorId,
                            ASC.Web.Community.Wiki.Common.Constants.EditPage,
                            pageName,
                            null,
                            GetListOfTagValForNotify(pageName, "edit wiki page", null));
                    Response.RedirectLC(ActionHelper.GetViewPagePath(this.ResolveUrlLC("Default.aspx"), pageName), this);
                    break;
                case SaveResult.OkPageRename:
                case SaveResult.Ok:
                    PrintInfoMessage(WikiResource.msgSaveSucess, infoType);
                    if (Action.Equals(ActionOnPage.AddNew))
                    {

                        WikiNotifyClient.SendNoticeAsync(
                            authorId,
                            ASC.Web.Community.Wiki.Common.Constants.NewPage,
                            null,
                            null,
                            GetListOfTagValForNotify(pageName));

                        WikiActivityPublisher.AddPage(PagesProvider.PagesGetByName(pageName, TenantId));

                        Response.RedirectLC(ActionHelper.GetEditPagePath(this.ResolveUrlLC("Default.aspx"), pageName), this);

                    }
                    else if (Action.Equals(ActionOnPage.AddNewFile))
                    {
                        WikiActivityPublisher.AddFile(PagesProvider.FilesGetByName(pageName, TenantId));
                        Response.RedirectLC(ActionHelper.GetEditFilePath(this.ResolveUrlLC("Default.aspx"), pageName), this);
                    }
                    else if (!IsFile)
                    {
                        bool isNewPage = !WikiActivityPublisher.EditPage(PagesProvider.PagesGetByName(pageName, TenantId));

                        if (isNewPage)
                        {
                            WikiNotifyClient.SendNoticeAsync(
                            authorId,
                            ASC.Web.Community.Wiki.Common.Constants.NewPage,
                            null,
                            null,
                            GetListOfTagValForNotify(pageName));
                        }
                        else
                        {
                            WikiNotifyClient.SendNoticeAsync(
                            authorId,
                            ASC.Web.Community.Wiki.Common.Constants.EditPage,
                            pageName,
                            null,
                            GetListOfTagValForNotify(pageName, "edit wiki page", null));
                        }

                    }
                    break;
                case SaveResult.FileEmpty:
                    PrintInfoMessage(WikiResource.msgFileEmpty, infoType);
                    break;
                case SaveResult.NoChanges:
                    PrintInfoMessage(WikiResource.msgNoChanges, infoType);
                    break;
                case SaveResult.PageNameIsEmpty:
                    PrintInfoMessage(WikiResource.msgPageNameEmpty, infoType);
                    break;
                case SaveResult.PageNameIsIncorrect:
                    PrintInfoMessage(WikiResource.msgPageNameIncorrect, infoType);
                    break;
                case SaveResult.SamePageExists:
                    PrintInfoMessage(WikiResource.msgSamePageExists, infoType);
                    break;
                case SaveResult.UserIdIsEmpty:
                    PrintInfoMessage(WikiResource.msgInternalError, infoType);
                    break;
                case SaveResult.OldVersion:
                    PrintInfoMessage(WikiResource.msgOldVersion, infoType);
                    break;
            }
        }

        private ITagValue[] GetListOfTagValForNotify(string objectID)
        {
            return GetListOfTagValForNotify(objectID, null, null);
        }

        private ITagValue[] GetListOfTagValForCategoryNotify(string objectID, string pageName)
        {
            List<ITagValue> values = new List<ITagValue>();



            UserInfo user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            string defPageHref = VirtualPathUtility.ToAbsolute(WikiManager.ViewVirtualPath);
            WikiSection section = (WikiSection)WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath + WikiManager.WikiSectionConfig).GetSection("wikiSettings");

            Pages page = PagesProvider.PagesGetByName(pageName, TenantId);


            if (page != null)
            {

                values.Add(new TagValue(ASC.Web.Community.Wiki.Common.Constants.TagPageName, page.PageName));
                values.Add(new TagValue(ASC.Web.Community.Wiki.Common.Constants.TagURL, ActionHelper.GetViewPagePath(defPageHref, page.PageName)));
                values.Add(new TagValue(ASC.Web.Community.Wiki.Common.Constants.TagUserName, user.DisplayUserName(true)));
                values.Add(new TagValue(ASC.Web.Community.Wiki.Common.Constants.TagUserURL, CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetUserProfile(user.ID, CommonLinkUtility.GetProductID()))));
                values.Add(new TagValue(ASC.Web.Community.Wiki.Common.Constants.TagDate, TenantUtil.DateTimeNow()));
                values.Add(new TagValue(ASC.Web.Community.Wiki.Common.Constants.TagPostPreview, HtmlUtility.GetText(EditPage.ConvertWikiToHtml(page.PageName, page.Body, defPageHref,
                                                section.ImageHangler.UrlFormat, TenantId), 120)));
                values.Add(new TagValue(ASC.Web.Community.Wiki.Common.Constants.TagCatName, objectID));



            }

            return values.ToArray();
        }

        private ITagValue[] GetListOfTagValForNotify(string objectID, string patternType, string commentBody)
        {
            List<ITagValue> values = new List<ITagValue>();



            UserInfo user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            string defPageHref = VirtualPathUtility.ToAbsolute(WikiManager.ViewVirtualPath);
            WikiSection section = (WikiSection)WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath + WikiManager.WikiSectionConfig).GetSection("wikiSettings");

            Pages page = PagesProvider.PagesGetByName(objectID, TenantId);


            if (page != null)
            {

                values.Add(new TagValue(ASC.Web.Community.Wiki.Common.Constants.TagPageName, String.IsNullOrEmpty(page.PageName) ? Resources.WikiResource.MainWikiCaption : page.PageName));
                values.Add(new TagValue(ASC.Web.Community.Wiki.Common.Constants.TagURL, CommonLinkUtility.GetFullAbsolutePath(ActionHelper.GetViewPagePath(defPageHref, page.PageName))));
                values.Add(new TagValue(ASC.Web.Community.Wiki.Common.Constants.TagUserName, user.DisplayUserName(true)));
                values.Add(new TagValue(ASC.Web.Community.Wiki.Common.Constants.TagUserURL, CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetUserProfile(user.ID, CommonLinkUtility.GetProductID()))));
                values.Add(new TagValue(ASC.Web.Community.Wiki.Common.Constants.TagDate, TenantUtil.DateTimeNow()));
                values.Add(new TagValue(ASC.Web.Community.Wiki.Common.Constants.TagPostPreview, HtmlUtility.GetText(EditPage.ConvertWikiToHtml(page.PageName, page.Body, defPageHref,
                                                section.ImageHangler.UrlFormat, TenantId), 120)));

                if (!string.IsNullOrEmpty(patternType))
                {
                    values.Add(new TagValue(ASC.Web.Community.Wiki.Common.Constants.TagChangePageType, patternType));
                }

                if (!string.IsNullOrEmpty(commentBody))
                {
                    values.Add(new TagValue(ASC.Web.Community.Wiki.Common.Constants.TagCommentBody, commentBody));
                }

            }

            return values.ToArray();
        }

        private void InitEditsLink()
        {
            hlEditPage.Text = WikiResource.cmdEdit;
            cmdSave.Text = WikiResource.cmdPublish;
            hlPreview.Text = WikiResource.cmdPreview;
            hlPreview.Attributes["onclick"] = string.Format("{0}();return false;", wikiEditPage.GetShowPrevFunctionName());
            //hlPreview.NavigateUrl = string.Format("javascript:{0}();", wikiEditPage.GetShowPrevFunctionName());
            hlPreview.NavigateUrl = string.Format("javascript:void(0);");
            cmdCancel.Text = WikiResource.cmdCancel;
            cmdCancel.Attributes["name"] = wikiEditPage.WikiFckClientId;
            cmdDelete.Text = WikiResource.cmdDelete;
            cmdDelete.OnClientClick = string.Format("javascript:return confirm(\"{0}\");", WikiResource.cfmDeletePage);



            hlPreview.Visible = Action.Equals(ActionOnPage.AddNew) || Action.Equals(ActionOnPage.Edit) || Action.Equals(ActionOnPage.CategoryEdit);

            if (IsFile)
            {
                hlEditPage.NavigateUrl = ActionHelper.GetEditFilePath(this.ResolveUrlLC("Default.aspx"), PageNameUtil.Decode(WikiPage));
            }
            else
            {
                hlEditPage.NavigateUrl = ActionHelper.GetEditPagePath(this.ResolveUrlLC("Default.aspx"), PageNameUtil.Decode(WikiPage));
            }

            if (isEmptyPage)
            {
                hlEditPage.Visible = pEditButtons.Visible = false;
                cmdDelete.Visible = false;
                if (Action.Equals(ActionOnPage.CategoryView))
                {
                    hlEditPage.Visible = true;
                }
            }
            else
            {
                UpdateEditDeleteVisible(_wikiObjOwner);
            }

            litVersionSeparatorDel.Visible = cmdDelete.Visible;

        }

        private void UpdateEditDeleteVisible(IWikiObjectOwner obj)
        {
            bool canEdit = false;
            bool canDelete = false;
            bool editVisible = Action.Equals(ActionOnPage.View) || Action.Equals(ActionOnPage.CategoryView);
            if (obj != null)
            {
                WikiObjectsSecurityObject secObj = new WikiObjectsSecurityObject(obj);
                canEdit = SecurityContext.CheckPermissions(secObj, ASC.Web.Community.Wiki.Common.Constants.Action_EditPage);
                canDelete = SecurityContext.CheckPermissions(secObj, ASC.Web.Community.Wiki.Common.Constants.Action_RemovePage) &&
                        !string.IsNullOrEmpty(obj.GetObjectId().ToString());
            }

            pEditButtons.Visible = !editVisible;
            hlEditPage.Visible = editVisible && canEdit;

            if (Version > 0 && (Action.Equals(ActionOnPage.View) || Action.Equals(ActionOnPage.CategoryView)))
            {
                hlEditPage.Visible = pEditButtons.Visible = false;
            }

            cmdDelete.Visible = editVisible && canDelete;
            litVersionSeparatorDel.Visible = cmdDelete.Visible;
        }
        #region Comments Functions
        private void InitCommentsView()
        {
            if (m_IsCategory)
                return;

            string pageName = PageNameUtil.Decode(WikiPage);
            commentList.Visible = true;
            List<CommentInfo> comments = new List<CommentInfo>();

            AppendChildsComments(comments, pageName, GetCommentInfo);

            commentList.Items = comments;

            ConfigureComments(commentList, pageName);



            commentList.TotalCount = GetCommentsCount(comments);
            commentList.CommentsCountTitle = commentList.TotalCount.ToString(CultureInfo.CurrentCulture);


        }


        private void ConfigureComments(CommentsList commentList, string pageName)
        {

            CommonControlsConfigurer.CommentsConfigure(commentList);

            commentList.Simple = false;
            commentList.BehaviorID = "_commentsWikiObj";

            commentList.IsShowAddCommentBtn = SecurityContext.CheckPermissions(ASC.Web.Community.Wiki.Common.Constants.Action_AddComment);

            commentList.JavaScriptAddCommentFunctionName = "_Default.AddComment";
            commentList.JavaScriptPreviewCommentFunctionName = "_Default.GetPreview";
            commentList.JavaScriptRemoveCommentFunctionName = "_Default.RemoveComment";
            commentList.JavaScriptUpdateCommentFunctionName = "_Default.UpdateComment";
            commentList.JavaScriptLoadBBcodeCommentFunctionName = "_Default.LoadCommentText";
            commentList.FckDomainName = "wiki_comments";


            commentList.ObjectID = pageName;
        }

        public CommentInfo GetPrevHTMLComment(string text, string commentId)
        {
            WikiComments comment;
            if (!string.IsNullOrEmpty(commentId))
            {
                comment = CommentsProvider.GetCommentById(new Guid(commentId), TenantId);
            }
            else
            {
                comment = new WikiComments();
                comment.Date = TenantUtil.DateTimeNow();
                comment.UserId = SecurityContext.CurrentAccount.ID;
                comment.Inactive = false;
            }

            comment.Body = text;
            CommentInfo info = GetCommentInfo(comment);
            info.IsEditPermissions = false;
            info.IsResponsePermissions = false;
            return info;
        }

        public CommentInfo GetCommentInfo(WikiComments comment)
        {
            CommentInfo info = new CommentInfo();

            info.CommentID = comment.Id.ToString();
            info.UserID = comment.UserId;
            info.TimeStamp = comment.Date;
            info.TimeStampStr = comment.Date.Ago();

            info.IsRead = true;
            info.Inactive = comment.Inactive;
            info.CommentBody = comment.Body;
            info.UserFullName = DisplayUserSettings.GetFullUserName(comment.UserId);
            info.UserAvatar = GetHtmlImgUserAvatar(comment.UserId);

            info.IsEditPermissions = SecurityContext.CheckPermissions(new WikiObjectsSecurityObject(comment), ASC.Web.Community.Wiki.Common.Constants.Action_EditRemoveComment);
            info.IsResponsePermissions = true;//UserPermissions.IsUserAbleToComment;
            info.UserPost = CoreContext.UserManager.GetUsers(comment.UserId).Title ?? "";
            return info;
        }


        private static int GetCommentsCount(ICollection<CommentInfo> comments)
        {
            int count = comments.Count;
            foreach (var info in comments)
            {
                count += GetCommentsCount(info.CommentList);
            }
            return count;
        }

        public static string GetHtmlImgUserAvatar(Guid userId)
        {
            string imgPath = UserPhotoManager.GetSmallPhotoURL(userId);
            if (imgPath != null)
                return "<img class=\"userMiniPhoto\" src=\"" + imgPath + "\"/>";

            return "";
        }

        #region Ajax functions for comments management

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string RemoveComment(string commentId, string pid)
        {

            WikiComments comment = CommentsProvider.RemoveComment(new Guid(commentId), TenantId);
            WikiActivityPublisher.DeletePageComment(PagesProvider.PagesGetByName(comment.PageName, TenantId), comment);
            return commentId;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string GetPreview(string text, string commentID)
        {
            CommentInfo info = GetPrevHTMLComment(text, commentID);
            var defComment = new CommentsList();
            ConfigureComments(defComment, null);

            return CommentsHelper.GetOneCommentHtmlWithContainer(
                    defComment,
                    info,
                    true,
                    false);

        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse AddComment(string parentCommentId, string pageName, string text, string pid)
        {
            AjaxResponse resp = new AjaxResponse();
            resp.rs1 = parentCommentId;

            WikiComments newComment = new WikiComments();

            newComment.Body = text;
            newComment.Date = TenantUtil.DateTimeNow();
            newComment.UserId = SecurityContext.CurrentAccount.ID;
            newComment.Inactive = false;
            if (!string.IsNullOrEmpty(parentCommentId))
            {
                newComment.ParentId = new Guid(parentCommentId);
            }
            newComment.PageName = pageName;


            newComment = CommentsProvider.SaveComment(newComment, TenantId);

            WikiActivityPublisher.AddPageComment(PagesProvider.PagesGetByName(newComment.PageName, TenantId), newComment);

            CommentInfo info = GetCommentInfo(newComment);


            var defComment = new CommentsList();
            ConfigureComments(defComment, pageName);

            int visibleCommentsCount = CommentsProvider.GetAllCommentsCount(pageName, TenantId);


            resp.rs2 = CommentsHelper.GetOneCommentHtmlWithContainer(
                            defComment,
                            info,
                            string.IsNullOrEmpty(parentCommentId),
                            visibleCommentsCount % 2 == 1);

            WikiNotifyClient.SendNoticeAsync(
                            SecurityContext.CurrentAccount.ID.ToString(),
                            ASC.Web.Community.Wiki.Common.Constants.EditPage,
                            pageName,
                            null,
                            GetListOfTagValForNotify(pageName, "new wiki page comment", newComment.Body));

            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse UpdateComment(string commentId, string text, string pid)
        {

            AjaxResponse resp = new AjaxResponse();
            resp.rs1 = commentId;
            if (text == null)
                return resp;
            WikiComments comment = CommentsProvider.GetCommentById(new Guid(commentId), TenantId);
            comment.Body = text;
            comment = CommentsProvider.SaveComment(comment, TenantId);

            WikiActivityPublisher.EditPageComment(PagesProvider.PagesGetByName(comment.PageName, TenantId), comment);

            resp.rs2 = text;

            //WikiNotifyClient.SendNoticeAsync(
            //                ASC.Web.Community.Wiki.Common.Constants.EditPage,
            //                comment.PageName,
            //                null,
            //                GetListOfTagValForNotify(comment.PageName, "editPageComment", comment.Body));

            return resp;
        }

        [AjaxPro.AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string LoadCommentText(string commentId, string pid)
        {
            WikiComments comment = CommentsProvider.GetCommentById(new Guid(commentId), TenantId);
            return comment.Body;
        }

        [AjaxPro.AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string ConvertWikiToHtml(string pageName, string wikiValue, string appRelativeCurrentExecutionFilePath,
                string imageHandlerUrl)
        {
            return EditPage.ConvertWikiToHtml(pageName, wikiValue, appRelativeCurrentExecutionFilePath,
                imageHandlerUrl, TenantId);
        }

        [AjaxPro.AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string ConvertWikiToHtmlWysiwyg(string pageName, string wikiValue, string appRelativeCurrentExecutionFilePath,
                string imageHandlerUrl)
        {
            return EditPage.ConvertWikiToHtmlWysiwyg(pageName, wikiValue, appRelativeCurrentExecutionFilePath,
                imageHandlerUrl, TenantId);
        }

        [AjaxPro.AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string CreateImageFromWiki(string pageName, string wikiValue, string appRelativeCurrentExecutionFilePath,
                string imageHandlerUrl)
        {
            return EditPage.CreateImageFromWiki(pageName, wikiValue, appRelativeCurrentExecutionFilePath,
                imageHandlerUrl, TenantId);
        }

        [AjaxPro.AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string UpdateTempImage(string fileName, string UserId, string tempFileName)
        {
            string outFileName;
            EditFile.MoveContentFromTemp(new Guid(UserId), tempFileName, fileName, ConfigLocation, PageWikiSection, TenantId, HttpContext.Current, RootPath, out outFileName);
            return outFileName;
        }

        [AjaxPro.AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public void CancelUpdateImage(string UserId, string tempFileName)
        {
            EditFile.DeleteTempContent(tempFileName, ConfigLocation, PageWikiSection, TenantId, HttpContext.Current);
        }

        [AjaxPro.AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string SearchPagesByStartName(string pageStartName)
        {
            List<Pages> pages = PagesProvider.PagesGetByStartName(pageStartName, TenantId);

            if (pages.Count > WikiManager.MaxPageSearchInLinkDlgResult)
                pages = pages.GetRange(0, WikiManager.MaxPageSearchInLinkDlgResult);
            StringBuilder sb = new StringBuilder();

            foreach (Pages p in pages)
            {
                sb.Append(@"<div class=""seachHelpItem"" onclick=""javascript:MouseSelectSearchPages(this);"">");
                sb.Append(p.PageName);
                sb.Append(@"</div>");
            }

            return sb.ToString();
        }

        [AjaxPro.AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string SearchFilesByStartName(string fileStartName)
        {
            List<Files> pages = PagesProvider.FilesGetByStartName(fileStartName, TenantId);

            if (pages.Count > WikiManager.MaxPageSearchInLinkDlgResult)
                pages = pages.GetRange(0, WikiManager.MaxPageSearchInLinkDlgResult);
            List<string> result = new List<string>();

            StringBuilder sb = new StringBuilder();

            foreach (Files f in pages)
            {
                sb.Append(@"<div class=""seachHelpItem"" onclick=""javascript:MouseSelectSearchPages(this);"">");
                sb.Append(f.FileName);
                sb.Append(@"</div>");
            }

            return sb.ToString();
        }


        #endregion


        protected void AppendChildsComments(List<CommentInfo> commentList, string pageName, GetCommentInfoHandler getCommentInfo)
        {
            List<WikiComments> list = CommentsProvider.GetTopCommentsByPageName(pageName, TenantId);
            foreach (WikiComments comment in list)
            {
                CommentInfo info = getCommentInfo(comment);
                List<CommentInfo> tempComments = new List<CommentInfo>();

                AppendChildsCommentsOfComment(tempComments, comment, getCommentInfo);

                info.CommentList = tempComments;

                commentList.Add(info);
            }


        }

        private void AppendChildsCommentsOfComment(List<CommentInfo> commentList, WikiComments wikiComment, GetCommentInfoHandler getCommentInfo)
        {
            List<WikiComments> list = CommentsProvider.GetTopCommentsByParentId(wikiComment.Id, TenantId);
            foreach (WikiComments comment in list)
            {
                CommentInfo info = getCommentInfo(comment);
                List<CommentInfo> tempComments = new List<CommentInfo>();

                AppendChildsCommentsOfComment(tempComments, comment, getCommentInfo);

                info.CommentList = tempComments;

                commentList.Add(info);
            }
        }


        #endregion

        #region IContextInitializer Members

        public void InitializeContext(HttpContext context)
        {
            _rootPath = context.Server.MapPath("~");
            _wikiSection = (WikiSection)WebConfigurationManager.OpenWebConfiguration(ConfigLocation).GetSection("wikiSettings");
        }

        #endregion
    }
}