using PeterHan.PLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontUseMutatedPlants
{
    [SkipSaveFileSerialization]
    internal class AllowUserMutationsButton : KMonoBehaviour
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
            bool isAllowed = gameObject.GetComponent<AllowUseMutationsComp>() && gameObject.GetComponent<AllowUseMutationsComp>().AllowUsageOfMutations;
            if (isAllowed)
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
            var allowUseMutationsComp = gameObject.GetComponent<AllowUseMutationsComp>();
            if (allowUseMutationsComp == null)
            {
                PUtil.LogWarning("AllowUseMutationsComp == null in OnToggleEnable");
                return;
            }
            allowUseMutationsComp.AllowUsageOfMutations = true;
            Game.Instance.userMenu.Refresh(base.gameObject);
        }

        private void OnToggleDisable()
        {
            var allowUseMutationsComp = gameObject.GetComponent<AllowUseMutationsComp>();
            if (allowUseMutationsComp == null)
            {
                PUtil.LogWarning("AllowUseMutationsComp == null in OnToggleDisable");
                return;
            }
            allowUseMutationsComp.AllowUsageOfMutations = false;
            Game.Instance.userMenu.Refresh(base.gameObject);
        }
    }
}
