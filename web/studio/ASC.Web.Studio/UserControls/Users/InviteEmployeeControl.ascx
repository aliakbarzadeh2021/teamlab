<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InviteEmployeeControl.ascx.cs"
    Inherits="ASC.Web.Studio.UserControls.Users.InviteEmployeeControl" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>

<div id="studio_invEmpDialog" style="display: none;">
    <ascwc:Container runat="server" ID="_inviteEmpContainer">
        <Header>
            <%=ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<Resources.Resource>("EmailInviteImportName")%>
        </Header>
        <Body>
            <div id="studio_invEmpContent">
                <input type="hidden" id="studio_invEmpContent_infoPanelID" value="<%=_inviteEmpContainer.ClientID%>_InfoPanel" />
                
                <div style="margin-bottom: 10px;">
                    <%=Resources.Resource.InviteUserEmailList%>
                </div>
                
                                
                <div style="margin-bottom:3px;">
                    <%=Resources.Resource.InviteUserTextTitle%>:
                </div>
                
                <div>
                    <textarea tabindex="1" id="studio_inviteUsersText" rows="4" style="width: 100%;"></textarea>
                </div>
                
                <div style="margin-top:10px;">
                    <%=Resources.Resource.InviteMailBoxTitle%>:
                </div>
                
                <input type="hidden" id="studio_inviteDefaultText" value="<%=Resources.Resource.InviteDefaultText%>"/>
                
                <div id="studio_inviteMailBoxList">                
                    <div name="1" id="studio_invite_box_1" class="clearFix" style="margin-top:3px;">
                    <input id="studio_invite_mail_1" tabindex="2" type="text" maxlength="60" class="textEdit" value="" style="width:100%;"/>                    
                    </div>                    
                </div>
                
                <div style="margin-top:10px;" id="studio_invite_addButton">
                    <a href="javascript:void(0);" tabindex="15" onclick="AuthManager.AddInviteMailBox();"><%=Resources.Resource.AddEmailButton%></a>
                </div>
                
                
                <div class="clearFix" style="margin-top: 16px;" id="emp_inv_panel_buttons">
                    <a tabindex="16" style="float: left;" onclick="AuthManager.SendInviteMails()" href="javascript:void(0);"
                        class="baseLinkButton">
                        <%=Resources.Resource.InviteButton %></a> <a tabindex="17" style="float: left; margin-left: 8px;"
                            onclick="AuthManager.CloseInviteEmployeeDialog()" href="javascript:void(0);"
                            class="grayLinkButton">
                            <%=Resources.Resource.CancelButton %></a>
                            
                            <div style="float:left; margin-left:20px;">
                            
                            <input type="checkbox" id="studio_inviteFullAccessState" <%=_defaultInviteAllRightsValue?"checked=\"checked\"":"" %> style="vertical-align:middle" />
                            <label for="studio_inviteFullAccessState" style="margin-left:3px;"><%=Resources.Resource.InviteFullAccessPrivilegesTitle%></label>
                            </div>
                </div>
                <div style="margin-top: 16px; display: none;" id="emp_inv_action_loader"
                    class="clearFix">                    
                    <div class="textMediumDescribe">
                        <%=Resources.Resource.PleaseWaitMessage%>
                    </div>
                    <img src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif")%>">
                </div>
            </div>
            <div id="studio_invEmpMessage" style="padding: 20px 0px; text-align: center; display: none;">
            </div>
        </Body>
    </ascwc:Container>
</div>

<div id="studio_invJoinDialog" style="display:none;">
     <ascwc:Container runat="server" ID="_inviteJoinContainer">
        <Header>
            <%=Resources.Resource.InviteToJoinTitle%>
        </Header>
        <Body>
         <div id="studio_invJoinContent">
            <div id="studio_invJoinInfo"></div>
             <div style="margin-bottom: 10px;">
                    <%=RenderTrustedDominTitle()%>
              </div>
              <div>
                    <input class="textEdit" type="text" id="studio_joinEmail" style="width: 100%"/>
              </div>
              <div style="margin-top:16px;" class="clearFix"  id="join_inv_panel_buttons">
                <a class="baseLinkButton" style="float:left;" href="javascript:void(0);" onclick="AuthManager.SendInviteJoinMail()"><%=Resources.Resource.SendInviteToJoinButton%></a>
                <a style="float: left; margin-left: 8px;"
                            onclick="AuthManager.CloseInviteJoinDialog()" href="javascript:void(0);"
                            class="grayLinkButton">
                            <%=Resources.Resource.CancelButton %></a>
              </div>
              <div style="margin-top: 16px; display: none;" id="join_inv_action_loader"
                    class="clearFix">                    
                    <div class="textMediumDescribe">
                        <%=Resources.Resource.PleaseWaitMessage%>
                    </div>
                    <img src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif")%>">
                </div>
           </div>     
            <div id="studio_invJoinMessage" style="padding: 20px 0px; text-align: center; display: none;">
            </div>
        </Body>
     </ascwc:Container>
</div>
