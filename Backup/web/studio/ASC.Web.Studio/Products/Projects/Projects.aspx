<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Page Language="C#" AutoEventWireup="true"  MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master" CodeBehind="projects.aspx.cs" Inherits="ASC.Web.Projects.Projects" %>
<%@ Import Namespace="ASC.Web.Projects.Classes"%>
<%@ MasterType  TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>

<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<%@ Register Assembly="ASC.Web.Studio" Namespace="ASC.Web.Studio.Controls.Common"
    TagPrefix="ascwc" %>  
    

<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="BTHeaderContent" runat="server">
   
   <script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("tasks.js") %>"></script>
   <script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("projects.js") %>"></script>
   <script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("milestones.js") %>"></script>
   <script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("reports.js") %>"></script>
   <script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("sorttable.js") %>"></script>
   <link href="<%= PathProvider.GetFileStaticRelativePath("projectTeam.css") %>" rel="stylesheet" type="text/css" />
   <link href="<%= PathProvider.GetFileStaticRelativePath("projects.css") %>" type="text/css" rel="stylesheet" />
   <link href="<%= PathProvider.GetFileStaticRelativePath("common.css") %>" type="text/css" rel="stylesheet" />
   <link href="<%= PathProvider.GetFileStaticRelativePath("tasks.css") %>" rel="stylesheet" type="text/css" />
   
</asp:Content>

<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">
           
    <asp:PlaceHolder runat="server" ID="_content"></asp:PlaceHolder>
                                      
</asp:Content>

<asp:Content ID="SidePanel" ContentPlaceHolderID="BTSidePanel" runat="server" >
    <ascwc:SideActions runat="server" ID="SideActionsPanel">
    </ascwc:SideActions>
    <ascwc:SideContainer runat="server" ID="_requestContainer">
    </ascwc:SideContainer>    
</asp:Content>
