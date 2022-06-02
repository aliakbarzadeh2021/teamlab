<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Page Language="C#" AutoEventWireup="true"  MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master" CodeBehind="milestones.aspx.cs" Inherits="ASC.Web.Projects.Milestones" %>
<%@ Import Namespace="ASC.Web.Projects.Classes"%>
<%@ MasterType  TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>

<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<%@ Register Assembly="ASC.Web.Studio" Namespace="ASC.Web.Studio.Controls.Common"
    TagPrefix="ascwc" %>    

<%@ Import Namespace="ASC.Web.Studio.Controls.Common" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="BTHeaderContent" runat="server">
   
   <script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("milestones.js") %>"></script>
   <script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("tasks.js") %>"></script>
   <script src="<%= PathProvider.GetFileStaticRelativePath("timetracking.js") %>" type="text/javascript"></script>
   <link href="<%= PathProvider.GetFileStaticRelativePath("milestones.css") %>" type="text/css" rel="stylesheet" />
   <link href="<%= PathProvider.GetFileStaticRelativePath("common.css") %>" rel="stylesheet" type="text/css" />
   <link href="<%= PathProvider.GetFileStaticRelativePath("tasks.css") %>" type="text/css" rel="stylesheet" />
   <link href="<%= PathProvider.GetFileStaticRelativePath("timetracking.css") %>" rel="stylesheet" type="text/css" />
   <link href="<%= PathProvider.GetFileStaticRelativePath("dashboard.css")%>" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">
        
    <asp:PlaceHolder runat="server" ID="active_content"></asp:PlaceHolder>
    <asp:PlaceHolder runat="server" ID="closed_content"></asp:PlaceHolder>
    <asp:PlaceHolder ID="content" runat="server"></asp:PlaceHolder>
                               
</asp:Content>

<asp:Content ID="SidePanel" ContentPlaceHolderID="BTSidePanel" runat="server" >
     
    <ascwc:SideActions runat="server" ID="SideActionsPanel"> 
    </ascwc:SideActions>
    <ascwc:SideNavigator runat="server" ID="SideNavigatorPanel">
    </ascwc:SideNavigator>
    
</asp:Content>
