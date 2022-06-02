using System;
using System.Reflection;

namespace ASC.PhotoManager
{
	public interface ISettings
    {
        Guid ID { get; }
        ISettings GetDefault();
    }

}
