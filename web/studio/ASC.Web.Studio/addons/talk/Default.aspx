﻿<%@ Assembly Name="ASC.Web.Talk" %>
<%@ Page Language="C#" MasterPageFile="~/Masters/BaseTemplate.master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ASC.Web.Talk.DefaultTalk" Title="Untitled Page" %>

<%@ Import Namespace="ASC.Web.Core.Utility.Skins" %>
<%@ Import Namespace="ASC.Web.Talk.Addon" %>
<%@ Import Namespace="ASC.Web.Talk.Resources" %>

<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascw" %>

<asp:Content ContentPlaceHolderID="TopContent" runat="server" ID="TopContent">    
  <asp:PlaceHolder ID="_topNavigatorPlaceHolder" runat="server"></asp:PlaceHolder>
</asp:Content>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
  <div style="height:20px;"></div>    
</asp:Content>

<asp:Content ID="TalkContent" ContentPlaceHolderID="PageContent" runat="server">
  <ascw:Container ID="mainContainer" runat="server">
	  <Body>
		  <table border="0" cellpadding="0" cellspacing="0" width="100%">
			  <tr>
				  <td colspan="4" class="headerBase" style="padding: 0 0 15px">
					  <%=TalkOverviewResource.OverviewSectionTitle%>
				  </td>
			  </tr>
			  <tr valign="top">
				  <td colspan="3" style="border-bottom: 1px solid #d1d1d1; padding: 0 35px 15px 0;">
					  <div><%=TalkOverviewResource.OverviewContent%></div>
					  <div style="padding-top: 10px;"><%=TalkOverviewResource.OverviewContentDescription%></div>
					  <div style="padding-top: 10px;"><%=TalkOverviewResource.AutoupdateContactListDescription%></div>
					  <div style="padding-top: 10px;"><%=string.Format(TalkOverviewResource.OverviewWebClientDescription, TalkAddon.GetTalkClientURL())%></div>
				  </td>
				  <td style="border-bottom: 1px solid #d1d1d1; padding-bottom: 15px;">
					  <div class="tintMedium" style="padding: 17px 25px 17px 40px; width: 232px;">
						  <span class="link bigLinkButton open-client" onclick="ASC.Controls.JabberClient.open()"><%=TalkOverviewResource.StartWebClientLink%></span>
						  <div class="textMediumDescribe" style="margin: 12px 0px; width: 200px;"><%=TalkOverviewResource.StartWebClientLinkDescription%></div>
					  </div>
				  </td>
			  </tr>
			  <tr>
				  <td colspan="4" class="talkAreaWithBottomBorder">
					  <div style="float: left;">
					    <%-- <img src="images/screenshot01.png" alt="" /> --%>
						  <img src="<%=WebImageSupplier.GetAbsoluteWebPath("screenshot01.png", TalkAddon.AddonID)%>" alt="" />
						  <div class="textBigDescribe talkScreenshot"><%=TalkOverviewResource.ChatTabs%></div>
				    </div>
					  <div class="talkScreenshots">
					    <%-- <img src="images/screenshot02.png" alt="" /> --%>
					    <img src="<%=WebImageSupplier.GetAbsoluteWebPath("screenshot02.png", TalkAddon.AddonID)%>" alt="" />
						  <div class="textBigDescribe talkScreenshot"><%=TalkOverviewResource.ContactListWithOptions%></div>
					  </div>
					  <div class="talkScreenshots">
						  <%-- <img src="images/screenshot03.png" alt="" /> --%>
						  <img src="<%=WebImageSupplier.GetAbsoluteWebPath("screenshot03.png", TalkAddon.AddonID)%>" alt="" />
						  <div class="textBigDescribe talkScreenshot"><%=TalkOverviewResource.YourStatusChanger%></div>
					  </div>
					  <div class="talkScreenshots">
						  <%-- <img src="images/screenshot04.png" alt="" /> --%>
						  <img src="<%=WebImageSupplier.GetAbsoluteWebPath("screenshot04.png", TalkAddon.AddonID)%>" alt="" />
						  <div class="textBigDescribe talkScreenshot"><%=TalkOverviewResource.MessageFieldOptions%></div>
					  </div>
				  </td>
			  </tr>
			  <tr>
				  <td colspan="2" class="headerBase" style="padding: 20px 0 15px">
				    <%=TalkOverviewResource.IntegrationWith3rdPartyAppsSectionTitle%>
				  </td>
			  </tr>
			  <tr valign="top">
				  <td colspan="3" style="padding: 0 35px 15px 0;">
					  <div>
					    <%=TalkOverviewResource.IntegrationWith3rdPartyAppsSectionContent%><br/>
					  </div>
					  <div style="margin-top: 15px;">
					    <%=string.Format(TalkOverviewResource.ThirdPartyAppsSettingsSectionContent,
				                "<ul><li>", 
					             "<b>"+ServerName+"</b></li><li>",
                                 "<b>" + ServerAddress + "</b></li><li>",
                                 "<b>" + ServerPort + "</b></li><li>",
                                 "<b>" + UserName + "</b></li></ul>",
							     "<b>"+JID+"</b>") %>
					  </div>
				  </td>
				  <td>
					  <div class="tintMedium" style="padding: 17px 0 17px 40px; width: 245px;">
						  <div class="headerBase" style="margin-bottom: 15px;"><%=TalkOverviewResource.Recommended%></div>
						  <div class="talkTrillianClientImage">
						    <a href="http://www.trillian.im/download/" class="external" title="<%=TalkOverviewResource.TrillianLink%>" target="_blank">Trillian</a>
						  </div>
						  <div class="talkMirandaIMClientImage">
						    <a href="http://www.miranda-im.org/" class="external" title="<%=TalkOverviewResource.MirandaIMLink%>" target="_blank">Miranda</a>
						    </div>
						  <div class="talkPidginClientImage">
						    <a href="http://pidgin.im/" class="external" title="<%=TalkOverviewResource.PidginLink%>" target="_blank">Pidgin</a>
						  </div>
						  <div class="talkPsiClientImage">
						    <a href="http://psi-im.org/" class="external" title="<%=TalkOverviewResource.PsiLink%>" target="_blank">Psi</a>
						  </div>							
					  </div>
				  </td>
			  </tr>
		  </table>
	  </Body>
  </ascw:Container>
</asp:Content>

<asp:Content ID="FooterContent" ContentPlaceHolderID="FooterContent" runat="server">    
  <asp:PlaceHolder runat="server" ID="_bottomNavigatorPlaceHolder"></asp:PlaceHolder>
</asp:Content>
