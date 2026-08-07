# Task Interrupt

Task Interrupt is a RimWorld 1.6 quality-of-life mod for stopping a controllable
pawn's current task without the draft/undraft dance.

Select one pawn or a mixed group and use **Interrupt**. Every safe selected
pawn stops the current task, keeps any queued tasks, and immediately returns
to normal AI decision-making. The command uses RimWorld's own interruption and
cleanup path, so reservations and carried items receive normal job-driver
cleanup.

## Safety

Task Interrupt deliberately refuses to interrupt:

- tasks RimWorld or another mod marks uninterruptible;
- medical care and medical rest;
- birth and labor;
- deathrest;
- caravan formation;
- rituals, quest tasks, and other organized lord activities;
- mental-state, downed, or dead pawns; and
- tasks marked as required to finish before another begins.

A drafted pawn shows no command at all. Drafting already replaces the civilian
task, so the command would only ever be a disabled button competing for space in
an already crowded gizmo row. Selecting a drafted pawn alongside undrafted ones
still shows the command; the drafted pawns are reported as skipped.

Forced work is supported but asks for confirmation by default. A short
per-pawn guard prevents a held or rapidly repeated input from cancelling each
new task the AI selects.

The keybinding is configured in RimWorld's normal controls and defaults to
**F**. Vanilla also uses F to toggle forbidden items, but the commands are
context-separated: Task Interrupt is available for controllable pawns and Toggle
forbidden is available for items. Those two definitions explicitly ignore only
each other for conflict reporting, so vanilla forbid remains intact without a
false startup warning. It can be rebound to another keyboard key through
RimWorld's ordinary controls. Task Interrupt does not install a mouse-input adapter
or a global input poll. The gizmo can be hidden in mod settings; hiding it also
hides its native contextual hotkey surface.
Alt-clicking the gizmo opens and highlights that setting via SpineLib's shared
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
- SpineLib 1.0

## Installation

Install Harmony and [SpineLib](https://steamcommunity.com/sharedfiles/filedetails/?id=3778463813), copy Task Interrupt into RimWorld's `Mods` directory,
then enable Harmony, SpineLib, and Task Interrupt. The default **F** binding can be
changed from RimWorld's normal Controls page.

See [research](docs/research/duplicate-check.md),
[architecture](docs/architecture.md), [compatibility](docs/compatibility.md),
and [verification](docs/verification.md).

Licensed under the MIT License.
