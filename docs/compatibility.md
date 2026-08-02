# Compatibility notes

## Designed compatibility

Task Break calls `IsCurrentJobPlayerInterruptible` and
`EndCurrentJob(InterruptForced)` rather than replacing or patching them. Mods
that refine those methods therefore remain authoritative. Task Break owns one
Harmony patch: a postfix on `Pawn.GetGizmos`. Its command uses a unique owner
ID, label, and group key. Its default `F` binding overlaps vanilla's item-only
Toggle forbidden command but remains context-separated on controllable pawns
and fully configurable. The two definitions symmetrically ignore only each
other in RimWorld's native conflict model; other binding conflicts remain
active. Keyboard bindings remain on the untouched base `Command` path. Task
Break does not patch the Controls dialog, poll global input, or add mouse
binding behavior. Hiding the gizmo also removes its contextual hotkey surface.

The current compatibility surface investigated includes Achtung, Perspective
Shift, Simple Baby Carry, Common Sense, Medieval Overhaul, Vanilla Factions
Expanded - Medieval, and Vehicle Framework. These mods have been observed in
community/current stacks touching job interruption or interruptibility. That
is a reason to keep Task Break off those patch targets, not a claim that every
combination has been live-tested.

## Expected behavior

- Achtung-owned queued or forced work remains queued; Task Break confirms the
  current forced job before using vanilla cleanup.
- Vehicle Framework can veto interruptibility through its existing check.
- Modded job definitions with `playerInterruptible=false` or
  `forceCompleteBeforeNextJob=true` are automatically protected.
- Modded doctor work using the ordinary Doctor work type is protected.

## Not claimed

No support is claimed for mods that replace pawn gizmos without composing the
returned enumerable, bypass RimWorld's job tracker, or represent safety-critical
work without vanilla interruption metadata, a lord, a quest, a ritual tag,
medical work, or medical-rest state. Such a case needs a minimal reproduction
before a domain-specific adapter is justified.
