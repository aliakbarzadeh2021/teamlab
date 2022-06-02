<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MainContent.ascx.cs"
    Inherits="ASC.Web.Files.Controls.MainContent" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascw" %>
<% #if (DEBUG) %>
<link href="<%= String.Concat(PathProvider.BaseAbsolutePath, "Controls/MainContent/MainContent.css" )%>" type="text/css" rel="stylesheet" />
<link href="<%=PathProvider.GetFileStaticRelativePath("common.css")%>" type="text/css" rel="stylesheet" />
<% #else %>
<link href="<%=PathProvider.GetFileStaticRelativePath("files-min.css")%>" type="text/css" rel="stylesheet" />
<% #endif %>

<script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("zeroclipboard.js") %>"></script>
<script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("common.js") %>"></script>
<script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("templatemanager.js") %>"></script>
<script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("servicemanager.js") %>"></script>
<script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("ui.js") %>"></script>
<script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("eventhandler.js") %>"></script>
<script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("foldermanager.js") %>"></script>
<script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("actionmanager.js") %>"></script>
<script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("tree.js") %>"></script>
<script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("dragdrop.js") %>"></script>
<script type="text/javascript" language="javascript" src="<%= PathProvider.GetResourcesPath() %>"></script>

<script type="text/javascript" language="javascript" src="<%= String.Concat(PathProvider.BaseAbsolutePath, "Controls/MainMenu/mainmenu.js" ) %>"></script>
<script type="text/javascript" language="javascript" src="<%= String.Concat(PathProvider.BaseAbsolutePath, "Controls/MainMenu/uploadmanager.js" ) %>"></script>

<%-- Info panel --%>
<div id="infoPanelContainer" class="infoPanel files_infoPanel">
    <div>&nbsp;</div>
</div>

