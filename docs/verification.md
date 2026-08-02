# Verification record

Task Break was built and exercised against the local RimWorld 1.6.4871
installation. Compilation alone is not treated as gameplay evidence.

## Final artifacts

- Clean build: `task-break-build-20260801-k`.
- Automated tests: 13/13 passed.
- Shipping DLL SHA-256:
  `4CBB664EB666FEAEE003814F0ECA81CAA75A05832ECBA5CD8149777BC82496D5`.
- Final release directory: `task-break-release-20260801-k`.
- Package checks: `RWT-BUILD-RELEASE-PACKAGE-VALID` and
  `RWT-BUILD-PACKAGE-VALID`.
- Preview SHA-256:
  `E97DF1949ACB7B85F8266578B6B9E891A3D1158C97BDF950AAED5AE1E6D4B22F`.
- Final Player.log SHA-256:
  `B6540E253F3D97E0C4FE19C173938E97B87C0973037D34076298F183008104A7`.

The allowlisted package contains only About metadata and the real in-game
preview, the RimWorld 1.6 DLL, Defs, Languages, and the MIT license. Source,
tests, symbols, logs, Engineering records, and local paths are excluded.

## Final exact-binary lane

Session `TaskBreak-ff20ba1136b24acb80474caf1db06c89` staged the shipping
DLL above with Harmony, the current Spine build, and the RimWorld Agent. The
game loaded the controlled colony paused.

| Scenario | Result |
| --- | --- |
| Native presentation | Passed. **Break task** rendered after vanilla commands at the far right with the native `Mouse3` label. |
| Primary side-button binding | Passed. A physical button-3 command event changed pawn 47108 from `GotoWander` job 530 to `Wait_Wander` job 555. Draft state remained false and queue, carried item, and reservation counts remained zero. |
| Secondary side-button binding | Passed. Primary was set to `None` and secondary to `Mouse3`. The same event changed `Wait` job 573 to `GotoWander` job 574. State remained clean. |
| Alt-click settings routing | Passed. Alt-left-click opened one `Dialog_ModSettings` at the exact Task Break setting. Job 574 did not change. |
| Save and reload | Passed. `TaskBreak-Final-4CBB` reloaded paused. The key binding remained primary `Mouse3`, secondary `None`. |
| Shutdown | Passed. The harness released the lane normally with exit code 0 and without forced termination. |

Portable screenshots are retained under `Engineering/evidence/` and the
shipping preview is the exact far-right final-binary capture.

## Additional live scenarios

| Scenario | Result |
| --- | --- |
| Active medical patient | Passed. A doctor ran `TendPatient` against pawn 47108. The patient's command was disabled with `Medical care is protected`; both job IDs remained unchanged. |
| Forced work | Passed for protection and decline. A forced `Wait` job opened the confirmation dialog and remained unchanged when the dialog was closed as decline. With confirmation disabled in settings, the job was interrupted normally. The harness could not target the dialog's affirmative button, so affirmative-dialog input remains unverified separately from the verified setting-disabled execution path. |
| Carried item | Passed for ownership safety. After interruption the test pawn still carried the same five Steel; it was not lost or destroyed. This artificial carry fixture proves cleanup does not corrupt ownership, but is not a complete haul-driver simulation. |
| Repeat guard | Passed. A second activation at the same paused tick did nothing; activation worked again after exactly 30 ticks. |
| Biotech child and colony mech | Passed. Both received a new AI job while remaining undrafted with empty queues and no leaked reservations. |
| Ideology colony slave | Passed. The slave received a new AI job while retaining slave and undrafted state. |
| Removal from copied save | Passed. The save loaded with Task Break removed and Spine retained; no missing-state failure occurred. |
| Settings visibility and persistence | Passed. The command could be hidden and restored through settings and remained correct after reload. |

## Automated coverage

The 13 isolated contracts cover the fail-closed policy matrix, forced-work
confirmation, the exact 30-tick activation guard and tick rollback, the
`Mouse3` default, public `DefOf` initialization, native keyboard-command
handling, both mouse binding slots, far-right ordering, absence of the draft
dance, active-patient protection, and consumed Spine Alt-click routing.

The tests were not weakened to obtain a pass. A live medical-patient failure
found during independent review resulted in a production fix and a new
regression contract.

## Log analysis and limits

The final lane contains no Task Break error or exception. RimWorld reports
that the unpublished Spine dependency has no public download or Workshop URL;
the suite must supply Spine's real distribution URL when it exists rather
than inventing one. Spine also triggers the existing
`ConnectedOutlineDrawer` startup-attribute warning. Neither warning originates
from Task Break gameplay code.

Direct constructed live jobs remain unverified for crafting, recreation,
sleep, ingestion, ritual, caravan formation, birth/labor, and deathrest.
Their fail-closed policy paths are covered automatically, but compatibility
must not be claimed beyond the evidence above.

## 2026-08-02 left-hand binding follow-up

- The native default is now `F`, with secondary binding `None`; the command
  displays RimWorld's ordinary `F` badge in the icon's upper-left corner.
- Vanilla **Allow/forbid** and Task Break mutually ignore one another for
  conflict reporting while retaining their selection-specific behavior.
- All 16 automated contracts passed after the change. That intermediate DLL
  was 21,504 bytes with SHA-256
  `C1F12BD49AC0FC9E03294D4A168FB2AFFA1B33AF8B777EAFEA65106F94F57ACD`.
- The eight-mod session loaded with `primary=F`, `secondary=None` and no
  keybinding-conflict warning. Players can still rebind the action to any
  supported keyboard or mouse binding in RimWorld's Controls menu.

## 2026-08-02 input compatibility hardening

- Hidden-gizmo activation now checks both assigned slots directly, preserving
  arbitrary keyboard bindings and Mouse3 through Mouse6. Visible keyboard
  activation remains on RimWorld's native gizmo path, while only side buttons
  use the update adapter.
- Gizmo, keyboard, and mouse activation all enter
  `TaskBreakController.ActivateSelected`, so unavailable actions use the same
  exact localized reason. Identical primary and secondary assignments are
  polled once.
- The binding-dialog prefix consumes input only after a real MouseDown event;
  Layout and Repaint passes return untouched. An open window or focused search
  field blocks gameplay activation during rebinding and text entry.
- All 18 automated contracts pass. The release build is 22,016 bytes with
  SHA-256
  `23D63043F43CFC64A35B148623D4E2D190EE16DF51001AE4333B30146F3CB284`.
- The allowlisted release package passed
  `RWT-BUILD-RELEASE-PACKAGE-VALID` and `RWT-BUILD-PACKAGE-VALID`; it contains
  the DLL but no symbols, source, tests, or local evidence.

## 2026-08-02 ordinary-window activation fix

- A combined-suite run exposed that normal play keeps two `ImmediateWindow`
  entries and `MainTabWindow_Inspect` in `WindowStack`. The former blanket
  `Count > 0` guard therefore prevented hidden `F` and Mouse3-Mouse6 bindings
  from activating during ordinary play.
- Assigned input now remains active for that normal non-blocking stack and is
  suppressed only for a focused search widget, a non-immediate dialog, or a
  window that absorbs all input. Settings, binding, confirmation, and other
  modal dialogs remain protected. The direct gizmo path is unchanged.
- All 20 automated contracts pass. The release build is 22,528 bytes with
  SHA-256
  `D9C9D87F3E4028DA5B875FB5D4E5F90E912CFF095E2512B1D0D9936A2894FC89`.
  The allowlisted `TaskBreak-window-fix-package-20260802-0532` package passed
  both package validation layers and excludes symbols, source, tests, and
  local evidence.
