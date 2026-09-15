# Repository Agent Instructions

## Scope

This repository is currently in pre-production planning. Do not create or modify production game code, Unity project files, dependencies, assets, migrations, deployment resources, or generated applications unless the master engineering plan explicitly authorizes that phase.

## Stack direction

The selected client direction is Unity + C#. Keep the future deterministic rules domain independent of Unity APIs, rendering, networking, wall-clock time, persistence, and environment variables. Treat the master plan and approved specifications in `docs/` as the source of truth.

## Secrets and environment

Never commit real credentials or secret values. `.env` files are local-only. `.env.example` must remain sanitized. Do not print secret values in reports.

## Planning and validation

Before edits, inspect repository instructions and preserve unrelated changes. Planning-only changes must remain documentation/configuration-only and must be validated with Markdown diagnostics, `git diff --check`, and a changed-file scope review.

Do not mark a planning gate `PASS` without repository-grounded evidence. Preserve `BLOCKED` where evidence is unavailable. Keep final authorization as `CODE WORK NOT APPROVED` until the master plan's gates pass and implementation is explicitly authorized.
