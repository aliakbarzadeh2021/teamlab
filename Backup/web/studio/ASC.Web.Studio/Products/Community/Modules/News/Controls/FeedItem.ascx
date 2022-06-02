<%@ Assembly Name="ASC.Web.Community.News" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FeedItem.ascx.cs" Inherits="ASC.Web.Community.News.Controls.FeedItem" %>
<td valign="top" style="padding-top:0px;">
    <table width="100%" border="0" cellpadding="0" cellspacing="0">
        <tr>
            <td align="right" valign="top" style="width: 40px; padding:0px 2px 0px 0px;">
                <asp:Literal runat="server" ID="Type"></asp:Literal>
            </td>
            <td valign="top" style="padding: 10px 20px 10px 2px;">
                <asp:HyperLink runat="server" ID="NewsLink" style="width: 390px; display: block;" class="longWordsBreak">
                </asp:HyperLink>
            </td>
        </tr>
    </table>
</td>
<td valign="top" class="newsDate" style="padding-right:20px;">
    <asp:Label runat="server" ID="Date"></asp:Label>
</td>
<td valign="top" >
    <asp:Literal runat="server" ID="profileLink"></asp:Literal>
</td>
