# Toolchain install for Phase 4

Date: 2026-09-23. Status: required before Unity Editor steps can run.

This file is a step-by-step install recipe the owner runs on this Mac.
It does not run anything itself. The agent can write code while the install
is in progress; Unity Editor steps wait until the editor is open.

The exact Unity editor version is `6000.3.22f1` per `SETUP_ADR.md`.
If the installed version differs, the real version is recorded in
`ProjectVersion.txt` and `PHASE_4_PRODUCTION_REPORT.md`.

## 1. Install Unity Hub

1. Open Safari.
2. Go to https://unity.com/download.
3. Click "Download Unity Hub for macOS".
4. Open the downloaded `.dmg`, drag `Unity Hub.app` into `/Applications`.
5. Launch Unity Hub from `/Applications`.
6. Sign in with the existing Unity ID (or create one if needed).

## 2. Install Unity Editor 6000.3.22f1

1. In Unity Hub, click the **Installs** tab (left sidebar).
2. Click **Install Editor** (top-right).
3. In the version list, click **Archive** (top-right of the version picker).
4. Choose **6000.3.22f1** and confirm.
5. On the **Add modules** screen, leave defaults and add:
   - **Mac Build Support (Mono)** — required for the smoke build.
   - **Documentation** — optional but useful.
6. Click **Install**. Wait for the download + install (1-3 GB, ~5-15 min).
7. After install, return to the **Projects** tab.

## 3. Activate a Personal (or Pro) license

1. In Unity Hub, click the user icon (top-right) → **Manage licenses**.
2. Click **Activate New License**.
3. Choose **Unity Personal** if you do not have a Pro seat (this project is
   local-only and not commercially distributed yet).
4. Accept the terms and return to the Projects tab.

## 4. Install .NET 8 SDK

1. Open Safari and go to https://dotnet.microsoft.com/download/dotnet/8.0.
2. Under **SDK 8.0.x**, click the **macOS Arm64** installer (this Mac is
   Apple Silicon). If it is Intel, pick **macOS x64** instead.
3. Open the downloaded `.pkg` and follow the installer.
4. After install, open Terminal (or this shell) and run:

   ```bash
   dotnet --version
   ```

   The output must be `8.0.x`. If `dotnet` is not found, restart the shell so
   `PATH` picks up `/usr/local/share/dotnet` or `$HOME/.dotnet`.

## 5. Confirm to the agent

Reply in chat:

```
Unity Hub installed: yes
Unity 6000.3.22f1 installed: yes (or: installed <actual version>)
Unity license active: yes
dotnet --version: 8.0.x
```

The agent will then:

- create the Unity project at the agreed path,
- run the smoke build,
- write `docs/PHASE_4_PRODUCTION_REPORT.md`,
- push the final commit.

## Common failure modes

- **Hub cannot find the editor archive.** Click the gear icon next to the
  installed editor and choose "Browse" to point at
  `/Applications/Unity/Hub/Editor/6000.3.22f1`. Open that editor at least
  once so Hub registers it.
- **`dotnet` not on PATH.** Run:

  ```bash
  echo 'export PATH="$PATH:/usr/local/share/dotnet"' >> ~/.zshrc
  source ~/.zshrc
  dotnet --version
  ```

- **Mac Gatekeeper blocks the editor.** System Settings → Privacy & Security
  → "Open Anyway" for Unity.
