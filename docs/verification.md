# Verification record

Task Break was built and exercised against RimWorld 1.6.4871. The current
implementation is keyboard-only and uses RimWorld's native command hotkey
surface; it owns no mouse-input adapter or global input poll.

## Current artifact

- Shipping DLL SHA-256:
  `B394220B871A48A33CD787ADC94D32D84EF9FD228BA53C2C63C97D768FDA44AC`.
- Shipping DLL size: 19,456 bytes.
- Assembly version: 1.0.0.0.
- Automated contracts: 15/15 passed, 76 assertions.
- Package validation: `RWT-BUILD-PACKAGE-VALID`; the shipping package has one
  DLL.
- Harmony ownership: one `Pawn.GetGizmos` postfix.
- Default binding: primary `F`, secondary `None`.

The keyboard-only revision removed two runtime input helpers, the binding-dialog
prefix, the `UIRoot_Play` input postfix, and the InputLegacy project reference.
The net source/test/documentation change removed 371 lines before this record
was refreshed.

## Final combined live lane

Session `coolnether-suite-7dec0af702844a49b737a13f4b23608f` loaded Task
Break with Spine, Better Work Tab, the other seven gameplay mods, Harmony,
Vehicle Framework, Save Our Ship 2, Ideology, and Biotech.

| Scenario | Result |
| --- | --- |
| Native F input | Passed. With only ordinary inspect windows open, RimWorld's native gizmo hotkey changed pawn 22374 from `Wait` job 563 to `Wait_Wander` job 988. |
| State safety | Passed. Draft remained false; queue, carried item, and reservation counts remained zero. |
| Input ownership | Passed. Harmony summary reports `CoolNether123.TaskBreak=1`; no dialog or global-update patch remains. |
| Filter by Example coexistence | Passed. The same lane activated **Allow by example** through an ordinary native click with `matched=1`, `activated=True`. |
| Save and reload | Passed. `FinalSuite_Current` completed load generation 1 and returned paused. |
| Developer mode | Passed. `devMode=True` after reload. |
| Logs | Passed. No matching in-game exception, no OnGUI misuse, and no Task Break error. |

Evidence session: `coolnether-suite-7dec0af702844a49b737a13f4b23608f`.

Final capture:
`final-suite-ready-20260802-062141-836.png`

## Covered safety behavior

Automated and prior focused gameplay coverage includes ordinary work, forced
work confirmation, protected medical patients, children, slaves, colony mechs,
carried items, repeat activation, removal from a copied save, settings
persistence, far-right gizmo ordering, and consumed Spine Alt-click routing.
Task Break never drafts or undrafts, never clears queued jobs, and defers to
`IsCurrentJobPlayerInterruptible` before using the vanilla forced-interruption
path.

The unpublished Spine dependency still produces RimWorld's metadata warning
about a missing public distribution URL. Runtime behavior is unaffected; no URL
is invented here.

The current candidate is a centralized-service and assembly-metadata rebuild
against Spine SHA-256
`3E857A09793BBFF839D0C18D197E480C9365B6384148F49F48669F068BBB9086`.
The combined live lane above remains exact evidence for its named historical
assembly; the parent release pass must record a final combined launch for this
candidate.
