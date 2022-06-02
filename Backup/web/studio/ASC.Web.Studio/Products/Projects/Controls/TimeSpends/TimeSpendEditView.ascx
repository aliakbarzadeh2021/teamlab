<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Core.Common" %>
<%@ Assembly Name="ASC.Common" %>


<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TimeSpendEditView.ascx.cs" Inherits="ASC.Web.Projects.Controls.TimeSpends.TimeSpendEditView" %>
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

    jq(function() {

        jq("[id$=editLogPanel_tbxDate]").mask('<%= System.DateTimeExtension.DateMaskForJQuery %>');
        jq('[id$=editLogPanel_tbxDate]').datepick({ selectDefaultDate: true, showAnim: '', popupContainer: "#editLogPanel" });
        jq("[id$=editLogPanel_tbxNote]").css('resize','none');

    });

</script>

<div id="editLogPanel" class="pm-hidden">

<ascwc:container id="_timetrackingContainer" runat="server">
    
    <header>    
        <%= ProjectsCommonResource.TimeTracking %>
    </header>
    
    <body>
        
        <div id="TimeLogTaskTitle" class="headerBase pm-headerPanelSmall-splitter pm-hidden"></div>
           
        <div class="infoPanel editLogPanel-infoPanel pm-hidden" id="TimeLogInfoPanel">
            <div class="editLogPanel-infoPanelBody">
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
        
        <div class="pm-headerPanelSmall-splitter" style="float:right;">
            <div class="headerPanelSmall">
                <b><%= TaskResource.TaskResponsible %>:</b>
            </div>
            <asp:DropDownList runat="server" ID="editLogPanel_ddlPerson"  CssClass="comboBox pm-report-select" Width="200px"/>
        </div>
        
        <div>
        <div class="pm-headerPanelSmall-splitter" style="float:left;margin-right:20px">
            <div class="headerPanelSmall">
                <b><%= ProjectsCommonResource.Time %>:</b>
            </div>
            <asp:TextBox runat="server" ID="editLogPanel_tbxHours"  CssClass="textEdit" Width="60px" MaxLength="5"
             onkeypress="javascript: return ASC.Projects.TimeSpendActionPage.keyPress(event,'editLogPanel_tbxHours');" />
            <span class="button-splitter"></span>
            <%= ProjectsCommonResource.Hours %>
        </div>
           
        <div class="pm-headerPanelSmall-splitter">
            <div class="headerPanelSmall">
                <b><%= ProjectsCommonResource.Date %>:</b>
            </div>
            <asp:TextBox runat="server" ID="editLogPanel_tbxDate" CssClass="pm-ntextbox textEditCalendar" style="margin-right: 3px" />
        </div>
        </div>   
       
        <div style="clear:both"></div>
           
        <div class="pm-headerPanelSmall-splitter">
            <div class="headerPanelSmall">
                <b><%= ProjectResource.ProjectDescription %>:</b>
            </div>
            <asp:TextBox runat="server" ID="editLogPanel_tbxNote"  TextMode="MultiLine" Rows="7"  CssClass="pm-ntextbox "  Width="99%"/>
        </div>
           
        <div class="pm-h-line" ><!– –></div>
           
        <div class="pm-action-block">
            <a href="javascript:void(0)" class="baseLinkButton">
                <%= ProjectsCommonResource.Save%>
            </a>
            <span class="button-splitter"></span>
            <a class="grayLinkButton" href="javascript:void(0)" onclick="PopupKeyUpActionProvider.CloseDialog();">
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