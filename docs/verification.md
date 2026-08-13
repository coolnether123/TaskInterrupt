# Verification record

Interrupt Task was rebuilt against RimWorld 1.6.4871 and SpineLib 1.1.0 on
2026-08-13. The most recent live behavior evidence remains the 2026-08-07
harness sessions recorded below.
The current implementation is keyboard-only and uses RimWorld's native command
hotkey surface; it owns no mouse-input adapter or global input poll.

## Current artifact

- Shipping DLL SHA-256:
  `075C0E7E9B7AB1DF43542D37D08E1A2B4B2146489792A5581FB3927A432FB943`.
- Shipping DLL size: 32,768 bytes.
- Assembly version: 1.0.0.0.
- Automated contracts: 18/18 passed, 80 assertions.
- Package validation: `RWT-BUILD-PACKAGE-VALID`; the shipping package has one
  DLL.
- Harmony ownership: one `Pawn.GetGizmos` postfix.
- Default binding: primary `F`, secondary `None`.

The keyboard-only revision removed two runtime input helpers, the binding-dialog
prefix, the `UIRoot_Play` input postfix, and the InputLegacy project reference.
The net source/test/documentation change removed 371 lines before this record
was refreshed.

## Final combined live lane — 2026-08-07

Session `coolnether-suite-5fdd4071f3b345cbbb68688fc8331371` used the maintained
`coolnether-suite` profile. All 16 declared non-core packages loaded, including
Spine, Better Work Tab, the other seven gameplay mods, Harmony, Vehicle
Framework, Save Our Ship 2, Ideology, Biotech, and Task Interrupt.

| Scenario | Result |
| --- | --- |
| Native F input | Passed. RimWorld's native gizmo hotkey changed pawn 40060 from `Wait` job 76 to `Wait_Wander` job 77. |
| State safety | Passed. Draft remained false; queue, carried item, and reservation counts remained zero. |
| Input ownership | Passed. Harmony summary reports `CoolNether123.TaskInterrupt=1`; no dialog or global-update patch remains. |
| Full-stack startup | Passed. All declared packages were active in the generated colony. |
| Logs | Passed for Task Interrupt. No Task Interrupt exception or error was present. Vehicle Framework emitted one dedicated-thread `ThreadAbortException` during normal lane shutdown; it is external to Task Interrupt. |

Evidence session: `coolnether-suite-5fdd4071f3b345cbbb68688fc8331371`.

Final capture:
`A:/Dev/RimWorld/Runtime/AgentLanes/1.6/coolnether-suite-5fdd4071f3b345cbbb68688fc8331371/ipc/captures/taskinterrupt-fullsuite-after-20260807-050726-411.png`

## Covered safety behavior

Automated and prior focused gameplay coverage includes ordinary work, forced
work confirmation, protected medical patients, children, slaves, colony mechs,
carried items, repeat activation, removal from a copied save, settings
persistence, far-right gizmo ordering, and consumed Spine Alt-click routing.
Task Interrupt never drafts or undrafts, never clears queued jobs, and defers to
`IsCurrentJobPlayerInterruptible` before using the vanilla forced-interruption
path.

Spine's public repository URL is present in `About.xml`, so the missing-URL
metadata warning does not apply. The current candidate was built against
SpineLib 1.1.0, SHA-256
`87D11805F615FE24AE8A0A28A5F0FC4C7D09D4BB805C17F83EE7E909B87729F3`.
## Final release-candidate gate — 2026-08-03

Passed 15 contracts (76 assertions), clean build, and package checks. Live
activation changed an ordinary Wait job to normal AI work without drafting the
pawn or leaving reservations. The F key label stayed at the top-left of the
rightmost mod gizmo. The same action passed beside RimHUD and Achtung after
Achtung's first-run help dialog was dismissed; the tooltip now stays focused on
the command rather than advertising Alt-click.

## Public-release gate — 2026-08-05

Re-run after the Task Interrupt rename, the drafted-pawn gizmo
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

## Final public-release gate — 2026-08-07

- The 15 automated contracts and 76 assertions passed.
- The centralized RimWorld 1.6 build passed with zero compiler errors and zero
  compiler warnings.
- The current shipping DLL is 22,016 bytes with SHA-256
  `E11F55BCDF68E65D6C1824F60464F29CA4B8977924EEF5FE0CC27B81BF83A015`.
- The Steam release allowlist contains only the runtime payload: `About`,
  `LoadFolders.xml`, the 1.6 assembly, `Shared`, `Languages`, `Patches`, and
  `Sounds`. `LICENSE` and `README.md` remain in the source repository but are
  intentionally excluded from the Steam payload.
- The fresh full compatibility lane passed native `F` activation and state
  safety with the complete `coolnether-suite` profile.

## Goofy Mode and settings persistence — 2026-08-07

Focused live session: `TaskBreak-eb8d1908bf8c43069d8484abab714c96`.

- The real in-game settings window showed `Goofy mode` off, then the live
  settings write used RimWorld's normal `Mod.WriteSettings()` path to turn it
  on. A follow-up in-game capture showed the green check and the command label
  changed from `Interrupt` to `Sneeze`.
