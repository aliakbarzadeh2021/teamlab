<%@ Page Language="C#" MasterPageFile="~/Masters/StudioTemplate.master" AutoEventWireup="true" EnableViewState="false"
    CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Studio._Default" Title="Untitled Page" %> 
<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>   
<%@ Import Namespace="ASC.Web.Studio.Utility" %> 
<%@ Import Namespace="ASC.Web.Core" %> 
<%@ Import Namespace="ASC.Core" %> 
<%@ Import Namespace="ASC.Core.Users" %> 
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
 
<asp:Content ID="DefaultPageContent" ContentPlaceHolderID="StudioPageContent" runat="server"> 

  <div id="GreetingBlock">
    
    <div style="padding-top:10px;">
    <asp:PlaceHolder ID="navPanelHolder" runat="server">
    </asp:PlaceHolder>
    </div>
    <div class="clearFix" style="padding-top:20px;">
    
    <%if(_showGettingStarted){ %>
    <div style="float:left;">
        <div class="videoBox borderBase tintLight">         
            <div class="borderBase headerBase gsHeader">
                <%=Resources.Resource.GettingStartingHeader%>
            </div>
            
            <div class="getstartMenuItem">
            <%if (ASC.Web.Studio.Core.SetupInfo.IsMassAddEnabled())
              {%>
             <a class="linkHeaderGetstart" href="#" onclick="getContactsPopupWindowDisplay('<%=ASC.Web.Studio.Core.SetupInfo.GetImportServiceUrl()%>', '<%=Resources.Resource.GetContacts%>'); return false;"><img alt="" align="absmiddle" src=<%=WebImageSupplier.GetAbsoluteWebPath("btn_invitepeople.png") %> /></a>
             <a class="linkHeaderGetstart" href="#" onclick="getContactsPopupWindowDisplay('<%=ASC.Web.Studio.Core.SetupInfo.GetImportServiceUrl()%>', '<%=Resources.Resource.GetContacts%>'); return false;"><%=ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<Resources.Resource>("AddEmployeesButton")%></a>
             <%}
              else
              { %>
             <a class="linkHeaderGetstart" href="#" onclick="AuthManager.ShowInviteEmployeeDialog(); return false;"><img alt="" style="margin-right:8px;" border="0" align="absmiddle" src=<%=WebImageSupplier.GetAbsoluteWebPath("btn_invitepeople.png") %> /></a>
             <a class="linkHeaderGetstart" href="#" onclick="AuthManager.ShowInviteEmployeeDialog(); return false;"><%=ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<Resources.Resource>("InviteEmloyeesButton")%></a>
             <%} %>
            </div><br clear="all" />
            
            <%if(IsProjectsEnabled) { %>
            <div class="getstartMenuItem">
             <a class="linkHeaderGetstart" href="<%=VirtualPathUtility.ToAbsolute("~/products/projects/settings.aspx")%>"><img alt="" align="absmiddle" src=<%=WebImageSupplier.GetAbsoluteWebPath("btn_importbasecamp.png") %> /></a>
             <a class="linkHeaderGetstart" href="<%=VirtualPathUtility.ToAbsolute("~/products/projects/settings.aspx")%>"><%=Resources.Resource.ImportFromBaseCamp%></a>
            </div><br clear="all" />
            <%} %>
            
            <div class="getstartMenuItem">
             <a class="linkHeaderGetstart" href="#" onclick="StudioManager.ShowGettingStartedVideo(); return false;"><img alt="" align="absmiddle" src=<%=WebImageSupplier.GetAbsoluteWebPath("btn_video.png") %> /></a>
             <a class="linkHeaderGetstart" href="#" onclick="StudioManager.ShowGettingStartedVideo(); return false;"><%=Resources.Resource.WatchVideoButton%></a>
            </div><br clear="all" />
            
            <div style="margin:23px 0 10px;" class="clearFix">
                <input id="studio_gettingStartedState" onclick="StudioManager.SaveGettingStartedState();" type="checkbox" style="height: 15px; margin-top: 0px; margin-bottom: 0px; float: left;"/>
                <label for="studio_gettingStartedState" style="float: left; margin-left: 3px;"><%=Resources.Resource.DontShowOnStartupText%></label>
            </div>
            
           
        </div>
        
        <%--video dialog--%>
        <div id="studio_GettingStartedVideoDialog" style="display:none;">
             <ascwc:Container runat="server" ID="_gettingStartedVideoContainer">
                <Header>
                    <%=Resources.Resource.GettingStartingHeader%>
                </Header>
                <Body>
                 <div>
                    <object width="640" height="385"><param name="movie" value="https://www.youtube.com/v/CU5qm_xiNck?version=3"></param><param name="allowFullScreen" value="true"></param><param name="allowscriptaccess" value="always"></param><embed src="https://www.youtube.com/v/CU5qm_xiNck?version=3" type="application/x-shockwave-flash" allowscriptaccess="always" allowfullscreen="true" width="640" height="385"></embed></object>
                 </div>
                 <div class="clearFix" style="margin-top:16px;">
                 <a class="grayLinkButton" style="float:left;" href="javascript:jq.unblockUI()"><%=Resources.Resource.CancelButton %></a>
                 </div>
                </Body>
             </ascwc:Container>
        </div>
        
        <div id="fb-root"></div><script src="https://connect.facebook.net/en_US/all.js#xfbml=1"></script><fb:like href="http://www.facebook.com/TeamLab" send="false" width="240" show_faces="false" font=""></fb:like>

    </div>
    <%} %>

    <div style="float:left;  <%if(_showGettingStarted){ %>width:670px;<%} %>">

        <div class="headerBaseBig" style="padding:15px 0 5px;">
        <%=String.Format(Resources.Resource.WelcomeUserMessage, CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).DisplayUserName(true))%>
        </div>
        
        <asp:Repeater runat="server" ID="_productRepeater">
          <ItemTemplate>
            <div class="product clearFix">
              <img alt="" src="<%#(Container.DataItem as IWebItem).GetLargeIconAbsoluteURL()%>" />
              <h2 class="title">
                <a class="linkHeaderLightBig" href="<%#VirtualPathUtility.ToAbsolute((string)Eval("StartURL"))%>"><%#HttpUtility.HtmlEncode((string)Eval("Name"))%></a>
              </h2>
              <span class="description"><%#HttpUtility.HtmlEncode((string)Eval("Description"))%></span>
            </div>
          </ItemTemplate>
        </asp:Repeater>
        
         
    </div>
    </div>
    
    <%if (!_showGettingStarted)
           { %>
           <div class="clearFix" style="margin-top:20px;">
        <div id="fb-root"></div><script src="https://connect.facebook.net/en_US/all.js#xfbml=1"></script><fb:like href="http://www.facebook.com/TeamLab" send="false" width="450" show_faces="false" font=""></fb:like>
        </div>
        <%} %>
    
 </div>
 

 <asp:PlaceHolder runat="server" ID="_afterRegistryWelcomePopupBoxHolder">
 <script language="javascript" type="text/javascript">
    jq(document).ready(function(){
       try {

			jq.blockUI({ message: jq("#studio_welcomeMessageBox"),
				css: {
					opacity: '1',
					border: 'none',
					padding: '0px',
					width: '400px',
					cursor: 'default',
					textAlign: 'left',
					'background-color': 'Transparent',
					'margin-left': '-200px',
					'top': '30%'
				},

				overlayCSS: {
					backgroundColor: '#aaaaaa',
					cursor: 'default',
					opacity: '0.3'
				},
				focusInput: false,
				fadeIn: 0,
				fadeOut: 0
			});
		}
		catch (e) { };
         
    });
 </script>
 <div id="studio_welcomeMessageBox" style="display:none;">
     <ascwc:Container runat="server" ID="_welcomeBoxContainer">
        <Header>
            <%=Resources.Resource.AfterRegistryMessagePopupTitle%>
        </Header>
        <Body>
         <div>
            <%=Resources.Resource.AfterRegistryMessagePopupText%>
         </div>
         <div class="clearFix" style="margin-top:16px;">
         <a class="baseLinkButton" style="float:left;" href="javascript:jq.unblockUI()"><%=Resources.Resource.ContinueButton %></a>
         </div>
        </Body>
     </ascwc:Container>
</div>
</asp:PlaceHolder>

</asp:Content>
