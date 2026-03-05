import * as vscode from "vscode";
import { runRad } from "./radCli";

/**
 * Status bar item that shows the current Radicle node status.
 */
export class NodeStatusBar implements vscode.Disposable {
  private statusBarItem: vscode.StatusBarItem;
  private timer: NodeJS.Timeout | undefined;

  constructor() {
    this.statusBarItem = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Left, 50);
    this.statusBarItem.command = "radicle.nodeStatus";
    this.statusBarItem.text = "$(radio-tower) Radicle";
    this.statusBarItem.tooltip = "Click to check Radicle node status";
    this.statusBarItem.show();
  }

  /** Start polling node status every `intervalMs` milliseconds. */
  startPolling(intervalMs: number = 30_000): void {
    this.refresh();
    this.timer = setInterval(() => this.refresh(), intervalMs);
  }

  async refresh(): Promise<void> {
    const result = await runRad("node status");
    if (result.exitCode === 0 && result.stdout.toLowerCase().includes("running")) {
      this.statusBarItem.text = "$(radio-tower) Radicle: Running";
      this.statusBarItem.backgroundColor = undefined;
    } else {
      this.statusBarItem.text = "$(radio-tower) Radicle: Stopped";
      this.statusBarItem.backgroundColor = new vscode.ThemeColor("statusBarItem.warningBackground");
    }
  }

  dispose(): void {
    if (this.timer) {
      clearInterval(this.timer);
    }
    this.statusBarItem.dispose();
  }
}
