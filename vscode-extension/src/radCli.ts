import { exec } from "child_process";

/** Result of executing a Radicle CLI command. */
export interface RadCommandResult {
  exitCode: number;
  stdout: string;
  stderr: string;
}

/**
 * Execute a `rad` CLI command and return its output.
 */
export function runRad(
  args: string,
  cwd?: string
): Promise<RadCommandResult> {
  return new Promise((resolve) => {
    exec(`rad ${args}`, { cwd }, (error, stdout, stderr) => {
      resolve({
        exitCode: error?.code ?? 0,
        stdout: stdout ?? "",
        stderr: stderr ?? "",
      });
    });
  });
}

/** Parsed Radicle identity. */
export interface RadProfile {
  nodeId: string;
  did: string;
  alias: string;
  home: string;
  sshAgent: string;
}

/** Parsed Radicle issue. */
export interface RadIssue {
  id: string;
  title: string;
  state: string;
  author: string;
}

/** Parsed Radicle patch. */
export interface RadPatch {
  id: string;
  title: string;
  state: string;
  author: string;
}

/** Parse `rad self` output into a RadProfile. */
export function parseProfile(output: string): RadProfile {
  const profile: RadProfile = {
    nodeId: "",
    did: "",
    alias: "",
    home: "",
    sshAgent: "",
  };
  for (const line of output.split("\n")) {
    const idx = line.indexOf(":");
    if (idx === -1) {
      continue;
    }
    const key = line.substring(0, idx).trim().toLowerCase();
    const value = line.substring(idx + 1).trim();
    switch (key) {
      case "node id":
      case "nid":
        profile.nodeId = value;
        break;
      case "did":
        profile.did = value;
        break;
      case "alias":
        profile.alias = value;
        break;
      case "home":
        profile.home = value;
        break;
      case "ssh-agent":
      case "ssh agent":
        profile.sshAgent = value;
        break;
    }
  }
  return profile;
}

/** Parse `rad issue list` output. */
export function parseIssues(output: string): RadIssue[] {
  const issues: RadIssue[] = [];
  const re = /(?<id>[a-f0-9]+)\s+(?<state>\S+)\s+(?<title>"[^"]*"|\S+)\s+(?<author>\S+)/;
  for (const line of output.split("\n")) {
    const m = line.match(re);
    if (m?.groups) {
      issues.push({
        id: m.groups.id,
        title: m.groups.title.replace(/^"|"$/g, ""),
        state: m.groups.state,
        author: m.groups.author,
      });
    }
  }
  return issues;
}

/** Parse `rad patch list` output. */
export function parsePatches(output: string): RadPatch[] {
  const patches: RadPatch[] = [];
  const re = /(?<id>[a-f0-9]+)\s+(?<state>\S+)\s+(?<title>"[^"]*"|\S+)\s+(?<author>\S+)/;
  for (const line of output.split("\n")) {
    const m = line.match(re);
    if (m?.groups) {
      patches.push({
        id: m.groups.id,
        title: m.groups.title.replace(/^"|"$/g, ""),
        state: m.groups.state,
        author: m.groups.author,
      });
    }
  }
  return patches;
}
