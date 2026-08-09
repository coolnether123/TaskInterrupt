using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using TaskInterrupt.Domain;
using TaskInterrupt.Runtime;
using static RimWorld.ModTestSupport.Test;

namespace TaskInterrupt.Tests
{
    internal static class Program
    {
        private static int Main()
        {
            Start("Task Interrupt contracts");
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
            Run("keybinding XML follows engine era",
                KeybindingXmlFollowsEngineEra);
            Run("assigned key uses native gizmo hotkey",
                AssignedKeyUsesNativeGizmoHotkey);
            Run("input stays on RimWorld's keyboard gizmo path",
                OnlyVanillaKeyboardGizmoOwnsInput);
            Run("mod gizmo sorts after vanilla", ModGizmoSortsAfterVanilla);
            Run("compiled implementation avoids draft dance", NoDraftDanceInAssembly);
            Run("active medical patients are protected",
                ActiveMedicalPatientsAreProtected);
            Run("patch installation tracks Spine success",
                PatchInstallationTracksSpineSuccess);
            Run("gizmo assessment is amortized per frame",
                GizmoAssessmentIsAmortizedPerFrame);
            Run("modern pawn selection avoids assembly-wide reflection",
                ModernPawnSelectionAvoidsAssemblyWideReflection);
            Run("Alt settings navigation consumes the gizmo",
                AltSettingsNavigationConsumesGizmo);
            return Finish();
        }

        private static void OrdinaryTaskIsAllowed()
        {
            TaskInterruptDecision decision = TaskInterruptPolicy.Evaluate(Facts());
            Require(decision.CanBreak, "ordinary work should be stoppable");
            Require(!decision.RequiresForcedConfirmation,
                "ordinary work should not require confirmation");
        }

        private static void ForcedTaskConfirms()
        {
            TaskInterruptDecision decision = TaskInterruptPolicy.Evaluate(
                Facts(isPlayerForced: true));
            Require(decision.CanBreak, "forced work remains explicitly stoppable");
            Require(decision.RequiresForcedConfirmation,
                "forced work must be confirmed by default");
        }

        private static void ProtectedStatesFailClosed()
        {
            AssertBlocked(Facts(isPlayerControlled: false),
                TaskInterruptBlockReason.NotPlayerControlled);
            AssertBlocked(Facts(hasCurrentTask: false),
                TaskInterruptBlockReason.NoCurrentTask);
            AssertBlocked(Facts(isIncapacitated: true),
                TaskInterruptBlockReason.Incapacitated);
            AssertBlocked(Facts(isInMentalState: true),
                TaskInterruptBlockReason.MentalState);
            AssertBlocked(Facts(isDrafted: true),
                TaskInterruptBlockReason.Drafted);
            AssertBlocked(Facts(isDeathresting: true),
                TaskInterruptBlockReason.Deathrest);
            AssertBlocked(Facts(isFormingCaravan: true),
                TaskInterruptBlockReason.FormingCaravan);
            AssertBlocked(Facts(hasOrganizedLord: true),
                TaskInterruptBlockReason.OrganizedActivity);
            AssertBlocked(Facts(mustCompleteBeforeNextTask: true),
                TaskInterruptBlockReason.MustComplete);
            AssertBlocked(Facts(isQuestOwned: true),
                TaskInterruptBlockReason.QuestOwned);
            AssertBlocked(Facts(isRitualOwned: true),
                TaskInterruptBlockReason.RitualOwned);
            AssertBlocked(Facts(isInLabor: true),
                TaskInterruptBlockReason.Labor);
            AssertBlocked(Facts(isMedicalCare: true),
                TaskInterruptBlockReason.MedicalCare);
        }

