using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace NGT
{
    class EnterTower : JobDriver
    {

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return
                Toils_Towers.GotoThing(TargetIndex.A,
                    PathEndMode.ClosestTouch); //Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
          
            yield return Toils_General.Wait( GetActor().GetStatValue(StatDefOf.MoveSpeed).SecondsToTicks() / 2 , TargetIndex.A);

            var enter = new Toil();
            enter.initAction = delegate
            {
                var actor = enter.actor;
                var pod = (BaseGuardTower)actor.CurJob.targetA.Thing;

                void action()
                {
                    if (pod.GetInner().InnerListForReading.Count >= pod.Capacity)
                    {
                        return;
                    }

                    actor.DeSpawn();
                    pod.TryAcceptThing(actor);
                }

                action();
            };
            enter.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return enter;
        }
       
    }
}
