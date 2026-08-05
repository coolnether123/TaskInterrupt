# Task Break

Task Break is a RimWorld 1.6 quality-of-life mod for stopping a controllable
pawn's current task without the draft/undraft dance.

Select one pawn or a mixed group and use **Break task**. Every safe selected
pawn stops the current task, keeps any queued tasks, and immediately returns to
normal AI decision-making. The command uses RimWorld's own interruption and
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

Forced work is supported but asks for confirmation by default. A short per-pawn
guard prevents a held or rapidly repeated input from cancelling each new task
the AI selects.

## Supported pawns

- player-controlled colonists;
- player-controlled slaves;
- player-controlled children; and
- overseen, player-controlled colony mechs.

Animals and arbitrary selectable entities are intentionally outside scope.

## Keybinding

The keybinding is configured in RimWorld's normal controls and defaults to
**F**. Vanilla also uses F to toggle forbidden items, but the commands are
context-separated: Task Break is available for controllable pawns and Toggle
forbidden is available for items. Those two definitions explicitly ignore only
each other for conflict reporting, so vanilla forbid remains intact without a
false startup warning. It can be rebound to another keyboard key through
RimWorld's ordinary controls.

Task Break does not install a mouse-input adapter or a global input poll.

The gizmo can be hidden in mod settings; hiding it also hides its native
contextual hotkey surface. Alt-clicking the gizmo opens and highlights that
setting via Spine's shared contextual-settings convention.

## Requirements

- RimWorld 1.6
- [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)
- [Spine](https://github.com/coolnether123/Spine) — the shared runtime used by
  CoolNether123 mods

## Installation

Install Harmony and Spine, copy `TaskBreak` into RimWorld's `Mods` folder, then
enable Harmony, Spine, and Task Break in that order.

The mod stores only global preferences and adds no game-save component, so it
is safe to add to or remove from an existing save.

## Documentation

- [Duplicate research](docs/research/duplicate-check.md)
- [Architecture](docs/architecture.md)
- [Compatibility](docs/compatibility.md)
- [Verification record](docs/verification.md)

## License

Released under the [MIT License](LICENSE). Harmony and Spine are used under
their own licenses.
