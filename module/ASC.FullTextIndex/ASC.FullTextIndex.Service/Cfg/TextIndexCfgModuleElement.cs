﻿using System.Configuration;
using System.Xml;

namespace ASC.FullTextIndex.Service.Cfg
{
	public class TextIndexCfgModuleElement : ConfigurationElement
	{
		[ConfigurationProperty("name", IsKey = true, IsRequired = true)]
		public string Name
		{
			get { return (string)this["name"]; }
			set { this["name"] = value; }
		}

		[ConfigurationProperty("connectionStringName")]
		public string ConnectionStringName
		{
			get { return (string)this["connectionStringName"]; }
			set { this["connectionStringName"] = value; }
		}

		public string Select
		{
			get;
			private set;
		}

		protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
		{
			reader.MoveToAttribute("name");
			Name = reader.Value;

			ConnectionStringName = reader.MoveToAttribute("connectionStringName") ? reader.Value : Name;

			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.EndElement) break;
				if (reader.NodeType == XmlNodeType.CDATA) Select = reader.Value;
			}
		}
	}
}
