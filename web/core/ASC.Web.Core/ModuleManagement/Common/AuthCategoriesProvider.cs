using System.Collections.Generic;
using ASC.Common.Security.Authorizing;


namespace ASC.Web.Core.ModuleManagement.Common {

	public class EmptyAuthCategoriesProvider : IAuthCategoriesProvider {

		public EmptyAuthCategoriesProvider() {
			
		}

		public virtual AuthCategory[] GetAuthCategories() {
			return new AuthCategory[0];
		}
	}
}
