using System;
using System.IO;
using System.Xml.Linq;
using TaskBreak.Domain;
using TaskBreak.Runtime;

namespace TaskBreak.Tests
{
    internal static class Program
    {
        private static int passed;

        private static int Main()
        {
            Run("ordinary task is interruptible", OrdinaryTaskIsAllowed);
            Run("forced task requires confirmation", ForcedTaskConfirms);
            Run("protected states fail closed", ProtectedStatesFailClosed);
            Run("game interruption contract wins", GameContractWins);
            Run("repeat activation is bounded", RepeatActivationIsBounded);
            Run("tick rollback starts a new session", TickRollbackIsAllowed);
            Run("keybinding defaults to F",
                KeybindingDefaultsToF);
            Run("F reuse ignores only vanilla forbid",
                ContextualFReuseIsSymmetric);
            Run("assigned key uses native gizmo hotkey",
                AssignedKeyUsesNativeGizmoHotkey);
            Run("input stays on RimWorld's keyboard gizmo path",
                OnlyVanillaKeyboardGizmoOwnsInput);
            Run("mod gizmo sorts after vanilla", ModGizmoSortsAfterVanilla);
            Run("implementation avoids draft dance", NoDraftDanceInSource);
            Run("active medical patients are protected",
                ActiveMedicalPatientsAreProtected);
            Run("gizmo assessment is amortized per frame",
                GizmoAssessmentIsAmortizedPerFrame);
            Run("Alt settings navigation consumes the gizmo",
                AltSettingsNavigationConsumesGizmo);
            Console.WriteLine($"PASS: {passed} Task Break contracts");
            return 0;
        }

        private static void OrdinaryTaskIsAllowed()
        {
            TaskBreakDecision decision = TaskBreakPolicy.Evaluate(Facts());
            Require(decision.CanBreak, "ordinary work should be stoppable");
            Require(!decision.RequiresForcedConfirmation,
                "ordinary work should not require confirmation");
        }

        private static void ForcedTaskConfirms()
        {
            TaskBreakDecision decision = TaskBreakPolicy.Evaluate(
                Facts(isPlayerForced: true));
            Require(decision.CanBreak, "forced work remains explicitly stoppable");
            Require(decision.RequiresForcedConfirmation,
                "forced work must be confirmed by default");
        }

        private static void ProtectedStatesFailClosed()
        {
            AssertBlocked(Facts(isPlayerControlled: false),
                TaskBreakBlockReason.NotPlayerControlled);
            AssertBlocked(Facts(hasCurrentTask: false),
                TaskBreakBlockReason.NoCurrentTask);
            AssertBlocked(Facts(isIncapacitated: true),
                TaskBreakBlockReason.Incapacitated);
            AssertBlocked(Facts(isInMentalState: true),
                TaskBreakBlockReason.MentalState);
            AssertBlocked(Facts(isDrafted: true),
                TaskBreakBlockReason.Drafted);
            AssertBlocked(Facts(isDeathresting: true),
                TaskBreakBlockReason.Deathrest);
            AssertBlocked(Facts(isFormingCaravan: true),
                TaskBreakBlockReason.FormingCaravan);
            AssertBlocked(Facts(hasOrganizedLord: true),
                TaskBreakBlockReason.OrganizedActivity);
            AssertBlocked(Facts(mustCompleteBeforeNextTask: true),
                TaskBreakBlockReason.MustComplete);
            AssertBlocked(Facts(isQuestOwned: true),
                TaskBreakBlockReason.QuestOwned);
            AssertBlocked(Facts(isRitualOwned: true),
                TaskBreakBlockReason.RitualOwned);
            AssertBlocked(Facts(isInLabor: true),
                TaskBreakBlockReason.Labor);
            AssertBlocked(Facts(isMedicalCare: true),
                TaskBreakBlockReason.MedicalCare);
        }

        private static void GameContractWins()
        {
            AssertBlocked(Facts(isPlayerInterruptible: false),
                TaskBreakBlockReason.GameProtected);
        }

        private static void RepeatActivationIsBounded()
        {
            var gate = new ActivationGate(30);
            Require(gate.TryEnter(7, 100), "first activation should enter");
            Require(!gate.TryEnter(7, 100), "same-tick repeat must be rejected");
            Require(!gate.TryEnter(7, 129), "cooldown edge must be rejected");
            Require(gate.TryEnter(7, 130), "activation should resume after cooldown");
            Require(gate.TryEnter(8, 100), "different pawns must be independent");
        }

        private static void TickRollbackIsAllowed()
        {
            var gate = new ActivationGate(30);
            Require(gate.TryEnter(7, 1000), "initial session should enter");
            Require(gate.TryEnter(7, 5),
                "a new game with a lower tick must not inherit cooldown");
        }

