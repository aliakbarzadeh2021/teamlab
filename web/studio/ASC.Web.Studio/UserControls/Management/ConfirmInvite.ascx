<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfirmInvite.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.ConfirmInvite" %>

    <asp:PlaceHolder runat="server" ID="_confirmHolder"> 
    
    <div class="tintMedium borderBase" style="border-right:none; border-left:none; border-bottom:none; margin:20px 0px; width: 400px; text-align:center;">
        <div class="headerBase" style="padding:10px 0px;"><%=Resources.Resource.JoinTitle%></div>
        <div class="textBigDescribe" style="padding-bottom:15px;"><%=Resources.Resource.ConfirmEmpInviteTitle%></div>
    </div>    
    
 
        <div aling="left" style="width: 400px; text-align:left;">
        
            <%--Email/Login--%>
            <div class="property clearFix">
                <div>                    
                        <%=Resources.Resource.EmailAsLogin%>:
                </div>
                <div style="margin-top:3px;">
                    <input type="text" id="studio_confirm_Login" value="<%=_loginEmailView%>" style="width:360px;" name="loginEmailInput" class="pwdLoginTextbox" />
                </div>
            </div>
            
            <%--FirstName--%>
            <div class="property clearFix" style="margin-top:15px;">
                <div>                    
                        <%=Resources.Resource.FirstName%>:
                </div>
                <div style="margin-top:3px;">
                    <input type="text" id="studio_confirm_FirstName" value="<%=_firstName%>" style="width:360px;" name="firstnameInput" class="pwdLoginTextbox" />
                </div>
            </div>
            
            <%--LastName--%>
            <div class="clearFix" style="margin-top:15px;">
                <div>                    
                        <%=Resources.Resource.LastName%>:
                </div>
                <div style="margin-top:3px;">
                    <input type="text" id="studio_confirm_LastName" value="<%=_lastName%>" style="width:360px;" name="lastnameInput" class="pwdLoginTextbox" />
                </div>
            </div>
            
            <%--Pwd--%>
            <div class="clearFix" style="margin-top:15px;">
                <div>                    
                        <%=Resources.Resource.InvitePassword%></span>:
                </div>
                <div style="margin-top:3px;">
                    <input type="password" id="studio_confirm_pwd"  value="" style="width:360px;" name="pwdInput" class="pwdLoginTextbox" />
                </div>
            </div>       
            
            <%--RePwd--%>
            <div class="clearFix" style="margin-top:15px;">
                <div>                    
                        <%=Resources.Resource.RePassword%></span>:
                </div>
                <div style="margin-top:3px;">
                    <input type="password" id="studio_confirm_repwd"  value="" style="width:360px;" name="repwdInput" class="pwdLoginTextbox" />
                </div>
            </div>
           
            <div class="clearFix" align="center" style="margin-top:20px;">
                <a class="baseLinkButton" style="width:100px; " href="#" onclick="AuthManager.ConfirmInvite(); return false;"><%=Resources.Resource.LoginRegistryButton%></a>
            </div>           
        </div>
    
    </asp:PlaceHolder>