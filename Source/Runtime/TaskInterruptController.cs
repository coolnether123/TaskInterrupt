using System.Collections.Generic;
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
        private static readonly List<Pawn> FramePawns = new List<Pawn>();

        internal static void SelectedSupportedPawns(List<Pawn> pawns)
        {
            pawns.Clear();
            TaskInterruptApi.AddSelectedPawns(pawns);
            int writeIndex = 0;
            for (int readIndex = 0; readIndex < pawns.Count; readIndex++)
            {
                Pawn pawn = pawns[readIndex];
                if (pawn != null && TaskInterruptApi.IsPlayerControlled(pawn))
                {
                    pawns[writeIndex++] = pawn;
                }
            }
            if (writeIndex < pawns.Count)
            {
                pawns.RemoveRange(writeIndex, pawns.Count - writeIndex);
            }

            // Stable pawn order makes the reported first rejection independent
            // of RimWorld's selection enumeration.
            pawns.Sort((left, right) =>
                TaskInterruptApi.ThingId(left).CompareTo(
                    TaskInterruptApi.ThingId(right)));
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
            SelectedSupportedPawns(FramePawns);
            TaskInterruptDecision result = new TaskInterruptDecision(
                TaskInterruptBlockReason.NoCurrentTask);
            for (int i = 0; i < FramePawns.Count; i++)
            {
                TaskInterruptDecision decision =
                    PawnTaskInterruptAssessment.Evaluate(FramePawns[i]);
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
            var pawns = new List<Pawn>();
            SelectedSupportedPawns(pawns);
            // Only ask when more than one pawn is selected. Interrupting one
            // pawn's forced work means the player changed their mind about that
            // pawn and a prompt is just a click in the way. Interrupting several
            // at once is the case where a forced task can be swept up without
            // the player noticing it was in the selection.
            bool needsConfirmation = false;
            if (pawns.Count > 1)
            {
                for (int i = 0; i < pawns.Count; i++)
                {
                    if (PawnTaskInterruptAssessment.Evaluate(pawns[i])
                        .RequiresForcedConfirmation)
                    {
                        needsConfirmation = true;
                        break;
                    }
                }
            }
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
