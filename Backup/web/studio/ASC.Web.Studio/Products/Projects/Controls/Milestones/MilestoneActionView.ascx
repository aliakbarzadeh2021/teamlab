<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MilestoneActionView.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.Milestones.MilestoneActionView" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>

<script>

    var dateFormat = '<%= System.DateTimeExtension.DateFormatPattern %>';
    var isFirst=true;

    jq(document).ready(function()
    {	
    //var mydata = {table:Days};

    //jq("#divJTemplate").setTemplateElement("myTemplateTable", null, {filter_data: false});
    //jq("#divJTemplate").processTemplate(mydata);
    
    ASC.Projects.Milestones._getDate();
    
    jq("#shift").removeAttr("checked");
    jq("#moveOutWeekend").removeAttr("checked");
    
    var date = ASC.Projects.Milestones._getCurrentPageDate();
    ASC.Projects.Milestones.drawMonth(date.getFullYear(),date.getMonth());
    
    
    ASC.Projects.Milestones._init();

    });

</script>

<div id="page_content">

    <div class="pm-milestone-bottomIndent">
        <%=MilestoneResource.ChooseYourMilestoneDate %>:
    </div>
    
    <div class="pm-milestones-rightColumn">
    
        <div class="pm-headerPanel-splitter">
            <%=MilestoneResource.MilestoneDate %>:<span class="splitter"></span>
            <asp:Label ID="Date" runat="server" Text="Label" Font-Size="Large"></asp:Label>
        </div>
        <div class="pm-headerPanel-splitter">
            <div class="headerPanel"><%=MilestoneResource.MilestoneTitle %></div>
            <asp:TextBox CssClass="textEdit" ID="tbxTitle" Width="400px" runat="server" MaxLength="100" />
            <div class="textBigDescribe"><%=MilestoneResource.Example %></div>
        </div>

        <div id="shiftMilestonesBox" class="pm-headerPanel-splitter pm-hidden">
            <div class="headerPanel"><%=MilestoneResource.ShiftMilestonesQuestion%></div>
            <div class="pm-milestone-bottomIndent"><%=MilestoneResource.ShiftMilestonesDescription%></div>
            <div class="pm-milestone-bottomIndent">
                <input id="shift" type="checkbox"/>&nbsp
                <label for="shift"> <%=MilestoneResource.ShiftMilestonesAnswer%></label>
            </div>    
            <div>
                <input id="moveOutWeekend" type="checkbox"  />&nbsp
                <label for="moveOutWeekend"> <%=MilestoneResource.KeepShiftedMilestoneOffWeekend%></label>
            </div>    
        </div>
        
        <div class="pm-milestone-bottomIndent">
            <input id="notify_manager" checked='checked' type="checkbox" />&nbsp
            <label for="notify_manager"> <%= MilestoneResource.RemindMe %></label>
        </div>
        <div>
            <input id="is_key" type="checkbox" <%= IsKey %> />&nbsp
            <label for="is_key"> <%= MilestoneResource.RootMilestone%></label>
        </div>
        
        <div class="pm-h-line"><!– –></div>
        
        <div id="actions">
            <a href="javascript:void(0)" onclick="ASC.Projects.Milestones.addNewMilestone();" class="baseLinkButton">
                <%= ActionType=="add"? MilestoneResource.AddThisMilestone : ProjectsCommonResource.SaveChanges%>
            </a>
                
            <span class="button-splitter"></span>
            <% if (Milestone.ID > 0) %>
            <% { %>
            <a class="grayLinkButton" href="milestones.aspx?prjID=<%=ProjectFat.Project.ID %>&ID=<%=Milestone.ID %>">
                <%= ProjectsCommonResource.Cancel%>
            </a>
            <% } %>
            <% else %>
            <% { %>
            <a class="grayLinkButton" href="milestones.aspx?prjID=<%=ProjectFat.Project.ID %>">
                <%= ProjectsCommonResource.Cancel%>
            </a>
            <% } %>
        </div>
        
        <div id='info-block' class='pm-ajax-info-block pm-hidden'>
            <span class="textMediumDescribe"><%= ActionType == "add" ? MilestoneResource.MilestoneAdded : MilestoneResource.SaveMilestone%></span>
            <br />
            <img src="<%=WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif")  %>" />
        </div>
        
    </div>

    <div id="pm-calendar-container" class="borderLight pm-headerPanel-splitter pm-milestones-leftColumn">
        <div class="pm-calendar-header">
            <img alt='<%=ProjectsCommonResource.NextMonth %>' title="<%=ProjectsCommonResource.NextMonth %>"
                onclick="javascript:ASC.Projects.Milestones._next();" align="absmiddle" id="nextImg"
                style="cursor: pointer; float: right" src="<%=WebImageSupplier.GetAbsoluteWebPath("right.gif", ProductEntryPoint.ID)   %>" />
            <div>
                <img alt='<%=ProjectsCommonResource.PrevMonth %>' title="<%=ProjectsCommonResource.PrevMonth %>"
                    onclick="javascript:ASC.Projects.Milestones._prev();" align="absmiddle" id="prevImg"
                    style="cursor: pointer; float: left" src="<%=WebImageSupplier.GetAbsoluteWebPath("left.gif", ProductEntryPoint.ID)   %>" />
                <div style="text-align: center" id="month_year">
                </div>
            </div>
        </div>
        
        <!--<div id="divJTemplate">&nbsp;</div>-->
        <div id="divTemplateContainer">&nbsp;</div>
        
        <input id="day" value="<%= ChoosenDate.Day %>" type="hidden" />
        <input id="month" value="<%= ChoosenDate.Month %>" type="hidden" />
        <input id="year" value="<%= ChoosenDate.Year %>" type="hidden" />
        <input id="oldDeadline" value="<%= Milestone.DeadLine.ToString(DateTimeExtension.DateFormatPattern) %>" type="hidden" />
    </div>
    
    <div class="pm-milestones-leftColumn">
        <div class="pm-milestone-bottomIndent">
        <%=MilestoneResource.Legend%>:
        </div>
        <div class="pm-milestone-legendRow">
            <img align="absmiddle" src="<%=WebImageSupplier.GetAbsoluteWebPath("milestone_status_active_16.png", ProductEntryPoint.ID)%>"
            alt="<%=MilestoneResource.OpenMilestone %>" title="<%=MilestoneResource.OpenMilestone %>" />
            <span class="button-splitter"></span>
            <%=MilestoneResource.OpenMilestone %>
        </div>
        <div class="pm-milestone-legendRow">
            <img align="absmiddle" src="<%=WebImageSupplier.GetAbsoluteWebPath("milestone_status_late_16.png", ProductEntryPoint.ID)%>"
            alt="<%=MilestoneResource.LateMilestone %>" title="<%=MilestoneResource.LateMilestone %>"  />
            <span class="button-splitter"></span>
            <%=MilestoneResource.LateMilestone %>
        </div>
        <div class="pm-milestone-legendRow">
            <img align="absmiddle" src="<%=WebImageSupplier.GetAbsoluteWebPath("milestone_status_completed_16.png", ProductEntryPoint.ID)%>"
            alt="<%=MilestoneResource.ClosedMilestone %>" title="<%=MilestoneResource.ClosedMilestone %>"  />
            <span class="button-splitter"></span>
            <%=MilestoneResource.ClosedMilestone %>
        </div>
    </div>

    <div style="clear: both"></div>
    