<%-- header --%>
<div class="folderHeader">
    <div class="clearFix">
        <div class="tree">
            <a id="files_showTreeView" class="tree" title="<%=FilesUCResource.OpenTree%>"></a>
            <div id="files_breadCrumbsContainer" class="files_breadCrumbsContainer">
            </div>
        </div>
        <div id="files_treeViewPanel" class="files_treeViewPanel files_popup_win" style="margin-top: 3px;">
            <div class="popup-corner" style="left: 7px;">
            </div>
            <div class="jstree jstree-default jstree-focused">
                <ul id="files_trewViewContainer">
                    <%if (FolderIDUserRoot != 0)%>
                    <%{ %>
                    <li id="tree_node_<%=FolderIDUserRoot%>" class="jstree-closed"><ins class="jstree-icon"
                        rel="expander"></ins><a id="tree_selector_<%=FolderIDUserRoot%>" title="<%=FilesUCResource.MyFiles%>"
                            href="#<%=FolderIDUserRoot%>" onclick="ASC.Files.UI.madeAnchor(<%=FolderIDUserRoot%>);return false;">
                            <ins class="jstree-icon myFiles" rel="folder"></ins>
                            <%=FilesUCResource.MyFiles%>
                        </a></li>
                    <%} %>
                    <%if (FolderIDShare != 0)%>
                    <%{ %>
                    <li id="tree_node_<%=FolderIDShare%>" class="jstree-closed"><ins class="jstree-icon"
                        rel="expander"></ins><a id="tree_selector_<%=FolderIDShare%>" title="<%=FilesUCResource.SharedForMe%>"
                            href="#<%=FolderIDShare%>" onclick="ASC.Files.UI.madeAnchor(<%=FolderIDShare%>);return false;">
                            <ins class="jstree-icon shareformeFiles" rel="folder"></ins>
                            <%=FilesUCResource.SharedForMe%>
                        </a></li>
                    <%} %>
                    <%if (FolderIDCommonRoot != 0)%>
                    <%{ %>
                    <li id="tree_node_<%=FolderIDCommonRoot%>" class="jstree-closed"><ins class="jstree-icon"
                        rel="expander"></ins><a id="tree_selector_<%=FolderIDCommonRoot%>" title="<%=FilesUCResource.CorporateFiles%>"
                            href="#<%=FolderIDCommonRoot%>" onclick="ASC.Files.UI.madeAnchor(<%=FolderIDCommonRoot%>);return false;">
                            <ins class="jstree-icon corporateFiles" rel="folder"></ins>
                            <%=FilesUCResource.CorporateFiles%>
                        </a></li>
                    <%} %>
                    <%if (FolderIDCurrentRoot != 0)
                      { %>
                    <li id="tree_node_<%=FolderIDCurrentRoot%>" class="jstree-closed"><ins class="jstree-icon"
                        rel="expander"></ins><a id="tree_selector_<%=FolderIDCurrentRoot%>" title="<%=FilesUCResource.ProjectFiles%>"
                            href="#<%=FolderIDCurrentRoot%>" onclick="ASC.Files.UI.madeAnchor(<%=FolderIDCurrentRoot%>);return false;">
                            <ins class="jstree-icon projectFiles" rel="folder"></ins>
                            <%=FilesUCResource.ProjectFiles%>
                        </a></li>
                    <%} %>
                </ul>
            </div>
        </div>
        <%if (ASC.Core.SecurityContext.IsAuthenticated)
          { %>
        <div id="files_treeViewPanelSelector" class="files_treeViewPanel files_popup_win">
            <div class="popup-corner" style="left: 21.5px;">
            </div>
            <div style="margin-bottom: 5px;">
                <b>
                    <%=FilesUCResource.SelectFolder%></b>
            </div>
            <div class="jstree jstree-default jstree-focused">
                <ul id="files_trewViewSelector">
                    <%if (FolderIDUserRoot != 0)
                      { %>
                    <li id="stree_node_<%=FolderIDUserRoot%>" class="jstree-closed"><ins class="jstree-icon"
                        rel="expander"></ins><a id="stree_selector_<%=FolderIDUserRoot%>" title="<%=FilesUCResource.MyFiles%>">
                            <ins class="jstree-icon myFiles" rel="folder"></ins>
                            <%=FilesUCResource.MyFiles%>
                        </a></li>
                    <%} %>
                    <%if (FolderIDShare != 0)%>
                    <%{ %>
                    <li id="stree_node_<%=FolderIDShare%>" class="jstree-closed"><ins class="jstree-icon"
                        rel="expander"></ins><a id="stree_selector_<%=FolderIDShare%>" title="<%=FilesUCResource.SharedForMe%>"
                            href="#<%=FolderIDShare%>">
                            <ins class="jstree-icon shareformeFiles" rel="folder"></ins>
                            <%=FilesUCResource.SharedForMe%>
                        </a></li>
                    <%} %>
                    <%if (FolderIDCommonRoot != 0)
                      { %>
                    <li id="stree_node_<%=FolderIDCommonRoot%>" class="jstree-closed"><ins class="jstree-icon"
                        rel="expander"></ins><a id="stree_selector_<%=FolderIDCommonRoot%>" title="<%=FilesUCResource.CorporateFiles%>">
                            <ins class="jstree-icon corporateFiles" rel="folder"></ins>
                            <%=FilesUCResource.CorporateFiles%>
                        </a></li>
                    <%} %>
                    <%if (FolderIDCurrentRoot != 0)
                      { %>
                    <li id="stree_node_<%=FolderIDCurrentRoot%>" class="jstree-closed"><ins class="jstree-icon"
                        rel="expander"></ins><a id="stree_selector_<%=FolderIDCurrentRoot%>" class="jstree-close-project"
                            title="<%=FilesUCResource.ProjectFiles%>">
                            <ins class="jstree-icon projectFiles" rel="folder"></ins>
                            <%=FilesUCResource.ProjectFiles%>
                        </a></li>
                    <%} %>
                </ul>
            </div>
        </div>
        <%} %>
    </div>
    <div id="headerContainer" class="headerContainer">
    </div>
