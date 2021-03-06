<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TemplateList.ascx.cs" Inherits="ASC.Web.Projects.Controls.TemplateList" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Projects.Controls.Reports" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Core.Users" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Web.Projects.Classes"%>
<%@ Import Namespace="ASC.Projects.Core.Domain.Reports"%>

<div id="divTemplateList">
    <asp:Repeater ID="repeaterTemplateList" runat="server">
        <HeaderTemplate>
            <table id="tableTemplateList" class="pm-tablebase" cellspacing="0" cellpadding="10" style="margin-bottom:20px;">
                <thead>
                    <tr id="tmplListHeader">
                        <td class="pm-template-checkbox">
                        </td>
                        <td id="thTemplName" style="width:100%;">
                            <%=ReportResource.TemplateName %>
                        </td>
                        <td style="text-align:center;">
                            <%=ReportResource.Notification %>
                        </td>
                        <td colspan="2" style="text-align:center;width:150px;">
                            <%=ReportResource.Actions%>
                        </td>
                    </tr>
                </thead>
                <tbody>
        </HeaderTemplate>
        <ItemTemplate>
            <tr id="<%# "Template" + (Container.DataItem as ReportTemplate).Id %>" class=" <%# Container.ItemIndex%2==0 ? "tintMedium" : "tintLight" %>">
                <td class="borderBase pm-template-checkbox">
                    <input id="<%# "selectTmpl" + (Container.DataItem as ReportTemplate).Id %>" type="checkbox" />
                </td>
                <td class="borderBase">
                    <div id="<%#"TmplName" + (Container.DataItem as ReportTemplate).Id %>" class="templateName">
                        <%# (Container.DataItem as ReportTemplate).Name %>
                    </div>
                </td>
                <td class="borderBase" style="white-space:nowrap;">
                    <input id="<%#"AutoGenValue" + (Container.DataItem as ReportTemplate).Id %>" type="hidden" value="<%#(Container.DataItem as ReportTemplate).AutoGenerated %>" />
                    <!--<img id='<%#"AutoGenImage" + (Container.DataItem as ReportTemplate).Id %>'  style='<%#(Container.DataItem as ReportTemplate).AutoGenerated ? "" : "display:none;" %>' alt='' border='0' align='absmiddle' src='<%#ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("mail_user.png") %>'
                    title="<%# GetAutugeneratedTime((Container.DataItem as ReportTemplate).Cron) %>" />-->
                    <span id='<%#"AutoGenTime" + (Container.DataItem as ReportTemplate).Id %>'  style='<%#(Container.DataItem as ReportTemplate).AutoGenerated ? "" : "display:none;" %>'>
                        <%# GetAutugeneratedTime((Container.DataItem as ReportTemplate).Cron) %>
                    </span>
                </td>
                <td class="borderBase">
                    <a id='<%#"TmplEditLink" + (Container.DataItem as ReportTemplate).Id %>' onclick="javascript:viewReportTemplateContainer(<%#(Container.DataItem as ReportTemplate).Id %>, '<%# GetTemplateName((Container.DataItem as ReportTemplate).Name) %>', <%#GetCronFields((Container.DataItem as ReportTemplate).Cron).period %>, <%#GetCronFields((Container.DataItem as ReportTemplate).Cron).periodItem %>, <%#GetCronFields((Container.DataItem as ReportTemplate).Cron).hour %>, <%#(Container.DataItem as ReportTemplate).AutoGenerated.ToString().ToLower() %>);" href="javascript:void(0);">
                        <img style='border:0px; cursor:pointer;' align='absmiddle' title='<%=ProjectsCommonResource.Edit%>' alt='<%=ProjectsCommonResource.Edit%>' src='<%=GetEditImageSrc() %>'/>
                    </a>
                </td>
                <td class="borderBase">
                    <a class="linkSmall" onclick="ASC.Projects.Reports.generateReportByTemplateInNewWindow(<%#(Container.DataItem as ReportTemplate).Id %>)" style="cursor:pointer;"><%=ReportResource.Generate %></a>
                </td>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
                </tbody>
            </table>
        </FooterTemplate>
    </asp:Repeater>
    <asp:PlaceHolder ID="plhTemplateActionButtons" runat="server">
        <a id="buttonDelTemplSelected" class="baseLinkButton" onclick="javascript:ASC.Projects.Reports.deleteTemplates('selected');"><%=ProjectsCommonResource.Delete %></a>
        <span class="splitter"></span>
        <a id="buttonDelTemplAll" class="baseLinkButton" onclick="javascript:ASC.Projects.Reports.deleteTemplates('all');"><%=ReportResource.ClearAllTemplates %></a>
    </asp:PlaceHolder>
</div>  

<div id="emptyScreenControl" style='<%= HasData ? "display:none" : "" %>'>
    <asp:PlaceHolder runat="server" ID="plhEmptyScreen"></asp:PlaceHolder>
</div>


