# RimWorld 1.6 API investigation

The authoritative local 1.6 decompile was inspected before implementation.

## Interruption contract

`Pawn_JobTracker.IsCurrentJobPlayerInterruptible()` checks both
`JobDef.playerInterruptible` and `JobDriver.PlayerInterruptable`, and rejects a
pawn on fire. Calling `EndCurrentJob(JobCondition.InterruptForced)` then:

- notifies the pawn's lord;
- releases reservations through `ClearReservationsForJob`;
- calls the current driver's `Cleanup`;
- cancels busy stances softly;
- drops or retains a carried thing according to the job definition/finalizer;
- keeps the existing queued-job collection; and
- asks the think tree for the next job.

Task Interrupt sets the same `Job.playerInterruptedForced` marker used by vanilla
ordered-job interruption before calling this API. It does not call
`ClearQueuedJobs`, `StopAll`, or draft-state setters.

## Vanilla safety metadata

`JobDef.playerInterruptible` and `forceCompleteBeforeNextJob` already express
important safety decisions, including uninterruptible ability casting,
vomiting, mind-controlled movement, and Biotech safety jobs. Task Interrupt honors
these fields dynamically for vanilla and modded job definitions.

Additional authoritative state used by the policy:

- `Pawn.IsColonistPlayerControlled` and `IsColonyMechPlayerControlled`;
- `Pawn.GetLord()` and `Pawn.IsFormingCaravan()`;
- `Job.quest`, `Job.ritualTag`, and `Job.playerForced`;
- `HediffSet.InLabor()`;
- doctor work type, `Job.restUntilHealed`, and urgent medical rest; and
- pawn drafted, downed, dead, mental-state, and deathrest state.

## Presentation and input

`Pawn.GetGizmos()` is extended with one postfix that appends one grouped
`Command_Action`. The command uses a normal `KeyBindingDef`, so RimWorld owns
key conflict presentation and produces only a `KeyDown` event. Both binding
keyboard slots remain player-configurable. The primary defaults to `F`; vanilla also
uses F for the item-only Toggle forbidden command, while Task Interrupt is exposed
for controllable pawns. The secondary defaults to `None`. The mod deliberately
uses only RimWorld's native command hotkey path: it does not extend the binding
dialog, translate mouse events, or poll input from `UIRoot_Play`. When the gizmo
is hidden, its contextual keyboard hotkey is hidden with it.

The postfix does not patch `EndCurrentJob` or
`IsCurrentJobPlayerInterruptible`; it calls them normally, preserving other
mods' ownership of those shared surfaces.
