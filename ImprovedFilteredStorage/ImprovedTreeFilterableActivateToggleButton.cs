using UnityEngine;
using PeterHan.PLib.UI;
using PeterHan.PLib.Core;
using System.Linq;

namespace ImprovedFilteredStorage
{
    [SkipSaveFileSerialization]
    public class ImprovedTreeFilterableActivateToggleButton : KMonoBehaviour
    {
        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe((int)GameHashes.RefreshUserMenu, OnRefreshUserMenu);
        }
        protected override void OnCleanUp()
        {
            Unsubscribe((int)GameHashes.RefreshUserMenu);
            base.OnCleanUp();
        }

        private void OnRefreshUserMenu(object _)
        {
            bool enabled = gameObject.GetComponent<ImprovedTreeFilterable>() && gameObject.GetComponent<ImprovedTreeFilterable>().Enabled;
            if (enabled)
            {
                Game.Instance.userMenu?.AddButton(gameObject, new KIconButtonMenu.ButtonInfo("action_power", Strings.BUTTON_DISABLE, OnToggleDisable));
            }
            else
            {
                Game.Instance.userMenu?.AddButton(gameObject, new KIconButtonMenu.ButtonInfo("action_power", Strings.BUTTON_ENABLE, OnToggleEnable));
            }
        }
        private void OnToggleEnable()
        {
            var improvedTreeFilterable = gameObject.GetComponent<ImprovedTreeFilterable>();
            if (improvedTreeFilterable == null)
            {
                PUtil.LogWarning("ImprovedTreeFilterable == null in OnToggleEnable");
                return;
            }

            TreeFilterable treeFilterable = gameObject.GetComponent<TreeFilterable>();
            improvedTreeFilterable.Enabled = true;
            improvedTreeFilterable.UpdateFilters(treeFilterable.AcceptedTags);

            DetailsScreen.Instance.DeactivateSideContent(); // complete ui refresh for the ~3sidescreens I touched, couldnt find a better way
            DetailsScreen.Instance.Refresh(gameObject);
            Game.Instance.userMenu.Refresh(base.gameObject);
            
        }
        private void OnToggleDisable()
        {
            var improvedTreeFilterable = gameObject.GetComponent<ImprovedTreeFilterable>();
            if (improvedTreeFilterable == null)
            {
                PUtil.LogWarning("ImprovedTreeFilterable == null in OnToggleDisable");
                return;
            }

            improvedTreeFilterable.Enabled = false;
            var userControlledCapacity = improvedTreeFilterable.userControlledCapacity;
            if (userControlledCapacity != null)
                userControlledCapacity.UserMaxCapacity = userControlledCapacity.MaxCapacity;

            TreeFilterable treeFilterable = gameObject.GetComponent<TreeFilterable>();
            if (treeFilterable != null)
                treeFilterable.UpdateFilters(new System.Collections.Generic.HashSet<Tag>(improvedTreeFilterable.GetAcceptedElements().Keys)); // regenerate fetchlist, cause we likely changed max capacity

            DetailsScreen.Instance.DeactivateSideContent(); // complete ui refresh for the ~3sidescreens I touched, couldnt find a better way
            DetailsScreen.Instance.Refresh(gameObject);
            Game.Instance.userMenu.Refresh(base.gameObject);
        }
    }
}

