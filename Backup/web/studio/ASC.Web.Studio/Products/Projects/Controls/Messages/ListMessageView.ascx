<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListMessageView.ascx.cs" Inherits="ASC.Web.Projects.Controls.Messages.ListMessageView" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<%@ Register Assembly="FredCK.FCKeditorV2" Namespace="FredCK.FCKeditorV2" TagPrefix="FCKeditorV2" %>

<style type='text/css'>
div.pm-h-line
{
	margin:0px;
	padding:0px;
}
</style>

<asp:Repeater runat="server" ID="rptContent" >
    <HeaderTemplate>
        <div class="pm-h-line"><!– –></div>
    </HeaderTemplate>
    <ItemTemplate>
        <div class="<%# Container.ItemIndex%2==0 ? "tintMedium" : "tintLight" %>">
        <%#GetMessage(Eval("Message") as Message)%>
        </div>
    </ItemTemplate>
    <SeparatorTemplate>
     <div class="pm-h-line"><!– –></div>
    </SeparatorTemplate>
    <FooterTemplate>
        <div class="pm-h-line"><!– –></div>
    </FooterTemplate>
</asp:Repeater>
<div>
<br />
<asp:PlaceHolder ID="phContent" runat="server"></asp:PlaceHolder>
</div>