        private static void GameContractWins()
        {
            AssertBlocked(Facts(isPlayerInterruptible: false),
                TaskInterruptBlockReason.GameProtected);
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
                "Shared",
                "Modern",
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
                "Shared",
                "Modern",
                "Defs",
                "KeyBindings.xml"));
            string ignoredByTaskInterrupt = keyBindings.Root?
                .Element("KeyBindingDef")?
                .Element("ignoreConflictsWith")?
                .Element("li")?
                .Value;
            Require(ignoredByTaskInterrupt == "Command_ItemForbid",
                "Task Interrupt must ignore only its contextual vanilla F peer");

            var vanillaPatch = XDocument.Load(Path.Combine(
                root,
                "Shared",
                "Modern",
                "Patches",
                "KeyBindingConflicts.xml"));
            string patchText = vanillaPatch.ToString();
            Require(patchText.Contains(
                    "Defs/KeyBindingDef[defName=\"Command_ItemForbid\"]",
                    StringComparison.Ordinal) &&
                patchText.Contains(
                    "TaskInterrupt_CancelCurrentTask",
                    StringComparison.Ordinal),
                "vanilla forbid must symmetrically ignore Task Interrupt");
        }

        private static void KeybindingXmlFollowsEngineEra()
        {
            string root = RepositoryRoot();
            string legacyPath = Path.Combine(
                root,
                "Shared",
                "Legacy",
                "Defs",
                "KeyBindings.xml");
            var legacy = XDocument.Load(legacyPath);
            Require(legacy.Root?.Element("KeyBindingDef")?.Element(
                    "ignoreConflictsWith") == null,
                "classic engines must not receive the modern conflict field");

            string loadFolders = File.ReadAllText(
                Path.Combine(root, "LoadFolders.xml"));
            Require(loadFolders.Contains(
                    "<v1.6>", StringComparison.Ordinal) &&
                loadFolders.Contains(
                    "<li>Shared/Modern</li>", StringComparison.Ordinal) &&
                loadFolders.Contains(
                    "<v1.5>", StringComparison.Ordinal) &&
                loadFolders.Contains(
                    "<li>Shared/Legacy</li>", StringComparison.Ordinal),
                "load folders must route modern and classic keybinding definitions separately");
        }

        private static void NoDraftDanceInAssembly()
        {
            string root = RepositoryRoot();
            using AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(
                Path.Combine(
                root,
                "1.6",
                "Assemblies",
                "TaskInterrupt.dll"));
            MethodReference[] calls = assembly.MainModule.Types
                .Where(type => type.Namespace.StartsWith(
                    "TaskInterrupt",
                    StringComparison.Ordinal))
                .SelectMany(type => type.Methods)
                .Where(method => method.HasBody)
                .SelectMany(method => method.Body.Instructions)
                .Where(instruction =>
                    instruction.OpCode == OpCodes.Call ||
                    instruction.OpCode == OpCodes.Callvirt)
                .Select(instruction => instruction.Operand as MethodReference)
                .Where(method => method != null)
                .ToArray();

            Require(!calls.Any(method => method.Name == "set_Drafted"),
                "Task Interrupt must not mutate draft state");
            Require(!calls.Any(method => method.Name == "ClearQueuedJobs"),
                "Task Interrupt must preserve queued jobs");
            Require(calls.Any(method => method.Name == "EndCurrentJob"),
                "Task Interrupt must use RimWorld's normal job-ending API");
        }

        private static void AssignedKeyUsesNativeGizmoHotkey()
        {
            string root = RepositoryRoot();
            string defOf = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Definitions",
                "TaskInterruptDefOf.cs"));
            string command = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Presentation",
                "Command_TaskInterrupt.cs"));
            Require(defOf.Contains(
                    "public static KeyBindingDef TaskInterrupt_CancelCurrentTask;",
                    StringComparison.Ordinal),
                "RimWorld binds only public static DefOf fields");
            Require(command.Contains(
                    "hotKey = TaskInterruptDefOf.TaskInterrupt_CancelCurrentTask;",
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
                "Command_TaskInterrupt.cs"));
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
                "TaskInterruptPatches.cs"));
            string command = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Presentation",
                "Command_TaskInterrupt.cs"));
            Require(patches.Contains(
                    "typeof(Pawn), nameof(Pawn.GetGizmos)",
                    StringComparison.Ordinal),
                "Task Interrupt must add only its pawn gizmo patch");
            Require(!patches.Contains("Dialog_DefineBinding",
                    StringComparison.Ordinal) &&
                !patches.Contains("UIRoot_Play",
                    StringComparison.Ordinal) &&
                !patches.Contains("Input.GetKeyDown",
                    StringComparison.Ordinal) &&
                !patches.Contains("Mouse3",
                    StringComparison.Ordinal),
                "Task Interrupt must not own mouse input, binding dialogs, or a global input poll");
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
                "TaskInterruptController.cs"));
            Require(controller.Contains(
                    "TaskInterruptText.Reason(decision.BlockReason)",
                    StringComparison.Ordinal),
                "every activation surface must preserve the command's specific unavailable explanation");
            Require(command.Contains(
                    "action = TaskInterruptController.ActivateSelected;",
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
                "PawnTaskInterruptAssessment.cs"));
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
                    "TaskInterruptApi.IsMedicalJob(actorJob)",
                    StringComparison.Ordinal),
                "active care must use the shared medical classifier so modded Doctor work is protected");
            Require(!assessment.Contains(
                    "reservationManager.IsReserved",
                    StringComparison.Ordinal),
                "unrelated reservations must not disable Task Interrupt");
        }

        private static void PatchInstallationTracksSpineSuccess()
        {
            string root = RepositoryRoot();
            string patches = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Patches",
                "TaskInterruptPatches.cs"));
            Require(patches.Contains(
                    "installed = Installer.TryPatch(",
                    StringComparison.Ordinal),
                "modern startup must not report installation after Spine rejects the patch");
        }

        private static void GizmoAssessmentIsAmortizedPerFrame()
        {
            string root = RepositoryRoot();
            string assessment = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Runtime",
                "PawnTaskInterruptAssessment.cs"));
            string controller = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Runtime",
                "TaskInterruptController.cs"));
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

        private static void ModernPawnSelectionAvoidsAssemblyWideReflection()
        {
            string root = RepositoryRoot();
            using AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(
                Path.Combine(
                    root,
                    "1.6",
                    "Assemblies",
                    "TaskInterrupt.dll"));
            MethodReference[] calls = assembly.MainModule.Types
                .Where(type => type.Namespace.StartsWith(
                    "TaskInterrupt",
                    StringComparison.Ordinal))
                .SelectMany(type => type.Methods)
                .Where(method => method.HasBody)
                .SelectMany(method => method.Body.Instructions)
                .Where(instruction =>
                    instruction.OpCode == OpCodes.Call ||
                    instruction.OpCode == OpCodes.Callvirt)
                .Select(instruction => instruction.Operand as MethodReference)
                .Where(method => method != null)
                .ToArray();

            Require(!calls.Any(method =>
                    method.DeclaringType.FullName == "System.Reflection.Assembly" &&
                    method.Name == "GetTypes"),
                "the 1.6 pawn gizmo path must use direct RimWorld APIs instead of scanning the game assembly");
        }

        private static void AltSettingsNavigationConsumesGizmo()
        {
            string root = RepositoryRoot();
            string command = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Presentation",
                "Command_TaskInterrupt.cs"));
            Require(command.Contains(
                    "if (BindSettings(",
                    StringComparison.Ordinal),
                "the command must branch on Spine's consumed Alt-click");
            Require(command.Contains(
                    "return new GizmoResult(GizmoState.Clear);",
                    StringComparison.Ordinal),
                "a consumed Alt-click must not reach the command action");
        }

        private static TaskInterruptFacts Facts(
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
            return new TaskInterruptFacts(
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
            TaskInterruptFacts facts,
            TaskInterruptBlockReason expected)
        {
            TaskInterruptDecision decision = TaskInterruptPolicy.Evaluate(facts);
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

    }
}
