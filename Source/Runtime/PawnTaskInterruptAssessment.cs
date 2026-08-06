using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using TaskInterrupt.Domain;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TaskInterrupt.Runtime
{
    /// <summary>
    /// Translates live pawn, job, lord, and medical activity into the immutable
    /// facts consumed by the fail-closed policy.
    /// </summary>
    internal static class PawnTaskInterruptAssessment
    {
        private static readonly HashSet<Pawn> ActiveMedicalPatients =
            new HashSet<Pawn>();
        private static Map indexedMap;
        private static int indexedFrame = -1;

        internal static TaskInterruptDecision Evaluate(Pawn pawn)
        {
            Job job = pawn?.jobs?.curJob;
            bool playerControlled = pawn != null &&
                (pawn.IsColonistPlayerControlled ||
                 pawn.IsColonyMechPlayerControlled);
            bool medicalCare = job != null &&
                (IsMedicalJob(job) ||
                 job.restUntilHealed ||
                 pawn.InBed() &&
                 HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn)) ||
                IsActiveMedicalPatient(pawn);

            var facts = new TaskInterruptFacts(
                playerControlled,
                job != null,
                pawn == null || pawn.Dead || pawn.Downed,
                pawn?.InMentalState ?? false,
                pawn?.Drafted ?? false,
                pawn?.Deathresting ?? false,
                pawn != null && pawn.IsFormingCaravan(),
                pawn?.GetLord() != null,
                pawn?.jobs?.IsCurrentJobPlayerInterruptible() ?? false,
                job?.def?.forceCompleteBeforeNextJob ?? false,
                job?.quest != null,
                !job?.ritualTag.NullOrEmpty() ?? false,
                pawn?.health?.hediffSet?.InLabor() ?? false,
                medicalCare,
                job?.playerForced ?? false);
            return TaskInterruptPolicy.Evaluate(facts);
        }

        private static bool IsActiveMedicalPatient(Pawn patient)
        {
            Map map = patient?.MapHeld;
            if (map == null)
            {
                return false;
            }

            int currentFrame = Time.frameCount;
            if (indexedMap != map || indexedFrame != currentFrame)
            {
                // Gizmo construction can assess every selected pawn repeatedly
                // in one render frame; share the doctor-target scan without
                // hiding direct job changes made while the game is paused.
                ActiveMedicalPatients.Clear();
                foreach (Pawn actor in map.mapPawns.AllPawnsSpawned)
                {
                    Job actorJob = actor?.jobs?.curJob;
                    Pawn target = actorJob?.targetA.Pawn;
                    if (target != null &&
                        actor != target &&
                        IsMedicalJob(actorJob))
                    {
                        ActiveMedicalPatients.Add(target);
                    }
                }

                indexedMap = map;
                indexedFrame = currentFrame;
            }

            return ActiveMedicalPatients.Contains(patient);
        }

        private static bool IsMedicalJob(Job job)
        {
            return job.def == JobDefOf.TendPatient ||
                job.workGiverDef?.workType == WorkTypeDefOf.Doctor ||
                job.bill is Bill_Medical;
        }
    }
}
