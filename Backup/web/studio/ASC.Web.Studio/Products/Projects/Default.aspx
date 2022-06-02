<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs"
 MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master" Inherits="ASC.Web.Projects.Dashboard" %>
<%@ Import Namespace="ASC.Web.Projects.Classes"%>
<%@ MasterType  TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>

<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="BTHeaderContent" runat="server">

    <script language="javascript">
    function getLeftByElement(elem)
    {
        var result = 0;
        var parent = elem.parentNode;
        var pWidth = 0;
        while(parent)
        {
            if(parent.className == 'studioWidgetBox')
            {
                result = 0;
                pWidth = parent.offsetWidth;
            }
            
            if(parent.offsetLeft)
            {
                result += parent.offsetLeft;
            }
            
            parent = parent.parentNode;
        }
        result += pWidth - 26;
        return result;
    }
    
    </script>
    <script src="<%= PathProvider.GetFileStaticRelativePath("reports.js") %>" type="text/javascript"></script>
    <link href="<%= PathProvider.GetFileStaticRelativePath("dashboard.css") %>" rel="stylesheet" type="text/css"/>
    <link href="<%= PathProvider.GetFileStaticRelativePath("tasks.css") %>" type="text/css" rel="stylesheet" />
</asp:Content>

<asp:Content runat="server" ID="PageContent" ContentPlaceHolderID="BTPageContent">
  <asp:PlaceHolder ID="_content" runat="server"></asp:PlaceHolder>  
</asp:Content>

<asp:Content ID="PageContentWithoutCommonContainer" ContentPlaceHolderID="BTPageContentWithoutCommonContainer" runat="server">

<asp:PlaceHolder ID="_navigationPanelContent" runat="server"></asp:PlaceHolder>

<asp:PlaceHolder ID="_widgetContainer"  runat="server"></asp:PlaceHolder>

</asp:Content>
