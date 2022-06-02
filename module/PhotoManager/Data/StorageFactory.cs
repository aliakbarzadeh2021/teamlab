using ASC.Core;
using ASC.PhotoManager.Model;

namespace ASC.PhotoManager.Data
{
	public static class StorageFactory
	{
		public static string Id = "photo";

		public static IImageStorage GetStorage()
		{
            return new ImageStorage2("photo", CoreContext.TenantManager.GetCurrentTenant().TenantId);
		}
	}
}