</div>
<%-- Main Content Header --%>
<div id="mainContentHeader" class="clearFix">
    <div class="mainContentHeader_item mainContentHeader_item_actions" style="padding: 0;">
        <div class="mainContentHeader_actions">
            <input id="files_selectAll_check" type="checkbox" title="<%=FilesUCResource.MainHeaderSelectAll%>" />
            <div id="files_actions_open" class="mainContentHeader_combobtn-b" title="<%=FilesUCResource.MainHeaderActionsOpen%>">
            </div>
        </div>
    </div>
    <div class="mainContentHeader_item">
        <div id="files_filter_open" class="mainContentHeader_filter">
            <div class="mainContentHeader_title">
                <%=FilesUCResource.MainHeaderFilter%></div>
            <div id="files_filter_category" class="mainContentHeader_combobtn" title="<%=FilesUCResource.MainHeaderFilterOpen%>">
                <span class="mainContentHeader_value">
                    <%=FilesUCResource.FilterAllButton%></span>
            </div>
            <div id="files_filter_value" class="mainContentHeader_combobtn files_filter_value"
                title="<%=FilesUCResource.MainHeaderFilterOpen%>" style="display: none;">
                <span class="mainContentHeader_value">
                    <%=FilesUCResource.FilterAllButton%></span>
            </div>
        </div>
    </div>
    <div class="mainContentHeader_item mainContentHeader_item_sort">
        <div class="mainContentHeader_sort">
            <div class="mainContentHeader_title">
                <%=FilesUCResource.MainHeaderSort%></div>
            <div id="files_sort_open" class="mainContentHeader_combobtn" title="<%=FilesUCResource.MainHeaderSortOpen%>">
                <span id="files_sort_value" class="mainContentHeader_value">
                    <%=FilesUCResource.ButtonSortName%></span>
            </div>
        </div>
    </div>
</div>
<%-- pop panel under Main Content Header--%>
<div id="mainContentHeader_panel" class="mainContentHeader_panel files_popup_win">
    <div id="files_filterPanel" class="files_popup_win">
        <div id="files_filter_show_0" class="files_topPanel_item">
            <a class="select">
                <%=FilesUCResource.FilterAllButton%></a>
        </div>
        <div id="files_filter_show_1" class="files_topPanel_item">
            <a>
                <%=FilesUCResource.Users%></a>
        </div>
        <div id="files_filter_show_2" class="files_topPanel_item">
            <a>
                <%=FilesUCResource.Departments%></a>
        </div>
        <div id="files_filter_show_3" class="files_topPanel_item">
            <a>
                <%=FilesUCResource.Types%></a>
        </div>
    </div>
    <div id="files_filter_usersPanel" class="files_topPanel_panel files_filter_usersPanel files_popup_win">
        <div id="files_filter_show_all" class="files_topPanel_item" style="width: 95%; margin-top: 0;">
            <a>
                <%=FilesUCResource.FilterAllButton%></a>
        </div>
        <asp:Repeater runat="server" ID="list_users">
            <ItemTemplate>
                <div id="files_filter_value_8_<%#(Container.DataItem as ASC.Core.Users.UserInfo).ID%>"
                    class="files_topPanel_item">
                    <a href="<%#CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetUserProfile((Container.DataItem as ASC.Core.Users.UserInfo).ID, ASC.Web.Files.Configuration.ProductEntryPoint.ID))%>"
                        title="<%#ASC.Core.Users.UserInfoExtension.DisplayUserName(Container.DataItem as ASC.Core.Users.UserInfo,true).ReplaceSingleQuote()%>">
                        <%#ASC.Core.Users.UserInfoExtension.DisplayUserName(Container.DataItem as ASC.Core.Users.UserInfo,true).ReplaceSingleQuote()%>
                    </a>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>
    <div id="files_filter_departmensPanel" class="files_topPanel_panel files_popup_win files_filter_departmensPanel">
        <div id="files_filter_show_0" class="files_topPanel_item" style="width: 95%; margin-top: 0;">
            <a>
                <%=FilesUCResource.FilterAllButton%></a>
        </div>
        <asp:Repeater runat="server" ID="list_departments">
            <ItemTemplate>
                <div id="files_filter_value_9_<%#(Container.DataItem as ASC.Core.Users.GroupInfo).ID%>"
                    class="files_topPanel_item">
                    <a href="<%#CommonLinkUtility.GetFullAbsolutePath(CommonLinkUtility.GetDepartment(ASC.Web.Files.Configuration.ProductEntryPoint.ID, (Container.DataItem as ASC.Core.Users.GroupInfo).ID))%>"
                        title="<%#(Container.DataItem as ASC.Core.Users.GroupInfo).Name.HtmlEncode().ReplaceSingleQuote()%>">
                        <%#(Container.DataItem as ASC.Core.Users.GroupInfo).Name.HtmlEncode().ReplaceSingleQuote()%>
                    </a>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>
    <div id="files_filter_typesPanel" class="files_topPanel_panel files_popup_win">
        <div id="files_filter_show_0" class="files_topPanel_item">
            <a>
                <%=FilesUCResource.FilterAllButton%></a>
        </div>
        <div id="files_filter_value_1_" class="files_topPanel_item">
            <a>
                <%=FilesUCResource.ButtonFilterFile%></a>
        </div>
        <div id="files_filter_value_2_" class="files_topPanel_item">
            <a>
                <%=FilesUCResource.ButtonFilterFolder%></a>
        </div>
        <div id="files_filter_value_3_" class="files_topPanel_item">
            <a>
                <%=FilesUCResource.ButtonFilterDocument%></a>
        </div>
        <div id="files_filter_value_4_" class="files_topPanel_item">
            <a>
                <%=FilesUCResource.ButtonFilterPresentation%></a>
        </div>
        <div id="files_filter_value_5_" class="files_topPanel_item">
            <a>
                <%=FilesUCResource.ButtonFilterSpreadsheet%></a>
        </div>
        <div id="files_filter_value_7_" class="files_topPanel_item">
            <a>
                <%=FilesUCResource.ButtonFilterPicture%></a>
        </div>
    </div>
    <div id="files_sortPanel" class="files_topPanel_panel files_popup_win files_sortPanel">
        <div id="files_sort_DateAndTime" class="files_topPanel_item">
            <a>
                <%=FilesUCResource.ButtonSortModified%></a></div>
        <div id="files_sort_Author" class="files_topPanel_item">
            <a>
                <%=FilesUCResource.ButtonSortAuthor%></a></div>
        <div id="files_sort_Size" class="files_topPanel_item">
            <a>
                <%=FilesUCResource.ButtonSortSize%></a></div>
        <div id="files_sort_AZ" class="files_topPanel_item">
            <a>
                <%=FilesUCResource.ButtonSortName%></a></div>
    </div>