</div>

<p style="display:none">

<textarea id="myTemplateTable" rows="0" cols="0">    

<table class="pm-tablebase borderLight" id="pm-calendar" border="1" cellpadding="0" cellspacing="0">
    <thead>
    <tr>
        <td><%=Days[0]%></td>
        <td><%=Days[1]%></td>
        <td><%=Days[2]%></td>
        <td><%=Days[3]%></td>
        <td><%=Days[4]%></td>
        <td><%=Days[5]%></td>
        <td><%=Days[6]%></td>
    </tr>
    </thead>
    <tbody>
    <tr>
    {#foreach $T.table as day}
        <td class="{$T.day.cssClass}" style="z-index: {$T.day.index};" onclick="ASC.Projects.Milestones._click(this);" id="{$T.day.index}">
            {$T.day.dayNumber}
            
            {#if $T.day.popupEvents != ""}
            
            <div class='pm-events'>
            <div style='bottom: 3px; left: -10px; display: none; opacity: 0;' class='pm-events-child' id='popup'>
            <div class='pm-events-child-container'>
            <div style='width: 200px;' class='pm-title  pm-dashboard-fadeParent'>
            
            {$T.day.popupEvents}
            
            </div>
            </div>
            <div class='pm-events-zub-img'>
            <img src='<%=WebImageSupplier.GetAbsoluteWebPath("zub.gif", ProductEntryPoint.ID) %>'></div>
            </div>
            </div>

            {#/if}
        </td>   
        {#if $T.day.index % 7 == 0}
        </tr>
        {#/if}
    {#/for}   
    </tbody>
</table>
  
</textarea>


<textarea id="templateContainer" rows="0" cols="0">    

<table class="pm-tablebase borderLight" id="pm-calendar" border="1" cellpadding="0" cellspacing="0" style="height:182px;width:273px;">
    <thead>
    <tr>
        <td><%=Days[0]%></td>
        <td><%=Days[1]%></td>
        <td><%=Days[2]%></td>
        <td><%=Days[3]%></td>
        <td><%=Days[4]%></td>
        <td><%=Days[5]%></td>
        <td><%=Days[6]%></td>
    </tr>
    </thead>
    <tbody>
    <tr>
    {#foreach $T.daysOfMonth as day}
        <td class="{$T.day.cssClass}" style="z-index: {$T.day.index};" onclick="ASC.Projects.Milestones._click(this);" id="{$T.day.index}">
            {$T.day.dayNumber}
            {#foreach $T.events as event}
            {#if $T.event.dayNumber == $T.day.dayNumber && $T.event.monthNumber == $T.day.monthNumber && $T.event.yearNumber == $T.day.yearNumber}
            <div class='pm-events'>
            <div style='bottom: 3px; left: -10px; display: none; opacity: 0;' class='pm-events-child' id='popup'>
            <div class='pm-events-child-container'>
            <div style='width: 200px;' class='pm-title  pm-dashboard-fadeParent'>
            
            {$T.event.popupEvents}
            
            </div>
            </div>
            <div class='pm-events-zub-img'>
            <img src='<%=WebImageSupplier.GetAbsoluteWebPath("zub.gif", ProductEntryPoint.ID) %>'></div>
            </div>
            </div>
            {#/if}
            {#/for}
        </td>   
        {#if $T.day.index % 7 == 0}
        </tr>
        {#/if}
    {#/for}   
    </tbody>
</table>
  
</textarea>

</p>