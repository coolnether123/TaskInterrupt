using System.Collections.Generic;
using System.Linq;
using TaskInterrupt.Bootstrap;
using TaskInterrupt.Compatibility;
using TaskInterrupt.Domain;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TaskInterrupt.Runtime
{
    /// <summary>
    /// Owns deterministic selection batching, confirmation, and execution so
    /// the command remains a thin RimWorld presentation adapter.
    /// </summary>
    internal static class TaskInterruptController
    {
        private const int RepeatGuardTicks = 30;
        private static readonly ActivationGate ActivationGate =
            new ActivationGate(RepeatGuardTicks);
        private static TaskInterruptDecision cachedFirstDecision;
        private static int cachedDecisionFrame = -1;

        internal static List<Pawn> SelectedSupportedPawns()
        {
            // Stable pawn order makes the reported first rejection independent
            // of RimWorld's selection enumeration.
            return TaskInterruptApi.SelectedPawns()
                .Where(pawn => pawn != null &&
                    TaskInterruptApi.IsPlayerControlled(pawn))
                .OrderBy(TaskInterruptApi.ThingId)
                .ToList();
        }

        internal static TaskInterruptDecision FirstDecision()
        {
            int currentFrame = Time.frameCount;
            if (cachedDecisionFrame == currentFrame)
            {
                return cachedFirstDecision;
            }

            // Grouped gizmos construct one command per pawn; a frame-local
            // aggregate avoids repeating the full selection safety scan.
            List<Pawn> pawns = SelectedSupportedPawns();
            TaskInterruptDecision result = new TaskInterruptDecision(
                TaskInterruptBlockReason.NoCurrentTask);
            for (int i = 0; i < pawns.Count; i++)
            {
                TaskInterruptDecision decision =
                    PawnTaskInterruptAssessment.Evaluate(pawns[i]);
                if (i == 0)
                {
                    result = decision;
                }
                if (decision.CanBreak)
                {
                    result = decision;
                    break;
                }
            }

            cachedFirstDecision = result;
            cachedDecisionFrame = currentFrame;
            return result;
        }

        internal static void ActivateSelected()
        {
            TaskInterruptDecision decision = FirstDecision();
            if (!decision.CanBreak)
            {
                TaskInterruptUiApi.ShowMessage(
                    TaskInterruptText.Reason(decision.BlockReason),
                    "RejectInput");
                return;
            }

            InterruptSelected();
        }

        private static void InterruptSelected()
        {
            List<Pawn> pawns = SelectedSupportedPawns();
            // Only ask when more than one pawn is selected. Interrupting one
            // pawn's forced work means the player changed their mind about that
            // pawn and a prompt is just a click in the way. Interrupting several
            // at once is the case where a forced task can be swept up without
            // the player noticing it was in the selection.
            bool needsConfirmation = pawns.Count > 1 &&
                pawns.Any(pawn =>
                    PawnTaskInterruptAssessment.Evaluate(pawn)
                        .RequiresForcedConfirmation);
            if (needsConfirmation &&
                TaskInterruptMod.Settings.ConfirmForcedTasks)
            {
                if (TaskInterruptUiApi.Confirm(
                        TaskInterruptText.Translate(
                            "TaskInterrupt_ConfirmForced", pawns.Count),
                        () => Execute(pawns)))
                {
                    return;
                }

                // Very old builds do not expose a confirmation window. Keep
                // the forced-work safety rule fail-closed on those APIs.
                return;
            }

            Execute(pawns);
        }

        private static void Execute(List<Pawn> pawns)
        {
            int stopped = 0;
            int skipped = 0;
            TaskInterruptBlockReason firstBlockReason =
                TaskInterruptBlockReason.None;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                TaskInterruptDecision decision =
                    PawnTaskInterruptAssessment.Evaluate(pawn);
                if (!decision.CanBreak ||
                    !ActivationGate.TryEnter(
                        TaskInterruptApi.ThingId(pawn),
                        TaskInterruptApi.CurrentTick()))
                {
                    if (firstBlockReason == TaskInterruptBlockReason.None)
                    {
                        firstBlockReason = decision.CanBreak
                            ? TaskInterruptBlockReason.ActivationCooldown
                            : decision.BlockReason;
                    }
                    skipped++;
                    continue;
                }

                TaskInterruptApi.MarkPlayerInterruptedForced(pawn);
                if (!TaskInterruptApi.EndCurrentJob(pawn))
                {
                    skipped++;
                    continue;
                }
                GoofyMode.Celebrate(pawn);
                stopped++;
            }

            if (stopped == 0 &&
                firstBlockReason != TaskInterruptBlockReason.None)
            {
                TaskInterruptUiApi.ShowMessage(
                    TaskInterruptText.Reason(firstBlockReason),
                    "RejectInput");
            }
            else if (skipped > 0)
            {
                TaskInterruptUiApi.ShowMessage(
                    TaskInterruptText.Translate(
                        "TaskInterrupt_Result", stopped, skipped),
                    "NeutralEvent");
            }
        }
    }
}