</div>
<%-- Main Content --%>
<div class="mainContent">
    <div id="files_mainContent">
    </div>
    <div id="files_pageNavigatorHolder" class="pager_show_more">
        <a class="grayLinkButton"></a>
    </div>
    <div id="emptyScreen" class="noContentBlock">
        <div class="drag_drop_image">
        </div>
        <div class="clear_filter">
            <a id="files_clearFilter_btn" href="">
                <%=FilesUCResource.ButtonClearFilter%>
            </a>
        </div>
        <div>
            <a id="files_emptyFolder_btn" href="" style="font-size: 22px; font-weight: normal;">
                <%=FilesUCResource.Upload%>
            </a>
        </div>
        <div style="font-size: 20px; margin-top: 5px; font-weight: normal; color: #787878;
            cursor: default;">
            <%=FilesUCResource.EmptyFolderDescribe%>
        </div>
        <div id="empty_move_message" style="font-size: 20px; margin-top: 5px; font-weight: normal;
            cursor: default; color: #787878; display: none;">
            <%=string.Format(FilesUCResource.MoveFromDesktop, "<font style='color:#323232'>", "</font>")%>
        </div>
    </div>
    <div id="emptyContainer" class="noContentBlock">
        <div class="clear_filter">
            <a id="files_clearFilterCont_btn" href="">
                <%=FilesUCResource.ButtonClearFilter%>
            </a>
        </div>
        <div style="font-size: 20px; margin-top: 20px; font-weight: normal; color: #787878;
            cursor: default;">
            <%=FilesUCResource.EmptyContainer%>
        </div>
    </div>
</div>
<%--popup window's--%>
<div id="files_actionsPanel" class="files_action_panel files_popup_win">
    <div class="popup-corner" style="left: 21.5px;">
    </div>
    <ul>
        <li id="files_downloadButton" class="files_download_button"><a>
            <%=FilesUCResource.ButtonDownload%>
            (<span></span>)</a> </li>
