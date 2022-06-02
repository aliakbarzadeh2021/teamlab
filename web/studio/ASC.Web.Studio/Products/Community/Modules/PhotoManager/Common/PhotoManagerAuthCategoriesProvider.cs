using ASC.Common.Security.Authorizing;

namespace ASC.Web.Community.PhotoManager
{
	public class PhotoManagerAuthCategoriesProvider : IAuthCategoriesProvider {

		public virtual AuthCategory[] GetAuthCategories() {
			return ASC.PhotoManager.PhotoConst.AuthorizingCategories;
		}
	}
}
