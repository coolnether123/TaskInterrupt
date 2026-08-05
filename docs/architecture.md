# Architecture

Task Interrupt has four small responsibilities:

1. `TaskInterruptPolicy` evaluates an immutable, game-independent fact snapshot.
2. `PawnTaskInterruptAssessment` maps current RimWorld state to that snapshot.
3. `TaskInterruptController` handles deterministic selected-pawn batching,
   confirmation, interruption, and repeat-input suppression.
4. `Command_TaskInterrupt` and `TaskInterruptPatches` provide the native gizmo surface.

The mod owns exactly one Harmony patch: a `Pawn.GetGizmos` postfix. Its
`Command_Action` uses RimWorld's normal `KeyBindingDef` and `hotKey` path, so
the game owns keyboard routing, dialog suppression, conflict handling, and
event consumption. Task Interrupt adds no binding-dialog patch, mouse adapter, or
global input poll. It stores no gameplay state and never consumes an IMGUI
event. While
commands are visible, it
indexes active medical targets on demand at most once per map/rendered frame and
shares one aggregate selection decision across the grouped pawn commands for
that frame. This avoids multiplying a map scan by both selection size and
command count, while still observing direct job orders made while the game is
paused. The bounded activation-only repeat guard is not serialized and a game
tick rollback is treated as a new session.

The primary binding defaults to `F`. Task Interrupt and vanilla's item-only Toggle
forbidden definition symmetrically list only each other in
`ignoreConflictsWith`. This uses RimWorld's native contextual-overlap contract:
both commands and rebindings remain intact, the false startup warning is
avoided, and unrelated binding conflicts continue to be detected normally.

Spine 1.0 owns settings rendering, scribing, contextual Alt-click routing,
scrolling, and highlighting. Task Interrupt contributes two setting definitions
and one rectangle binding. Gameplay safety semantics remain in Task Interrupt.

The policy takes facts instead of RimWorld objects so its complete safety
matrix can run off-game. No compatibility adapters are introduced until a
verified mod requires behavior beyond the vanilla public contracts.

## One-use helpers

Seven helpers currently have one direct production caller:

- `TaskInterruptController.FirstDecision` keeps selection and policy traversal out
  of the gizmo renderer. It should remain a controller boundary unless a
  future command needs the same query.
- `TaskInterruptText.Reason` keeps the localized block-reason mapping out of the
  command class. It should remain separate because the mapping is a complete
  presentation concern rather than command behavior.
- `ActivationGate.RemoveExpired` isolates the bounded-cache maintenance loop
  from admission logic. It should remain a private helper; extracting a shared
  service would be premature unless another real consumer needs activation
  throttling with the same semantics.
- `PawnTaskInterruptAssessment.IsActiveMedicalPatient` owns the bounded, on-demand
  scan for an active doctor targeting the selected pawn. It should remain a
  private safety-policy probe; inlining it would obscure the distinction
  between the pawn's own job and care being performed by another pawn.
- `TaskInterruptPatches.Install` is the mod's idempotent patch lifecycle boundary.
  It should remain separate from the mod constructor so Harmony ownership and
  future uninstall behavior stay isolated, even though bootstrap calls it once.
- `AssignedKeyActivation.IsPressed` has one production caller. It should remain
  a Task Interrupt input-policy seam because its hidden/visible and duplicate-slot
  behavior is directly regression-tested without loading Unity; it should not
  move into Spine unless another consumer needs exactly the same policy.
- `AssignedInputSuppression.ShouldSuppress` has one production caller. It
  should remain a Task Interrupt input-safety seam so the ordinary play stack and
  blocking dialogs can be regression-tested without loading Unity. Moving this
  three-flag policy into Spine would be premature unless another consumer
  needs the same hidden-binding behavior.
