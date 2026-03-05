import * as vscode from "vscode";
import { runRad, parseProfile } from "./radCli";
import { IssueTreeProvider, PatchTreeProvider } from "./treeProviders";
import { NodeStatusBar } from "./nodeStatusBar";

export function activate(context: vscode.ExtensionContext): void {
  const workspaceRoot = vscode.workspace.workspaceFolders?.[0]?.uri.fsPath;

  // ── Tree view providers ─────────────────────────────────────
  const issueProvider = new IssueTreeProvider(workspaceRoot);
  const patchProvider = new PatchTreeProvider(workspaceRoot);
  vscode.window.registerTreeDataProvider("radicleIssues", issueProvider);
  vscode.window.registerTreeDataProvider("radiclePatches", patchProvider);

  // ── Status bar ──────────────────────────────────────────────
  const statusBar = new NodeStatusBar();
  statusBar.startPolling();
  context.subscriptions.push(statusBar);

  // ── Commands ────────────────────────────────────────────────

  context.subscriptions.push(
    vscode.commands.registerCommand("radicle.init", async () => {
      const name = await vscode.window.showInputBox({ prompt: "Repository name" });
      if (!name) {
        return;
      }
      const desc = (await vscode.window.showInputBox({ prompt: "Description" })) ?? "";
      const branch =
        (await vscode.window.showInputBox({ prompt: "Default branch", value: "main" })) ?? "main";
      const result = await runRad(
        `init --name "${name}" --description "${desc}" --default-branch "${branch}"`,
        workspaceRoot
      );
      showResult("Init", result);
    })
  );

  context.subscriptions.push(
    vscode.commands.registerCommand("radicle.clone", async () => {
      const rid = await vscode.window.showInputBox({ prompt: "Radicle ID (rad:…)" });
      if (!rid) {
        return;
      }
      const result = await runRad(`clone ${rid}`, workspaceRoot);
      showResult("Clone", result);
    })
  );

  context.subscriptions.push(
    vscode.commands.registerCommand("radicle.sync", async () => {
      const result = await runRad("sync", workspaceRoot);
      showResult("Sync", result);
    })
  );

  context.subscriptions.push(
    vscode.commands.registerCommand("radicle.auth", async () => {
      const result = await runRad("auth");
      showResult("Auth", result);
    })
  );

  context.subscriptions.push(
    vscode.commands.registerCommand("radicle.self", async () => {
      const result = await runRad("self");
      if (result.exitCode === 0) {
        const profile = parseProfile(result.stdout);
        vscode.window.showInformationMessage(
          `Radicle identity: ${profile.alias} (${profile.did})`
        );
      } else {
        vscode.window.showErrorMessage(`Failed: ${result.stderr || result.stdout}`);
      }
    })
  );

  context.subscriptions.push(
    vscode.commands.registerCommand("radicle.nodeStart", async () => {
      const result = await runRad("node start");
      showResult("Node Start", result);
      statusBar.refresh();
    })
  );

  context.subscriptions.push(
    vscode.commands.registerCommand("radicle.nodeStop", async () => {
      const result = await runRad("node stop");
      showResult("Node Stop", result);
      statusBar.refresh();
    })
  );

  context.subscriptions.push(
    vscode.commands.registerCommand("radicle.nodeStatus", async () => {
      const result = await runRad("node status");
      if (result.exitCode === 0) {
        vscode.window.showInformationMessage(`Node status: ${result.stdout.trim()}`);
      } else {
        vscode.window.showWarningMessage("Radicle node is not running");
      }
      statusBar.refresh();
    })
  );

  context.subscriptions.push(
    vscode.commands.registerCommand("radicle.issueList", async () => {
      issueProvider.refresh();
    })
  );

  context.subscriptions.push(
    vscode.commands.registerCommand("radicle.issueCreate", async () => {
      const title = await vscode.window.showInputBox({ prompt: "Issue title" });
      if (!title) {
        return;
      }
      const desc = (await vscode.window.showInputBox({ prompt: "Description" })) ?? "";
      const result = await runRad(
        `issue open --title "${title}" --description "${desc}"`,
        workspaceRoot
      );
      showResult("Create Issue", result);
      issueProvider.refresh();
    })
  );

  context.subscriptions.push(
    vscode.commands.registerCommand("radicle.patchList", async () => {
      patchProvider.refresh();
    })
  );

  context.subscriptions.push(
    vscode.commands.registerCommand("radicle.patchCreate", async () => {
      const message = await vscode.window.showInputBox({ prompt: "Patch message" });
      if (!message) {
        return;
      }
      const result = await runRad(`patch open --message "${message}"`, workspaceRoot);
      showResult("Create Patch", result);
      patchProvider.refresh();
    })
  );

  context.subscriptions.push(
    vscode.commands.registerCommand("radicle.issuesRefresh", () => {
      issueProvider.refresh();
    })
  );

  context.subscriptions.push(
    vscode.commands.registerCommand("radicle.patchesRefresh", () => {
      patchProvider.refresh();
    })
  );
}

function showResult(
  label: string,
  result: { exitCode: number; stdout: string; stderr: string }
): void {
  if (result.exitCode === 0) {
    vscode.window.showInformationMessage(`${label}: ${result.stdout.trim() || "Success"}`);
  } else {
    vscode.window.showErrorMessage(`${label} failed: ${result.stderr || result.stdout}`);
  }
}

export function deactivate(): void {
  // nothing to clean up beyond disposables
}
