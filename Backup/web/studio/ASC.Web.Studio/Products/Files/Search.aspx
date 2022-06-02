<%@ Assembly Name="ASC.Web.Files" %>

<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/Files/Masters/BasicTemplate.Master"
    CodeBehind="Search.aspx.cs" Inherits="ASC.Web.Files.Search" %>

<%@ MasterType TypeName="ASC.Web.Files.Masters.BasicTemplate" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Files.Classes" %>
<%@ Import Namespace="ASC.Web.Files.Configuration" %>
<%@ Import Namespace="ASC.Web.Files.Resources" %>

<%@ Register Assembly="ASC.Web.Files" Namespace="ASC.Web.Files" TagPrefix="ascf" %>

<asp:Content runat="server" ID="HeaderContent" ContentPlaceHolderID="BTHeaderContent">
    <% #if (DEBUG) %>
    <link href="<%= String.Concat(PathProvider.BaseAbsolutePath, "Controls/MainContent/MainContent.css" )%>"
        type="text/css" rel="stylesheet" />
    <link href="<%=PathProvider.GetFileStaticRelativePath("common.css")%>" type="text/css"
        rel="stylesheet" />
    <% #else %>
    <link href="<%=PathProvider.GetFileStaticRelativePath("files-min.css")%>" type="text/css"
        rel="stylesheet" />
    <% #endif %>
    
    <script type="text/javascript" language="javascript" src="<%= PathProvider.GetFileStaticRelativePath("common.js") %>"></script>
</asp:Content>

<asp:Content ID="CommonContainer"  ContentPlaceHolderID="BTPageContent" runat="server">
    <script type="text/javascript">
        jq(function() {
            ASC.Files.Utils.FolderImagesFiles = '<%= PathProvider.GetImagePath("filetype/")%>';

            jq("#searchedDocuments div.document:not(.sub-folder)").each(function() {
                var title = jq(this).find("a.title").text().trim();
                var urlIcn = ASC.Files.Utils.getFileTypeIcon(title);
                jq(this).find("div.icon").css("background", "url('" + urlIcn + "') no-repeat");
            });
        });
    </script>
    
    <asp:Repeater ID="EmployeesSearchRepeater" runat="server">
        <HeaderTemplate>
            <div class="headerBase" style="vertical-align: middle;  padding-bottom:10px;">
                <img align="absmiddle" src="<%=WebImageSupplier.GetAbsoluteWebPath("home.png")%>"
                    style="margin-right: 5px;" alt=" <%=FilesCommonResource.Employees%>" /><%=FilesCommonResource.Employees%></div>
            <table cellpadding="14" cellspacing="0" style="width:100%">
                <tbody>
        </HeaderTemplate>
        <ItemTemplate>
                    <tr class="<%#Container.ItemIndex%2==0 ? "tintMedium" : "tintLight"%>">
                        <td style="width: 100%;padding: 5px 5px 5px 30px;" class="borderBase">
                            <div>
                                <a href="<%#StudioUserInfoExtension.GetUserProfilePageURL(((UserInfo)Container.DataItem), ProductEntryPoint.ID)%>"
                                    class="linkHeaderLightMedium">
                                    <%#HtmlUtility.SearchTextHighlight(SearchText.HtmlEncode(), ((UserInfo)Container.DataItem).DisplayUserName())%>
                                </a>
                            </div>
                            <div class="textBigDescribe">
                                <%=FilesCommonResource.Department%>:&nbsp;<%#HtmlUtility.SearchTextHighlight(SearchText, HttpUtility.HtmlEncode(((UserInfo)Container.DataItem).Department))%>,&nbsp;
                                <%=FilesCommonResource.Position%>:&nbsp;<%#HtmlUtility.SearchTextHighlight(SearchText, HttpUtility.HtmlEncode(((UserInfo)Container.DataItem).Title))%>
                            </div>
                        </td>
                    </tr>
        </ItemTemplate>
        <FooterTemplate>
                </tbody> 
            </table>
            <%if (NumberOfStaffFound > 5)%>
            <%{%>
            <div style="padding: 10px 0px 5px; text-align:right;">          
                <%=FilesCommonResource.TotalFound%>: <%=NumberOfStaffFound%>&nbsp;&nbsp;
                <a href="<%=Page.ResolveUrl(string.Format("~/employee.aspx?pid={0}&search={1}", ProductEntryPoint.ID, SearchText))%>">
                    <%=FilesCommonResource.ShowAllResults%>
                </a>
            </div>
            <%}%>
            <div style="padding: 10px 0px 5px; text-align: right;">&nbsp; </div>  
        </FooterTemplate>
    </asp:Repeater>
    
    <asp:Repeater ID="ContentSearchRepeater" runat="server">
        <HeaderTemplate>
            <div id="searchedDocuments">
                <div  class="headerBase" >
                    <img align="absmiddle" src="<%=WebImageSupplier.GetAbsoluteWebPath("product_logo.png", ProductEntryPoint.ID)%>"
                        style="margin-right: 5px;" alt="<%=FilesCommonResource.Documents%>"><%=FilesCommonResource.Documents%></div>
        </HeaderTemplate>
        <ItemTemplate>
                <div class="document borderBase <%#Container.ItemIndex%2==0 ? "tintMedium" : "tintLight"%> <%#(Container.DataItem as ASC.Web.Files.Search.SearchItem).IsFolder ? "sub-folder" :String.Empty%>" >
                    <div class="icon"></div>
                    <div style="margin-left:35px; overflow:hidden;">
                        <div>
                            <a class="linkHeaderLightMedium title" alt="<%#(Container.DataItem as SearchItem).FileTitle%>"
                                title="<%#(Container.DataItem as ASC.Web.Files.Search.SearchItem).FileTitle%>"
                                href="<%#(Container.DataItem as ASC.Web.Files.Search.SearchItem).ItemUrl%>">
                                <%#HtmlUtility.SearchTextHighlight(SearchText, (Container.DataItem as ASC.Web.Files.Search.SearchItem).FileTitle)%>
                            </a>
                        </div>
                        <div>
                            <%#HtmlUtility.SearchTextHighlight(SearchText, GetShortenContent((Container.DataItem as ASC.Web.Files.Search.SearchItem).Body))%>
                        </div>
                        <div class="textBigDescribe">
                        <div style="float:left;">
                            <%#Eval("FolderPathPart")%>
                        </div>
                        <div style="float:right">
                            <%=FilesCommonResource.TitleOwner%>&nbsp;<%#(Container.DataItem as ASC.Web.Files.Search.SearchItem).Owner%>
                            <span class="separator">|</span><%=FilesCommonResource.TitleUploaded%>:&nbsp;<%#(Container.DataItem as ASC.Web.Files.Search.SearchItem).Uploaded%>
                            <%#(Container.DataItem as ASC.Web.Files.Search.SearchItem).IsFolder ? String.Empty:
                                "<span class='separator'>|</span>" + FilesCommonResource.Size + " " + (Container.DataItem as ASC.Web.Files.Search.SearchItem).Size%>
                        </div>
                        <div style="clear:both;"></div>
                    </div>
                </div>
            </div>
        </ItemTemplate>
        <FooterTemplate>
            </div>
        </FooterTemplate>
    </asp:Repeater>
   
    <%if ((EmployeesSearchRepeater.Items.Count == 0) && (ContentSearchRepeater.Items.Count == 0))%>
    <%{%>
    <div style="padding: 75px;">
        <div><%=FilesCommonResource.NothingFound%></div>
    </div>   
    <%}%>
</asp:Content>
