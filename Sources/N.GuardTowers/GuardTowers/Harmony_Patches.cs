using HarmonyLib;
using RimWorld;
using Verse;

namespace NGT
{
    [StaticConstructorOnStartup]
    internal class Harmony_Patches
    {
        static Harmony_Patches()
        {
            //Log.Message("Hello World!");
            var harmony = new Harmony("rimworld.ngt.guardtowers");

            var original = typeof(GameEnder).GetMethod("CheckOrUpdateGameOver");
            var postfix = typeof(Patches).GetMethod("CheckOrUpdateGameOver_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(postfix));
            harmony.PatchAll();
        }
    }
}