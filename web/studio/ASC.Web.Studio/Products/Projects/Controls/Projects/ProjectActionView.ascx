<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectActionView.ascx.cs" Inherits="ASC.Web.Projects.Controls.Projects.ProjectActionView" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>

<%@ Import Namespace="ASC.Web.Projects.Resources" %>
<%@ Import Namespace="ASC.Web.Projects.Configuration"%>
<%@ Import Namespace="ASC.Projects.Core.Domain"%>

<asp:PlaceHolder runat="server" ID="_premiumStubHolder"></asp:PlaceHolder>

<%if(HasTemplates){ %>
    <div id="TemplatesComboboxContainer" style="<%=TemplatesContainerStyle%>">
		<script type="text/javascript" language="javascript" src="<%=ASC.Web.Projects.Classes.PathProvider.GetFileStaticRelativePath("projecttemplates.js") %>"></script>
		<link href="<%= ASC.Web.Projects.Classes.PathProvider.GetFileStaticRelativePath("projecttemplates.css") %>" rel="stylesheet" type="text/css" />
		
		<div id="TemplatesDropdownContainer" runat="server" style="float: right;"></div>
		<div id="ClearSelectedTemplate" onclick="ProjectTemplatesController.ClearSelectedTemplate();" class="projectTemplatesDottedLink" style="float: right; display: none; margin: 4px 6px 0 12px;"><%=ProjectResource.Clear %></div>
		<div id="SelectedTemplateTitle" style="float: right;dispaly:none;margin-top: 4px;overflow: hidden;white-space: nowrap;max-width: 350px;"></div>
		
		<input type="hidden" id="SelectedTemplateTitleHidden" value="<span style='color: #787878;'><%=ProjectResource.AppliedTemplate %></span>" />
		<input type="hidden" id="SelectedTemplateID" value="0" />
    </div>    
    <%} %>

<div id="page_content">

    <div class="pm-headerPanel-splitter">
        <div class="headerPanel"><%=ProjectResource.ProjectTitle %></div>
        <asp:TextBox ID="tbxTitle" Width=99% runat="server" CssClass="textEdit" 
            MaxLength="128"></asp:TextBox>
        <div class="textBigDescribe"><%= ProjectResource.Example %></div>
    </div>

    <div class="pm-headerPanel-splitter">
        <div class="headerPanel"><%=ProjectResource.ProjectDescription %></div>
        <asp:TextBox ID="tbxDescription" Width=99% runat="server" TextMode="MultiLine" Rows="6" CssClass="nonstretch"></asp:TextBox>
    </div>
    
    <div class="pm-headerPanel-splitter clearFix">
        <div class="headerPanel"><%=ProjectResource.ProjectLeader %></div>
        <% if(ASC.Web.Projects.Classes.Global.IsAdmin)%>
        <% {%>
        <div style="float:right;">
            <input id="notify" type="checkbox" style="margin: 1px 8px 0 0;" />
            <label for="notify"><%= ProjectResource.NotifyProjectLeader %></label>
        </div>
        <% }%>
        <ascwc:AdvancedUserSelector runat="server" ID="userSelector"></ascwc:AdvancedUserSelector>
    </div>    
    
    <div class="pm-headerPanel-splitter">
        <div class="headerPanel"><%=ProjectResource.Tags %></div>
        <div class="DoNotHideTagsAutocompleteContainer">
            <asp:TextBox ID="tbxTags" Width=99% autocomplete="off" runat="server" CssClass="textEdit" MaxLength="8000"></asp:TextBox>
        </div>
        <div id="TagsAutocompleteContainer" style="position:absolute;//margin-top: -1px;"></div>
        <div class="textBigDescribe"><%=ProjectResource.EnterTheTags %></div>
    </div>   
    
    <div class="pm-headerPanel-splitter tintMedium">        
        <div class="headerBase pm-projectSettings-container">
        <% if (!_securityEnable) { %>
        <img alt="" title="<%=Resources.Resource.RightsRestrictionNote %>" style="margin-right:5px;" src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("premium.png")%>" />
        <% } %>
        <%= ProjectResource.HiddenProject %></div>
        <div class="pm-projectSettings-container"><%=ProjectResource.InformationForHidden %></div>
        <div class="pm-projectSettings-container">
            <input id="isHiddenProject" type="checkbox" style="margin: 1px 8px 0 0;"/>
            <label for="isHiddenProject"><%= ProjectResource.IUnerstandForHidden %></label>
        </div>
        <input type="hidden" id="secureState" value="<%=_securityEnable?"1":"0"%>" />
    </div>

    <div class="pm-h-line"><!-- --></div>

    <div id="actions">
        <a href="javascript:void(0)" onclick="javascript:ASC.Projects.Projects.addNewProject();" class="baseLinkButton">
            <%=ProjectResource.AddNewProject %>
        </a>
        <span class="button-splitter"></span>
        <a class="grayLinkButton" href="projects.aspx"><%= ProjectsCommonResource.Cancel %></a>
    </div>

    <div id='info-block' class='pm-ajax-info-block' style="display:none;">
        <span class="textMediumDescribe"><%=  ProjectResource.ProjectAdded%> </span><br />
        <img src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif")  %>" />
    </div>
    
    <script language="javascript">
        document.getElementById('<%=tbxTags.ClientID %>').onkeyup = ASC.Projects.Projects.tagsAutocomplete;
        document.getElementById('<%=tbxTags.ClientID %>').onkeydown = ASC.Projects.Projects.IfEnterDown;

        jq(function() {
        
            if (jq('#secureState').val() == 0) {
                jq('#isHiddenProject').attr('checked', false);
                jq('#isHiddenProject').click(function() {
                    jq('#isHiddenProject').attr('checked', false);
                    PremiumStubManager.ShowDialog();
                });
            }
        })
    </script>
    
    <asp:HiddenField ID="HiddenFieldForTbxTagsID" runat="server"/>

</div>
