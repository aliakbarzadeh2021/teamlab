<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VisitedUsers.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Users.VisitedUsers" %>
<%@ Import Namespace="ASC.Web.Controls" %>

<div style="margin: 10px 0px; padding: 5px;" class="borderBase">
    <asp:Repeater ID="rpVisitedUsers" runat="server">
        <HeaderTemplate>
            <table cellpadding="3" cellspacing="3">
                <tr>
                    <td style="white-space: nowrap; vertical-align: top; font-family: Arial; font-size: 18px;
                        color: #2A2A2A;">
                        <asp:Literal ID="ltUserVisitedCount" runat="server" Text='<%# string.Format("{0} ({1}):", Resources.Resource.Visited, UserVisitedCount) %>' />
                    </td>
                    <td style="padding-top: 8px; vertical-align: top;">
                    <div class="clearFix">
        </HeaderTemplate>
        <ItemTemplate>
            <div style="padding: 0px 10px 7px 0px; float:left;">
                <asp:Literal ID="ltProfile" runat="server" Text='<%# RenderProfileLink(((ASC.Core.Users.UserInfo)Container.DataItem)) %>' />
            </div>
        </ItemTemplate>
        <FooterTemplate>
            </div></td> </tr> </table></FooterTemplate>
    </asp:Repeater>
</div>
