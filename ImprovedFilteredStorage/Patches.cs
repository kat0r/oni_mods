using HarmonyLib;
using Unity;
using UnityEngine;
using System.Collections.Generic;
using SideScreenRef = DetailsScreen.SideScreenRef;
using PeterHan.PLib.UI;
using PeterHan.PLib.Core;
using PeterHan.PLib.Detours;
using PeterHan.PLib.Database;

namespace ImprovedFilteredStorage
{
    public class Patches : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary();
            new PLocalization().Register();
        }

        [HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
        public static class DetailsScreen_OnPrefabInit_Patch
        {
            internal static void Postfix(List<SideScreenRef> ___sideScreens, GameObject ___sideScreenContentBody)
            {
                PUIUtils.AddSideScreenContent<ImprovedTreeFilterableSideScreen>();
            }
        }

        //[HarmonyPatch(typeof(TreeFilterable), "OnSpawn")]
        //public static class TreeFilterable_OnSpawn_Patch
        //{
        //    internal static void Postfix(TreeFilterable __instance)
        //    {
        //        __instance.gameObject.AddOrGet<ImprovedTreeFilterableActivateToggleButton>();
        //    }
        //}

        [HarmonyPatch(typeof(TreeFilterable), "UpdateFilters")]
        public static class TreeFilterable_UpdateFilters_Patch
        {
            internal static void Postfix(TreeFilterable __instance, IList<Tag> filters)
            {
                var improvedTreeFilterable = __instance.gameObject.GetComponent<ImprovedTreeFilterable>();
                if (improvedTreeFilterable != null && improvedTreeFilterable.Enabled)
                {
                    improvedTreeFilterable.UpdateFilters(filters);
                }
            }
        }
        [HarmonyPatch(typeof(FilteredStorage), "OnFilterChanged")]
        private static class FilteredStorage_OnFilterChanged
        {
            private static readonly IDetouredField<FilteredStorage, KMonoBehaviour> ROOT = PDetours.DetourField<FilteredStorage, KMonoBehaviour>("root");

            internal static void Postfix(FilteredStorage __instance, Tag[] tags)
            {
                var improvedTreeFilterable = ROOT.Get(__instance).GetComponent<ImprovedTreeFilterable>();
                if (improvedTreeFilterable != null && improvedTreeFilterable.Enabled)
                {
                    improvedTreeFilterable.GenerateFetchList(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(FilteredStorage), "OnStorageChanged")] //(object data)
        private static class FilteredStorage_OnStorageChanged
        {
            private static readonly IDetouredField<FilteredStorage, KMonoBehaviour> ROOT = PDetours.DetourField<FilteredStorage, KMonoBehaviour>("root");

            internal static void Postfix(FilteredStorage __instance)
            {
            var improvedTreeFilterable = ROOT.Get(__instance).GetComponent<ImprovedTreeFilterable>();
                if (improvedTreeFilterable != null && improvedTreeFilterable.Enabled)
                {
                    //PUtil.LogDebug("FilteredStorage_OnStorageChanged -> GenerateFetchList");
                    improvedTreeFilterable.GenerateFetchList(__instance);
                }
            }
        }

        // dont show the usual slider/max capacity side screen
        [HarmonyPatch(typeof(CapacityControlSideScreen), "IsValidForTarget")]
        public static class CapacityControlSideScreen_IsValidForTarget
        {
            internal static void Postfix(GameObject target, ref bool __result)
            {
                if (target.GetComponent<ImprovedTreeFilterable>() && target.GetComponent<ImprovedTreeFilterable>().Enabled)
                {
                    __result = false;
                }
            }
        }

        // the buildings we actually care about
        [HarmonyPatch(typeof(BuildingLoader), "CreateBuildingComplete")]
        public class Patch_BuildingConfigManager_OnPrefabInit
        {
            // StorageLocker and OrbitalCargoModule use this
            internal static void Postfix(ref GameObject __result, GameObject go, BuildingDef def)
            {
                if (__result.GetComponent<StorageLocker>() != null)
                {
                    __result.AddOrGet<ImprovedTreeFilterable>();
                }
            }
        }

        [HarmonyPatch(typeof(RationBoxConfig), "ConfigureBuildingTemplate")]
        public static class Patch_RationBoxConfig_ConfigureBuildingTemplate
        {
            internal static void Postfix(GameObject go, Tag prefab_tag)
            {
                go.AddOrGet<ImprovedTreeFilterable>();
            }
        }

        [HarmonyPatch(typeof(RefrigeratorConfig), "DoPostConfigureComplete")]
        public static class Patch_RefrigeratorConfig_DoPostConfigureComplete
        {
            internal static void Postfix(GameObject go)
            {
                go.AddOrGet<ImprovedTreeFilterable>();
            }
        }

        [HarmonyPatch(typeof(StorageLockerSmartConfig), "DoPostConfigureComplete")]
        public static class Patch_StorageLockerSmartConfig_DoPostConfigureComplete
        {
            internal static void Postfix(GameObject go)
            {
                go.AddOrGet<ImprovedTreeFilterable>();
            }
        }
    }
}