        private static void KeybindingDefaultsToF()
        {
            string root = RepositoryRoot();
            var document = XDocument.Load(Path.Combine(
                root,
                "Defs",
                "KeyBindings.xml"));
            string first = document.Root?
                .Element("KeyBindingDef")?
                .Element("defaultKeyCodeA")?
                .Value;
            string second = document.Root?
                .Element("KeyBindingDef")?
                .Element("defaultKeyCodeB")?
                .Value;
            Require(first == "F" && second == "None",
                "F must ship as the fast configurable default");
        }

        private static void ContextualFReuseIsSymmetric()
        {
            string root = RepositoryRoot();
            var keyBindings = XDocument.Load(Path.Combine(
                root,
                "Defs",
                "KeyBindings.xml"));
            string ignoredByTaskBreak = keyBindings.Root?
                .Element("KeyBindingDef")?
                .Element("ignoreConflictsWith")?
                .Element("li")?
                .Value;
            Require(ignoredByTaskBreak == "Command_ItemForbid",
                "Task Break must ignore only its contextual vanilla F peer");

            var vanillaPatch = XDocument.Load(Path.Combine(
                root,
                "Patches",
                "KeyBindingConflicts.xml"));
            string patchText = vanillaPatch.ToString();
            Require(patchText.Contains(
                    "Defs/KeyBindingDef[defName=\"Command_ItemForbid\"]",
                    StringComparison.Ordinal) &&
                patchText.Contains(
                    "TaskBreak_CancelCurrentTask",
                    StringComparison.Ordinal),
                "vanilla forbid must symmetrically ignore Task Break");
        }

