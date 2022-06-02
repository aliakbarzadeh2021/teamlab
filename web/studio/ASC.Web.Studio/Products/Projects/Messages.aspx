<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Page Language="C#"  AutoEventWireup="true" EnableViewState="true" EnableViewStateMac="true"  MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master"
 CodeBehind="messages.aspx.cs" Inherits="ASC.Web.Projects.Messages" %>
<%@ Import Namespace="ASC.Web.Projects.Classes"%>
<%@ MasterType  TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<%@ Register Assembly="ASC.Web.Studio" Namespace="ASC.Web.Studio.Controls.Common"
    TagPrefix="ascwc" %>  
    
<asp:Content ID="HeaderContent" ContentPlaceHolderID="BTHeaderContent" runat="server">
   
   <script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("messages.js") %>"></script>
   <link href="<%= PathProvider.GetFileStaticRelativePath("messages.css") %>"type="text/css" rel="stylesheet" />   

</asp:Content>

<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">
    <asp:PlaceHolder ID="_content" runat="server"></asp:PlaceHolder>
</asp:Content>

<asp:Content ID="SidePanel" ContentPlaceHolderID="BTSidePanel" runat="server" >
    <ascwc:SideActions runat="server" ID="SideActionsPanel">
    </ascwc:SideActions>
</asp:Content>
