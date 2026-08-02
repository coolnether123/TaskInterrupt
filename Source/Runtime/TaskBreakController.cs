using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TaskBreak.Bootstrap;
using TaskBreak.Domain;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TaskBreak.Runtime
{
    internal static class TaskBreakController
    {
        private const int RepeatGuardTicks = 30;
        private static readonly ActivationGate ActivationGate =
            new ActivationGate(RepeatGuardTicks);
        private static TaskBreakDecision cachedFirstDecision;
        private static int cachedDecisionFrame = -1;

        internal static IReadOnlyList<Pawn> SelectedSupportedPawns()
        {
            return Find.Selector.SelectedPawns
                .Where(pawn => pawn != null &&
                    (pawn.IsColonistPlayerControlled ||
                     pawn.IsColonyMechPlayerControlled))
                .OrderBy(pawn => pawn.thingIDNumber)
                .ToList();
        }

        internal static TaskBreakDecision FirstDecision()
        {
            int currentFrame = Time.frameCount;
            if (cachedDecisionFrame == currentFrame)
            {
                return cachedFirstDecision;
            }

            IReadOnlyList<Pawn> pawns = SelectedSupportedPawns();
            TaskBreakDecision result = new TaskBreakDecision(
                TaskBreakBlockReason.NoCurrentTask);
            for (int i = 0; i < pawns.Count; i++)
            {
                TaskBreakDecision decision =
                    PawnTaskBreakAssessment.Evaluate(pawns[i]);
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
            TaskBreakDecision decision = FirstDecision();
            if (!decision.CanBreak)
            {
                Messages.Message(
                    TaskBreakText.Reason(decision.BlockReason),
                    MessageTypeDefOf.RejectInput,
                    historical: false);
                return;
            }

            BreakSelected();
        }

        private static void BreakSelected()
        {
            IReadOnlyList<Pawn> pawns = SelectedSupportedPawns();
            bool needsConfirmation = pawns.Any(pawn =>
                PawnTaskBreakAssessment.Evaluate(pawn)
                    .RequiresForcedConfirmation);
            if (needsConfirmation &&
                TaskBreakMod.Settings.ConfirmForcedTasks)
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    "TaskBreak_ConfirmForced".Translate(pawns.Count),
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
            TaskBreakBlockReason firstBlockReason =
                TaskBreakBlockReason.None;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                TaskBreakDecision decision =
                    PawnTaskBreakAssessment.Evaluate(pawn);
                if (!decision.CanBreak ||
                    !ActivationGate.TryEnter(
                        pawn.thingIDNumber,
                        Find.TickManager.TicksGame))
                {
                    if (firstBlockReason == TaskBreakBlockReason.None)
                    {
                        firstBlockReason = decision.CanBreak
                            ? TaskBreakBlockReason.ActivationCooldown
                            : decision.BlockReason;
                    }
                    skipped++;
                    continue;
                }

                pawn.jobs.curJob.playerInterruptedForced = true;
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                stopped++;
            }

            if (stopped == 0 &&
                firstBlockReason != TaskBreakBlockReason.None)
            {
                Messages.Message(
                    TaskBreakText.Reason(firstBlockReason),
                    MessageTypeDefOf.RejectInput,
                    historical: false);
            }
            else if (skipped > 0)
            {
                Messages.Message(
                    "TaskBreak_Result".Translate(stopped, skipped),
                    MessageTypeDefOf.NeutralEvent,
                    historical: false);
            }
        }
    }
}
