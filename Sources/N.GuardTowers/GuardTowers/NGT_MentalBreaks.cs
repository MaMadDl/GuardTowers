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

            //keep a backup from tower since we are gonna change some stats like capacity
            bckupTower = tower = (pawn.MentalState.def.Worker as ShootoutStateWorker).towerHolder;
            shotCounts =0;
            targetLocations = new List<Thing>();

        }

        public override void MentalStateTick()
        {
            base.MentalStateTick();

            //we need to make sure which position to use all the time first or second
            Log.ErrorOnce("Check Positions:  "+tower.Position.ToString() + "\t" 
                              + ((LocalTargetInfo)tower).CenterVector3.ToIntVec3().ToString(), 1);

            if(shotCounts > 20)
            {
                RecoverFromState();
            }

            if (tower.GetInner().Contains(pawn))
            {
                if (targetLocations.Count == 0)
                {
                    SelectRandomLocation();
                    Log.Warning(targetLocations.Count.ToString()); // check if value is correct
                }
                target = targetLocations.RandomElement();

                tower.AttackVerb.TryStartCastOn(target);
                
                //(tower.AttackVerb as Verb_GuardTowers).castShotMentalBreak(target);

            }
            else
            {
                var job = new Job(DefDatabase<JobDef>.GetNamed("NGT_EnterTower"), tower);
                bool ret = pawn.jobs.TryTakeOrderedJob(job, JobTag.InMentalState);
                if (!ret)
                {
                    Log.Warning("couldn't do the job snapping out of it");
                    base.RecoverFromState();
                }
                if (pawn.Position == tower.InteractionCell)
                {
                    //its good for now block exit and fill cap so no1 else can enter while its goin
                    tower.EjectAllContents();
                    tower.Capacity = 1;
                }
            }
        }

        public override void Notify_AttackedTarget(LocalTargetInfo hitTarget)
        {
            base.Notify_AttackedTarget(hitTarget);
            shotCounts++;
            Log.Warning("shooting");
        }

        public override void PostEnd()
        {
            
            base.PostEnd();
            Log.Warning("recovering");

            tower.FixBonusStats(tower.GetInner().InnerListForReading);
            tower.GetInner().TryDropAll(tower.InteractionCell, pawn.MapHeld, ThingPlaceMode.Near);
            tower.Capacity = bckupTower.Capacity;
        }

        private void SelectRandomLocation()
        {
            //var rgn = ((LocalTargetInfo)tower).CenterVector3.ToIntVec3()
            //                                    .GetRegion(pawn.MapHeld, RegionType.Set_Passable);

            List<Thing> list = pawn.MapHeld.listerThings.ThingsInGroup(ThingRequestGroup.Everything);

            targetLocations = list.Where((item) =>  item.Position.DistanceTo(tower.Position) < 
                                            pawn.equipment.Primary.def.Verbs.First().range ).ToList(); 
        }

        private List<Thing> targetLocations;
        public Thing target;
        private BaseGuardTower tower;
        private BaseGuardTower bckupTower;
        private int shotCounts;
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
