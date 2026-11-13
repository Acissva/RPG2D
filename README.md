# RPG2D

RPG2D is a 2D adventure role-playing game project built with Unity / Tuanjie Engine.

v0.0.19 adds an AI guide NPC with free text input, whole replies and the latest four successful question / answer pairs. The request path is Unity → C# business API → Python AI service → DeepSeek. Gameplay / failure-recovery regression and Windows candidate observations passed. The C# candidate uses net9.0 / Release / win-x64, framework-dependent Production; health / notice returned 200 while OpenAPI / admin GET remained 404.

NpcChatConfig and NpcChatSettings use ConfigSystem; AI uses 70 seconds separately from notices. GameServerSettings defaults and the existing asset use http://localhost:5080. On this machine that root avoids the proxy path observed with 127.0.0.1. The rebuilt candidate completed a local reply and propagated cancellation when quitting during its next request; its process exited with code 0 and the fixture drained all requests. The earlier real conversation remains the paid sample (453 tokens, 72 UTF-16 reply units). UI / input observations follow user feedback. Formal builds use main and the v0.0.19 directory; release commit / tag and artifact identities are maintained in the design version records.

## Current Source Structure

The v0.0.19-pre structure separates common HTTP transport from notice business logic. NoticeSystem calls the common HTTP scheduler and holds its own notice request handle. It retains notice response validation, snapshots and previous-content policy; native request cleanup belongs to HttpRequestSystem. The Notice type and directory migration has been checked against the actual source, build and editor log. Private probe variables and diagnostic strings use Notice names.

GameServerSettings.TryValidate validates the common service root and timeout and returns a Uri. NoticeSystem constructs the api/notice URL; the Editor configuration tool calls the same common validation method. The existing configuration asset, resource key, metadata and serialized fields are unchanged.

The notice view script GUID, original StartScene component and all five UI references are unchanged; the learning probe GUID also remains intact. Five ordinary C# files received new GUIDs during the migration. Four old import IDs were recovered and had no serialized asset references; the old Model import ID was unavailable, so that search is explicitly incomplete. Current metadata is retained with this recorded deviation and successful build / runtime evidence.

Model interfaces and their single small implementations remain together in I…Model.cs, following the existing project convention. INoticeModel.cs therefore contains both INoticeModel and NoticeModel; there is no separate NoticeModel.cs.

LimitedDownloadHandler is located in Assets/Scripts/Runtime/System/HttpRequestSystem. It accepts a body byte limit for each instance, reuses a 4 KiB receive buffer, checks declared and accumulated length, and decodes UTF-8 strictly. The notice caller continues to use 16 KiB; the downloader contains no notice fields or model references.

HttpRequestFailure and HttpRequestResult define common technical failure, HTTP status, Content-Type, body and diagnostic data, with result strings normalized to empty values. IHttpRequestHandle / HttpRequestHandle provide terminal state and idempotent cancellation / disposal; construction and completion remain internal. The actual handle code passed 11 controlled callback checks.

IHttpRequestSystem / HttpRequestSystem support GET and JSON POST through one request lifecycle, with a separate handle and bounded download handler for each request. POST encodes the supplied string as strict UTF-8 and owns an UploadHandlerRaw alongside the request and download handler. RuntimeUpdateSystem drives completion; the scheduler releases native resources before delivering the result, and cancellation suppresses that request's callback. The system is registered directly after RuntimeUpdateSystem.

A real editor health GET and a UTF-8 JSON POST logged Failure=None, HTTP=200 and IsDone=True with the expected bodies. On 2026-11-05, the actual client Debug build passed with 0 warnings and 0 errors. Two real requests completed independently with one callback each; a second observation cancelled the pending POST from the GET callback, leaving zero POST callbacks and one GET callback. The isolated host recorded the matching abort. Earlier host-unavailable observations remain in the development history.

Nine real Unity boundary observations passed on 2026-11-05: empty and exact-limit bodies, declared and accumulated oversize rejection, UTF-8 byte segments, invalid UTF-8 rejection, complete HTTP 503 content, unparsed HTTP 200 JSON errors, and timeout. An isolated learning architecture also verified deinitialization and the later Play-stop quit observation: terminal handles, zero result callbacks, no update listener and rejection of new requests. The quit host samples still recorded completion, so this does not establish server-side task cancellation.

The actual client Runtime Debug build passed with 0 warnings and 0 errors. The actual Runtime / Editor joint build passed with 0 errors and one existing System.Numerics.Vectors reference conflict warning. After the connection refactor, the Editor configuration menu logged success and the real notice flow logged Loading to Ready through HttpRequestSystem and NoticeSystem.OnCompleted with the original notice body.

Real editor notice regressions passed against the isolated host: HTTP 503, malformed JSON and an incompatible protocol retain the previous content; closing the actual title panel cancels only the notice, keeps the snapshot unchanged afterwards, and allows an independent health request to complete once with HTTP 200. The host recorded the matching notice abort and health completion. Reopening the panel returned to Ready. The connection asset was restored exactly to port 5080 / 5 seconds, the scenario mode to ready, and config validation passed.

Current project tests passed: 9 EditMode and 4 PlayMode cases. An EditMode Run All also ran TextMeshPro package tests, with 13 passes and 5 fixed line-count assertion failures in two layout groups; these are recorded separately, with the unique cause unresolved. No package code, font, or assertions were changed to remove the failures. The PlayMode badge also includes 15 cached package results, which were not rerun. TestRunner's temporary runInBackground change has returned to its original value after the editor restart.

The API manual has been regenerated for the current source layout. Editor integration passed: real NPC dialogue, a dungeon round trip, title return, cancellation while starting the game, and cancellation of a pending refresh when quitting Play. The connection asset was restored exactly to port 5080 / 5 seconds. Movement, HUD and visual interaction observations use the user's completion feedback; native host events and editor logs provide the request / scene evidence.

The Windows candidate in Builds/v0.0.19-pre-candidate built successfully and ran in all three old-client / new-server, new-client / new-server and new-client / old-server combinations. A pending notice was aborted during actual Player exit: the process exited with code 0 about 1.867 seconds after the fixture received the request, and its log recorded Callback aborted / Shutdown. The fixture nevertheless recorded response_completed, so server task cancellation is not established. The old Builds/v0.0.16 Player is preserved.

After Ubuntu / Redis was restored, the same candidate Production process refilled and hit the valid Production notice cache with a finite TTL. The Windows build's generated LiberationSans fallback cache difference was restored; source-font configuration is unchanged. All 214 script inputs and six protected assets match the pre-build baseline. Candidate acceptance is separate from release identity, which is recorded by the final main commit, annotated version tag and published artifact evidence.

On 2026-10-27, the downloader migration preserved its original .meta, and GameServerSystem changed only its three downloader references. The actual client project passed a VS2022 MSBuild Debug build with 0 warnings and 0 errors. Subsequent transport concurrency, cancellation, lifecycle and body-boundary results are recorded above.

## Version Control

This Unity project uses Git for local version control and Git LFS for large binary assets.

Tracked project content includes:

- Assets
- Packages
- ProjectSettings

Ignored generated content includes:

- Library
- Temp
- Logs
- obj
- UserSettings
