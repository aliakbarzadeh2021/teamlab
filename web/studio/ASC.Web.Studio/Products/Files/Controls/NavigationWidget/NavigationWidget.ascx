<%@ Assembly Name="ASC.Web.Files" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NavigationWidget.ascx.cs"
    Inherits="ASC.Web.Files.Controls.NavigationWidget" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<% #if (DEBUG) %>
    <link href="<%= String.Concat(ASC.Web.Files.Classes.PathProvider.BaseAbsolutePath, "Controls/NavigationWidget/NavigationWidget.css" )%>" type="text/css" rel="stylesheet" />
<% #endif %>

<ul class="files_navigation">
    <%if (FolderIDUserRoot != 0)%>
    <%{ %>
    <li>
        <a id="files_navigation_<%=FolderIDUserRoot%>"
           class="myFiles"
           title="<%=FilesUCResource.MyFiles%>"
           href="#<%=FolderIDUserRoot%>" >
           <%=FilesUCResource.MyFiles%>
        </a>
    </li>
    <%} %>
    <%if (FolderIDShare != 0)%>
    <%{ %>
    <li>
        <a id="files_navigation_<%=FolderIDShare%>"
           class="shareformeFiles"
           title="<%=FilesUCResource.SharedForMe%>"
           href="#<%=FolderIDShare%>" >
           <%=FilesUCResource.SharedForMe%>

           <%--<span id="newinshare_<%=FolderIDShare%>" class="new_inshare" ></span>--%>
           </a>
    </li>
    <%} %>
    <%if (FolderIDCommonRoot != 0)%>
    <%{ %>
    <li>
        <a id="files_navigation_<%=FolderIDCommonRoot%>"
           class="corporateFiles"
           title="<%=FilesUCResource.CorporateFiles%>"
           href="#<%=FolderIDCommonRoot%>" >
           <%=FilesUCResource.CorporateFiles%>

           <%--<span id="newinshare_<%=FolderIDCommonRoot%>" class="new_inshare" ></span>--%>
           </a>
    </li>
    <%} %>
    <%if (FolderIDTrash != 0)%>
    <%{ %>
    <li>
        <a id="files_navigation_<%=FolderIDTrash%>"
           class="trash files_navigation_last"
           title="<%=FilesUCResource.Trash%>"
           href="#<%=FolderIDTrash%>" ><%=FilesUCResource.Trash%></a>
    </li>
    <%} %>
</ul>