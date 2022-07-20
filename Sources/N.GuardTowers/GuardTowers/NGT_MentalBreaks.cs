using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

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
            tower = (pawn.MentalState.def.Worker as ShootoutStateWorker).towerHolder;
            targetLocations = new List<Thing>();
            towerCap = tower.Capacity;
            
        }

        public virtual void StartAttackSequence(Pawn pawn)
        {

            if (targetLocations.Count == 0)
            {
                SelectRandomLocation();
                //Log.Warning(targetLocations.Count.ToString()); // check if value is correct
            }

            if (warmUpVal > 0)
            {
                warmUpVal -= 1;
            }
            else
            {
                Thing target = targetLocations.RandomElement();

                var attackVerb = pawn.TryGetAttackVerb(target);
                var statValue = pawn.GetStatValue(StatDefOf.AimingDelayFactor);
                warmUpVal = (attackVerb.verbProps.warmupTime * statValue).SecondsToTicks() +
                                            attackVerb.verbProps.AdjustedCooldownTicks(attackVerb, pawn);
                tower.OrderAttack(target);
            }
        }

        public override void MentalStateTick()
        {
            base.MentalStateTick();

            //we need to make sure which position to use all the time first or second
            //Log.ErrorOnce("Check Positions:  " + tower.Position.ToString() + "\t"
            //                  + ((LocalTargetInfo)tower).CenterVector3.ToIntVec3().ToString(), 1);

            if (tower.GetInner().Contains(pawn))
            {

                StartAttackSequence(pawn);

            }
            else
            {
                if (pawn.InMentalState)
                {
                    var job = new Job(DefDatabase<JobDef>.GetNamed("NGT_EnterTower"), tower);
                    bool ret = pawn.jobs.TryTakeOrderedJob(job, JobTag.InMentalState);
                    if (!ret)
                    {
                        //Log.Warning("couldn't do the job snapping out of it");
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
        }

        public override void PostEnd()
        {

            tower.FixBonusStats(tower.GetInner().InnerListForReading);
            tower.GetInner().TryDropAll(tower.InteractionCell, pawn.MapHeld, ThingPlaceMode.Near, (pawn, res) => ((Pawn)pawn).jobs.ClearQueuedJobs());
            tower.Capacity = towerCap;

            base.PostEnd();

        }

        private void SelectRandomLocation()
        {
            //var rgn = ((LocalTargetInfo)tower).CenterVector3.ToIntVec3()
            //                                    .GetRegion(pawn.MapHeld, RegionType.Set_Passable);

            List<Thing> list = pawn.MapHeld.listerThings.ThingsInGroup(ThingRequestGroup.Everything);

            targetLocations = list.Where((item) => item.Position.DistanceTo(tower.Position) <
                                            pawn.equipment.Primary.def.Verbs.First().range).ToList();
        }

        protected List<Thing> targetLocations;
        protected BaseGuardTower tower;
        protected int towerCap;
        protected int warmUpVal;
    }

    public class MB_ShootoutSlaughter : MB_Shootout
    {
        public override void StartAttackSequence(Pawn pawn)
        {
            Pawn pawnTarget = pawn;
            //base.StartAttackSequence(pawn);
            if (targetLocations.Count == 0)
            {

                List<Thing> list = pawn.MapHeld.listerThings.ThingsInGroup(ThingRequestGroup.Pawn)
                    .Where((p) => (p as Pawn).IsColonist).ToList(); ;

                list.Remove(pawn);  // remove the pawn alrdy in mental break
                list.ForEach(i => Log.Warning(i.ToString()));


                targetLocations = list.Where((item) => item.Position.DistanceTo(tower.Position) <
                                             pawn.equipment.Primary.def.Verbs.First().range).ToList();

                pawnTarget = targetLocations.RandomElement() as Pawn;
                tower.OrderAttack(pawnTarget);

            }
            else
            {

                if (tower.Position.DistanceTo((IntVec3)tower.TargetCurrentlyAimingAt) > pawn.equipment.Primary.def.Verbs.First().range)
                {
                    // clearing list to get new target
                    targetLocations.Clear();   
                }
                
            }
        }
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
            //Where(t => t.GetInner().InnerListForReading.Except()
            {

                return false;
            }
            
            List<Thing> pawnsInMap = pawn.MapHeld.listerThings.ThingsInGroup(ThingRequestGroup.Pawn)
                    .Where((p) => (p as Pawn).IsColonist).ToList();
            if (pawnsInMap.Count == 1)
            {
                return false;
            }

            towerHolder = towerContainer.Where(t => pawn.CanReach(t, PathEndMode.InteractionCell, Danger.Unspecified))
                                      .OrderBy(x => ((LocalTargetInfo)pawn).Cell.DistanceTo(((LocalTargetInfo)x).Cell)).First();

            return true;

        }

        public BaseGuardTower towerHolder;
    }


}
