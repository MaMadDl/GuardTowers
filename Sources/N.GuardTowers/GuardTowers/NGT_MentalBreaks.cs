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
		//public override bool ForceHostileTo(Thing t)
		//{
		//	return true;
		//}
		//public override bool ForceHostileTo(Faction f)
		//{
		//	return true;
		//}
		//public override RandomSocialMode SocialModeMax()
		//{
		//	return RandomSocialMode.Off;
		//}
        public override void PostStart(string reason)
        {
            base.PostStart(reason);
            tower = (pawn.MentalState.def.Worker as ShootoutStateWorker).towerHolder;

            var jobDef = DefDatabase<JobDef>.GetNamed("NGT_EnterTower");
            var job = new Job(jobDef, tower);
            bool ret = pawn.jobs.TryTakeOrderedJob(job, JobTag.InMentalState);
            if (ret)
            {
                Log.Warning("job successful");
            }
        }

        public override void MentalStateTick()
        {
            base.MentalStateTick();
        }

        public BaseGuardTower tower;
    }

    
    public class ShootoutStateWorker : MentalStateWorker
    {
        public override bool StateCanOccur(Pawn pawn)
        {

            if (!base.StateCanOccur(pawn))
            {
                return false;
            }
            if (!pawn.Spawned)
            {
                return false;
            }

            if (pawn.equipment.Primary != null)
            {
                if (!pawn.equipment.Primary.def.IsRangedWeapon)
                {
                    return false;
                }
            }
            else 
            {
                return false;
            }

            var towerContainer = pawn.MapHeld.listerBuildings.allBuildingsColonist.OfType<BaseGuardTower>();
            if (towerContainer.Count() < 1 || towerContainer.Where(t => pawn.CanReach(t, PathEndMode.InteractionCell, Danger.Unspecified)).Count() < 1)
            {

                return false;
            }

            
            towerHolder= towerContainer.Where(t => pawn.CanReach(t, PathEndMode.InteractionCell, Danger.Unspecified))
                                      .OrderBy(x => ((LocalTargetInfo)pawn).Cell.DistanceTo(((LocalTargetInfo)x).Cell)).First();
            
            return true;

        }

        public BaseGuardTower towerHolder;
    }
}
