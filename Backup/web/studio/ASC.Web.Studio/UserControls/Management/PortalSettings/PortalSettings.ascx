<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PortalSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.PortalSettings" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ Import Namespace="ASC.Web.Core.Users" %>

<div class="headerBase borderBase" id="portalSettingsTitle">
        <%=Resources.Resource.PortalSettings%>
    </div>
<div id="studio_portalSettingsInfo">
    
</div>

<div id="studio_portalSettingsBox">
    <div class="clearFix">
       
        <input id="privatePortal" type="radio" value="0" name="portalType" <%=_isPublic?"":"checked=\"checked\""%>/>     
        <label class="headerBaseSmall" for="privatePortal"><%=Resources.Resource.PrivatePortal%></label>        
        
        <input id="publicPortal" type="radio" value="1" name="portalType" <%=_isPublic?"checked=\"checked\"":""%>/>     
        <label class="headerBaseSmall" for="publicPortal"><%=Resources.Resource.PublicPortal%></label>        
       
    </div>
    
    <div id="publicPortalDescription" class="description"  <%=_isPublic?"":"style=\"display:none;\""%>>
        <%=Resources.Resource.PublicPortalDescription%>
        
        <div class="publicItems">
        <asp:Repeater runat="server" ID="_productRepeater">
            <ItemTemplate>                
                <div class="clearFix">
                    <input class="publicItem" id="publicItem_<%#Eval("Id")%>" value="<%#Eval("Id")%>" type="checkbox"  <%#((bool)Eval("Public"))?"checked=checked":""%> />
                    <label for="publicItem_<%#Eval("Id")%>"><%#Eval("Name")%></label>
                </div>
            </ItemTemplate>
        </asp:Repeater>
        </div>
                
    </div>
    
    <div id="privatePortalDescription" class="description" <%=_isPublic?"style=\"display:none;\"":""%>>
        <%=Resources.Resource.PrivatePortalDescription%>
    </div>
    
    <div class="btnBox clearFix"> 
        <a id="savePortalSettingsBtn" href="javascript:void(0);" class="baseLinkButton"><%=Resources.Resource.SaveButton%></a>
    </div>

</div>