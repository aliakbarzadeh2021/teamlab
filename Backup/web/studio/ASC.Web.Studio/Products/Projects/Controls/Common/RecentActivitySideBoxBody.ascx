<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RecentActivitySideBoxBody.ascx.cs"
    Inherits="ASC.Web.Projects.Controls.Common.RecentActivitySideBoxBody" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Core.Users.Activity" %>
<%@ Import Namespace="ASC.Core.Users" %>

<asp:Repeater runat="server" ID="rptContent">
    <HeaderTemplate>
    </HeaderTemplate>
    <ItemTemplate>
        <div class="pm-activityContainer">
            <%# StudioUserInfoExtension.RenderProfileLink(ASC.Core.CoreContext.UserManager.GetUsers((Container.DataItem as UserActivity).UserID), ProductEntryPoint.ID)%>
            <div style="overflow: hidden;">
            <span class="pm-grayText"><%# (Container.DataItem as UserActivity).ActionText.ToLower()%> &laquo;<%# GetEntityType(Container.DataItem as UserActivity)%>&raquo;</span>
            <br />
            <a href="<%#(Container.DataItem as UserActivity).URL.HtmlEncode()%>" title="<%# (Container.DataItem as UserActivity).Title.HtmlEncode().ReplaceSingleQuote()%>">
                <%# ASC.Web.Controls.HtmlUtility.GetText((Container.DataItem as UserActivity).Title, 20).HtmlEncode().ReplaceSingleQuote()%>
            </a>               
            </div>
        </div>
    </ItemTemplate>
    <FooterTemplate>
    </FooterTemplate>
</asp:Repeater>
