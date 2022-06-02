<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Assembly Name="ASC.Web.Studio" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PopUpRequest.ascx.cs"
 Inherits="ASC.Web.Projects.Controls.Common.PopUpRequest" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration" %>
<%@ Import Namespace="ASC.Projects.Core.Domain" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<div class="popupContainerClass">

    <div class="containerHeaderBlock">
    
        <table cellspacing="0" cellpadding="0" border="0" style="width: 100%; height: 0px;">
        <tbody>
            <tr valign="top">
                <td><%=RequestResource.Requests%></td>
                <td class="popupCancel">
                    <div onclick="PopupKeyUpActionProvider.ClearActions(); jq.unblockUI();" class="cancelButton"/>
                </td>
            </tr>
        </tbody>
        </table>
        
    </div>
    
    <div class="containerBodyBlock" id="requestBody">

        <div class="pm-headerPanelSmall-splitter">
            <%= RenderActionTitle() %>
        </div>

        <%if(HasTemplate){ %>
        <div class="pm-headerPanelSmall-splitter">
            <div class="headerPanelSmall">
                <b>
                    <%=ProjectResource.ProjectTemplate %>:
                </b>
            </div>
            <input class="textEdit" type="text" style="width:99%;" value="<%=TemplateTitle%>" readonly="readonly" />
        </div>
        <%} %>

        <div class="pm-headerPanelSmall-splitter">
            <div class="headerPanelSmall">
                <b>
                    <%=ProjectResource.ProjectTitle %>:
                </b>
            </div>
            <input class="textEdit" id="txtTitle" type="text" style="width:99%;" value="<%=Title%>" />
        </div>
        
        <div class="pm-headerPanelSmall-splitter">
            <div class="headerPanelSmall">
                <b>
                    <%=ProjectResource.ProjectDescription%>:
                </b>    
            </div>
            <textarea id="txtDescription" cols="20" rows="4" style="width:99%;"><%=Description%></textarea>
        </div>
        
        <div class="pm-headerPanelSmall-splitter">
            <div class="headerPanelSmall">
                <b>
                    <%=ProjectResource.ProjectLeader %>:
                </b>
            </div>
            <select id="Select" class="comboBox" style="width:99%;">
                <%=UsersByDepartments() %>
            </select>
        </div>
        
        <div class="pm-headerPanelSmall-splitter">
            <div class="headerPanelSmall">
                <b>
                    <%=ProjectResource.Tags %>:
                </b>        
            </div>
            <div class="DoNotHideTagsAutocompleteContainer" runat="server">
                <input ID="_tbxTags" style="width:99%;" autocomplete="off" class="textEdit" value="<%=Tags%>"/>
            </div>
            <div id="TagsAutocompleteContainer" style="position:absolute;//margin-top: -1px;"></div>
            <div class="textBigDescribe"><%=ProjectResource.EnterTheTags %></div>
        </div>
        
        <div class="pm-headerPanelSmall-splitter">
            <% if (!_securityEnable) { %>
            <img alt="" title="<%=Resources.Resource.RightsRestrictionNote %>" style="margin-right:5px;" src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("premium.png")%>" />
            <% } %>
            <input id="isHiddenProject" <%= _securityEnable? "": "disabled='disabled'" %> type="checkbox" <%= IsHidden() ? "checked='checked'" : "" %> />
            <label for="isHiddenProject"><%= ProjectResource.HiddenProject %></label>
        </div>
        
        <div class="pm-headerPanelSmall-splitter">
            <div class="headerPanelSmall">
                <b>
                    <%=MilestoneResource.Status %>:
                </b>
            </div>
            <div>  
                <input id="open" name="status" type="radio" <%= GetProjectStatus()==0 ? "checked='checked'" : "" %> />
                <label for="open"> <%=ProjectResource.ActiveProject%></label>
                <span class="splitter"></span>
                <input id="closed" name="status" type="radio" <%= GetProjectStatus()==1 ? "checked='checked'" : "" %> />
                <label for="closed"> <%=ProjectResource.ClosedProject%></label>
            </div>
        </div>
        
        <div class="pm-h-line" ><!– –></div>
        
        <div style="text-align:left;">
            <a href="javascript:void(0)" onclick="ASC.Projects.Projects.accept()" class="baseLinkButton"><%=RequestResource.Accept %></a>
            <span class="button-splitter"></span>
            <a class="baseLinkButton" href="javascript:void(0)" onclick="ASC.Projects.Projects.reject()"><%=RequestResource.Reject %></a>
            <span class="button-splitter"></span>
            <a class="grayLinkButton" href="javascript:void(0)" onclick="jq.unblockUI();"><%=ProjectsCommonResource.Cancel %></a>
        </div>  

        <input id="requestID" type="hidden" value="<%=GetRequestID()%>" />
        
        <script language="javascript">
            document.getElementById('_tbxTags').onkeyup = ASC.Projects.Projects.tagsAutocomplete;
            document.getElementById('_tbxTags').onkeydown = ASC.Projects.Projects.IfEnterDown;
        </script>
    
        <input id="_HiddenFieldForTbxTagsID" type="hidden" value="_tbxTags" />
        
    </div>
    
</div>
