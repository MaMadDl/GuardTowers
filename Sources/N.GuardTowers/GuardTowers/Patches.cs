using RimWorld;
using Verse;

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
}