- With a selected pawn on `Wait`, the native `F` hotkey changed the job to
  `Wait_Wander` while draft, queued-job, carried-item, and reservation state
  stayed clear. The Unity capture visibly showed `AH-CHOO!` over the pawn and
  the `Sneeze` gizmo.
- The Goofy setting was saved to the isolated lane's normal mod-settings file
  as `<goofyMode>True</goofyMode>`, then the saved colony was loaded through
  `dev-run load-save ... --pause-after-load`; the live setting readback stayed
  `True` after the load.
- The direct Player.log scan found zero Task Interrupt error, warning,
  exception, or missing-sound/clip lines. The harness's package-list entry is
  the only matching line.
- The Steam package's `TaskInterrupt_Achoo` `SoundDef` is present with four
  non-empty RIFF/WAVE clips, and the fresh launch of the updated source
  reported the package installed, active, enabled, compatible, and running as
  `Task Interrupt` with settings category `Task Interrupt`.

Captures:

- Settings off: `A:/Dev/RimWorld/Runtime/AgentLanes/1.6/TaskBreak-eb8d1908bf8c43069d8484abab714c96/ipc/captures/goofy-settings-before-20260807-052053-664.png`
- Settings on and `Sneeze` label: `A:/Dev/RimWorld/Runtime/AgentLanes/1.6/TaskBreak-eb8d1908bf8c43069d8484abab714c96/ipc/captures/goofy-current-20260807-052538-309.png`
- Live Goofy activation: `A:/Dev/RimWorld/Runtime/AgentLanes/1.6/TaskBreak-eb8d1908bf8c43069d8484abab714c96/ipc/captures/goofy-activation-after-20260807-052631-185.png`

## Compatibility cascade and final focused lane — 2026-08-07

The shared cascade executed 61 actions successfully for 1.6, 1.5, 1.4, 1.3,
1.2, 1.1, 1.0, 0.19, 0.18, 0.17, 0.16, 0.15, 0.14, 0.13, and Alpha 4.
The execution journal is recorded in `Engineering/cascade-evidence.json` with
SHA-256 `CE55AF162CAE5F26FD5BFBCA5F85713764B24AFF149A81F587232439C9B7537C`.
Each version has a staged `TaskInterrupt.dll` with a recorded size and hash.

The final focused runtime lane was RimWorld 1.6.4871. It verified the native
`Interrupt` gizmo, forced-job interruption to no current job, Alt-click
settings navigation, Goofy mode save/reopen persistence, the live `Sneeze`
label, and the same interruption behavior through `Sneeze`. The direct log
scan found no Task Interrupt-generated error, warning, exception, or missing
audio entries.

Only 1.6 was runtime-tested because the harness reported 1.6 as the only
runnable installed game and no older RimWorld executable was available on the
configured game roots. The older targets are therefore compile/cascade
verified, not runtime verified.

The final Steam archive is
`A:/Dev/RimWorld/Releases/TaskInterrupt-1.0.0-steam-rw1.6-20260807-final2.zip`.
It contains 13 runtime files, includes `LoadFolders.xml` and `Shared/Defs`,
and contains neither `README.md` nor `LICENSE`. The preview's corrected
`CoolNether123` author credit is included in that archive.

## Published 1.6-only package refresh — 2026-08-08

- The 16 automated contracts and 78 assertions passed.
- The release package advertises only RimWorld 1.6 and contains exactly one
  shipping assembly under `1.6/Assemblies`.
- Legacy definitions and historical-version assemblies are excluded from the
  published ZIP.
- The shipping `TaskInterrupt.dll` is 32,256 bytes with SHA-256
  `1E8EC2D526B642F0D24A46F02543EC20F5D952426D643B1C776452D2E1388A9B`.
- The staged package passed `RWT-BUILD-PACKAGE-VALID` after its metadata and
  load-folder routing were pruned to RimWorld 1.6.

## Final performance and compatibility gate — 2026-08-08

- The 18 automated contracts and 80 assertions passed against the freshly
  rebuilt assembly.
- The shipping DLL was built against the Steam SpineLib 1.0.1 assembly with
  SHA-256
  `A63C2DC0D0FA138251E02144C282BF93C0CD1ADA552803DD67F86F9E11301201`.
- The active-patient scan now routes every targeted job through the shared
  medical classifier, including modded work assigned to the Doctor work type.
- Task Interrupt records installation only when Spine reports that the pawn
  gizmo patch succeeded.
- Dubs Performance Analyzer measured the complete pawn-gizmo postfix at
  0.025823 ms per rendered frame on average over 1,999 sampled frames. The
  selection decision averaged 0.018239 ms and used no assembly-wide reflection.
- Clean focused lane `TaskBreak-4e2b44a7877c4db3a6f39780cb6fedac`
  loaded RimWorld 1.6.4871 with Steam SpineLib, Task Interrupt, and Dubs. The
  live `Interrupt` gizmo was enabled, Harmony reported exactly one Task
  Interrupt patch, and the Player log contained no Task Interrupt exception or
  error.
