<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TaskBlockTemplateView.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.ProjectTemplates.Tasks.TaskBlockTemplateView" %>
<%@ Import Namespace="ASC.Web.Projects.Classes"%>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>

<%@ Register Src="TaskBlockViewTemplateRow.ascx" TagPrefix="ctrl" TagName="TaskBlockViewRow" %>

<style type="text/css" >
.pm-task-group-action-block
{
  padding-top:15px;
}
</style>


<asp:Repeater runat="server" ID="rptContent" OnItemDataBound="rptContent_OnItemDataBound">
    <HeaderTemplate>
        <table class="pm-tablebase pm-tasks-block" cellpadding="10" cellspacing="0" style="width: 100%">
            <thead>
                <tr>                   
                    <td  class="borderBase"style="padding-left: 39px;">
                        <%= TaskResource.TaskTitle%>                        
                    </td>
                   
                    <td class="borderBase" style="width:150px;">
                        <%= TaskResource.TaskResponsible%>
                    </td>
                </tr>
            </thead>
            <tbody>
              </tbody> 
      </table> 
            <div id="pm_openTaskContainer_<%=BlockMilestone!=null?BlockMilestone.Id : 0%>_<%=Template.Id%>">
    </HeaderTemplate>
    <ItemTemplate>
        <ctrl:TaskBlockViewRow runat="server" ID="_taskBlockViewRow" />
    </ItemTemplate>
    <FooterTemplate>
           </div>
    </FooterTemplate>
</asp:Repeater>

<%--<div style="padding-top:10px;">
    <a style="float:right;background-image: none;" class="grayLinkButton" href="javascript:void(0)" onclick="javascript:ASC.Projects.TaskActionPage.init(-1,<%=BlockMilestone != null ? BlockMilestone.Id.ToString() : "0"%>, <%=UrlParameters.UserID != Guid.Empty
                           ? String.Concat("'", UrlParameters.UserID.ToString(), "'")
                           : "null"%>); ASC.Projects.TaskActionPage.show()">
        <img align="absmiddle" style="border: 0px none; margin: 1px;" src="<%=WebImageSupplier.GetAbsoluteWebPath("plus.png", ProductEntryPoint.ID)%>" />
        <%= TaskResource.AddNewTask %>
    </a>    
</div>--%>







