<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TimeLineView.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.Common.TimeLineView" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>


<asp:Repeater runat="server" ID="rptContainer" OnItemDataBound="rptContent_OnItemDataBound">
    <HeaderTemplate>
        <table class="pm-tablebase pm-headerPanel-splitter" cellpadding="12" cellspacing="0">
        <% if (RenderHeader) %>
        <%{%>
         <thead>
            <tr>
              <td>
                    <%= ProjectsCommonResource.Time %>
              </td>
              <td>
                    <%= ProjectsCommonResource.Type %>
              </td>
              <td>
                    <%= ProjectsCommonResource.NewsTitle %>
              </td>              
            </tr>
         </thead>
         <%}%>    
          <tbody>           
    </HeaderTemplate>
    <ItemTemplate> 
            <asp:Literal runat="server" ID="_ltlDate"></asp:Literal>            
            <tr class=" <%# Container.ItemIndex%2==0 ? "tintMedium" : "tintLight" %>">
                <td class="borderBase textBigDescribe" style="white-space:nowrap;">
                    <%# GetTime((DateTime)Eval("Date"))%>
                </td>
                <td class="borderBase">
                    <%# Eval("EntityPlate")%>
                </td>                               
                <td class="borderBase">
                    <% if(ShowActivityDate) { %><div style="width: 480px; overflow: hidden;"><% } %> 
                    <% else { %><div style="width: 530px; overflow: hidden;"><% } %> 
                        <%# Eval("Message")%>
                        <span class="splitter"></span>
                        <%# Eval("Author")%>
                    </div>
                </td>    
            </tr>         
    </ItemTemplate>   
    <FooterTemplate>
        </tbody>
       </table>
    </FooterTemplate>
</asp:Repeater>


