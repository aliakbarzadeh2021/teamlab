<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ChoiseOfTimeInterval.ascx.cs"
 Inherits="ASC.Web.Projects.Controls.Common.ChoiseOfTimeInterval" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascctl" %>   

<div style="width: 450px;">
    <div style="float:left">
        <select class="comboBox" style="width:190px;"  id="selectDate" onchange="ASC.Projects.TimeInterval.changeTimeRange('<%= System.DateTimeExtension.DateFormatPattern %>','<%= System.DateTimeExtension.DateMaskForJQuery %> ');">
                <option id="0" value="0"><%= ReportResource.Today %></option>
                <option id="1" value="1"><%= ReportResource.Yesterday%></option>
                <option id="2" value="2"><%= ReportResource.ThisWeek %></option>
                <option id="3" value="3" selected="selected"><%= ReportResource.LastWeek %></option>
                <option id="4" value="4"><%= ReportResource.ThisMonth %></option>
                <option id="5" value="5"><%= ReportResource.LastMonth %></option>
                <option id="6" value="6"><%= ReportResource.ThisYear %></option>
                <option id="7" value="7"><%= ReportResource.LastYear %></option>
                <option id="8" value="8"><%= ReportResource.Other %></option>
        </select>
    </div>
    
    <div>
        <div id="FromDate"  style="float:left; padding-left:15px; padding-right:15px">
            <asp:TextBox runat="server" ID="tbxFromDate" onkeypress="ASC.Projects.TimeInterval.keyPress(event);" CssClass="pm-ntextbox textEditCalendar" Width="75px" />
        </div>

        <div id="ToDate">
            <asp:TextBox runat="server" ID="tbxToDate" onkeypress="ASC.Projects.TimeInterval.keyPress(event);" CssClass="pm-ntextbox textEditCalendar" Width="75px" />
        </div>
    </div>
    
    <div style="clear:left">
    </div> 

    <script type="text/javascript">

        jq(function(){  
     
            var timeRange = jq("[id$=hiddenCurrentTimeRange]").val();

            jq("#"+timeRange).attr('selected','true');

            jq("[id$=tbxFromDate],[id$=tbxToDate]").mask('<%= System.DateTimeExtension.DateMaskForJQuery %>');
            
			  jq('[id$=tbxFromDate]').datepick(
						{
							onSelect:			function(dates){jq('[id$=tbxToDate]').datepick('option', 'minDate', dates[0] || null); ASC.Projects.TimeInterval.changeDateRange();},
							selectDefaultDate:	true,
							maxDate:			jq.datepick.parseDate(jq.datepick.dateFormat,jq('[id$=tbxToDate]').val()),
							showAnim:			''
						});
			  jq('[id$=tbxToDate]').datepick(
						{
							onSelect:			function(dates){jq('[id$=tbxFromDate]').datepick('option', 'maxDate', dates[0] || null); ASC.Projects.TimeInterval.changeDateRange();},
							selectDefaultDate:	true,
							minDate:			jq.datepick.parseDate(jq.datepick.dateFormat,jq('[id$=tbxFromDate]').val()),
							showAnim:			''
						});
        });

    </script>

    <asp:HiddenField ID="hiddenCurrentTimeRange" runat="server" />
</div>