<%--        <li id="files_shareaccess" class="files_share_button"><a>
            <%=FilesUCResource.ButtonShareAccess%>
            (<span></span>)</a> </li> --%>
        <li id="files_movetoButton" class="files_moveto_button"><a>
            <%=FilesUCResource.ButtonMoveTo%>
            (<span></span>)</a> </li>
        <li id="files_copytoButton"><a>
            <%=FilesUCResource.ButtonCopyTo%>
            (<span></span>)</a> </li>
        <li id="files_restoreButton"><a>
            <%=FilesUCResource.ButtonRestore%>
            (<span></span>)</a> </li>
        <li id="files_deleteButton" class="files_delete_button"><a>
            <%=FilesUCResource.ButtonDelete%>
            (<span></span>)</a> </li>
        <li id="files_emptyTrashButton"><a>
            <%=FilesUCResource.ButtonEmptyTrash%></a> </li>
    </ul>
</div>
<div id="files_actionPanel" class="files_action_panel files_popup_win">
    <div class="popup-corner" style="left: 20px;">
    </div>
    <ul id="files_actionPanel_files">
        <li id="files_open_files" class="files_open_files"><a>
            <%=FilesUCResource.OpenFile%></a> </li>
        <li id="files_edit_files" class="files_edit_files"><a>
            <%=FilesUCResource.ButtonEdit%></a> </li>
        <li id="files_editOO_files" class="files_editOO_files"><a>
            <%=FilesUCResource.ButtonEditOO%></a> </li>
        <li id="files_download_files" class="files_download_button"><a>
            <%=FilesUCResource.DownloadFile%></a> </li>
        <li id="files_shareaccess_files" class="files_share_button"><a>
            <%=FilesUCResource.ButtonShareAccess%></a> </li>
        <li id="files_unsubscribe_files"><a>
            <%=FilesUCResource.Unsubscribe%></a> </li>
        <li id="files_getlink_files" class="files_getlink_files"><a>
            <%=FilesUCResource.ButtonGetLink%></a> </li>
        <%--<li class="comming_soon">
<a><%=FilesUCResource.ButtonEmailLink%></a>
</li> --%>
        <li id="files_uploads_files" class="files_uploads_files"><a>
            <%=FilesUCResource.ButtonUploadNewVersions%></a> </li>
        <li id="files_versions_files" class="files_versions_files"><a>
            <%=FilesUCResource.ButtonShowVersions%>(<span></span>)</a> </li>
        <li id="files_moveto_files" class="files_moveto_button"><a>
            <%=FilesUCResource.ButtonMoveTo%></a> </li>
        <li id="files_copyto_files"><a>
            <%=FilesUCResource.ButtonCopyTo%></a> </li>
        <li id="files_rename_files"><a>
            <%=FilesUCResource.ButtonRename%></a> </li>
        <li id="files_restore_files"><a>
            <%=FilesUCResource.ButtonRestore%></a> </li>
        <li id="files_remove_files" class="files_delete_button"><a>
            <%=FilesUCResource.ButtonDelete%></a> </li>
    </ul>
    <ul id="files_actionPanel_folders">
        <li id="files_open_folders"><a>
            <%=FilesUCResource.OpenFolder%></a> </li>
        <li id="files_download_folders" class="files_download_button"><a>
            <%=FilesUCResource.DownloadFolder%></a> </li>
        <li id="files_shareAccess_folders" class="files_share_button"><a>
            <%=FilesUCResource.ButtonShareAccess%></a> </li>
        <li id="files_unsubscribe_folders"><a>
            <%=FilesUCResource.Unsubscribe%></a> </li>
<%--<li class="comming_soon">
<a><%=FilesUCResource.ButtonGetLink%></a>
</li>
<li class="comming_soon">
<a><%=FilesUCResource.ButtonEmailLink%></a>
</li>--%>
        <li id="files_moveto_folders" class="files_moveto_button"><a>
            <%=FilesUCResource.ButtonMoveTo%></a> </li>
        <li id="files_copyto_folders"><a>
            <%=FilesUCResource.ButtonCopyTo%></a> </li>
        <li id="files_rename_folders"><a>
            <%=FilesUCResource.ButtonRename%></a> </li>
        <li id="files_restore_folders"><a>
            <%=FilesUCResource.ButtonRestore%></a> </li>
        <li id="files_remove_folders" class="files_delete_button"><a>
            <%=FilesUCResource.ButtonDelete%></a> </li>
    </ul>
