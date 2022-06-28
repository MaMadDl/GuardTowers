using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using RimWorld.Planet;
using System;
using System.Linq;

namespace NGT
{
    internal class Patches
    {
        public static void CheckOrUpdateGameOver_Postfix(GameEnder __instance)
        {
            var maps = Find.Maps;
            foreach (var map in maps)
            {
                var thingList = map.listerThings.ThingsInGroup(ThingRequestGroup.ThingHolder);
                foreach (var thing in thingList)
                {
                    if (!(thing is BaseGuardTower { HasAnyContents: true }))
                    {
                        continue;
                    }

                    __instance.gameEnding = false;
                    return;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill)
     , new[] { typeof(DamageInfo?), typeof(Hediff) })]
    static class guardtower_deathEvent
    {
        static void Prefix(Pawn __instance)
        {
            var towerContainer = __instance.MapHeld.listerBuildings.allBuildingsColonist.OfType<BaseGuardTower>();
            var towerHolder = towerContainer.Where(t => t.GetInner().Contains(__instance)).First();
            towerHolder.GetInner().TryDrop(__instance, towerHolder.InteractionCell, __instance.MapHeld, ThingPlaceMode.Near, out _);
        }


    }

    ////[HarmonyPatch(typeof(VerbProperties),"isMortar",MethodType.Setter)]
    //static class GuardTowerUnObstractedPatch
    //{
    //    static System.Reflection.MethodBase TargetMethod()
    //    {
    //        return typeof(VerbProperties).GetMethod("isMortar",
    //            System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).MakeGenericMethod(typeof(bool));
    //    }
    //    static void Postfix()

    //    {
    //        FileLog.Log("workssss???");
    //        Log.ErrorOnce("postfix", 1);
    //    }
    //}

    //[HarmonyPatch(typeof(VerbProperties))]
    //[HarmonyPatch(MethodType.Normal)]
    //class RPatch
    //{
    //    [HarmonyReversePatch]
    //    [HarmonyPatch(typeof(VerbProperties), "isMortar", MethodType.Setter)]
    //    public static void IsMortar( bool value)
    //    {
    //        FileLog.Log("inMortar");
    //        //___instance.
    //    }
    //}

}