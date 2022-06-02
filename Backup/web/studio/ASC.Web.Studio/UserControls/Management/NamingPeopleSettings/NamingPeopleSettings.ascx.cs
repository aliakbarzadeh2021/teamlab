using System;
using System.Collections.Generic;
using System.Web.UI;
using AjaxPro;
using ASC.Data.Storage;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Core.Users;
using ASC.Core;
using ASC.Web.Studio.Core;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("NamingPeopleController")]
    public partial class NamingPeopleSettings : System.Web.UI.UserControl
    {
        public static string Location { get { return "~/UserControls/Management/NamingPeopleSettings/NamingPeopleSettings.ascx"; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(this.GetType());
            Page.ClientScript.RegisterClientScriptInclude(typeof(string), "peoplename_script", WebPath.GetPath("usercontrols/management/namingpeoplesettings/js/namingpeople.js"));
            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "namingpeople_style", "<link rel=\"stylesheet\" type=\"text/css\" href=\"" + WebSkin.GetUserSkin().GetAbsoluteWebPath("usercontrols/management/namingpeoplesettings/css/<theme_folder>/namingpeople.css") + "\">", false);
            
            var schemas = new List<object>();
            var currentSchemaId = CustomNamingPeople.Current.Id;

            foreach(var schema in CustomNamingPeople.GetSchemas())
            {
                schemas.Add(new
                {
                    Id = schema.Key,
                    Name = schema.Value,
                    Current = string.Equals(schema.Key, currentSchemaId, StringComparison.InvariantCultureIgnoreCase)
                });
            }           

            namingSchemaRepeater.DataSource = schemas;
            namingSchemaRepeater.DataBind();
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object GetPeopleNames(string schemaId)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

             var names = CustomNamingPeople.GetPeopleNames(schemaId);

             return new
             {
                 Id = names.Id,
                 UserCaption = names.UserCaption,
                 UsersCaption = names.UsersCaption,
                 GroupCaption = names.GroupCaption,
                 GroupsCaption = names.GroupsCaption,
                 UserPostCaption = names.UserPostCaption,
                 RegDateCaption = names.RegDateCaption,
                 GroupHeadCaption = names.GroupHeadCaption,
                 GlobalHeadCaption = names.GlobalHeadCaption
             };

        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SaveNamingSettings(string schemaId)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                CustomNamingPeople.SetPeopleNames(schemaId);
                return new { Status = 1, Message = Resources.Resource.SuccessfullySaveSettingsMessage };

            }
            catch (Exception e)
            {
                return new { Status = 0, Message = e.Message };
            }         
        }

    
        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object SaveCustomNamingSettings(string usrCaption, string usrsCaption, string grpCaption, string grpsCaption,
                                               string usrStatusCaption, string regDateCaption, 
                                               string grpHeadCaption, string globalHeadCaption) 
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var names = new PeopleNamesItem();
                if (String.IsNullOrEmpty((usrCaption ?? "").Trim())
                    || String.IsNullOrEmpty((usrsCaption ?? "").Trim())
                    || String.IsNullOrEmpty((grpCaption ?? "").Trim())
                    || String.IsNullOrEmpty((grpsCaption ?? "").Trim())
                    || String.IsNullOrEmpty((usrStatusCaption ?? "").Trim())
                    || String.IsNullOrEmpty((usrsCaption ?? "").Trim())
                    || String.IsNullOrEmpty((usrsCaption ?? "").Trim())
                    || String.IsNullOrEmpty((usrsCaption ?? "").Trim()))
                {
                    throw new Exception(Resources.Resource.ErrorEmptyFields);
                }


                names.UserCaption = usrCaption.Trim();
                names.UsersCaption = usrsCaption.Trim();
                names.GroupCaption = grpCaption.Trim();
                names.GroupsCaption = grpsCaption.Trim();
                names.UserPostCaption = usrStatusCaption.Trim();
                names.RegDateCaption = regDateCaption.Trim();
                names.GroupHeadCaption = grpHeadCaption.Trim();
                names.GlobalHeadCaption = globalHeadCaption.Trim();                

                CustomNamingPeople.SetPeopleNames(names);

                return new { Status = 1, Message = Resources.Resource.SuccessfullySaveSettingsMessage };

            }
            catch (Exception e)
            {
                return new { Status = 0, Message = e.Message };
            }            
        }
    }
}