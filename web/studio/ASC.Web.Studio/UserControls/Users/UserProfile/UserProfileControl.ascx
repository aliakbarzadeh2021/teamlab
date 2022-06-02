<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserProfileControl.ascx.cs"
    Inherits="ASC.Web.Studio.UserControls.Users.UserProfileControl" %>
<%@ Register TagPrefix="ucc" Namespace="ASC.Web.Studio.UserControls.Users.UserProfile" Assembly="ASC.Web.Studio" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="ASC.Web.Core.Users" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Data.Storage" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>

<div id="studio_userProfileCardInfo"></div>
<div class="userProfileCard clearFix">
    <div class="additionInfo">
      <div class="userPhoto">
        <img alt="" class="userPhoto" src="<%=MainImgUrl%>" />
      </div>
       <%if( !MyStaffMode){%>
      <ul class="info">
        <li class="field contact <%=UserIsOnline() ? "online" : "offline" %> even">
          <span><%=UserIsOnline() ? Resources.Resource.Online : Resources.Resource.Offline%></span>
        </li>
      </ul>
      <%} %>
        <%--edit thumbnail--%>
    <%if( MyStaffMode && UserHasAvatar){ %>
		<a style="margin-top:5px;" onclick="UserPhotoThumbnail.ShowDialog();" href="javascript:void(0);" class="baseLinkButton<%=(SetupInfo.WorkMode == WorkMode.Promo ? " promoAction" : "") %>"><%=Resources.Resource.EditThumbnailPhoto%></a>
    <%} %>
    </div>
    <div class="userInfo">
        <table class="info">
            <tr class="field">
                <td class="name textBigDescribe"><%=Resources.Resource.Name%></td>
                <td class="value"><%=HttpUtility.HtmlEncode(DisplayUserSettings.GetFullUserName(UserInfo))%></td>
            </tr>

            <%if(!String.IsNullOrEmpty(UserInfo.Department)){%>
            <tr class="field">
                <td class="name textBigDescribe"><%=ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<Resources.Resource>("Department")%></td>
                <td class="value"><%=RenderDepartment()%></td>
            </tr>
            <%}%>

            <%if(!String.IsNullOrEmpty(UserInfo.Title)){%>
            <tr class="field">
                <td class="name textBigDescribe"><%=ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<Resources.Resource>("UserPost")%></td>
                <td class="value"><%=HttpUtility.HtmlEncode(UserInfo.Title)%></td>
            </tr>
            <%}%>

            <%if (UserInfo.Sex!=null){%>
            <tr class="field">
                <td class="name textBigDescribe"><%=Resources.Resource.Sex%></td>
                <td class="value"><%=(UserInfo.Sex.HasValue ? UserInfo.Sex.Value ? Resources.Resource.MaleSexStatus : Resources.Resource.FemaleSexStatus : string.Empty)%></td>
            </tr>
            <%}%>

            <%if(UserInfo.BirthDate.HasValue && !String.IsNullOrEmpty(UserInfo.BirthDate.Value.ToShortDateString())){%>
            <tr class="field">
                <td class="name textBigDescribe"><%=Resources.Resource.Birthdate%></td>
                <td class="value"><%=UserInfo.BirthDate == null ? string.Empty : UserInfo.BirthDate.Value.ToShortDateString()%></td>
            </tr>
            <%}%>

            <%if(UserInfo.WorkFromDate.HasValue && !String.IsNullOrEmpty(UserInfo.WorkFromDate.Value.ToShortDateString())){%>
            <tr class="field">
                <td class="name textBigDescribe"><%=ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<Resources.Resource>("WorkFromDate")%></td>
                <td class="value"><%=UserInfo.WorkFromDate == null ? string.Empty : UserInfo.WorkFromDate.Value.ToShortDateString()%></td>
            </tr>
            <%}%>
            
            <%if (ShowUserLocation) {%>
            <tr class="field">
                <td class="name textBigDescribe"><%=Resources.Resource.Location%></td>
                <td class="value contact"><%=HttpUtility.HtmlEncode(UserInfo.Location)%></td>
            </tr>
            <%}%>

            <%if(!String.IsNullOrEmpty(UserInfo.Email)){%>
            <tr class="field">
                <td class="name textBigDescribe"><%=Resources.Resource.Email%></td>
                <td class="value contact"><a class="mail" href="mailto:<%=UserInfo.Email.ToLower()%>"><%=HttpUtility.HtmlEncode(UserInfo.Email.ToLower())%></a>
                            <%--change pwd--%>
                <%if( MyStaffMode && (_self || _allowChangePwd)){%>
                    <span style="padding-left:10px;"><%=Resources.Resource.EmailHint%></span><a onclick="javascript:AuthManager.ShowPwdChanger('<%=SecurityContext.CurrentAccount.ID %>'); return false;" href="#" style="padding-left:10px;"><%=Resources.Resource.ChangeSelfPwd%></a>
                <%}%>
                </td>
            </tr>
            <%}%>

            <asp:Repeater ID="SocialContacts" runat="server">
              <ItemTemplate>
                <tr class="field">
                  <td class="name textBigDescribe"><%#GetContactsName(Container.DataItem as Dictionary<String, String>)%></td>
                  <td class="value contact"><%#GetContactsLink(Container.DataItem as Dictionary<String, String>)%></td>
                </tr>
              </ItemTemplate>
            </asp:Repeater>

            <%if(!String.IsNullOrEmpty(UserInfo.Notes)){%>
            <tr class="field">
                <td class="name textBigDescribe" valign="top"><%=Resources.Resource.Comments%></td>
                <td class="value"><%=HttpUtility.HtmlEncode(UserInfo.Notes)%></td>
            </tr>
            <%}%>
