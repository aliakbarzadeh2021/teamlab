<%@ Assembly Name="ASC.Projects.Core" %>
<%@ Assembly Name="ASC.Web.Projects" %>
<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Products/Projects/Masters/BasicTemplate.Master" CodeBehind="projectSettings.aspx.cs" Inherits="ASC.Web.Projects.ProjectSettings" %>
<%@ Import Namespace="ASC.Web.Projects.Classes"%>
<%@ MasterType  TypeName="ASC.Web.Projects.Masters.BasicTemplate" %>
<%@ Import Namespace="ASC.Web.Studio.Controls.Common" %>
 
<%@ Register Assembly="ASC.Web.Studio" Namespace="ASC.Web.Studio.Controls.Common"  TagPrefix="ascwc" %>   
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="aswc" %>
<%@ Import Namespace="ASC.Web.Projects.Resources" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="BTHeaderContent" runat="server">

    <link href="<%= PathProvider.GetFileStaticRelativePath("projects.css") %>" rel="stylesheet" type="text/css" />

    <script type="text/javascript" src="<%= PathProvider.GetFileStaticRelativePath("projects.js") %>"></script>

    <script type="text/javascript" language="javascript">
        jq(function() {

            jq("div.infoPanel div").css('display', 'none');

            if (jq('#secureState').val() == 0) {

                jq('#isHiddenProject').click(function() {
                    if (jq('#isHiddenProject').attr('checked')) {
                        jq('#isHiddenProject').attr('checked', !jq('#isHiddenProject').attr('checked'));
                        PremiumStubManager.ShowDialog();
                    }
                });
            }

        });
    </script>

</asp:Content>

<asp:Content ID="PageContent" ContentPlaceHolderID="BTPageContent" runat="server">

    <asp:PlaceHolder runat="server" ID="_premiumStubHolder"></asp:PlaceHolder>
    
    <div class="pm-headerPanel-splitter">
        <div class="headerPanel"><%=ProjectResource.ProjectTitle %></div>
        <asp:TextBox ID="tbxTitle" Width=99% runat="server" CssClass="textEdit" MaxLength=128></asp:TextBox>
    </div>

    <div class="pm-headerPanel-splitter">
        <div class="headerPanel"><%=ProjectResource.ProjectDescription %></div>
        <asp:TextBox ID="tbxDescription" Width=99% runat="server" TextMode="MultiLine" Rows="6" CssClass="nonstretch"></asp:TextBox>
    </div>

    <div class="pm-headerPanel-splitter clearFix">
        <div class="headerPanel"><%=ProjectResource.ProjectLeader %></div>
        <aswc:AdvancedUserSelector runat="server" ID="userSelector"></aswc:AdvancedUserSelector>
    </div>
    
    <div class="pm-headerPanel-splitter">
        <div class="headerPanel"><%=ProjectResource.Tags %></div>
        <div class="DoNotHideTagsAutocompleteContainer">
            <asp:TextBox ID="tbxTags" Width=99% autocomplete="off" runat="server" CssClass="textEdit" MaxLength=8000></asp:TextBox>
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
            <input id="isHiddenProject" type="checkbox" <%= IsHidden() ? "checked='checked'" : "" %> />
            <label for="isHiddenProject"><%= ProjectResource.IUnerstandForEditHidden %></label>
        </div>        
         <input type="hidden" id="secureState" value="<%=_securityEnable?"1":"0"%>" />
    </div>
    
    <div class="pm-headerPanel-splitter">
        <div class="headerPanel"><%=MilestoneResource.Status %></div>
        <div style="float:left;">
            <input id="open" name="status" type="radio" <%= GetProjectStatus()==0 ? "checked='checked'" : "" %> />
            &nbsp<label for="open"> <%=ProjectResource.ActiveProject%></label>
        </div>
        <div>
            <span class="splitter"></span>
            <input id="closed" name="status" type="radio" <%= GetProjectStatus()==1 ? "checked='checked'" : "" %> />
            &nbsp<label for="closed"> <%=ProjectResource.ClosedProject%></label>
        </div>
    </div>

    <div class="pm-h-line" style="margin-left:0px;margin-top:20px; clear:both"><!-- --></div>

    <div class="pm-headerPanel-splitter">
        <a id="updateButton" href="javascript:void(0)" onclick="ASC.Projects.Projects.saveProject();" class="baseLinkButton"><%=ProjectsCommonResource.SaveChanges%></a>
        <div id='info-block-forUpdate' class='pm-ajax-info-block' style="display:none;">
        <span class="textMediumDescribe"><%=  ProjectResource.SaveProject%> </span><br />
        <img src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif")  %>" />
        </div>
    </div>

    <div class="pm-headerPanel-splitter tintMedium">
            <div class="headerBase pm-projectSettings-container"><%=ProjectResource.ProjectDeleted%></div>
            <div class="pm-projectSettings-container"><%=ProjectResource.Information%></div>
            <div class="pm-projectSettings-container">
                <input id="understand" type="checkbox"/>
                &nbsp<label for="understand"> <%=ProjectResource.IUnerstand%></label>
            </div>
    </div>

    <div class="pm-headerPanel-splitter">
        <a id="deleteButton" href="javascript:void(0)" onclick="ASC.Projects.Projects.deleteProject();" class="baseLinkButton"><%=ProjectResource.ProjectDeleteButton%></a>
        <div id='info-block-forDelete' class='pm-ajax-info-block' style="display:none;">
        <span class="textMediumDescribe"><%=  ProjectResource.ProjectDeleted%> </span><br />
        <img src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif")  %>" />
        </div>
    </div>        
    
    <script language="javascript">
        document.getElementById('<%=tbxTags.ClientID %>').onkeyup = ASC.Projects.Projects.tagsAutocomplete;
        document.getElementById('<%=tbxTags.ClientID %>').onkeydown = ASC.Projects.Projects.IfEnterDown;
    </script>
    
    <asp:HiddenField ID="HiddenFieldForTbxTagsID" runat="server"/>
    
</asp:Content>

<asp:Content ID="SidePanel" ContentPlaceHolderID="BTSidePanel" runat="server" >
    <ascwc:SideActions runat="server" ID="SideActionsPanel"> 
    </ascwc:SideActions>
</asp:Content>