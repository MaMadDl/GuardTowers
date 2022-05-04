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

        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }

        public override void PostStart(string reason)
        {
            base.PostStart(reason);
            tower = (pawn.MentalState.def.Worker as ShootoutStateWorker).towerHolder;

        }

        public override void MentalStateTick()
        {
            base.MentalStateTick();

            if (tower.GetInner().Contains(pawn))
            {
                SelectRandomLocation(); // => updates target

            }
            else
            {
                var job = new Job(DefDatabase<JobDef>.GetNamed("NGT_EnterTower"), tower);
                bool ret = pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                if (!ret)
                {
                    Log.Warning("couldn't do the job snapping out of it");
                    base.RecoverFromState();
                }
                tower.EjectAllContents(); 
            }

        }

        private void SelectRandomLocation()
        {
           
        }

        private LocalTargetInfo target;
        private BaseGuardTower tower;
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
