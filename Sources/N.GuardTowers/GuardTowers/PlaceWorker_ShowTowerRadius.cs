using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace NGT
{
    
    public class PlaceWorker_ShowColonistsRadius : PlaceWorker
    {

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            _ = Find.CurrentMap;
            if (thing == null)
            {
                return;
            }
			if (thing.def.IsBlueprint || thing.def.IsFrame)
            {
				List<IntVec3> t= GenAdj.OccupiedRect(center, rot, def.Size).ExpandedBy(6).ToList();
				GenDraw.DrawFieldEdges(t);
				return;
            }
            ThingOwner<Pawn> colList = ((BaseGuardTower)thing).GetInner();
            int i = 0;
            foreach (var col in colList.InnerListForReading) {
                i= (i+2)%8;  
                GenDraw.DrawCircleOutline(center.ToVector3(),col.equipment.Primary.def.Verbs[0].range,(SimpleColor)i);
            }
        }
	}

	public class PlaceWorker_NeverAdjacentTower : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			foreach (IntVec3 c in GenAdj.OccupiedRect(center, rot, def.Size).ExpandedBy(6))
			{
				if (c.InBounds(map))
				{
					List<Thing> list = map.thingGrid.ThingsListAt(c);
					for (int i = 0; i < list.Count; i++)
					{
						Thing thing2 = list[i];
						ThingDef thingDef;
						if (thing2 != thingToIgnore && ((thing2.def.category == ThingCategory.Building && thing2.def == def) || ((thing2.def.IsBlueprint || thing2.def.IsFrame) && (thingDef = (thing2.def.entityDefToBuild as ThingDef)) != null && thingDef == def)))
						{
							return "CannotPlaceAdjacentTower".Translate();
						}
					}
				}
			}
			return true;
		}
	}
}