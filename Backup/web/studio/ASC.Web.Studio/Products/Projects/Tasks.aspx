<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Page Language="C#" AutoEventWireup="true"  MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master"
 CodeBehind="tasks.aspx.cs" Inherits="ASC.Web.Projects.Tasks" %>
<%@ Import Namespace="ASC.Web.Projects.Classes"%>
<%@ MasterType  TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>

<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<%@ Register Assembly="ASC.Web.Studio" Namespace="ASC.Web.Studio.Controls.Common"
    TagPrefix="ascwc" %>  

<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<asp:Content ID="Content1" ContentPlaceHolderID="BTHeaderContent" runat="server">
   <script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("tasks.js") %>"></script>
   <script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("timetracking.js") %>"></script>
   <script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("reports.js") %>"></script>
   <link href="<%= PathProvider.GetFileStaticRelativePath("tasks.css") %>" type="text/css" rel="stylesheet" />
   <link href="<%= PathProvider.GetFileStaticRelativePath("timetracking.css")%>" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">
     <asp:PlaceHolder runat="server" ID="_content"></asp:PlaceHolder>
     <asp:PlaceHolder ID="timeSpendPlaceHolder" runat="server"></asp:PlaceHolder>   
</asp:Content>
<asp:Content ID="SidePanel" ContentPlaceHolderID="BTSidePanel" runat="server" >
 <ascwc:SideActions runat="server" ID="SideActionsPanel">

 </ascwc:SideActions>
 
</asp:Content>