        private static void NoDraftDanceInSource()
        {
            string root = RepositoryRoot();
            string controller = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Runtime",
                "TaskBreakController.cs"));
            Require(!controller.Contains(".Drafted =", StringComparison.Ordinal),
                "Task Break must not mutate draft state");
            Require(!controller.Contains("ClearQueuedJobs", StringComparison.Ordinal),
                "Task Break must preserve queued jobs");
            Require(controller.Contains(
                    "EndCurrentJob(JobCondition.InterruptForced)",
                    StringComparison.Ordinal),
                "Task Break must use the vanilla forced interruption path");
        }

        private static void AssignedKeyUsesNativeGizmoHotkey()
        {
            string root = RepositoryRoot();
            string defOf = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Definitions",
                "TaskBreakDefOf.cs"));
            string command = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Presentation",
                "Command_TaskBreak.cs"));
            Require(defOf.Contains(
                    "public static KeyBindingDef TaskBreak_CancelCurrentTask;",
                    StringComparison.Ordinal),
                "RimWorld binds only public static DefOf fields");
            Require(command.Contains(
                    "hotKey = TaskBreakDefOf.TaskBreak_CancelCurrentTask;",
                    StringComparison.Ordinal),
                "the command must let RimWorld render and activate its key");
        }

        private static void ModGizmoSortsAfterVanilla()
        {
            string root = RepositoryRoot();
            string command = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Presentation",
                "Command_TaskBreak.cs"));
            Require(command.Contains(
                    "Order = float.MaxValue;",
                    StringComparison.Ordinal),
                "the mod command must stay to the far right of vanilla gizmos");
        }

        private static void OnlyVanillaKeyboardGizmoOwnsInput()
        {
            string root = RepositoryRoot();
            string patches = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Patches",
                "TaskBreakPatches.cs"));
            string command = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Presentation",
                "Command_TaskBreak.cs"));
            Require(patches.Contains(
                    "typeof(Pawn), nameof(Pawn.GetGizmos)",
                    StringComparison.Ordinal),
                "Task Break must add only its pawn gizmo patch");
            Require(!patches.Contains("Dialog_DefineBinding",
                    StringComparison.Ordinal) &&
                !patches.Contains("UIRoot_Play",
                    StringComparison.Ordinal) &&
                !patches.Contains("Input.GetKeyDown",
                    StringComparison.Ordinal) &&
                !patches.Contains("Mouse3",
                    StringComparison.Ordinal),
                "Task Break must not own mouse input, binding dialogs, or a global input poll");
            Require(command.Contains(
                    "alsoClickIfOtherInGroupClicked = false;",
                    StringComparison.Ordinal),
                "a grouped multi-selection command must invoke its aggregate action only once");
            Require(command.Contains(
                    "return base.GizmoOnGUI(topLeft, maxWidth, parms);",
                    StringComparison.Ordinal),
                "keyboard bindings must remain on RimWorld's vanilla command path");
            string controller = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Runtime",
                "TaskBreakController.cs"));
            Require(controller.Contains(
                    "TaskBreakText.Reason(decision.BlockReason)",
                    StringComparison.Ordinal),
                "every activation surface must preserve the command's specific unavailable explanation");
            Require(command.Contains(
                    "action = TaskBreakController.ActivateSelected;",
                    StringComparison.Ordinal),
                "the vanilla gizmo and keyboard hotkey must share one activation entrypoint");
        }

        private static void ActiveMedicalPatientsAreProtected()
        {
            string root = RepositoryRoot();
            string assessment = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Runtime",
                "PawnTaskBreakAssessment.cs"));
            Require(assessment.Contains(
                    "IsActiveMedicalPatient(pawn)",
                    StringComparison.Ordinal),
                "the selected patient must be assessed against active care");
            Require(assessment.Contains(
                    "Pawn target = actorJob?.targetA.Pawn",
                    StringComparison.Ordinal) &&
                assessment.Contains(
                    "ActiveMedicalPatients.Add(target)",
                    StringComparison.Ordinal) &&
                assessment.Contains(
                    "ActiveMedicalPatients.Contains(patient)",
                    StringComparison.Ordinal),
                "care protection must index and query the exact targeted patient");
            Require(assessment.Contains(
                    "job.def == JobDefOf.TendPatient",
                    StringComparison.Ordinal) &&
                assessment.Contains(
                    "job.bill is Bill_Medical",
                    StringComparison.Ordinal),
                "tending and active medical bills must both be protected");
            Require(!assessment.Contains(
                    "reservationManager.IsReserved",
                    StringComparison.Ordinal),
                "unrelated reservations must not disable Task Break");
        }

        private static void GizmoAssessmentIsAmortizedPerFrame()
        {
            string root = RepositoryRoot();
            string assessment = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Runtime",
                "PawnTaskBreakAssessment.cs"));
            string controller = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Runtime",
                "TaskBreakController.cs"));
            Require(assessment.Contains(
                    "indexedFrame != currentFrame",
                    StringComparison.Ordinal) &&
                assessment.Contains(
                    "ActiveMedicalPatients.Contains(patient)",
                    StringComparison.Ordinal) &&
                !assessment.Contains(
                    "TicksGame",
                    StringComparison.Ordinal),
                "medical targets must refresh by rendered frame so paused direct orders cannot reuse a stale tick cache");
            Require(controller.Contains(
                    "cachedDecisionFrame == currentFrame",
                    StringComparison.Ordinal),
                "duplicate grouped commands must share one selection assessment per frame");
        }

        private static void AltSettingsNavigationConsumesGizmo()
        {
            string root = RepositoryRoot();
            string command = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Presentation",
                "Command_TaskBreak.cs"));
            Require(command.Contains(
                    "if (BindSettings(",
                    StringComparison.Ordinal),
                "the command must branch on Spine's consumed Alt-click");
            Require(command.Contains(
                    "return new GizmoResult(GizmoState.Clear);",
                    StringComparison.Ordinal),
                "a consumed Alt-click must not reach the command action");
        }

        private static TaskBreakFacts Facts(
            bool isPlayerControlled = true,
            bool hasCurrentTask = true,
            bool isIncapacitated = false,
            bool isInMentalState = false,
            bool isDrafted = false,
            bool isDeathresting = false,
            bool isFormingCaravan = false,
            bool hasOrganizedLord = false,
            bool isPlayerInterruptible = true,
            bool mustCompleteBeforeNextTask = false,
            bool isQuestOwned = false,
            bool isRitualOwned = false,
            bool isInLabor = false,
            bool isMedicalCare = false,
            bool isPlayerForced = false)
        {
            return new TaskBreakFacts(
                isPlayerControlled,
                hasCurrentTask,
                isIncapacitated,
                isInMentalState,
                isDrafted,
                isDeathresting,
                isFormingCaravan,
                hasOrganizedLord,
                isPlayerInterruptible,
                mustCompleteBeforeNextTask,
                isQuestOwned,
                isRitualOwned,
                isInLabor,
                isMedicalCare,
                isPlayerForced);
        }

        private static void AssertBlocked(
            TaskBreakFacts facts,
            TaskBreakBlockReason expected)
        {
            TaskBreakDecision decision = TaskBreakPolicy.Evaluate(facts);
            Require(!decision.CanBreak, $"{expected} should be blocked");
            Require(decision.BlockReason == expected,
                $"expected {expected}, got {decision.BlockReason}");
            Require(!decision.RequiresForcedConfirmation,
                "blocked work must never request confirmation");
        }

        private static string RepositoryRoot()
        {
            return Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                ".."));
        }

        private static void Run(string name, Action action)
        {
            action();
            passed++;
            Console.WriteLine("PASS: " + name);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
