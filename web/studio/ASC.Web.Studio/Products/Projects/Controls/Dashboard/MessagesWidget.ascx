<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MessagesWidget.ascx.cs"
 Inherits="ASC.Web.Projects.Controls.Dashboard.MessagesWidget" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>


<div class="pm-dashboard-fadeParent">
<table cellpadding="3">
    <asp:Repeater ID="MessagesRepeater" runat="server">
    <ItemTemplate>
    <tr>
        <td valign="top">
            <div>
                <img src="<%#(Container.DataItem as MessageVM).CreatedByAvatarUrl%>"/>
            </div>   
            <div class="pm-dashboard-MessagesWidget-dateTimeContainer">
                <div class="pm-dashboard-bottomIndent">
                    <%#(Container.DataItem as MessageVM).CreatedDateString%>
                </div>    
                <div class="pm-dashboard-bottomIndent">
                    <%#(Container.DataItem as MessageVM).CreatedTimeString%>
                </div>    
            </div>
        </td>
        <td valign="top">
                    <div class="pm-dashboard-bottomIndent" style="white-space:nowrap;">
                        <div class="pm-dashboard-fade"></div>
                        <div style="white-space:nowrap; max-width:190px; overflow:hidden; float:left;">
                            <a href="<%#(Container.DataItem as MessageVM).MessageUrl %>" title="<%# (Container.DataItem as MessageVM).Message.Title.HtmlEncode() %>">
                                <%# ASC.Web.Controls.HtmlUtility.GetText((Container.DataItem as MessageVM).Message.Title, 26).HtmlEncode()%>
                            </a>&nbsp
                        </div>
                        <span class='pm-dashboard-leftIndent-small <%#((Container.DataItem as MessageVM).IsReaded)?"pm-grayText":"pm-redText"%>'>(<%#(Container.DataItem as MessageVM).CommentCount %>)</span>
                    </div>
                             
                    <div class="pm-dashboard-bottomIndent" style="white-space:nowrap;">
                        <div class="pm-dashboard-fade"></div>
                        <%#ASC.Web.Controls.HtmlUtility.GetText((Container.DataItem as MessageVM).Message.Content, 120).HtmlEncode()%>
                    </div>
                    
                    <div class="pm-dashboard-bottomIndent" style="white-space:nowrap;">
                        <div class="pm-dashboard-fade"></div>
                        <span><%=ProjectResource.Project %>:</span>
                        <a class="linkAction pm-dashboard-leftIndent-small" title="<%#(Container.DataItem as MessageVM).Message.Project.Title.HtmlEncode()%>"
                         href="projects.aspx?prjID=<%#(Container.DataItem as MessageVM).Message.Project.ID%>">
                            <%#(Container.DataItem as MessageVM).Message.Project.Title.HtmlEncode()%>
                        </a> 
                    </div>
                
                    <div class="pm-dashboard-bottomIndent" style="overflow:hidden;white-space:nowrap;">
                        <div class="pm-dashboard-fade"></div>
                        <span><%=MessageResource.From %>:</span>
                        <span class="pm-dashboard-leftIndent-small"><%#(Container.DataItem as MessageVM).CreatedByProfileLink%></span>
                    </div>
        </td>
    </tr>
    </ItemTemplate>
    </asp:Repeater> 
</table>
</div>
