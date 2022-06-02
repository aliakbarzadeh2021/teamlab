<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PwdTool.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.PwdTool" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<div id="studio_pwdChangerDialog" style="display: none;">
    <input type="hidden" id="studio_pwdChangerDialog_userID" />
    <ascwc:Container runat="server" ID="_userPwdContainer">
        <Header>
            <%=Resources.Resource.PasswordSettingsTitle%>
        </Header>
        <Body>
            <div id="studio_pwdChangeContent">
                <%--<div id="pwdchange_info">
            </div>--%>
                <input type="hidden" id="pwdchange_info_id" value="<%=_userPwdContainer.ClientID%>_InfoPanel" />
                <div class="clearFix">
                    <div>
                        <%=Resources.Resource.NewPassword%>:
                    </div>
                    <div style="margin: 3px 0px;">
                        <input type="password" id="user_newPwd" class="textEdit" style="width: 99%;" /></div>
                </div>
                <div class="clearFix" style="margin-top: 10px;">
                    <div>
                        <%=Resources.Resource.RePassword%>:
                    </div>
                    <div style="margin: 3px 0px;">
                        <input type="password" id="user_rePwd" class="textEdit" style="width: 99%;" /></div>
                </div>
                <div class="clearFix" id="pwd_panel_buttons" style="margin-top: 16px;">
                    <a class="baseLinkButton" style="float: left;" href="javascript:AuthManager.ChangePwd();">
                        <%=Resources.Resource.ChangeButton%></a> <a class="grayLinkButton" style="float: left;
                            margin-left: 8px;" href="javascript:AuthManager.ClosePwdChanger();">
                            <%=Resources.Resource.CancelButton%></a>
                </div>
                <div style="padding-top: 16px; display: none;" id="pwd_action_loader"
                    class="clearFix">
                    <div class="textMediumDescribe">
                        <%=Resources.Resource.PleaseWaitMessage%>
                    </div>
                    <img src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif")%>">
                </div>
            </div>
            <div id="studio_pwdChangeMessage" style="padding: 20px 0px; text-align: center; display: none;">
            </div>
        </Body>
    </ascwc:Container>
</div>
<div id="studio_pwdReminderDialog" style="display: none;">
    <ascwc:Container runat="server" ID="_pwdRemainderContainer">
        <Header>
            <%=Resources.Resource.PasswordRemindTitle%>
        </Header>
        <Body>
            <div id="studio_pwdReminderContent">
                <input type="hidden" id="studio_pwdReminderInfoID" value="<%=_pwdRemainderContainer.ClientID%>_InfoPanel" />
                <div class="clearFix">
                    <div>
                        <%=Resources.Resource.EmailForReminder%>:
                    </div>
                    <div style="margin: 3px 0px;">
                        <input type="text" id="studio_emailPwdReminder" class="textEdit" style="width: 99%;" />
                    </div>
                </div>
                <div class="clearFix" style="margin-top: 16px;" id="pwd_rem_panel_buttons">
                    <a class="baseLinkButton" style="float: left;" href="javascript:AuthManager.RemindPwd();">
                        <%=Resources.Resource.RemindPwdButton%></a> <a class="grayLinkButton" style="float: left;
                            margin-left: 8px;" href="javascript:AuthManager.ClosePwdReminder();">
                            <%=Resources.Resource.CancelButton%></a>
                </div>
                <div style="padding-top: 16px; display: none;" id="pwd_rem_action_loader" class="clearFix">
                    <div class="textMediumDescribe">
                        <%=Resources.Resource.PleaseWaitMessage%>
                    </div>
                    <img src="<%=ASC.Web.Core.Utility.Skins.WebImageSupplier.GetAbsoluteWebPath("ajax_progress_loader.gif")%>">
                </div>
            </div>
            <div id="studio_pwdReminderMessage" style="padding: 20px 0px; text-align: center;
                display: none;">
            </div>
        </Body>
    </ascwc:Container>
</div>
