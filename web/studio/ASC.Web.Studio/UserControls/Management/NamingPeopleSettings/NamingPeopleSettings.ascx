<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NamingPeopleSettings.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.NamingPeopleSettings" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>

 <div class="headerBase borderBase" id="namingPeopleTitle">
                <%=Resources.Resource.NamingPeopleSettings%>
            </div>
<div id="studio_namingPeopleInfo">
</div>
<div id="studio_namingPeopleBox">
       
     <table>   
     
     <%--schema--%>
     <tr valign="top">
        
        <td class="caption headerBaseSmall schemaCaption"><%=Resources.Resource.NamingPeopleSchema%>:</td>
        <td><select id="namingPeopleSchema" class="comboBox">
          <asp:Repeater runat="server" ID="namingSchemaRepeater">            
                <ItemTemplate>
                    <option <%#((bool)Eval("Current"))?"selected":""%> value="<%#(String)Eval("Id")%>">
                      <%#HttpUtility.HtmlEncode((String)Eval("Name"))%>
                    </option>
                </ItemTemplate>
           </asp:Repeater>
        </select>   </td>
        <td></td><td></td>
       
      </tr>  
      
      <%--user--%>
      <tr>
        
        <td class="caption"><%=Resources.Resource.UserCaption%>:</td>
        <td><input class="textEdit" id="usrcaption" type="text" maxlength="30"/></td>
        <td><class="caption"><%=Resources.Resource.UsersCaption%>:</td>
        <td><input class="textEdit" id="usrscaption" type="text" maxlength="30"/></td>
      </tr>
      
      <%--group--%>
      <tr>
        <td class="caption"><%=Resources.Resource.GroupCaption%>:</td>
        <td><input class="textEdit" id="grpcaption" type="text" maxlength="30"/></td>        
        <td class="caption"><%=Resources.Resource.GroupsCaption%>:</td>
        <td><input class="textEdit" id="grpscaption" type="text" maxlength="30"/></td>
      </tr>
      
      <%--post--%>
      <tr>        
        <td class="caption"><%=Resources.Resource.UserStatusCaption%>:</td>
        <td><input class="textEdit" id="usrstatuscaption" type="text" maxlength="30"/></td>
        <td></td><td></td>
      </tr>
      
      <%--red date--%>
      <tr>
        <td class="caption"><%=Resources.Resource.RegDateCaption%>:</td>
        <td><input class="textEdit" id="regdatecaption" type="text" maxlength="30"/></td>
        <td></td><td></td>
      </tr>
      
       <%--group head--%>
      <tr>
        <td class="caption"><%=Resources.Resource.GroupHeadCaption%>:</td>
        <td><input class="textEdit" id="grpheadcaption" type="text" maxlength="30"/></td>
        <td></td><td></td>
      </tr>
      
      <%--global head--%>
      <tr>
        <td class="caption"><%=Resources.Resource.GlobalHeadCaption%>:</td>
        <td><input class="textEdit" id="globalheadcaption" type="text" maxlength="30"/></td>
        <td></td><td></td>
      </tr>
               
     </table>
         
     <div class="btnBox clearFix">
            <a id="saveNamingPeopleBtn" class="baseLinkButton<%=(SetupInfo.WorkMode == WorkMode.Promo?" promoAction":"") %>" href="javascript:void(0);"><%=Resources.Resource.SaveButton %></a>
     </div>
</div>    