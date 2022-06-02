<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TimeSpendRecord.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.TimeSpends.TimeSpendRecord" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<div id="timeSpendRecord<%= GetID()%>" class="<%= CssClass %>" onmouseover="ASC.Projects.TimeSpendActionPage.showActions(<%= GetID()%>);" onmouseout="ASC.Projects.TimeSpendActionPage.hideActions(<%= GetID()%>);">
<table width="100%" cellpadding="5" cellspacing="0" class="pm-tablebase pm-tablebase-timeTracking">

    <tr height="36px">
        <td class="borderBase pm-ts-dateColumn"  style="border-top:0px !important">
            <span id="date_ts<%= GetID()%>" class="pm-grayText">
                <%= TimeSpend.Date.ToLocalTime().ToString(DateTimeExtension.DateFormatPattern)%>
            </span>
        </td>
        <td class="borderBase pm-ts-personColumn" style="border-top:0px !important">
            <span id="person_ts<%= GetID()%>">
                <%= ProfileLink()%>
            </span>
        </td>
        <td class="borderBase pm-ts-hoursColumn" style="border-top:0px !important">
            <strong id="hours_ts<%= GetID()%>">
                <%=  TimeSpend.Hours.ToString("N2")%>
            </strong>
        </td>
        <td class="borderBase pm-ts-noteColumn" title="<%=  GetTitle()%>" style="border-top:0px !important">
            <div id="note_ts<%= GetID()%>" style="width:340px;overflow:hidden;">
                <%=  GetNote()%>
            </div>
        </td>
        <td class="borderBase pm-ts-actionsColumn" style="border-top:0px !important">
            <div id="ts_actions<%= GetID()%>" style="display:none;">
                <%= GetAction()%>
            </div>
        </td>
    </tr>

</table>
</div>