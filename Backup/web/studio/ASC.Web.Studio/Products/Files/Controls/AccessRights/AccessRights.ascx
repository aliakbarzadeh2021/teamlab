<%@ Assembly Name="ASC.Web.Files" %>
<%@ Assembly Name="ASC.Web.Controls" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AccessRights.ascx.cs"
    Inherits="ASC.Web.Files.Controls.AccessRights" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascw" %>

<% #if (DEBUG) %>
    <link href="<%= String.Concat(ASC.Web.Files.Classes.PathProvider.BaseAbsolutePath, "Controls/AccessRights/AccessRights.css" )%>" type="text/css" rel="stylesheet" />
<% #endif %>

<script type="text/javascript" language="javascript" src="<%= String.Concat(PathProvider.BaseAbsolutePath, "Controls/AccessRights/accessrights.js" ) %>"></script>

<%--<div id="files_newInShare" class="popupModal">
    <ascw:container id="newInShareDialog" runat="server">
        <header><span id="newInShareDialogHeader"></span></header>
        <body>
            <div id="newInShareBody" class="newInShareBody">
                <div class="clearFix header" style="margin: 0 25px;">
                    <div class="name"><%=FilesUCResource.DocumentName%></div>
                    <div class="author"><%=FilesUCResource.Author%></div>
                    <div class="date" style="float:left;"><%=FilesUCResource.CreatingDate%></div>
                </div>
                <ul class="change_table"></ul>
            </div>
            <a class="grayLinkButton" onclick="PopupKeyUpActionProvider.CloseDialog(); return false;">
                <%=FilesUCResource.ButtonOk%>
            </a>
        </body>
    </ascw:container>
</div>--%>

<div id="files_shareAccessDialogContainer" class="popupModal">
    <ascw:container id="shareAccessDialog" runat="server">
        <header>
		<!--<img alt="" style="margin-right:5px;" src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("premium.png")%>"/>-->
		<%=FilesUCResource.ShareAccessHeader%></header>
        <body>
            <div id="share_main">
                <div id="files_rightPopup" class="files_action_panel files_popup_win" style="min-width:136px;" >
                    <ul>
                        <li id="files_read"><a><%=FilesUCResource.AceStatusEnum_Read%></a></li>
                        <li id="files_readwrite"><a><%=FilesUCResource.AceStatusEnum_ReadWrite%></a></li>
                        <li id="files_restrict"><a><%=FilesUCResource.AceStatusEnum_Restrict%></a></li>
                    </ul>
				</div>
                <ascw:ViewSwitcher ID="shareAccessTabs" runat="server" RenderAllTabs="true">
                    <tabitems>
						<ascw:ViewSwitcherTabItem runat="server" ID="UserAccess" IsSelected="true" DivId="UserAccess" OnClickText="ASC.Files.Share.initViewSwitcher(1);">
							<ul id="files_usersAccessBody" class="shareAccessBody"></ul>
                            <div id="shareSelectUserPanel" class="clearFix" style="padding-top: 10px">
                                <div style="float:left;">
                                    <ascw:AdvancedUserSelector runat="server" id="shareUserSelector">
                                    </ascw:AdvancedUserSelector>
                                </div>
                                <a id="shareUserSelector_add" class="grayLinkButton" style="float: left; margin-left: 8px;" >
                                    <%=FilesUCResource.ButtonAddUser%>
                                </a>
                            </div>
						</ascw:ViewSwitcherTabItem>
						<ascw:ViewSwitcherTabItem runat="server" ID="GroupAccess" DivId="GroupAccess" OnClickText="ASC.Files.Share.initViewSwitcher(2);">
							<div class="noContentBlock shareEmptyGroup"><%=FilesUCResource.ButtonEmptyGroup%></div>
							<ul id="files_groupsAccessBody" class="shareAccessBody"></ul>
                            <div id="shareSelectGroupPanel" class="clearFix" style="padding-top: 10px">            
                                <div style="float:left;">
                                    <select id="selectGroup" class="comboBox">
                                        <option value='emptyGroup' selected='selected' class="tintMedium" ><%=FilesUCResource.ButtonSelectGroup%></option>
                                        <%=RenderGroupsList()%>
                                    </select>
                                </div>
                                <a id="shareGroupSelector_add" class="grayLinkButton" style="float: left; margin-left: 8px;" >
                                    <%=FilesUCResource.ButtonAddGroup%>
                                </a>                
                            </div>
						</ascw:ViewSwitcherTabItem>
					</tabitems>
                </ascw:ViewSwitcher>
            </div>
            <%--<div id="share_manage">            
                <div style="margin-bottom:16px">
                    <div class="headerBase"><%=FilesUCResource.GroupTitle%></div>
                    <input type="text" id="files_groupName" class="textEdit" style="width: 100%" maxlength="120" />
                </div>
                <div class="headerBase"><%=FilesUCResource.UsersInGroup%></div>
                <ul id="usersInGroup" class="groupUsers"></ul>
            </div>--%>
            <%--Управление--%>
            <div style="padding-top: 16px">
                <a id="files_shareAccess_save" class="baseLinkButton"><%=FilesUCResource.ButtonSave%></a>
                <a id="files_shareAccess_cancel" class="grayLinkButton" style="margin-left: 8px;"><%=FilesUCResource.ButtonCancel%></a>
                <%--<a id="share_btnManage" class="grayLinkButton share_btnManage"><%=FilesUCResource.ButtonCreateGroup%></a>--%>
            </div>
        </body>
    </ascw:container>
</div>

<div id="group_remove_dialog" class="files_popup_win" style="width:280px;z-index:800;">
    <ascw:container id="confirmGroupDialog" runat="server">
        <header><%=FilesUCResource.GroupRemoveHeader%></header>
        <body>
            <div class="clearFix">
                <%=FilesUCResource.GroupConfirmRemove%>
                <div id="groupRemoveText" class="clearFix" style="overflow: hidden; padding-bottom: 10px;">
                </div>
                <a id="groupRemoveBtn" class="grayLinkButton" style="float: left;">
                    <%=FilesUCResource.ButtonYesRemove%></a>
            </div>
        </body>
    </ascw:container>
</div>