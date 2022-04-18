using UnityEngine;
using Verse;

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
            ThingOwner<Pawn> colList = ((BaseGuardTower)thing).GetInner();
            int i = 0;
            foreach (var col in colList.InnerListForReading) {
                i= (i+2)%8;
               
                GenDraw.DrawCircleOutline(center.ToVector3(),col.equipment.Primary.def.Verbs[0].range,(SimpleColor)i);
            }
        }
    }
}