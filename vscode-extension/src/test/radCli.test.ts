import * as assert from "assert";
import { parseProfile, parseIssues, parsePatches } from "../radCli";

suite("radCli Parser Tests", () => {
  test("parseProfile parses valid output", () => {
    const output = [
      "Node ID : z6Mkg2stTwq6MxKj84iWniBW2pRkGKFa7mPFJQTnDMtz5PN1",
      "DID     : did:key:z6Mkg2stTwq6MxKj84iWniBW2pRkGKFa7mPFJQTnDMtz5PN1",
      "Alias   : alice",
      "Home    : /home/alice/.radicle",
      "SSH Agent: running",
    ].join("\n");

    const profile = parseProfile(output);
    assert.strictEqual(profile.nodeId, "z6Mkg2stTwq6MxKj84iWniBW2pRkGKFa7mPFJQTnDMtz5PN1");
    assert.strictEqual(profile.did, "did:key:z6Mkg2stTwq6MxKj84iWniBW2pRkGKFa7mPFJQTnDMtz5PN1");
    assert.strictEqual(profile.alias, "alice");
    assert.strictEqual(profile.home, "/home/alice/.radicle");
    assert.strictEqual(profile.sshAgent, "running");
  });

  test("parseProfile handles empty output", () => {
    const profile = parseProfile("");
    assert.strictEqual(profile.nodeId, "");
    assert.strictEqual(profile.alias, "");
  });

  test("parseIssues parses valid output", () => {
    const output = [
      'abc1234 open "Fix the build" alice',
      'def5678 closed "Update README" bob',
    ].join("\n");

    const issues = parseIssues(output);
    assert.strictEqual(issues.length, 2);
    assert.strictEqual(issues[0].id, "abc1234");
    assert.strictEqual(issues[0].title, "Fix the build");
    assert.strictEqual(issues[0].state, "open");
    assert.strictEqual(issues[0].author, "alice");
    assert.strictEqual(issues[1].state, "closed");
  });

  test("parseIssues returns empty for blank input", () => {
    const issues = parseIssues("");
    assert.strictEqual(issues.length, 0);
  });

  test("parsePatches parses valid output", () => {
    const output = [
      'aaa1111 open "Add feature X" alice',
      'bbb2222 merged "Fix bug Y" bob',
    ].join("\n");

    const patches = parsePatches(output);
    assert.strictEqual(patches.length, 2);
    assert.strictEqual(patches[0].id, "aaa1111");
    assert.strictEqual(patches[0].title, "Add feature X");
    assert.strictEqual(patches[1].state, "merged");
  });

  test("parsePatches returns empty for blank input", () => {
    const patches = parsePatches("");
    assert.strictEqual(patches.length, 0);
  });
});
