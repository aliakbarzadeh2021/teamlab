<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>

<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master"
    CodeBehind="history.aspx.cs" Inherits="ASC.Web.Projects.History" %>
<%@ Import Namespace="ASC.Web.Projects.Classes"%>
<%@ MasterType TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>

<%@ Register Src="~/Products/Projects/Controls/Common/TimeLineView.ascx" TagPrefix="ctrl" TagName="TimeLineView" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascctl" %>   

<asp:Content ID="HeaderContent" ContentPlaceHolderID="BTHeaderContent" runat="server">

    <script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("history.js") %>"></script>

    <link href="<%= PathProvider.GetFileStaticRelativePath("history.css") %>" rel="stylesheet" type="text/css" />

    <script type="text/javascript" language="javascript">

     jq(function(){  
     
      jq("[id$=ApplyFilterButton]").addClass('baseLinkButton promoAction');

      var timeRange = jq("[id$=hiddenCurrentTimeRange]").val();

      jq("#"+timeRange).attr('selected','true');
      
      jq("[id$=tbxStartDate],[id$=tbxFinishDate]").mask('<%= System.DateTimeExtension.DateMaskForJQuery %>');

      jq('[id$=tbxStartDate]').datepick(
				{
					onSelect:			function(dates){jq('[id$=tbxFinishDate]').datepick('option', 'minDate', dates[0] || null); ASC.Projects.History.changeDateRange();},
					selectDefaultDate:	true,
					maxDate:			jq.datepick.parseDate(jq.datepick.dateFormat,jq('[id$=tbxFinishDate]').val()),
					showAnim:			''
				});
	  jq('[id$=tbxFinishDate]').datepick(
				{
					onSelect:			function(dates){jq('[id$=tbxStartDate]').datepick('option', 'maxDate', dates[0] || null); ASC.Projects.History.changeDateRange();},
					selectDefaultDate:	true,
					minDate:			jq.datepick.parseDate(jq.datepick.dateFormat,jq('[id$=tbxStartDate]').val()),
					showAnim:			''
				});
     });
 
    </script>
    
</asp:Content>
<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">

    <div style="margin-top: 0px;" class="pm-h-line"><!– –></div>
    
    <div id="historyFilters">
    
    <div style="float:left">
        <dl>
            <dt class="textBigDescribe"><%= ReportResource.ChooseTimeInterval %>: </dt>
            <dd>
                <select class="comboBox" style="width:190px;" id="selectDate" onchange="ASC.Projects.History.changeTimeRange();">
                <option id="0" value="0"><%= ReportResource.Today %></option>
                <option id="1" value="1"><%= ReportResource.Yesterday%></option>
                <option id="2" value="2" selected="selected"><%= ReportResource.ThisWeek %></option>
                <option id="3" value="3"><%= ReportResource.LastWeek %></option>
                <option id="4" value="4"><%= ReportResource.ThisMonth %></option>
                <option id="5" value="5"><%= ReportResource.LastMonth %></option>
                <option id="6" value="6"><%= ReportResource.ThisYear %></option>
                <option id="7" value="7"><%= ReportResource.LastYear %></option>
                <option id="8" value="8"><%= ReportResource.Other %></option>
            </select>
            <asp:HiddenField ID="hiddenCurrentTimeRange" runat="server" />
            </dd>
        </dl> 
    </div>
    
    <div>
            <div id="StartDate"  style="float:left; padding-left:15px; padding-right:15px">
                <asp:TextBox runat="server" ID="tbxStartDate" onkeypress="ASC.Projects.History.keyPress(event);" CssClass="pm-ntextbox textEditCalendar" Width="75px" />
            </div>

            <div id="FinishDate">
                <asp:TextBox runat="server" ID="tbxFinishDate" onkeypress="ASC.Projects.History.keyPress(event);" CssClass="pm-ntextbox textEditCalendar" Width="75px" />
            </div>
    </div>
    
    <div id="additionalOptions" style="clear:left">
        <dl>
            <dt class="textBigDescribe"><%=ReportResource.User %>: </dt>
            <dd>
                <asp:DropDownList ID="ddlUsers" runat="server" class="comboBox" Width="190px">
                </asp:DropDownList>
            </dd>
        </dl>

        <dl>
            <dt class="textBigDescribe"><%=ProjectsCommonResource.Type %>: </dt>
            <dd>
                <asp:DropDownList ID="ddlModule" runat="server" class="comboBox" Width="190px">
                </asp:DropDownList>
            </dd>
        </dl>
    </div> 
    
    </div>
    
    <div class="pm-h-line" style="margin-top:10px"><!– –></div>
 
    <asp:Button ID="ApplyFilterButton" OnClientClick="javascript: if(!ASC.Projects.History.validDate()) {alert(ASC.Projects.Resources.IncorrectDate);return false; } " OnClick="ApplyFilterButton_Click" runat="server" Text="" Height="20px" />

    <ascctl:TreeViewProScriptManager runat="server" ID="ascctlTreeViewProScriptManager" />
    
    <script type='text/javascript'>
    var timeRange = jq("[id$=hiddenCurrentTimeRange]").val();
    jq("#"+timeRange).attr('selected','true');
    </script>

    <div id="container">
        <asp:PlaceHolder ID="_content" runat="server"></asp:PlaceHolder>
    </div>
    
    <div id="EmptyScreenContainer" runat="server" visible="false">
    </div>

</asp:Content>
<asp:Content ID="SidePanel" ContentPlaceHolderID="BTSidePanel" runat="server">
</asp:Content>
