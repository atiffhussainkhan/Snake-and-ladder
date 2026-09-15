# ADR-001: Unity and Deterministic Domain Setup

Date: 2026-09-07. Status: selected under delegated owner approval.
Applies to DEC-002, DEC-009 and DEC-010. Installation/build verification is BLOCKED.

## Selected configuration

| Concern | Decision | Verification before implementation delivery |
| --- | --- | --- |
| Editor | Unity 6.3 LTS, exact initial pin 6000.3.22f1 | Install via Hub; record ProjectVersion.txt; review patch issues before upgrade |
| Language | Unity-supported C# 9 subset; no records, init-only setters, unsafe code or runtime code generation | Compile domain in Unity and external host |
| API | .NET Standard 2.1 for shared domain; checked integer rules arithmetic | No UnityEngine reference; conformance suite |
| Backends | Mono for editor iteration; IL2CPP ARM64 for Android validation | Same vectors and hashes on both |
| First client | Android 10+, ARM64, OpenGL ES 3, 4 GB RAM, 720p portrait | Physical floor-device results; this is a target, not measured support |
| Development | Windows 11 x64 | Editor compilation and smoke suite |
| Other platforms | iOS, WebGL, desktop releases and Facebook integration deferred | Separate platform ADR and evidence |
| Rendering | Universal 3D (URP) template for later 2.5D presentation | Pin editor-compatible template packages at authorized creation |
| Tests | Unity Test Framework 1.6.0; EditMode NUnit for domain, PlayMode for client | Resolve exact package with editor and commit lockfile |
| Formatting | Existing EditorConfig, four-space C#, UTF-8/CRLF; warnings as errors for owned domain | CI formatting/compile diagnostics after project exists |
| Packages | No third-party runtime dependencies initially; no floating versions | Commit manifest, lockfile, assembly definitions and meta files |
| CI policy | Pull-request fast checks; bounded scheduled simulation separately | Add executable workflow only in authorized validation phase |

Unity documents [6000.3.22f1](https://unity.com/releases/editor/whats-new/6000.3.22f1)
and [6.3 LTS support](https://unity.com/releases/unity-6/support).
The profile choices follow [.NET compatibility](https://docs.unity3d.com/6000.3/Documentation/Manual/dotnet-profile-support.html)
and [C# compatibility](https://docs.unity3d.com/6000.3/Documentation/Manual/csharp-compiler.html).
Use [Test Framework 1.6](https://docs.unity3d.com/Packages/com.unity.test-framework@1.6/manual/index.html)
and [platform requirements](https://docs.unity3d.com/6000.3/Documentation/Manual/system-requirements.html)
for project verification. This pin is not a claim to be the newest patch.

## Assembly ownership and interfaces

These future paths reconcile the documentation-only root with master plan section 10.
Creating them is a later gated action.

| Future path / assembly | Responsibilities | Dependencies |
| --- | --- | --- |
| Assets/Domain / BlockRivals.Domain | State, actions, rules, events, canonical serialization, deterministic RNG | .NET Standard base libraries only |
| Assets/Application / BlockRivals.Application | Intent routing, lifecycle, explicit timeout actions | Domain |
| Assets/AI / BlockRivals.AI | Observation-to-action policies | Domain |
| Assets/Infrastructure / BlockRivals.Infrastructure | Storage, clock, network and secure seed adapters | Application, Domain |
| Assets/Presentation / BlockRivals.Presentation | Input, animation, audio, accessibility | Application, Domain, Unity |
| Assets/Simulation / BlockRivals.Simulation | Bounded headless experiments | Domain, AI |
| Assets/Tests | EditMode, PlayMode and shared vectors | Tested assemblies; test-only packages |

Domain assembly definition sets noEngineReferences. One source tree is compiled by
Unity and the external simulator; no duplicate rules or checked-in DLL copy. Future
external tooling projects require explicit exceptions to the generated csproj/sln
ignore policy under their own tooling path.

The contract is `Apply(state, action) -> result`: success returns new state, ordered
events and canonical hash; rejection returns a stable error with unchanged state,
RNG cursor and event sequence. It reads no clock, filesystem, environment, network or
Unity APIs. Host-supplied configuration and explicit timeout actions are inputs.
Observations exclude secret seeds and future randomness. Actions carry match ID,
actor seat, monotonically increasing action ID and expected state version.
Action IDs are per (match, actor seat), start at 1, and advance only on acceptance.
Validate match and actor authority first, then look up the ID: an identical canonical
payload retry returns the stored receipt even after settlement; changed payload under
that ID rejects Conflict. New IDs must be exactly lastAccepted+1, then pass terminal,
state-version and phase checks in that order. Invalid IDs/versions reject unchanged.
State version begins at 0 and increments once per accepted external action including
terminal failures. Internal setup transitions produce the initial version-0 state.
History retains all accepted payloads for the bounded match. Rejected IDs can be retried
with corrected input; they were never accepted.

## Runtime boundary

First playable target: offline local play and AI. No accounts, chat, ads, real-money
purchases, analytics, cloud saves or network transport. All designed game mechanics
remain in scope. Online multiplayer is deferred to a separate release gate; an offline
host is not a secure online authority. The offline host owns state, with the same
intent/event interfaces reserved for a future server.

Future online policy: server-generated seed, authenticated actors, no client outcomes,
snapshot plus event cursor on resync, and idempotent commands. Disconnect does not
pause the turn: explicit timeout applies the same default as local play. Three
consecutive missed turns replace the seat with Balanced AI until match end; later
reconnect is spectator-only. Deployment and ranked play require separate evidence.

## Measurable targets

Device measurements remain BLOCKED. Selected floor-device targets: 30 fps, p95 frame
time <= 33.3 ms over 20 minutes, peak process memory <= 512 MiB, cold interactive start
<= 10 seconds, domain action p99 <= 5 ms excluding rendering/storage. Record actual
device, OS, ambient temperature and thermal behavior. Animations last <= 1.5 seconds
and are skippable; reduced-motion transitions <= 0.2 seconds. Neither changes outcomes.
Numbers remain readable at 720p; cues use text/shape as well as color; audio has visual
equivalents; all choices support single taps without reaction-speed rewards.

## Local evidence and environment

Inspection found no Unity editor at the standard Hub editor path and no Unity command
on PATH. The dotnet executable exists, but `dotnet --list-sdks` returned no SDKs.
This is a toolchain blocker, not a compiler failure or proof no custom editor exists.
No installed version or license readiness is assumed. The environment example remains
documentation-only; server secrets never enter Unity assets. Replays capture explicit
configuration rather than rereading environment variables.
