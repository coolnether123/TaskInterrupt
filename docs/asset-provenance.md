# Asset provenance

Where every bundled non-code asset came from, and under what terms. This file
exists because the previous sneeze audio arrived with no recorded source and
every metadata chunk stripped, and working out whether it was safe to publish
cost an hour of forensics that this page would have made unnecessary.

Not part of the release payload. `Engineering/build.json` ships `About`, the
assembly, `Defs`, `Languages`, `Patches` and `Sounds` only.

## Sounds/TaskInterrupt/Sneeze*.wav

Goofy mode's sneeze. Four grains behind the `TaskInterrupt_Achoo` SoundDef.

| File | Source | Licence |
| --- | --- | --- |
| `Sneeze1.wav`, `Sneeze2.wav`, `Sneeze3.wav` | [markbryant, "Real Sneeze, Atchoo - 3 Sneezes, recorded in voiceover booth.wav"](https://freesound.org/people/markbryant/sounds/611187/), uploaded 2021-12-10 | Creative Commons 0 |
| `Sneeze0.wav` | [sidequesting, "sneeze.wav"](https://freesound.org/people/sidequesting/sounds/558936/), uploaded 2021-02-17 | Creative Commons 0 |

Both licences were read from the Freesound pages directly, not taken on trust:
"You can copy, modify, distribute and perform the sound, even for commercial
purposes, all without the need of asking permission to the author."

CC0 is a public-domain dedication and requires no attribution, so neither the
About nor the Workshop description credits them. This record is for us.

The first three are separate takes from one recording, which is why they share
an `ICRD` timestamp of `2021-12-10T11:31:26Z` and differ only by `ITRK`. That
timestamp matches the Freesound upload date exactly.

### Processing applied

Sourced files were 48 kHz (markbryant) and 44.1 kHz (sidequesting), peaking at
-0.6 and -1.8 dBFS. As shipped they are:

- mono, 44.1 kHz, 16-bit PCM. Mono is required, not cosmetic: the SoundDef is
  `MapOnly` with a `distRange`, so Unity spatialises it, and Unity only
  spatialises mono clips. A stereo clip plays non-positionally with no error.
- metadata chunks stripped
- silence trimmed to a 40 dB floor, with 5 ms fades so the cuts cannot click
- loudness-matched to an audible RMS of -19.0 to -20.9 dBFS with peaks at or
  below -1.0 dBFS. The sources were peak-matched but perceptually 5 dB apart,
  which would have made the random pick audible as a pick.
