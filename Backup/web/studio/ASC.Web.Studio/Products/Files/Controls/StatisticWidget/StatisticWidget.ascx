<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StatisticWidget.ascx.cs"
    Inherits="ASC.Web.Files.Controls.StatisticWidget" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<% #if (DEBUG) %>
    <link href="<%= String.Concat(ASC.Web.Files.Classes.PathProvider.BaseAbsolutePath, "Controls/StatisticWidget/StatisticWidget.css" )%>"
        type="text/css" rel="stylesheet" />
<% #endif %>

<div class="clearFix quotaContainer">
    <div class="quotaUsage"><%=FilesUCResource.QuotaUsage%>:</div>
    <div class="quotaValue"></div>
</div>
<div id="quotaBarContainer" class="quotaProgressBorder">
	<div class="quotaProgressBar" style="width: 0px"></div>
</div>
