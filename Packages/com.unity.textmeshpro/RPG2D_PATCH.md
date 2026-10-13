# RPG2D TextMeshPro profiling patch

Base package: Tuanjie-provided com.unity.textmeshpro 3.0.9.
Engine: Tuanjie 1.9.3 / 2022.3.62t11.
Prepared: 2026-10-13.

Changed files: Scripts/Runtime/TMPro_UGUI_Private.cs and Tests/Runtime/TMP_RuntimeTests.cs.
Original SHA-256: 10D904488B890908E6A629EB4DFE46415B87ABC7CF6A0B6CFA6B79D754B349EC.
Patched SHA-256: 17880F46EF0ED1ABE3D82021CC5EF9CA4A8C91F2E4BA928114308EDED93136C5.

SetArraySizes owns one ProfilerMarker.Auto scope. The two extra End calls inside SetArraySizesWithoutATP were removed. Early returns and exceptional exits close the outer scope through Dispose. Text processing, font behavior, ATP selection and public APIs are unchanged.

Reason: the Tuanjie wrapper added a Begin/End pair but retained the inner function's End calls. Profiling UI text emitted Non-matching Profiler.EndSample: TMP.SetArraySizes, and the same unexpected error log failed two project PlayMode tests. The tests and their error checks remain unchanged.

The runtime test fixture explicitly uses the LiberationSans SDF asset included in TMP Essential Resources and asserts that it exists. Its fixed line-count cases depend on this font; the project's SourceHanSansSC default produces different wrapping. Test assertions and the game's default font remain unchanged.

The complete 63-file runtime source set compiles against the installed engine with 0 warnings and 0 errors. UI sampling recovery was confirmed by the user. After importing and compiling the fixed fixture, all 19 PlayMode cases pass: 4 project cases and 15 TMP cases. The previously verified 9 EditMode cases remain passed. The embedded package's tests are retained; no test or log checking was disabled.

Keep this embedded package, original metadata and upstream license in project source control. Reassess the patch when upgrading TextMeshPro or Tuanjie. This is a project compatibility patch, not a claim about upstream release or issue-tracker status.
