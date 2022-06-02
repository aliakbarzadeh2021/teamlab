<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserImporter.ascx.cs"
    Inherits="ASC.Web.Studio.UserControls.Users.UserImporter" %>    
<%@ Register Assembly="ASC.Web.Controls" Namespace="ASC.Web.Controls" TagPrefix="ascwc" %>
<%@ Import Namespace="ASC.Web.Studio" %>
<%@ Import Namespace="ASC.Web.Studio.UserControls.Users" %> 
<%@ Import Namespace="ASC.Core.Users"%>
 
 <div id="studio_impEmpDialog" style="display:none;">
 <ascwc:Container ID="_importContainer" runat="server">
 <Header>
 <div id="studio_impEmpTitle"><%=ASC.Web.Studio.Core.Users.CustomNamingPeople.Substitute<Resources.Resource>("UserImport")%></div>
 </Header>
 <Body>
    
    <div id="studio_impEmpContent">
        <div id="studio_importerMessage"></div>
     
        <div id="studio_importerSelector" style="display:none;">            
            <div style="margin-bottom:16px;">
                <%=Resources.Resource.ImportSelectorStepDescription.ReplaceSingleQuote()%>
            </div>
                    
            <div style="margin-bottom:10px; padding-left:10px;">    
                <input id="studio_csv_importer" checked="checked" value="<%=(int)UserImportType.CSV %>" type="radio" name="studio_importerType"/>
                <label for="studio_csv_importer"><%=Resources.Resource.CSV%></label> 
                 <div class="textMediumDescribe" style="padding:5px 25px;">
                    <%=Resources.Resource.CSVImportDescription%>
                </div>            
            </div>
            
            <div style="padding-left:10px;">
                <input id="studio_outlook_importer" value="<%=(int)UserImportType.Outlook %>" type="radio" name="studio_importerType"/>
                <label for="studio_outlook_importer"><%=Resources.Resource.Outlook%></label>   
                 <div class="textMediumDescribe" style="padding:5px 25px;">
                    <%=Resources.Resource.OutlookImportDescription.ReplaceSingleQuote()%>
                </div>     
            </div>
            
            
            
            
         <%--   <div style="padding-left:10px; margin-top:10px;">    
                <input id="studio_ad_importer" value="<%=(int)UserImportType.ActiveDirectory %>" type="radio" name="studio_importerType"/>
                <label for="studio_ad_importer"><%=Resources.Resource.ActiveDirectory%></label>        
                <div class="textMediumDescribe" style="padding:5px 25px;">
                    <%=Resources.Resource.ActiveDirectoryImportDescription %>
                </div>
            </div>--%>
            
         <div class="clearFix" style="margin-top: 16px;">
             <a class="baseLinkButton" href="javascript:void(0);" onclick="UserImporterManager.SelectImporter()"
                 style="float:left;"><%=Resources.Resource.NextButton%></a> 
             
             <a class="grayLinkButton" href="javascript:void(0);"
                     onclick="UserImporterManager.CloseImportDialog()" style="float: left; margin-left: 8px;"><%=Resources.Resource.CancelButton%></a>
         </div>
     </div>
        
        <div id="studio_adImportHolder" style="display:none;">        
            <div style="margin-bottom:16px;">
                <%=Resources.Resource.ActiveDirectoryImportStepDescription%></div>
            <table cellpadding="5" cellspacing="0" style="width: 100%;">
                <tr>
                    <td style="white-space: nowrap; padding-right: 5px;">
                        <%=Resources.Resource.DomainUserName%>:
                    </td>
                    <td style="width: 100%;">
                        <input type="text" class="textEdit" style="width: 100%;" value="<%=_domainUserName%>"
                            id="ad_usrname" />
                    </td>
                </tr>
                <tr>
                    <td style="white-space: nowrap; padding-right: 5px;">
                        <%=Resources.Resource.Password%>:
                    </td>
                    <td>
                        <input type="password" class="textEdit" style="width: 100%;" value="" id="ad_pwd" />
                    </td>
                </tr>
            </table>
            
            <div class="clearFix" style="margin-top: 16px;">
            <a class="baseLinkButton" href="javascript:void(0);" onclick="UserImporterManager.DisplayStep('0')"
                 style="float: left;"><%=Resources.Resource.BackButton%></a> 
                 
             <a class="baseLinkButton" href="javascript:void(0);" onclick="UserImporterManager.ImportFromAD()"
                 style="float: left; margin-left: 8px;"><%=Resources.Resource.NextButton%></a> 
             
             <a class="grayLinkButton" href="javascript:void(0);"
                     onclick="UserImporterManager.CloseImportDialog()" style="float: left; margin-left: 8px;"><%=Resources.Resource.CancelButton%></a>
            </div>
         
        </div>

        <div id="studio_fileImportHolder" style="display:none;">
            
            <div id="studio_csvDescription" style="margin-bottom:16px;">
                <%=string.Format(Resources.Resource.CSVImportStepDescription,"<br/>")%></div>   
                
                
            <div id="studio_outlookDescription" style="margin-bottom:16px;">
                <%=Resources.Resource.OutlookImportStepDescription.ReplaceSingleQuote()%></div>                  
            <div id="studio_impEmpFileName" style="margin-bottom:5px;"></div>
            <div class="clearFix">
                <a id="studio_importerFileUploader" class="baseLinkButton" href="javascript:void(0);"><%=Resources.Resource.SelectFileButton%></a>
            </div>         
            
            <div class="clearFix" style="margin-top: 30px;">
            <a class="baseLinkButton" href="javascript:void(0);" onclick="UserImporterManager.DisplayStep('0')"
                 style="float: left;"><%=Resources.Resource.BackButton%></a> 
                 
             <a class="baseLinkButton" href="javascript:void(0);" onclick="UserImporterManager.ImportFromFile()"
                 style="float: left; margin-left: 8px;"><%=Resources.Resource.NextButton%></a> 
             
             <a class="grayLinkButton" href="javascript:void(0);"
                     onclick="UserImporterManager.CloseImportDialog()" style="float: left; margin-left: 8px;"><%=Resources.Resource.CancelButton%></a>
            </div>
        </div>
       
         <div id="studio_confirmImportHolder" style="display:none;">             
             <table cellpadding="3" cellspacing="0" style="width: 100%; margin-top: 2px;">
                 <tr class="headerColumn">
                     <td style="width: 85px; padding-bottom: 10px; padding-left: 4px;">
						<input type="checkbox" id="UsersImporterSelectAllCheckbox" style="float: left;"/>
						<label for="UsersImporterSelectAllCheckbox" onselectstart="return false;" style="float: left; margin: 2px 0px 0px 5px;"><%=Resources.Resource.AddButton%></label>
                     </td>
                     <td style="width: 100px; padding-bottom: 10px;">
						<input type="checkbox" id="UsersImporterInviteSelectAllCheckbox" style="float: left;"/>
						<label for="UsersImporterInviteSelectAllCheckbox" onselectstart="return false;" style="float: left; margin: 2px 0px 0px 5px;"><%=Resources.Resource.InviteButton%></label>
                     </td>
                     <td style="width: 160px; padding-bottom: 10px; padding-left: 5px;">
                         <%=Resources.Resource.FirstName%>
                     </td>
                     <td style="width: 160px; padding-bottom: 10px; padding-left: 3px;">
                         <%=Resources.Resource.LastName%>
                     </td>
                     <td style="padding-bottom: 10px; padding-left: 1px;">
                         <%=Resources.Resource.Email%>
                     </td>
                 </tr>
             </table>
            
             <div class="borderBase" id="studio_importList" style="height: 300px; overflow: hidden; overflow-x: hidden; overflow-y: auto;">
             </div>
             
             <div class="clearFix textMediumDescribe">
				<%=Resources.Resource.InviteNamelessContactsLabel%>
             </div>
             
             <%--Existing users count label--%>
             <div class="clearFix" style="margin-top: 8px;" id="ExistingUsersCountLabel">&nbsp;</div>
             <input type="hidden" id="ExistingUsersCountLabelHidden" value="<%=Resources.Resource.AlreadyExistingUsersCountLabel%>" />
             
             <%if(HasDepartments) { %>
				 <div class="clearFix" style="margin: 20px 0 0 -3px;">
					<input type="checkbox" id="ImportContactsAddToDepChk" style="float: left;"/>
					<label for="ImportContactsAddToDepChk" style="float: left; margin-left: 3px; margin-top: 2px;"><%=Resources.Resource.AddTo%></label>
					<div style="float: left; width: 310px; margin-left: 15px; margin-top: -2px;">
						<select id="ImoportContactsDepartmentSelect" class="comboBox" >
							<%=RenderDepartOptions()%>
						</select>
					</div>
				 </div>
             <%} %>
             
             <div class="clearFix" style="margin-top: 22px;">
				<a class="baseLinkButton" id="studio_impConfirmBack" href="javascript:UserImporterManager.DisplayStep('0');" style="float: left; padding-left: 25px;  padding-right: 25px;"><%=Resources.Resource.BackButton%></a>				
				<%--<a class="baseLinkButton" href="javascript:void(0);" onclick="UserImporterManager.ConfirmUserImport()" style="float: left; margin-left: 8px;"><%=Resources.Resource.ImportButton%></a>--%>             
				
				<a class="baseLinkButton" href="javascript:void(0);" onclick="inpurtUsersFromPopup();" style="float: left;"><%=Resources.Resource.ImportButton%></a>
				<a class="grayLinkButton" href="javascript:void(0);" onclick="UserImporterManager.CloseImportDialog()" style="float: left; margin-left: 8px;"><%=Resources.Resource.CancelButton%></a>
				
                <div style="float:left; margin-left:20px;">                
					<input type="checkbox" id="studio_inviteWithFullAcess" <%=DefaultInviteWithFullAccess?"checked='checked'":"" %> style="vertical-align:middle" />
					<label for="studio_inviteWithFullAcess" style="margin-left:3px;"><%=Resources.Resource.InviteFullAccessPrivilegesTitle%></label>
                </div>
            </div>
         </div>
         
     </div>
    
     <div id="studio_impEmpMessage" style="padding: 20px 0px; text-align: center; display: none;">
            <%= Resources.Resource.FinishImportUserTitle%>
    </div>

</Body>
</ascwc:Container>
</div>