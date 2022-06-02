<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GreetingSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.GreetingSettings" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
        
<div class="headerBase borderBase greetingTitle">
        <%=Resources.Resource.GreetingSettingsTitle%>
</div>
<div id="studio_setInfGreetingSettingsInfo"></div>
<div id="studio_greetingSettingsBox" class="clearFix">
        
        <div class="clearFix">
             <div class="headerBaseSmall" style="text-align:right; padding-right:10px; width:60px; float:left;">
              <%=Resources.Resource.GreetingTitle%>:                         
              </div>
               
             <div style="float:left;">
               <input type="text" class="textEdit" maxlength="150" id="studio_greetingHeader" value="<%=HttpUtility.HtmlEncode(_tenantInfoSettings.CompanyName)%>"/>
             </div>
         </div>
                        
        <div class="clearFix" style="margin-top:15px;">
            <div class="headerBaseSmall" style="text-align:right; padding-right:10px; width:60px; float:left;">
                <%=Resources.Resource.GreetingLogo%>:                                 
            </div>
            
            <div style="float:left;">
                <div class="clearFix">
                            <div style="float:left; width:280px;">
                                <img id="studio_greetingLogo" class="borderBase" style="padding:10px;" src="<%=_tenantInfoSettings.GetAbsoluteCompanyLogoPath()%>"/>                                    
                            </div>
                            <div style="margin-left:10px; float:left;"> 
                                <div class="textBigDescribe" style="margin-bottom:10px; width:200px;">
                                    <%=Resources.Resource.LogoSettingsLimits%>
                                </div>
                                <input type="hidden" id="studio_greetingLogoPath" value=""/>                    
                                <a id="studio_logoUploader" href="javascript:void(0);"><%=Resources.Resource.ChangeLogoButton %></a>
                            </div>
                </div>
            </div>               
            
        </div>                 

     <div class="clearFix" style="margin-top:20px;">
                <a id="saveGreetSettingsBtn" class="baseLinkButton<%=(SetupInfo.WorkMode == WorkMode.Promo?" promoAction":"") %>"  href="javascript:void(0);" ><%=Resources.Resource.SaveButton %></a>
                <a id="restoreGreetSettingsBtn" class="grayLinkButton<%=(SetupInfo.WorkMode == WorkMode.Promo?" promoAction":"") %>" href="javascript:void(0);" ><%=Resources.Resource.RestoreDefaultButton%></a>
     </div>               
</div>
      