# RPG2D NavMeshPlus compatibility patch

Upstream: h8man/NavMeshPlus 0.2.23.
Base commit: 8fb73cc139cbb468524bc1862fcd0fbcccad44f4.
Prepared: 2026-10-13 for Tuanjie 1.9.3 / 2022.3.62t11.

Only NavMeshComponents/Scripts/NavMeshBuilder2d.cs is changed. Collider source resolution uses compositeOperation and the actual collider.composite owner under TUANJIE_1_9_OR_NEWER as well as UNITY_2023_1_OR_NEWER. Earlier Unity versions retain the original usedByComposite path. Null input / missing composite owners are checked before mesh creation. Mesh generation, area assignment and movement behavior are unchanged.

Reason: this Tuanjie version exposes the newer composite APIs while retaining Unity 2022.3 version symbols. The upstream conditional therefore selected the obsolete API branch. MainGameScene baked trees successfully but logged BoundTilemap / CollisionTilemap mesh is null and skipped their obstacle sources; an earlier Wall bake also raised MissingComponentException during this resolution.

The runtime sources have a separate compile check against the installed engine. After applying the embedded patch, the user confirmed baking and movement recovery. Editor.log records Sources 45 instead of 43 without the prior null-mesh / missing-component messages, and the saved MainGameScene references the updated baked asset. The actual runtime and editor projects compile successfully; the editor retains an existing MSB3277 reference warning. No claim is made about the exact internal value of the obsolete native getter.

Keep this embedded package, its original metadata and upstream license in project source control. The unchanged pinned Git dependency documents the upstream version; the embedded package takes precedence. Reassess this patch when upgrading either Tuanjie or NavMeshPlus.
