# Duplicate and community investigation

Investigated 2026-08-01 for RimWorld 1.6. Searches covered the Steam Workshop,
GitHub, RimWorld community discussions, Reddit, the local Workshop corpus, and
the canonical CoolNether123 Discord/context collection. No matching proposal
was found in the canonical local collection.

## Closest released ideas

### Stop Gizmo - Workshop 3687438256

This is the closest user-facing match. Its cached Workshop page advertised a
generic Stop button for pawns, animals, mechs, and other controllable entities,
an `X` default key, interruption of whatever the entity was doing, and keeping
queued jobs. The item was posted on 2026-03-18 and now reports that it was
removed for Steam Community/content-guideline reasons. No public source or
license was found, so no code was inspected or copied.

Task Break remains distinct and justified because Stop Gizmo is unavailable,
not maintained, broader in entity scope, collision-prone by default, and does
not document the guarded medical, labor, caravan, ritual, quest, mental-state,
deathrest, mixed-selection, or forced-work behavior required here.

Source: <https://steamcommunity.com/sharedfiles/filedetails/?id=3687438256>

### Achtung! - Workshop 730936602

Achtung is an actively developed command-and-forced-work overhaul. Community
documentation describes its stop behavior in the context of its own forced
work and queued jobs. It is not a small standalone safety-focused replacement
for cancelling an ordinary current task. Task Break does not copy or depend on
Achtung and does not modify its job definitions.

Sources:

- <https://steamcommunity.com/sharedfiles/filedetails/?id=730936602>
- <https://github.com/pardeike/Achtung2>
- <https://www.brrai.nz/>

### Developer command

RimWorld development mode includes `EndCurrentJob(InterruptForced)`. It proves
the engine has an authoritative interruption operation, but a dev-only menu
tool is not a normal player workflow, has no mixed-selection policy, and gives
no safety explanation.

Source: <https://rimworldwiki.com/wiki/Development_mode>

## Decision

Proceed. No maintained available mod was found that provides the same compact,
guarded player experience. Existing work was used only as behavioral and
compatibility research.
