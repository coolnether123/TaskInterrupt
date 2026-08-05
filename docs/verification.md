# Verification record

Task Interrupt was built and exercised against RimWorld 1.6.4871. The current
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
| Input ownership | Passed. Harmony summary reports `CoolNether123.TaskInterrupt=1`; no dialog or global-update patch remains. |
| Filter by Example coexistence | Passed. The same lane activated **Allow by example** through an ordinary native click with `matched=1`, `activated=True`. |
| Save and reload | Passed. `FinalSuite_Current` completed load generation 1 and returned paused. |
| Developer mode | Passed. `devMode=True` after reload. |
| Logs | Passed. No matching in-game exception, no OnGUI misuse, and no Task Interrupt error. |

Evidence session: `coolnether-suite-7dec0af702844a49b737a13f4b23608f`.

Final capture:
`final-suite-ready-20260802-062141-836.png`

## Covered safety behavior

Automated and prior focused gameplay coverage includes ordinary work, forced
work confirmation, protected medical patients, children, slaves, colony mechs,
carried items, repeat activation, removal from a copied save, settings
persistence, far-right gizmo ordering, and consumed Spine Alt-click routing.
Task Interrupt never drafts or undrafts, never clears queued jobs, and defers to
`IsCurrentJobPlayerInterruptible` before using the vanilla forced-interruption
path.

At the time of this record Spine had no public distribution URL, so RimWorld
emitted its missing-URL metadata warning. Spine's repository is public as of
2026-08-05 and `About.xml` names it, so that warning no longer applies.

The current candidate is a centralized-service and assembly-metadata rebuild
against Spine SHA-256
`3E857A09793BBFF839D0C18D197E480C9365B6384148F49F48669F068BBB9086`.
The combined live lane above remains exact evidence for its named historical
assembly; the parent release pass must record a final combined launch for this
candidate.
## Final release-candidate gate — 2026-08-03

Passed 15 contracts (76 assertions), clean build, and package checks. Live
activation changed an ordinary Wait job to normal AI work without drafting the
pawn or leaving reservations. The F key label stayed at the top-left of the
rightmost mod gizmo. The same action passed beside RimHUD and Achtung after
Achtung's first-run help dialog was dismissed; the tooltip now stays focused on
the command rather than advertising Alt-click.

## Public-release gate — 2026-08-05

Re-run after the Task Break to Task Interrupt rename, the drafted-pawn gizmo
rule, and Goofy mode. Everything below was measured on this date.

- `dotnet run --project Tests\Mod.Tests.csproj -c Release` reported
  `PASS: 15 Task Interrupt contracts (76 assertions)`, exit code 0. Coverage:
  ordinary task is interruptible; forced task requires confirmation; protected
  states fail closed; game interruption contract wins; repeat activation is
  bounded; tick rollback starts a new session; keybinding defaults to F; F
  reuse ignores only vanilla forbid; assigned key uses native gizmo hotkey;
  input stays on RimWorld's keyboard gizmo path; mod gizmo sorts after vanilla;
  compiled implementation avoids draft dance; active medical patients are
  protected; gizmo assessment is amortized per frame; Alt settings navigation
  consumes the gizmo.
- Centralized build through `Invoke-RimWorldBuild.ps1` for RimWorld 1.6 with
  resolved `harmony,spine`: zero errors, zero warnings.
- Shipping `TaskInterrupt.dll` — 22,016 bytes, SHA-256
  `7B8B969A4C545C3DEB8DD499D1E8831A777FD9FA8C746A311A9440262C0B3761`.
- Built against `Spine.dll` — 113,152 bytes, SHA-256
  `FEFFA6D8EEF395D5C494BA335D0AD2A7823792C91A5967CBE2DBC2417FFD18CD`.
- Isolated lane `FactionLens-a860c97c4d16494b94a853df1c9d06b0` reached
  `programState=Playing` with Task Interrupt among its active mods and no
  exception in `Player.log`.

Two defects this gate caught that the preceding suite build did not:

1. `Tests\Mod.Tests.csproj` still compiled `TaskBreakDecision.cs`,
   `TaskBreakFacts.cs` and `TaskBreakPolicy.cs`, none of which survived the
   rename, so these contracts had silently stopped compiling. The suite build
   compiles `Source` only, which is why a green suite build proved nothing
   about the tests.
2. `Engineering\build.json` omitted `Sounds` from `releaseIncludePaths`. The
   packaged mod would have carried Goofy mode's `SoundDef` with none of the
   audio it names, so an enabled Goofy mode would have been silent and logged
   a missing-clip error on every activation.

Not covered by this gate: a fresh combined live lane with the full mod list,
last exercised at the 2026-08-03 gate under the mod's previous name.
