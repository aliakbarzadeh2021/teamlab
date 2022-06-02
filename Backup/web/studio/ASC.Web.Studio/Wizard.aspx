<%@ Page Language="C#" AutoEventWireup="true" EnableViewState="false" MasterPageFile="~/Masters/StudioTemplate.master"
    CodeBehind="Wizard.aspx.cs" Inherits="ASC.Web.Studio.Wizard" %>
<%@ Import Namespace="ASC.Web.Studio" %>
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<asp:Content ID="WizardPageContent" ContentPlaceHolderID="StudioPageContent" runat="server">
    
    <div align="center">    
    <asp:PlaceHolder runat="server" ID="_adminFormHolder">    
                
        <div class="headerLightMedium clearFix" style="margin-top:40px;">
          <a href="auth.aspx"><img class="logo" src="<%=_tenantInfoSettings.GetAbsoluteCompanyLogoPath()%>" border="0" alt="" /></a>
          <div style="padding:20px 0px;"><%=HttpUtility.HtmlEncode(_tenantInfoSettings.CompanyName)%></div>
        </div>    
        <div class="headerLightBig clearFix" style="margin-top:15px;">
            <%=Resources.Resource.SuccessfullyInstalTitle%>
        </div>
        <div align="left" style="width:600px;">        
        <div style="margin:20px 0px; font-size:14px;">
            <%=string.Format(Resources.Resource.SetAdminStepDescription, "<br/><br/>")%>
        </div>  
        <div id="studio_wizardMessage"></div>        
            
            <%--firstname--%>
                <div style="margin-top:10px;">
                    <%=Resources.Resource.FirstName%>:
                </div>
                <div style="margin-top:3px;">
                    <input  class="pwdLoginTextbox" style="width:560px;" type="text" id="studio_adminFirstName" value="<%=HttpUtility.HtmlEncode(_defaultFirstName)%>" />
                </div>
            
            <%--lastname--%>
             <div style="margin-top:10px;">
                    <%=Resources.Resource.LastName%>:
             </div>
             <div style="margin-top:3px;">
                    <input  class="pwdLoginTextbox" style="width:560px;" type="text" id="studio_adminLastName" value="<%=HttpUtility.HtmlEncode(_defaultLastName)%>" />
              </div>
              
            <%--email--%>
            <div style="margin-top:10px;">
                    <%=Resources.Resource.Email%>:
             </div>
            <div style="margin-top:3px;">
                    <input  class="pwdLoginTextbox" style="width:560px;" type="text" id="studio_adminEmail" value="" />
             </div>
            
            <%--pwd--%>
            <div style="margin-top:10px;">
                    <%=Resources.Resource.Password%>:
             </div>
            <div style="margin-top:3px;">
                    <input  class="pwdLoginTextbox" type="password" style="width:560px;" id="studio_adminPwd" value="" />
             </div>
            
           
        <div class="clearFix" align="center" style="margin-top:30px;">
            <a class="baseLinkButton" style="width:100px;" onclick="AuthManager.CreateAdmin();" href="javascript:void(0);"><%=Resources.Resource.ContinueButton%></a>
        </div>
        
    </div>
    </asp:PlaceHolder>
    </div>
    
</asp:Content>