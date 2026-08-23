# Interrupt Task

Interrupt Task stops a controllable pawn's current task without making you
draft and undraft them. It supports RimWorld 1.6.

Select one pawn or a mixed group and use **Interrupt**. Every selected pawn
whose work is safe to interrupt stops the current task, keeps the queue, and
returns to normal AI behavior. RimWorld's own job system cleans up reservations
and carried items.

## Safety

Interrupt Task deliberately refuses to interrupt:

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

Forced work can be interrupted, but the mod asks for confirmation by default.
A short per-pawn delay stops a held or repeated key from cancelling each new
task the AI chooses.

The keybinding uses RimWorld's normal controls and defaults to **F**. The base
game also uses F to toggle forbidden items, but the commands appear in
different contexts. Interrupt Task is available for controllable pawns, while
Toggle forbidden is available for items. They ignore only each other during
conflict checks, so both commands keep working without a false startup warning.
You can rebind Interrupt Task to another keyboard key.

The mod does not add mouse handling or poll for global input. You can hide the
gizmo in the mod settings, which also disables its hotkey. Alt-click the gizmo
to open that setting through SpineLib.

## Supported pawns

- player-controlled colonists;
- player-controlled slaves;
- player-controlled children; and
- overseen, player-controlled colony mechs.

Animals and other selectable entities are outside the mod's scope.

## Requirements

- RimWorld 1.6
- Harmony
- SpineLib 1.1.0 or newer

Published packages contain the RimWorld 1.6 build only.

## Installation

1. Install Harmony and [SpineLib](https://steamcommunity.com/sharedfiles/filedetails/?id=3778463813).
2. Download the ZIP from the [latest GitHub release](https://github.com/coolnether123/TaskInterrupt/releases/latest).
3. Extract Interrupt Task into RimWorld's `Mods` directory.
4. Enable Harmony, SpineLib, and Interrupt Task.

The default **F** binding can be changed from RimWorld's normal Controls page.

See [research](docs/research/duplicate-check.md),
[architecture](docs/architecture.md), [compatibility](docs/compatibility.md),
and [verification](docs/verification.md).

Licensed under the MIT License.
