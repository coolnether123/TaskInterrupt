# Verification record

Task Break was built and exercised against RimWorld 1.6.4871. The current
implementation is keyboard-only and uses RimWorld's native command hotkey
surface; it owns no mouse-input adapter or global input poll.

## Current artifact

- Shipping DLL SHA-256:
  `88DF4680FC4428F3543F12FE79A6D5EEE7BA4E004962DAB8C44EFBB96EE8FAC9`.
- Shipping DLL size: 19,968 bytes.
- Automated contracts: 15/15 passed.
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

Evidence root:
`C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\coolnether-suite-7dec0af702844a49b737a13f4b23608f`

Final capture:
`ipc\captures\final-suite-ready-20260802-062141-836.png`

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
