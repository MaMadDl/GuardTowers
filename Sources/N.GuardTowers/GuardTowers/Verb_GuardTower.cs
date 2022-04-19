using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace NGT
{
    public class Verb_GuardTowers : Verb_Shoot
    {
        private BaseGuardTower tower;
        private List<Verb> verbss;
        private Dictionary<Pawn, int> warmupDictionary;

        

        public override void Reset()
        {
            base.Reset();
            tower = (BaseGuardTower)caster;
            warmupDictionary = new Dictionary<Pawn, int>();
        }

        public void ResetVerb()
        {
            if (tower == null)
            {
                tower = (BaseGuardTower)caster;
            }

            warmupDictionary = new Dictionary<Pawn, int>();
            ((BaseGuardTower)caster).UpdateRange();

            foreach (var pawn in tower.GetInner().InnerListForReading)
            {
                
                if (pawn.TryGetAttackVerb(currentTarget.Thing) == null)
                {
                    foreach(var verb in pawn.equipment.PrimaryEq.AllVerbs)
                    {
                        verb.caster = pawn;
                        
                    }
                    
                }
                else
                {
                    pawn.TryGetAttackVerb(currentTarget.Thing).caster = pawn;
                }

                
            }
        }

        protected override bool TryCastShot()
        {
            
            verbss = new List<Verb>();
            
            if (tower == null)
            {
                tower = (BaseGuardTower)caster;
            }

            var pawns = tower.GetInner().InnerListForReading;
            var newDictionary = new Dictionary<Pawn, int>();
            if (warmupDictionary == null)
            {
                warmupDictionary = new Dictionary<Pawn, int>();
            }
            else
            {
                foreach (var pawn in pawns)
                {
                    if (!warmupDictionary.ContainsKey(pawn))
                    {
                        continue;
                    }

                    newDictionary[pawn] = warmupDictionary[pawn];
                }
            }

            warmupDictionary = newDictionary;

            foreach (var pawn in pawns)
            {

                if (pawn.TryGetAttackVerb(currentTarget.Thing) == null)
                {
                    continue;
                }

                var verb = pawn.TryGetAttackVerb(currentTarget.Thing);
               
                if (checkWarmup(pawn, verb))
                {
                    verbss.Add(verb);
                }
            }

            //foreach (var pair in warmupDictionary)
            //{
            //    Log.Message($"{GenTicks.TicksGame} - {pair.Key}: {pair.Value}");
            //}

            if (!verbss.Any())
            { 
                return false;
            }

            //Log.Message($"Found {verbss.Count} verbs");
            foreach (var vb in verbss)
            {
                //Log.Message($"{vb}");
                vb.caster = caster;
                
                //vb.WarmupComplete();
                vb.TryStartCastOn(currentTarget);
               
            }

            return true;
        }
        private bool checkWarmup(Pawn shooter, Verb attackVerb)
        {
            if (attackVerb.IsMeleeAttack)
            {
                return false;
            }

            if (warmupDictionary == null)
            {
                warmupDictionary = new Dictionary<Pawn, int>();
            }

            if (warmupDictionary.ContainsKey(shooter) && warmupDictionary[shooter] > 0)
            {
                warmupDictionary[shooter] -= 10;
                return false;
            }

            var returnValue = warmupDictionary.ContainsKey(shooter);
            
            var statValue = shooter.GetStatValue(StatDefOf.AimingDelayFactor);
            warmupDictionary[shooter] = (attackVerb.verbProps.warmupTime * statValue).SecondsToTicks() +
                                        attackVerb.verbProps.AdjustedCooldownTicks(attackVerb, shooter);

            return returnValue;
        }
    }
}