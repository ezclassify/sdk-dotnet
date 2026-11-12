/**
 * Contract tests for check-dependency-age.mjs, the third-party publish-time
 * quarantine over the NuGet restore graph and the pinned .NET tools.
 *
 * Hermetic: the gate is pointed at throwaway restore graphs and tool manifests
 * and at a local http server that stands in for nuget.org's registration hive,
 * so no test reaches the network. Run with `node --test scripts/*.test.mjs`.
 */
import { execFile } from 'node:child_process';
import { mkdtempSync, writeFileSync } from 'node:fs';
import { createServer } from 'node:http';
import { tmpdir } from 'node:os';
import { dirname, resolve } from 'node:path';
import { after, before, describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { fileURLToPath } from 'node:url';

const repositoryRoot = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const gate = resolve(repositoryRoot, 'scripts/check-dependency-age.mjs');

const DAY_MS = 86_400_000;
const NUGET_ORG = 'https://api.nuget.org/v3/index.json';

/** Registration stand-in: lowercase `id/version` to its leaf document. */
const leaves = {};
/** Every path the stand-in was asked for, to prove what was not requested. */
const requests = [];

let server;
let registration;

function isoDaysAgo(days) {
    return new Date(Date.now() - days * DAY_MS).toISOString();
}

function publish(id, version, published, listed = true) {
    leaves[`${id.toLowerCase()}/${version.toLowerCase()}`] = { listed, published };
}

before(async () => {
    server = createServer((req, res) => {
        const path = req.url ?? '/';
        requests.push(path);
        const match = /^\/([^/]+)\/([^/]+)\.json$/.exec(path);
        const key = match ? `${decodeURIComponent(match[1])}/${decodeURIComponent(match[2])}` : '';
        if (key.startsWith('broken.package/')) {
            res.writeHead(500).end('{}');
            return;
        }
        // Accepts the connection and never answers, for the overall bound.
        if (key.startsWith('hanging.package/')) return;
        const leaf = leaves[key];
        if (!leaf) {
            res.writeHead(404).end('{}');
            return;
        }
        res.writeHead(200, { 'content-type': 'application/json' });
        res.end(JSON.stringify(leaf));
    });
    await new Promise((done) => server.listen(0, '127.0.0.1', done));
    registration = `http://127.0.0.1:${server.address().port}`;
});

after(async () => {
    server.closeAllConnections();
    await new Promise((done, fail) => server.close((err) => (err ? fail(err) : done())));
});

function scratch(name, contents) {
    const dir = mkdtempSync(resolve(tmpdir(), 'ezclassify-dependency-age-'));
    const path = resolve(dir, name);
    writeFileSync(path, contents);
    return path;
}

/**
 * Write a project.assets.json in the shape restore writes. `libraries` maps
 * `Id/version` to `package` or `project`.
 */
function assetsWith(libraries = {}, sources = [NUGET_ORG]) {
    return scratch(
        'project.assets.json',
        JSON.stringify({
            version: 4,
            targets: {},
            libraries: Object.fromEntries(
                Object.entries(libraries).map(([key, type]) => [key, { type, path: key.toLowerCase() }]),
            ),
            project: {
                restore: { sources: Object.fromEntries(sources.map((source) => [source, {}])) },
            },
        }),
    );
}

function toolsWith(tools = {}) {
    return scratch(
        'dotnet-tools.json',
        JSON.stringify({
            version: 1,
            isRoot: true,
            tools: Object.fromEntries(
                Object.entries(tools).map(([id, version]) => [id, { version, commands: [id] }]),
            ),
        }),
    );
}

/**
 * Run the gate. Asynchronous on purpose: the stand-in registry is served from
 * this same process, so a synchronous spawn would block the event loop and the
 * gate's requests would never be answered.
 */
function run({ assets = assetsWith(), tools = toolsWith(), env = {} } = {}) {
    return new Promise((settle) => {
        execFile(
            process.execPath,
            [gate],
            {
                cwd: repositoryRoot,
                env: {
                    ...process.env,
                    EZCLASSIFY_DEPENDENCY_AGE_ASSETS: assets,
                    EZCLASSIFY_DEPENDENCY_AGE_TOOL_MANIFESTS: tools,
                    EZCLASSIFY_DEPENDENCY_AGE_NUGET_REGISTRATION: registration,
                    ...env,
                },
                encoding: 'utf8',
            },
            (error, stdout, stderr) => {
                settle({ status: error === null ? 0 : (error.code ?? 1), output: `${stdout}${stderr}` });
            },
        );
    });
}

describe('dependency quarantine, NuGet restore graph', () => {
    it('refuses a package published inside the window and says when it clears', async () => {
        const published = isoDaysAgo(2);
        publish('Fresh.Package', '1.4.0', published);
        const result = await run({ assets: assetsWith({ 'Fresh.Package/1.4.0': 'package' }) });
        assert.equal(result.status, 1, result.output);
        assert.match(result.output, /1 third-party package\(s\) were published less than 7 day\(s\) ago/);
        const clears = new Date(Date.parse(published) + 7 * DAY_MS).toISOString();
        assert.ok(result.output.includes(`Fresh.Package@1.4.0 (`), result.output);
        assert.ok(result.output.includes(`published ${published}, clears ${clears}`), result.output);
    });

    it('passes a package published outside the window', async () => {
        publish('Settled.Package', '2.0.1', isoDaysAgo(40));
        const result = await run({ assets: assetsWith({ 'Settled.Package/2.0.1': 'package' }) });
        assert.equal(result.status, 0, result.output);
        assert.match(result.output, /Dependency quarantine passed: 1 third-party NuGet package\(s\)/);
    });

    it('asks the registration hive by lowercase id and version', async () => {
        publish('Mixed.Case', '3.0.0-Preview.1', isoDaysAgo(40));
        const before = requests.length;
        const result = await run({ assets: assetsWith({ 'Mixed.Case/3.0.0-Preview.1': 'package' }) });
        assert.equal(result.status, 0, result.output);
        assert.deepEqual(requests.slice(before), ['/mixed.case/3.0.0-preview.1.json']);
    });

    it('skips a project reference as first-party, asking no registry', async () => {
        // The stand-in does not know the project and would answer 404, which
        // fails closed, so a pass proves it was not looked up.
        const before = requests.length;
        const result = await run({ assets: assetsWith({ 'EzClassify.Rest.Client/1.0.0-dev.1': 'project' }) });
        assert.equal(result.status, 0, result.output);
        assert.match(result.output, /skipped 1 project reference\(s\) as first-party/);
        assert.equal(requests.length, before);
    });

    it('holds an EzClassify-named package to the window like any other', async () => {
        publish('EzClassify.Lookalike', '1.0.0', new Date().toISOString());
        const result = await run({ assets: assetsWith({ 'EzClassify.Lookalike/1.0.0': 'package' }) });
        assert.equal(result.status, 1, result.output);
        assert.match(result.output, /EzClassify\.Lookalike@1\.0\.0 \(.*\) published/);
    });

    it('checks a package shared by two projects once and one resolved at two versions at both', async () => {
        publish('Twice.Package', '1.0.0', isoDaysAgo(90));
        publish('Twice.Package', '1.1.0', isoDaysAgo(1));
        const before = requests.length;
        const result = await run({
            assets: [
                assetsWith({ 'Twice.Package/1.0.0': 'package' }),
                assetsWith({ 'Twice.Package/1.0.0': 'package', 'Twice.Package/1.1.0': 'package' }),
            ].join(','),
        });
        assert.equal(result.status, 1, result.output);
        assert.match(result.output, /Twice\.Package@1\.1\.0 \(.*\) published/);
        assert.doesNotMatch(result.output, /Twice\.Package@1\.0\.0 \(.*\) published/);
        assert.equal(requests.length - before, 2);
    });

    it('checks the pinned tools too', async () => {
        publish('fresh-tool', '6.3.1', isoDaysAgo(3));
        const result = await run({ tools: toolsWith({ 'fresh-tool': '6.3.1' }) });
        assert.equal(result.status, 1, result.output);
        assert.match(result.output, /fresh-tool@6\.3\.1 \(.*dotnet-tools\.json\) published/);
    });

    it('fails closed when the registry cannot be reached', async () => {
        publish('Settled.Package', '2.0.1', isoDaysAgo(40));
        const result = await run({
            assets: assetsWith({ 'Settled.Package/2.0.1': 'package' }),
            env: { EZCLASSIFY_DEPENDENCY_AGE_NUGET_REGISTRATION: 'http://127.0.0.1:1' },
        });
        assert.equal(result.status, 1, result.output);
        assert.match(result.output, /fails closed/);
        assert.match(result.output, /Settled\.Package@2\.0\.1: could not reach http:\/\/127\.0\.0\.1:1/);
    });

    it('fails closed on a registry error and on a version the registry does not know', async () => {
        const result = await run({
            assets: assetsWith({ 'Broken.Package/1.0.0': 'package', 'Phantom.Package/9.9.9': 'package' }),
        });
        assert.equal(result.status, 1, result.output);
        assert.match(result.output, /Broken\.Package@1\.0\.0: http:\/\/127\.0\.0\.1:\d+ answered 500/);
        assert.match(result.output, /Phantom\.Package@9\.9\.9: http:\/\/127\.0\.0\.1:\d+ answered 404/);
    });

    it('fails closed on a version with no publish time', async () => {
        publish('Unlisted.Package', '1.0.0', '1900-01-01T00:00:00+00:00', false);
        leaves['dateless.package/1.0.0'] = { listed: true };
        const result = await run({
            assets: assetsWith({ 'Unlisted.Package/1.0.0': 'package', 'Dateless.Package/1.0.0': 'package' }),
        });
        assert.equal(result.status, 1, result.output);
        assert.match(result.output, /Unlisted\.Package@1\.0\.0: .* is unlisted, so the registry gives it no publish time/);
        assert.match(result.output, /Dateless\.Package@1\.0\.0: the registry reports no publish time/);
    });

    it('still reports a real violation found alongside an unreadable package', async () => {
        publish('Fresh.Package', '1.4.0', isoDaysAgo(2));
        const result = await run({
            assets: assetsWith({ 'Broken.Package/1.0.0': 'package', 'Fresh.Package/1.4.0': 'package' }),
        });
        assert.equal(result.status, 1, result.output);
        assert.match(result.output, /Broken\.Package@1\.0\.0: .*answered 500/);
        assert.match(result.output, /Fresh\.Package@1\.4\.0 \(.*\) published/);
    });

    it('fails closed on a restore that used a feed it cannot date', async () => {
        publish('Settled.Package', '2.0.1', isoDaysAgo(40));
        const result = await run({
            assets: assetsWith({ 'Settled.Package/2.0.1': 'package' }, [NUGET_ORG, 'https://feed.example.invalid/v3/index.json']),
        });
        assert.equal(result.status, 1, result.output);
        assert.match(result.output, /restored with https:\/\/feed\.example\.invalid\/v3\/index\.json, a feed this gate cannot date/);
    });

    it('fails closed within its overall bound when the registry never answers', async () => {
        const started = Date.now();
        const result = await run({
            assets: assetsWith({ 'Hanging.Package/1.0.0': 'package' }),
            env: { EZCLASSIFY_DEPENDENCY_AGE_OVERALL_TIMEOUT_MS: '500' },
        });
        assert.equal(result.status, 1, result.output);
        assert.match(result.output, /Hanging\.Package@1\.0\.0: the run passed its 500 ms bound before it was checked/);
        assert.ok(Date.now() - started < 10_000, `took ${Date.now() - started} ms`);
    });

    it('takes a shorter window from the environment', async () => {
        publish('Fresh.Package', '1.4.0', isoDaysAgo(2));
        const result = await run({
            assets: assetsWith({ 'Fresh.Package/1.4.0': 'package' }),
            env: { EZCLASSIFY_DEPENDENCY_AGE_DAYS: '1' },
        });
        assert.equal(result.status, 0, result.output);
        assert.match(result.output, /older than 1 day\(s\)/);
    });

    it('refuses a missing restore graph instead of passing', async () => {
        const result = await run({ assets: resolve(tmpdir(), 'no-such-dir', 'project.assets.json') });
        assert.equal(result.status, 1, result.output);
        assert.match(result.output, /project\.assets\.json does not exist\. Run `dotnet restore EzClassify\.sln` first\./);
    });
});
