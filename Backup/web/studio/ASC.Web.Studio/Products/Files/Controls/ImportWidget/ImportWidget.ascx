<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ImportWidget.ascx.cs"
    Inherits="ASC.Web.Files.Controls.ImportWidget" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Import Namespace="ASC.Web.Files.Import" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascw" %>
<% #if (DEBUG) %>
<link href="<%= String.Concat(PathProvider.BaseAbsolutePath, "Controls/ImportWidget/ImportWidget.css" )%>"
    type="text/css" rel="stylesheet" />
<% #endif %>

<script type="text/javascript" language="javascript" src="<%= String.Concat(PathProvider.BaseAbsolutePath, "Controls/ImportWidget/importwidget.js" ) %>"></script>

<script type="text/javascript" language="javascript">
    if (typeof ASC === "undefined") ASC = {};
    if (typeof ASC.Files === "undefined") ASC.Files = (function() { return {} })();
    if (typeof ASC.Files.Constants === "undefined") ASC.Files.Constants = {}

    ASC.Files.Constants.URL_OAUTH_GOOGLE = "<%=ASC.Web.Files.Import.Google.OAuth.Location%>";
    ASC.Files.Constants.URL_OAUTH_BOXNET = "<%=ASC.Web.Files.Import.Boxnet.BoxLogin.Location%>";
</script>

<ul id="files_import_container">
    <% if (ImportConfiguration.SupportGoogleImport) %>
    <% { %>
    <li class="import_from_google">
        <%=FilesUCResource.ImportFromGoogle%>
    </li>
    <% } %>
    <% if (ImportConfiguration.SupportZohoImport) %>
    <% { %>
    <li class="import_from_zoho">
        <%=FilesUCResource.ImportFromZoho%>
    </li>
    <% } %>
    <% if (ImportConfiguration.SupportBoxNetImport) %>
    <% { %>
    <li class="import_from_boxnet">
        <%=FilesUCResource.ImportFromBoxNet%>
    </li>
    <% } %>
</ul>
<div id="LoginDialogTemp" class="popupModal">
    <ascw:container id="LoginDialog" runat="server">
        <header><%=FilesUCResource.ImportFromZoho%></header>
        <body>
            <div>
                <div>
                    <%=FilesUCResource.Login%></div>
                <input type="text" id="files_login" class="textEdit" />
            </div>
            <div>
                <div>
                    <%=FilesUCResource.Password%></div>
                <input type="password" id="files_pass" class="textEdit" />
            </div>
            <div class="action-block">
                <a id="files_submitLoginDialog" class="baseLinkButton">
                    <%=FilesUCResource.ButtonOk%>
                </a><span class="button-splitter"></span><a class="grayLinkButton" onclick="PopupKeyUpActionProvider.CloseDialog();return false;">
                    <%=FilesUCResource.ButtonCancel%>
                </a>
            </div>
            <div style="display: none;" class="ajax-info-block">
                <span class="textMediumDescribe">
                    <%=FilesUCResource.ProcessAuthentificate%>
                </span>
                <br />
                <img alt="" src="<%=WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif")%>" />
            </div>
        </body>
    </ascw:container>
</div>
<div id="ImportDialogTemp" class="popupModal">
    <ascw:container id="ImportDialog" runat="server">
        <header>
           <span class="header-content"><%=FilesUCResource.ImportFromGoogle%></span>
        </header>
        <body>
            <div id="import_data">
            </div>
            <div id="import_to_folder">
                <%=FilesUCResource.ImportToFolder%>:&nbsp; <a id="files_importToFolderBtn" style="margin-bottom: -3px;"
                    class="import_tree"></a>&nbsp; <font id="files_importToFolder">
                        <%=FilesUCResource.MyFiles%></font> <font>&nbsp;>&nbsp;</font><span><%=FilesUCResource.ImportFromGoogle%></span>
                <input type="hidden" id="files_importToFolderId" value="" />
            </div>
            <div>
                <%=FilesUCResource.IfFileNameConflict%>:
                <label>
                    <input type="radio" name="file_conflict" value="true" checked="checked" />
                    <%=FilesUCResource.Overwrite%>
                </label>
                <label>
                    <input type="radio" name="file_conflict" value="false" />
                    <%=FilesUCResource.Ignore%>
                </label>
            </div>
            <div class="seporator">
            </div>
            <div class="clearFix">
                <div class="action-block" style="float: left;">
                    <a id="files_startImportData" class="baseLinkButton">
                        <%=FilesUCResource.ButtonImport%>
                    </a><span class="button-splitter"></span><a class="grayLinkButton" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                        <%=FilesUCResource.ButtonCancel%>
                    </a>
                </div>
                <div class="action-block-progress" style="float: left; display: none;">
                    <a id="files_import_minimize" class="baseLinkButton">
                        <%=FilesUCResource.ButtonMinimize%>
                    </a>
                </div>
                <div class="import_progress_panel" style="display: none;">
                    <div>
                        <div style="float: left;">
                            <span class="textBigDescribe">
                                <%=FilesUCResource.ImportProgress%>: </span>
                        </div>
                        <div>
                        </div>
                        <div style="float: right;">
                            <span class="textBigDescribe"><span class="percent">0</span>%</span>
                        </div>
                        <br style="clear: both;" />
                    </div>
                    <div class="studioFileUploaderProgressBorder">
                        <div class="studioFileUploaderProgressBar" style="width: 0px">
                        </div>
                    </div>
                </div>
            </div>
        </body>
    </ascw:container>
</div>
<div id="import_progress_min" class="import_progress_min" style="display: none;">
    <div class="files_minimize_uploader" style="width: 0px">
    </div>
</div>
