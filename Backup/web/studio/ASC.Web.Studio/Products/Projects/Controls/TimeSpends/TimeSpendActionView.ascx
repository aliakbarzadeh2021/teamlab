<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Common" %>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TimeSpendActionView.ascx.cs" Inherits="ASC.Web.Projects.Controls.TimeSpends.TimeSpendActionView" %>
<%@ Import Namespace="ASC.Web.Projects.Classes"%>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Projects.Core" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="System" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascctl" %> 


<script>

    jq(function(){  
  
                    jq("[id$=addLogPanel_tbxDate]").mask('<%= System.DateTimeExtension.DateMaskForJQuery %>');
                    jq('[id$=addLogPanel_tbxDate]').datepick({ selectDefaultDate: true, showAnim: '', popupContainer: "#addLogPanel" });
                    jq("[id$=addLogPanel_tbxNote]").css('resize','none');
                 });

</script>

<div id="addLogPanel" class="pm-hidden">

<ascwc:container id="_timetrackingContainer" runat="server">
    
    <header>    
        <%= ProjectsCommonResource.TimeTracking %>
    </header>
    
    <body>
        
        <div id="TimeLogTaskTitle" class="headerBase pm-headerPanelSmall-splitter"></div>
           
        <div class="infoPanel addLogPanel-infoPanel">
            <div class="addLogPanel-infoPanelBody">
                <span class="headerBase pm-grayText">
                    <%= ProjectsCommonResource.SpentTotally %>
                </span>
                <span class="button-splitter"></span>
                <span id="TotalHoursCount" class="headerBase"></span>
                <span class="button-splitter"></span>
                <span class="headerBase pm-grayText">
                    <%= ProjectsCommonResource.Hours %>
                </span>
            </div>
        </div> 
        
        <div class="pm-headerPanelSmall-splitter" style="float:right">
            <div class="headerPanelSmall">
                <b><%= TaskResource.TaskResponsible %>:</b>
            </div>
            <select style="width: 200px;" class="comboBox pm-report-select" id="addLogPanel_ddlPerson"></select>
        </div>
        
        <div>   
        <div class="pm-headerPanelSmall-splitter" style="float:left;margin-right:20px">
            <div class="headerPanelSmall">
                <b><%= ProjectsCommonResource.AddTime %>:</b>
            </div>
            <asp:TextBox runat="server" ID="addLogPanel_tbxHours" onkeypress="javascript: return ASC.Projects.TimeSpendActionPage.keyPress(event,'addLogPanel_tbxHours','<%=System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator %>');"  CssClass="textEdit" Width="60px" MaxLength="5"/>
            <span class="button-splitter"></span>
            <%= ProjectsCommonResource.Hours %>
        </div>
           
        <div class="pm-headerPanelSmall-splitter">
            <div class="headerPanelSmall">
                <b><%= ProjectsCommonResource.Date %>:</b>
            </div>
            <asp:TextBox runat="server" ID="addLogPanel_tbxDate" CssClass="pm-ntextbox textEditCalendar" style="margin-right: 3px"/>
            <ascctl:TreeViewProScriptManager runat="server" ID="ascctlTreeViewProScriptManager" />
        </div>
        </div>
        
        <div style="clear:both"></div>
           
        <div class="pm-headerPanelSmall-splitter">
            <div class="headerPanelSmall">
                <b><%= ProjectResource.ProjectDescription %>:</b>
            </div>
            <asp:TextBox runat="server" ID="addLogPanel_tbxNote"  TextMode="MultiLine" Rows="7"  CssClass="pm-ntextbox "  Width="99%"/>
        </div>
           
        <div class="pm-h-line" ><!– –></div>
           
        <div class="pm-action-block">
            <a href="javascript:void(0)" class="baseLinkButton">
                <%= ProjectsCommonResource.LogTime%>
            </a>
            <span class="button-splitter"></span>
            <a class="grayLinkButton" href="javascript:void(0)" onclick="javascript: jq.unblockUI();">
                <%= ProjectsCommonResource.Cancel%>
            </a>
        </div>
            
        <div class="pm-ajax-info-block" style="display: none;">
            <span class="textMediumDescribe">
                <%= ProjectsCommonResource.Saving %></span><br />
            <img src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif")  %>" />
        </div>
        
        <input id="NumberDecimalSeparator" type="hidden" value="<%= System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.ToString() %>" />
            
    </body>
</ascwc:container>

</div>
