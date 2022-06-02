using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using ASC.Web.Core.ModuleManagement;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Controls.Common;

namespace ASC.Web.Community.Blogs.Controls
{
    public partial class ActionContainer : BaseUserControl
    {
        public PlaceHolder ActionsPlaceHolder { get; set; }

        public ActionContainer()
        {
            ActionsPlaceHolder = new PlaceHolder();
        }

        protected void Page_Load(object sender, EventArgs e)
        {            
            List<Shortcut> shortcuts = new List<Shortcut>();
            var currentModule = UserOnlineManager.Instance.GetCurrentModule() as Module;
            if (currentModule != null)
            {
                foreach (var shCateg in currentModule.ShortcutCategoryCollection)
                    shortcuts.AddRange(shCateg.GetCurrentShortcuts());
            }

            if (shortcuts.Count == 0)
                return;

            var actions = shortcuts.FindAll(s => s.Type == ShortcutType.Action);
            var navigations = shortcuts.FindAll(s => s.Type == ShortcutType.Navigation);

            if (actions.Count > 0 || ActionsPlaceHolder.Controls.Count > 0)
            {
                var actionsControl = new SideActions();
                foreach (var shortcut in actions)
                {
                    actionsControl.Controls.Add(new NavigationItem()
                    {
                        Name = shortcut.Name,
                        Description = shortcut.Description,                        
                        URL = shortcut.AbsoluteActionURL,
                        IsPromo = (SetupInfo.WorkMode== WorkMode.Promo)
                    });
                }
                _actionHolder.Controls.Add(actionsControl);

                if (ActionsPlaceHolder.Controls.Count > 0)
                    actionsControl.Controls.Add(ActionsPlaceHolder);
            }

            if (navigations.Count > 0)
            {
                var navigationControl = new SideNavigator();
                foreach (var shortcut in navigations)
                {
                    navigationControl.Controls.Add(new NavigationItem()
                    {
                        Name = shortcut.Name,
                        Description = shortcut.Description,                        
                        URL = shortcut.AbsoluteActionURL

                    });
                }

                _actionHolder.Controls.Add(navigationControl);

            }

        }
    }
}