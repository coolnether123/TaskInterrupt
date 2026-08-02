# Architecture

Task Break has four small responsibilities:

1. `TaskBreakPolicy` evaluates an immutable, game-independent fact snapshot.
2. `PawnTaskBreakAssessment` maps current RimWorld state to that snapshot.
3. `TaskBreakController` handles deterministic selected-pawn batching,
   confirmation, interruption, and repeat-input suppression.
4. `Command_TaskBreak` and `TaskBreakPatches` provide the native gizmo surface.

The mod patches `Pawn.GetGizmos` with a postfix and narrowly prefixes
`Dialog_DefineBinding.DoWindowContents` only to capture Mouse3-Mouse6 while
Task Break's own binding dialog is awaiting input. The prefix returns directly
to vanilla for every other binding and every non-side-button event. A small
`UIRoot_Play.UIRootUpdate` postfix reads only Task Break's assigned side-button
bindings, because RimWorld's command renderer recognizes keyboard events but
not side-mouse events. It does nothing behind an open window and uses the
ordinary `KeyBindingDef.JustPressed` path only when the gizmo is hidden, so a
hidden command remains fully rebindable without duplicating visible keyboard
activation. It stores no gameplay state. While commands are visible, it
indexes active medical targets on demand at most once per map/rendered frame and
shares one aggregate selection decision across the grouped pawn commands for
that frame. This avoids multiplying a map scan by both selection size and
command count, while still observing direct job orders made while the game is
paused. The bounded activation-only repeat guard is not serialized and a game
tick rollback is treated as a new session.

The primary binding defaults to `F`. Task Break and vanilla's item-only Toggle
forbidden definition symmetrically list only each other in
`ignoreConflictsWith`. This uses RimWorld's native contextual-overlap contract:
both commands and rebindings remain intact, the false startup warning is
avoided, and unrelated binding conflicts continue to be detected normally.

Spine 1.2 owns settings rendering, scribing, contextual Alt-click routing,
scrolling, and highlighting. Task Break contributes two setting definitions
and one rectangle binding. Gameplay safety semantics remain in Task Break.

The policy takes facts instead of RimWorld objects so its complete safety
matrix can run off-game. No compatibility adapters are introduced until a
verified mod requires behavior beyond the vanilla public contracts.

## One-use helpers

Five helpers currently have one direct caller:

- `TaskBreakController.FirstDecision` keeps selection and policy traversal out
  of the gizmo renderer. It should remain a controller boundary unless a
  future command needs the same query.
- `TaskBreakText.Reason` keeps the localized block-reason mapping out of the
  command class. It should remain separate because the mapping is a complete
  presentation concern rather than command behavior.
- `ActivationGate.RemoveExpired` isolates the bounded-cache maintenance loop
  from admission logic. It should remain a private helper; extracting a shared
  service would be premature unless another real consumer needs activation
  throttling with the same semantics.
- `PawnTaskBreakAssessment.IsActiveMedicalPatient` owns the bounded, on-demand
  scan for an active doctor targeting the selected pawn. It should remain a
  private safety-policy probe; inlining it would obscure the distinction
  between the pawn's own job and care being performed by another pawn.
- `TaskBreakPatches.Install` is the mod's idempotent patch lifecycle boundary.
  It should remain separate from the mod constructor so Harmony ownership and
  future uninstall behavior stay isolated, even though bootstrap calls it once.