<%--
            <tr class="field">
                <td class="name textBigDescribe"><%=Resources.Resource.UserCommunication%></td>
                <td class="value"><%=RenderUserCommunication()%></td>
            </tr>
--%>
            <%if(UserInfo.Status == ASC.Core.Users.EmployeeStatus.Terminated){%>
             <tr valign="top" style="height:30px;">
                <td style="width:110px; text-align:right;" class="textBigDescribe"><div class="cornerAll disabledHeader" style="float:right;"><%=Resources.Resource.DisabledEmployeeTitle%></div></td>
                <td style="padding-left:20px;"><%=UserInfo.TerminatedDate == null ? string.Empty : UserInfo.TerminatedDate.Value.ToShortDateString()%></td>
            </tr>
            <%}%>
            <%if(!MyStaffMode){%>
            <tr valign="top" style="height:30px;">
                <td colspan="2" style="padding-top:20px;"><%=RenderEditDelete() %></td>
            </tr>
            <%}%>
            
            <% if (MyStaffMode && SetupInfo.ThirdPartyAuthEnabled){%>
            <tr valign="top" class="field">
                <td class="name textBigDescribe">
                </td>
                <td  class="value">
                    <div class="clearFix row">&nbsp;</div>
                </td>
            </tr>
             <tr valign="top" class="field">
                <td class="name textBigDescribe social">
               <%= Resources.Resource.AssociateAccount %></td>
                <td class="value">
                    <asp:PlaceHolder runat="server" ID="_accountPlaceholder"></asp:PlaceHolder>
                      <div class="hint textBigDescribe">
                        <%=Resources.Resource.AssociateAccountHint %>
                        </div>
                </td>
            </tr>
            <%}%>
        </table>
    </div>    
</div>
<%if (MyStaffMode && _allowEditSelf)
  {%>
<div class="borderBase clearFix" style="margin-top:20px; border-bottom:none; border-right:none; border-left:none; padding-top:10px;">
    <%--edit profile--%>
    <a style="float:left; margin-right:8px;" onclick="javascript:StudioUserMaker.ShowEditUserDialog('<%=SecurityContext.CurrentAccount.ID %> ',AuthManager.EditProfileCallback); return false;" href="#"
     class="baseLinkButton<%=(SetupInfo.WorkMode == WorkMode.Promo ? " promoAction" : "") %>"><%=Resources.Resource.EditSelfProfile%></a>
</div>

<%}%>
<asp:PlaceHolder runat="server" ID="_editControlsHolder"></asp:PlaceHolder>