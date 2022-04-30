using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using RimWorld;
using Verse;

namespace NGT
{
    public class MB_Shootout : MentalState
    {
		public override bool ForceHostileTo(Thing t)
		{
			return true;
		}
		public override bool ForceHostileTo(Faction f)
		{
			return true;
		}
		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
        public override void PreStart()
        {
            
            base.PreStart();
            if (!this.CheckPreRequisite())
            {
                /// Add Method To prevent 
            }

        }
        protected bool CheckPreRequisite()
        {
            try
            {
                if (!pawn.equipment.Primary.def.IsRangedWeapon)
                    {
                        return false;
                    }

                var towerContainer = pawn.MapHeld.listerBuildings.allBuildingsColonist.OfType<BaseGuardTower>();
                tower = towerContainer.Where(t => pawn.CanReach(t, PathEndMode.InteractionCell, Danger.Unspecified))
                                      .OrderBy(x=> ((LocalTargetInfo)pawn).Cell.DistanceTo(((LocalTargetInfo)x).Cell)).First();

            }
            catch
            {
                Messages.Message("No Tower that Pawn Can Go to", MessageTypeDefOf.ThreatSmall);
                return false;
            }

            return true;

        }

        public override void MentalStateTick()
        {
            base.MentalStateTick();
        }

        public BaseGuardTower tower;
    }
}
