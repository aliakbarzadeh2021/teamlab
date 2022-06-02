using System.Configuration;

namespace ASC.FullTextIndex.Service.Cfg
{
	public class TextIndexCfgModuleCollection : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new TextIndexCfgModuleElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((TextIndexCfgModuleElement)element).Name;
		}
	}
}
