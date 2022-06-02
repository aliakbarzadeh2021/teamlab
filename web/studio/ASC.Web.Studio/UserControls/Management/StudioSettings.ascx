<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StudioSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.StudioSettings" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<%@ Register Assembly="ASC.Web.Studio" Namespace="ASC.Web.Studio.Controls.Users" TagPrefix="ascwc" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Core.Users" %>
        
        <%--login settings--%>
        <asp:PlaceHolder ID="_enterSettingsHolder" runat="server">
            <div class="headerBase borderBase" style="margin-top: 20px; padding-left: 15px; padding-bottom: 5px;
                border-top: none; border-right: none; border-left: none;">
                <%=Resources.Resource.EnterSettings%>
            </div>
            <div id="studio_setInfEnterSettingsInfo">
            </div>
            <div id="studio_enterSettingsBox" style="padding: 20px 15px;">
                <div class="clearFix" style="padding: 3px 0px;">
                    <div style="float: left">
                        <input type="checkbox" <%=(ASC.Core.CoreContext.Configuration.DemoAccountEnabled?"checked=\"checked\"":"") %>
                            id="studio_demoEnterState" />
                    </div>
                    <div style="float: left; margin-top: 3px; margin-left: 3px;">
                        <label for="studio_demoEnterState">
                            <%=Resources.Resource.EnabledDemoEnter%></label>
                    </div>
                </div>
                <div class="clearFix" style="margin-top: 20px;">
                    <a class="baseLinkButton<%=(SetupInfo.WorkMode == WorkMode.Promo?" promoAction":"") %>" style="float: left;" onclick="StudioManagement.SaveEnterSettings();"
                        href="javascript:void(0);">
                        <%=Resources.Resource.SaveButton %></a>
                </div>
            </div>
        </asp:PlaceHolder>        
        
        <%--side panel settings--%>
        <asp:PlaceHolder ID="_studioViewSettingsHolder" runat="server">
            <div class="headerBase borderBase" style="margin-top: 20px; padding-left: 15px; padding-bottom: 5px;
                border-top: none; border-right: none; border-left: none;">
                <%=Resources.Resource.StudioVisualSettings%>
            </div>
            <div id="studio_setInfStudioViewSettingsInfo">
            </div>
            <div id="studio_studioViewSettingsBox" style="padding: 20px 15px;">
                <div class="clearFix">
                   <div style="float:left;">
                    <input id="studio_sidePanelLeftViewType" type="radio" <%=(_studioViewSettings.LeftSidePanel?"checked=\"checked\"":"") %> name="sidePanelViewType" />
                    </div>
                    <div style="float:left; margin-top:3px; margin-left:5px;">
                    <label for="studio_sidePanelLeftViewType"><%=Resources.Resource.SidePanelLeftView%></label>
                    </div>
                    <div style="float:left; margin-left:20px;">
                    <input id="studio_sidePanelRightViewType" type="radio" <%=(!_studioViewSettings.LeftSidePanel?"checked=\"checked\"":"") %> name="sidePanelViewType" />
                    </div>
                    <div style="float:left; margin-top:3px; margin-left:5px;">
                    <label for="studio_sidePanelRightViewType"><%=Resources.Resource.SidePanelRightView%></label>
                    </div>
               </div>
                <div class="clearFix" style="margin-top: 20px;">
                    <a class="baseLinkButton<%=(SetupInfo.WorkMode == WorkMode.Promo?" promoAction":"") %>" style="float: left;" onclick="StudioManagement.SaveStudioViewSettings();"
                        href="javascript:void(0);">
                        <%=Resources.Resource.SaveButton %></a>
                </div>
            </div>
        </asp:PlaceHolder>
        
        <%--portal settings--%>
        <asp:PlaceHolder ID="_portalSettingsHolder" runat="server"></asp:PlaceHolder>         
         
         <%--timezone & language--%>
        <div class="headerBase borderBase" style="margin-top: 20px; padding-left: 15px; padding-bottom: 5px;
                border-top: none; border-right: none; border-left: none;">
                <%=Resources.Resource.StudioTimeLanguageSettings%>
         </div>
         <div id="studio_lngTimeSettingsInfo">            
         </div>
         <div id="studio_lngTimeSettingsBox" style="padding:0px 20px 15px 20px;"> 
            <div class="clearFix" style="margin-top:20px;">
                <div  class="headerBaseSmall" style="float:left; width:90px; text-align:right; padding-right:10px;"><%=Resources.Resource.Language%>:</div>
                <div style="float:left; width:200px;" class="studioHeaderBaseSmallValue"><%=RenderLanguageSelector()%></div>
            </div>
            
            <div class="clearFix" style="margin-top:20px;">
                <div class="headerBaseSmall" style="float:left; width:90px; text-align:right; padding-right:10px;"><%=Resources.Resource.TimeZone%>:</div>
                <div style="float:left;  width:200px" class="studioHeaderBaseSmallValue"><%=RenderTimeZoneSelector()%></div>
            </div>
            
            <div class="clearFix" style="margin-top: 20px;">
                    <a class="baseLinkButton<%=(SetupInfo.WorkMode == WorkMode.Promo?" promoAction":"") %>" style="float: left;" onclick="StudioManagement.SaveLanguageTimeSettings();"
                        href="javascript:void(0);">
                        <%=Resources.Resource.SaveButton %></a>
                </div>
         </div> 
         
         <%--DNS settings--%>
        <asp:PlaceHolder ID="_dnsSettingsHolder" runat="server">
            <div class="headerBase borderBase clearFix" style="margin-top: 20px; padding-left: 15px; padding-bottom: 5px; border-top: none; border-right: none; border-left: none;">
				<div style="float: left;"><%=Resources.Resource.DnsSettings%></div>
				<div style="float:right; padding-top:5px;"><a target="_blank" class="linkDescribe" href="<%=ASC.Web.Studio.UserControls.Management.StudioSettings.ModifyHowToAdress("howto.aspx#Admin_DNS")%>"><%=Resources.Resource.Help%></a></div>
            </div>
            <div id="studio_enterDnsBox" style="padding: 20px 15px;">
				<%if (!ASC.Core.CoreContext.Configuration.Standalone) { %>
					<div style="padding: 3px 0px;">
							<div style="margin:3px 0;">
							<label for="studio_TenantAlias">
								<%=Resources.Resource.PortalAddress%></label>
							</div>
							<input type="text" class="textEdit" maxlength="150" style="width: 323px;" id="studio_TenantAlias" value="<%=ASC.Core.CoreContext.TenantManager.GetCurrentTenant().TenantAlias??string.Empty%>" />
							<span id="studio_TenantBaseDomain"><%=TenantBaseDomain%></span>
					</div>
                <%} %>
                <div class="clearFix" style="padding: 3px 0px;">
                        <div class="clearFix" style="margin:3px 0;">
							<input type="checkbox" id="studio_enableDnsName" onclick="jq('#studio_dnsName').attr('disabled', !this.checked);" style="margin-left: 0; float: left;" <%=EnableDnsChange ? "checked='checked'" : "" %>/>
							<label for="studio_enableDnsName" style="float: left; margin-top: 2px; margin-left: 2px;" onselectstart="return false;" onmousedown="return false;" ondblclick="return false;">
								<%=Resources.Resource.CustomDomainName%>
							</label>
                        </div>
                        <div class="clearFix">
							<input type="text" class="textEdit" maxlength="150" style="width: 400px;" id="studio_dnsName" value="<%=ASC.Core.CoreContext.TenantManager.GetCurrentTenant().MappedDomain??string.Empty%>" <%=EnableDnsChange ? "" : "disabled='disabled'" %>/>
                        </div>
                </div>
                <div class="clearFix" style="margin-top: 20px;">
                    <a class="baseLinkButton<%=(SetupInfo.WorkMode == WorkMode.Promo?" promoAction":"") %>" style="float: left;" onclick="StudioManagement.SaveDnsSettings();" href="javascript:void(0);">
                        <%=Resources.Resource.SaveButton %></a>
                </div>
                <p id="dnsChange_sent" style="display:none;"></p>
            </div>
        </asp:PlaceHolder>  
         
        <%--trusted mail domain--%>
         <asp:PlaceHolder ID="_mailDomainSettings" runat="server"></asp:PlaceHolder>