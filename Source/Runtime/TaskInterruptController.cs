using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TaskInterrupt.Bootstrap;
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

        internal static IReadOnlyList<Pawn> SelectedSupportedPawns()
        {
            // Stable pawn order makes the reported first rejection independent
            // of RimWorld's selection enumeration.
            return Find.Selector.SelectedPawns
                .Where(pawn => pawn != null &&
                    (pawn.IsColonistPlayerControlled ||
                     pawn.IsColonyMechPlayerControlled))
                .OrderBy(pawn => pawn.thingIDNumber)
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
            IReadOnlyList<Pawn> pawns = SelectedSupportedPawns();
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
                Messages.Message(
                    TaskInterruptText.Reason(decision.BlockReason),
                    MessageTypeDefOf.RejectInput,
                    historical: false);
                return;
            }

            InterruptSelected();
        }

        private static void InterruptSelected()
        {
            IReadOnlyList<Pawn> pawns = SelectedSupportedPawns();
            bool needsConfirmation = pawns.Any(pawn =>
                PawnTaskInterruptAssessment.Evaluate(pawn)
                    .RequiresForcedConfirmation);
            if (needsConfirmation &&
                TaskInterruptMod.Settings.ConfirmForcedTasks)
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    TaskInterruptText.Translate(
                        "TaskInterrupt_ConfirmForced", pawns.Count),
                    () => Execute(pawns),
                    destructive: false));
                return;
            }

            Execute(pawns);
        }

        private static void Execute(IReadOnlyList<Pawn> pawns)
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
                        pawn.thingIDNumber,
                        Find.TickManager.TicksGame))
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

                pawn.jobs.curJob.playerInterruptedForced = true;
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                GoofyMode.Celebrate(pawn);
                stopped++;
            }

            if (stopped == 0 &&
                firstBlockReason != TaskInterruptBlockReason.None)
            {
                Messages.Message(
                    TaskInterruptText.Reason(firstBlockReason),
                    MessageTypeDefOf.RejectInput,
                    historical: false);
            }
            else if (skipped > 0)
            {
                Messages.Message(
                    TaskInterruptText.Translate(
                        "TaskInterrupt_Result", stopped, skipped),
                    MessageTypeDefOf.NeutralEvent,
                    historical: false);
            }
        }
    }
}