</div>
<%--dialog window--%>
<div id="files_confirm_remove" class="popupModal">
    <ascw:container id="confirmRemoveDialog" runat="server">
        <header><%=FilesUCResource.ConfirmRemove%></header>
        <body>
            <div id="confirmRemoveText">
            </div>
            <div id="confirmRemoveList" class="files_remove_list">
                <dl>
                    <dt class="confirmRemoveFolders">
                        <%=FilesUCResource.Folders%>:</dt>
                    <dd class="confirmRemoveFolders">
                    </dd>
                    <dt class="confirmRemoveFiles">
                        <%=FilesUCResource.Documents%>:</dt>
                    <dd class="confirmRemoveFiles">
                    </dd>
                </dl>
            </div>
            <span id="confirmRemoveTextDescription" class="textMediumDescribe clearFix" style="padding: 10px 0 0;">
                <%=FilesUCResource.ConfirmRemoveDescription%>
            </span>
            <div class="clearFix" style="padding-top: 16px">
                <a id="removeConfirmBtn" class="baseLinkButton" style="float: left;">
                    <%=FilesUCResource.ButtonOk%>
                </a><a class="grayLinkButton" href="" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;"
                    style="float: left; margin-left: 8px;">
                    <%=FilesUCResource.ButtonCancel%>
                </a>
            </div>
        </body>
    </ascw:container>
</div>
<div id="files_overwriteFiles" class="popupModal">
    <ascw:container id="confirmOverwriteDialog" runat="server">
        <header><%=FilesUCResource.ConfirmOverwrite%></header>
        <body>
            <div id="files_overwrite_msg" style="overflow: hidden;">
            </div>
            <ul id="files_overwrite_list" class="files_overwrite_list">
            </ul>
            <div class="clearFix" style="margin-top: 15px;">
                <a id="files_overwrite_overwrite" class="baseLinkButton" style="float: left;">
                    <%=FilesUCResource.ButtonRewrite%>
                </a>
                <a id="files_overwrite_skip" class="grayLinkButton" style="float: left; margin-left: 8px;">
                    <%=FilesUCResource.ButtonSkip%>
                </a>
                <a id="files_overwrite_cancel" class="grayLinkButton" style="float: left; margin-left: 8px;">
                    <%=FilesUCResource.ButtonCancel%>
                </a>
            </div>
        </body>
    </ascw:container>
</div>
<div id="files_get_link" class="popupModal">
    <script type="text/javascript" language="javascript">
        ZeroClipboard.setMoviePath('<%=VirtualPathUtility.ToAbsolute("~/products/files/flash/zeroclipboard/ZeroClipboard10.swf")%>');
    </script>
    <ascw:container id="files_getLinkDialog" runat="server">
        <header><%=FilesUCResource.GetLinkDialog%></header>
        <body>
            <span class="clearFix"><%=FilesUCResource.GetLinkDescribe%></span>
            <%--<a><%=FilesUCResource.GetShortLink%></a>--%>
            <input type="text" id="files_getlink_link" class="textEdit getLink_text" readonly="readonly" />
            <a id="files_getlink_actions" class="baseLinkButton">
                <span id="files_getlink_copy">
                <%=FilesUCResource.CopyToClipboard%></span>
            </a>
            <a class="grayLinkButton" style="margin-left:8px;" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                <%=FilesUCResource.ButtonClose%></a>
        </body>
    </ascw:container>
</div>
<%-- progress --%>
<div id="files_bottom_loader">
    <div id="files_progress_template" class="files_progress_box">
        <div class="headerBaseMedium">
        </div>
        <div class="progress_wrapper">
            <div class="progress">
            </div>
            <span class="percent">0</span>
        </div>
        <span class="textSmallDescribe"></span>
    </div>
</div>
<asp:PlaceHolder runat="server" ID="CommonContainer" />