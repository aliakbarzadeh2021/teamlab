<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FavoriteWidget.ascx.cs"
    Inherits="ASC.Web.Files.Controls.FavoriteWidget" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascw" %>

<% #if (DEBUG) %>
    <link href="<%= String.Concat(PathProvider.BaseAbsolutePath, "Controls/FavoriteWidget/FavoriteWidget.css" )%>" type="text/css" rel="stylesheet" />
<% #endif %>

<script type="text/javascript" language="javascript" src="<%= String.Concat(PathProvider.BaseAbsolutePath, "Controls/FavoriteWidget/favoritewidget.js" ) %>"></script>

<ul id="files_favoritesBody" class="favoritesBody" ></ul>

<div id="files_favoritesEmpty" class="favoritesEmpty textMediumDescribe"><%=FilesUCResource.FavoritesEmpty%></div>
<a id="favorites_addcurrent" href="" ><%=FilesUCResource.ButtonFavoritesAdd%></a>

<div id="favorites_prompt_name" class="files_popup_win" style="width:280px; z-index:2;">
	<ascw:container id="promptFavoritesDialog" runat="server">
		<header><%=FilesUCResource.FavoritesCaption%></header>
		<body>
			<span class="clearFix" style="padding-bottom: 5px;"><%=FilesUCResource.FavoritesDescribe%></span>
			<div class="clearFix">
				<input type="text" id="favoriteNewName" class="textEdit favorites_prompt_input" maxlength="170"/>
				<a id="favoriteOkBtn" class="grayLinkButton" style="float: left;"><%=FilesUCResource.ButtonAdd%></a>
			</div>
		</body>
	</ascw:container>
</div>

<div id="favorites_remove_dialog" class="files_popup_win" style="width:280px;">
	<ascw:container id="confirmFavoritesDialog" runat="server">
		<header><%=FilesUCResource.FavoritesCaptionRemove%></header>
		<body>
			<div class="clearFix">
				<div id="favoriteRemoveText" class="clearFix" style="overflow:hidden; padding-bottom: 10px;"></div>
				<a id="favoriteRemoveBtn" class="grayLinkButton" style="float: left;"><%=FilesUCResource.ButtonYesRemove%></a>			
			</div>
		</body>
	</ascw:container>
</div>
