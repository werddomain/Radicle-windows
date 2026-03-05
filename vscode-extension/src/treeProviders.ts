import * as vscode from "vscode";
import { runRad, RadIssue, RadPatch, parseIssues, parsePatches } from "./radCli";

/**
 * Tree data provider that shows Radicle issues in the sidebar.
 */
export class IssueTreeProvider implements vscode.TreeDataProvider<RadIssue> {
  private _onDidChange = new vscode.EventEmitter<RadIssue | undefined>();
  readonly onDidChangeTreeData = this._onDidChange.event;

  constructor(private workspaceRoot: string | undefined) {}

  refresh(): void {
    this._onDidChange.fire(undefined);
  }

  getTreeItem(element: RadIssue): vscode.TreeItem {
    const item = new vscode.TreeItem(element.title, vscode.TreeItemCollapsibleState.None);
    item.description = `[${element.state}] by ${element.author}`;
    item.tooltip = `${element.id}\n${element.title}\nState: ${element.state}\nAuthor: ${element.author}`;
    item.iconPath = new vscode.ThemeIcon(element.state === "open" ? "issues" : "issue-closed");
    return item;
  }

  async getChildren(): Promise<RadIssue[]> {
    const result = await runRad("issue list", this.workspaceRoot);
    if (result.exitCode !== 0) {
      return [];
    }
    return parseIssues(result.stdout);
  }
}

/**
 * Tree data provider that shows Radicle patches in the sidebar.
 */
export class PatchTreeProvider implements vscode.TreeDataProvider<RadPatch> {
  private _onDidChange = new vscode.EventEmitter<RadPatch | undefined>();
  readonly onDidChangeTreeData = this._onDidChange.event;

  constructor(private workspaceRoot: string | undefined) {}

  refresh(): void {
    this._onDidChange.fire(undefined);
  }

  getTreeItem(element: RadPatch): vscode.TreeItem {
    const item = new vscode.TreeItem(element.title, vscode.TreeItemCollapsibleState.None);
    item.description = `[${element.state}] by ${element.author}`;
    item.tooltip = `${element.id}\n${element.title}\nState: ${element.state}\nAuthor: ${element.author}`;
    item.iconPath = new vscode.ThemeIcon(
      element.state === "merged" ? "git-merge" : element.state === "open" ? "git-pull-request" : "git-pull-request-closed"
    );
    return item;
  }

  async getChildren(): Promise<RadPatch[]> {
    const result = await runRad("patch list", this.workspaceRoot);
    if (result.exitCode !== 0) {
      return [];
    }
    return parsePatches(result.stdout);
  }
}
