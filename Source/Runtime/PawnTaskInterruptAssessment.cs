using System.Collections.Generic;
using TaskInterrupt.Compatibility;
using TaskInterrupt.Domain;
using UnityEngine;
using Verse;
using Verse.AI;
#if TASK_INTERRUPT_USE_SPINE
using RimWorld;
#endif

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
            Job job = TaskInterruptApi.CurrentJob(pawn);
            bool playerControlled = TaskInterruptApi.IsPlayerControlled(pawn);
            bool medicalCare = job != null &&
                (TaskInterruptApi.IsMedicalJob(job) ||
                 TaskInterruptApi.IsRestUntilHealed(job) ||
                 TaskInterruptApi.IsInBed(pawn)) ||
                IsActiveMedicalPatient(pawn);

            var facts = new TaskInterruptFacts(
                playerControlled,
                job != null,
                pawn == null || TaskInterruptApi.IsDead(pawn) ||
                    TaskInterruptApi.IsDowned(pawn),
                TaskInterruptApi.IsMentalState(pawn),
                TaskInterruptApi.IsDrafted(pawn),
                TaskInterruptApi.IsDeathresting(pawn),
                TaskInterruptApi.IsFormingCaravan(pawn),
                TaskInterruptApi.HasLord(pawn),
                TaskInterruptApi.IsCurrentJobPlayerInterruptible(pawn),
                TaskInterruptApi.HasForceCompleteBeforeNextJob(job),
                TaskInterruptApi.HasQuest(job),
                TaskInterruptApi.HasRitualTag(job),
                TaskInterruptApi.IsInLabor(pawn),
                medicalCare,
                TaskInterruptApi.IsPlayerForced(job));
            return TaskInterruptPolicy.Evaluate(facts);
        }

        private static bool IsActiveMedicalPatient(Pawn patient)
        {
#if TASK_INTERRUPT_USE_SPINE
            Map map = patient?.Map;
            if (map == null)
            {
                return false;
            }

            int currentFrame = Time.frameCount;
            if (indexedMap != map || indexedFrame != currentFrame)
            {
                ActiveMedicalPatients.Clear();
                foreach (Pawn actor in map.mapPawns.AllPawnsSpawned)
                {
                    Job actorJob = actor?.jobs?.curJob;
                    Pawn target = actorJob?.targetA.Pawn;
                    Job job = actorJob;
                    if (target != null &&
                        actor != target &&
                        (job.def == JobDefOf.TendPatient ||
                         job.bill is Bill_Medical))
                    {
                        ActiveMedicalPatients.Add(target);
                    }
                }

                indexedMap = map;
                indexedFrame = currentFrame;
            }

            return ActiveMedicalPatients.Contains(patient);
#else
            Map map = TaskInterruptApi.MapHeld(patient);
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
                foreach (Pawn actor in TaskInterruptApi.SpawnedPawns(map))
                {
                    Job actorJob = TaskInterruptApi.CurrentJob(actor);
                    Pawn target = TaskInterruptApi.TargetPawn(actorJob);
                    if (target != null &&
                        actor != target &&
                        TaskInterruptApi.IsMedicalJob(actorJob))
                    {
                        ActiveMedicalPatients.Add(target);
                    }
                }

                indexedMap = map;
                indexedFrame = currentFrame;
            }

            return ActiveMedicalPatients.Contains(patient);
#endif
        }

    }
}
