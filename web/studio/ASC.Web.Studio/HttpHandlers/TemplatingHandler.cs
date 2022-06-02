using System;
using System.Data;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using System.Xml.Linq;
using ASC.Data.Storage;

namespace ASC.Web.Studio.HttpHandlers
{
  public class TemplatingHandler : IHttpHandler
  {
    private static Dictionary<String, XmlDocument> _xslTemplates = null;

    public bool IsReusable
    {
      // To enable pooling, return true here.
      // This keeps the handler in memory.
      get { return false; }
    }

    public String GetModuleResource(String ResourceClassTypeName, String ResourseKey)
    {
      if (string.IsNullOrEmpty(ResourseKey))
        return string.Empty;
      try
      {
        return (String)Type.GetType(ResourceClassTypeName).GetProperty(ResourseKey, BindingFlags.Static | BindingFlags.Public).GetValue(null, null);
      }
      catch (Exception)
      {
        return String.Empty;
      }
    }

    public void ProcessRequest(HttpContext context)
    {
      if (_xslTemplates == null)
        _xslTemplates = new Dictionary<String, XmlDocument>();

      if (String.IsNullOrEmpty(context.Request["id"]) || String.IsNullOrEmpty(context.Request["name"]))
        return;

      String TemplateName = context.Request["name"];
      String TemplatePath = context.Request["id"];
      //String TemlateKey = String.Format("{0}-{1}", TemplateName, CultureInfo.CurrentCulture.Name);
      //if (_xslTemplates.ContainsKey(TemlateKey))
      //{
      //  context.Response.ContentType = "text/xml";
      //  context.Response.Write(_xslTemplates[TemlateKey].InnerXml);
      //  return;
      //}

      XmlDocument Template = new XmlDocument();
      try
      {
        Template.Load(context.Server.MapPath(String.Format("~{0}{1}.xsl", TemplatePath, TemplateName)));
      }
      catch (Exception)
      {
        return;
      }
      if (Template.GetElementsByTagName("xsl:stylesheet").Count == 0)
        return;

      Dictionary<String, String> Aliases = new Dictionary<String, String>();

      XmlNodeList RegisterAliases = Template.GetElementsByTagName("register");
      while ((RegisterAliases = Template.GetElementsByTagName("register")).Count > 0)
      {
        XmlNode RegisterAlias = RegisterAliases.Item(0);
        if (!String.IsNullOrEmpty(RegisterAlias.Attributes["alias"].Value) && !String.IsNullOrEmpty(RegisterAlias.Attributes["type"].Value))
          Aliases.Add(RegisterAlias.Attributes["alias"].Value, RegisterAlias.Attributes["type"].Value);
        RegisterAlias.ParentNode.RemoveChild(RegisterAlias);
      }

      XmlNodeList CurrentResources = Template.GetElementsByTagName("resource");

      while ((CurrentResources = Template.GetElementsByTagName("resource")).Count > 0)
      {
        XmlNode CurrentResource = CurrentResources.Item(0);
        if (!String.IsNullOrEmpty(CurrentResource.Attributes["name"].Value))
        {
          String[] FullName = CurrentResource.Attributes["name"].Value.Split('.');
          if (FullName.Length == 2 && Aliases.ContainsKey(FullName[0]))
          {
            XmlText ResourceValue = Template.CreateTextNode(GetModuleResource(Aliases[FullName[0]], FullName[1]));
            CurrentResource.ParentNode.InsertBefore(ResourceValue, CurrentResource);
          }
        }
        CurrentResource.ParentNode.RemoveChild(CurrentResource);
      }

      //_xslTemplates.Add(TemlateKey, Template);

      context.Response.ContentType = "text/xml";
      context.Response.Write(Template.InnerXml);
    }
  }
}
