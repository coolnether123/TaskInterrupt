# Task Break

Task Break is a RimWorld 1.6 quality-of-life mod for stopping a controllable
pawn's current task without the draft/undraft dance.

Select one pawn or a mixed group and use **Break task**. Every safe selected
pawn stops the current task, keeps any queued tasks, and immediately returns
to normal AI decision-making. The command uses RimWorld's own interruption and
cleanup path, so reservations and carried items receive normal job-driver
cleanup.

## Safety

Task Break deliberately refuses to interrupt:

- tasks RimWorld or another mod marks uninterruptible;
- medical care and medical rest;
- birth and labor;
- deathrest;
- caravan formation;
- rituals, quest tasks, and other organized lord activities;
- mental-state, drafted, downed, or dead pawns; and
- tasks marked as required to finish before another begins.

Forced work is supported but asks for confirmation by default. A short
per-pawn guard prevents a held or rapidly repeated input from cancelling each
new task the AI selects.

The keybinding is configured in RimWorld's normal controls and defaults to
**F**. Vanilla also uses F to toggle forbidden items, but the commands are
context-separated: Task Break is available for controllable pawns and Toggle
forbidden is available for items. Those two definitions explicitly ignore only
each other for conflict reporting, so vanilla forbid remains intact without a
false startup warning. It can be rebound to any keyboard key or to
Mouse 3 through Mouse 6 in either binding slot. Task Break adds side-button
capture only while its own binding dialog is waiting for input; other controls
and ordinary mouse clicks remain unchanged. The gizmo can be hidden in mod
settings.
Alt-clicking the gizmo opens and highlights that setting via Spine's shared
contextual-settings convention.

## Supported pawns

- player-controlled colonists;
- player-controlled slaves;
- player-controlled children; and
- overseen, player-controlled colony mechs.

Animals and arbitrary selectable entities are intentionally outside scope.

## Requirements

- RimWorld 1.6
- Harmony
- Spine 1.2 or newer

See [research](docs/research/duplicate-check.md),
[architecture](docs/architecture.md), [compatibility](docs/compatibility.md),
and [verification](docs/verification.md).

Licensed under the MIT License.
