using HarmonyLib;
using Unity;
using UnityEngine;
using System.Collections.Generic;
using PeterHan.PLib.Core;
using PeterHan.PLib.Database;
using PeterHan.PLib.Options;
using System;
using System.Linq;

namespace DontUseMutatedPlants
{
    public class Patches : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary();
            new POptions().RegisterOptions(this, typeof(DontUseMutatedPlantsOptions));
            new PLocalization().Register();
        }
          
        static bool ShouldPickupBeBlocked(Pickupable pickup, Storage destination)
        {
            var mutantPlant = pickup?.GetComponent<MutantPlant>();
            if (mutantPlant == null)
                return false;

            if (mutantPlant.IsOriginal)
                return false;

            // always allow seed analyzers
            if (destination.GetComponent<GeneticAnalysisStationConfig>() != null)
                return false;

            if (destination.GetComponent<AllowUseMutationsComp>() is AllowUseMutationsComp comp)
            {
                if (!mutantPlant.IsIdentified || comp.AllowUsageOfMutations == false)
                    return true;
            }

            return false;
        }

        [HarmonyPatch(typeof(FetchAreaChore.StatesInstance), "Begin")]
        public static class FetchAreaChore_Begin
        {
            internal static void Postfix(ref List<Pickupable> ___fetchables, FetchChore ___rootChore)
            {
                ___fetchables = ___fetchables.Where(ele => !ShouldPickupBeBlocked(ele, ___rootChore.destination)).ToList();
            }
        }

        [HarmonyPatch(typeof(FetchManager), "IsFetchablePickup")]
        //[HarmonyPatch(new Type[] { typeof(KPrefabID), typeof(FetchChore), typeof(Storage) }]
        public static class FetchManager_IsFetchablePickup2
        {
            internal static void Postfix(ref bool __result, Pickupable pickup, Storage destination)
            {
                if (ShouldPickupBeBlocked(pickup, destination))
                    __result = false;
            }
        }

        [HarmonyPatch(typeof(CookingStationConfig), "DoPostConfigureComplete")]
        public static class Patch_CookingStationConfig_DoPostConfigureComplete
        {
            internal static void Postfix(GameObject go)
            {
                go.AddOrGet<AllowUseMutationsComp>();
            }
        }

        [HarmonyPatch(typeof(GourmetCookingStationConfig), "DoPostConfigureComplete")]
        public static class Patch_GourmetCookingStationConfig_DoPostConfigureComplete
        {
            internal static void Postfix(GameObject go)
            {
                go.AddOrGet<AllowUseMutationsComp>();
            }
        }

        [HarmonyPatch(typeof(MicrobeMusherConfig), "DoPostConfigureComplete")]
        public static class Patch_MicrobeMusherConfig_DoPostConfigureComplete
        {
            internal static void Postfix(GameObject go)
            {
                go.AddOrGet<AllowUseMutationsComp>();
            }
        }

        [HarmonyPatch(typeof(FishFeederConfig), "DoPostConfigureComplete")]
        public static class Patch_FishFeederConfig_DoPostConfigureComplete
        {
            internal static void Postfix(GameObject go)
            {
                go.AddOrGet<AllowUseMutationsComp>();
            }
        }

        [HarmonyPatch(typeof(SolidConduitInboxConfig), "DoPostConfigureComplete")]
        public static class Patch_SolidConduitInboxConfig_DoPostConfigureComplete
        {
            internal static void Postfix(GameObject go)
            {
                if (DontUseMutatedPlantsOptions.Instance.AddToConveyorLoader)
                    go.AddOrGet<AllowUseMutationsComp>();
            }
        }
    }
}
