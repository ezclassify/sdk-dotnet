#!/usr/bin/env node
/**
 * Dependency quarantine gate.
 *
 * Refuses third-party NuGet packages that were published inside a quarantine
 * window, so a freshly compromised release has to sit on nuget.org for a while
 * before it can enter this repository's dependency tree.
 *
 * Reads what restore resolved rather than the manifests. The repository commits
 * no NuGet lockfile, so the exact set is each solution project's
 * `obj/project.assets.json`, which `dotnet restore` writes: every package and
 * version restore chose, transitive ones included. Run the gate after restore
 * and before build, so a package's build targets never run before it is
 * cleared. The pinned .NET tools in `.config/dotnet-tools.json` and
 * `docs/.config/dotnet-tools.json` are checked as well.
 *
 * - A `package` entry is checked against the publish time nuget.org records for
 *   that exact version (the registration leaf's `published`).
 * - A `project` entry is a project reference: first-party code in this
 *   checkout, never published, so it is skipped without a request.
 * - A restore that names an http feed other than nuget.org fails the gate,
 *   because a package from that feed cannot be dated here. A local folder
 *   source is not a registry; a package it supplies is still dated against
 *   nuget.org by id and version.
 *
 * Fails closed: a registry that cannot be read, a version with no publish time
 * (nuget.org gives an unlisted version none), or an unexpected restore graph is
 * a failure, never a silent pass. Every request has a timeout and the whole run
 * has an overall bound, so a registry that accepts connections and never
 * answers cannot hang the job.
 *
 * Usage:
 *
 *     node scripts/check-dependency-age.mjs [--days=N] [--assets=PATH,PATH,...]
 *     [--tool-manifest=PATH,PATH,...] [--registration=URL]
 *
 * Each flag also has an environment form, which is what the tests use:
 * `EZCLASSIFY_DEPENDENCY_AGE_DAYS`, `EZCLASSIFY_DEPENDENCY_AGE_ASSETS`,
 * `EZCLASSIFY_DEPENDENCY_AGE_TOOL_MANIFESTS` (both comma-separated), and
 * `EZCLASSIFY_DEPENDENCY_AGE_NUGET_REGISTRATION` (the registration base asked
 * for publish times). `EZCLASSIFY_DEPENDENCY_AGE_OVERALL_TIMEOUT_MS` shortens
 * the overall bound.
 *
 * @module
 */
import { existsSync, readFileSync } from 'node:fs';
import { dirname, relative, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

/**
 * Default quarantine window. A week is long enough that a malicious release has
 * usually been reported and pulled, and short enough that ordinary upgrades are
 * not held up for a sprint. Dependabot's cooldown matches it.
 */
const DEFAULT_QUARANTINE_DAYS = 7;

const MILLISECONDS_PER_DAY = 86_400_000;

/**
 * How long one publish-time request may take. A required check must fail on a
 * registry that accepts the connection and never answers, not hang the job.
 */
const REQUEST_TIMEOUT_MS = 20_000;

/** Upper bound on the whole run; anything unchecked when it passes fails. */
const DEFAULT_OVERALL_TIMEOUT_MS = 5 * 60_000;

/** Requests in flight at once. */
const CONCURRENCY = 4;

/**
 * nuget.org's registration hive that lists every package, SemVer 2.0.0 ones
 * included; its leaf for one version records when it was published. The
 * SemVer 1.0.0 hive answers 404 for a SemVer 2.0.0 version. This hive serves
 * every document gzip-encoded, which fetch decodes from Content-Encoding.
 */
const DEFAULT_REGISTRATION = 'https://api.nuget.org/v3/registration5-gz-semver2';

/** The feed restore uses by default, and the only http feed this gate can date. */
const NUGET_ORG_FEED = 'https://api.nuget.org/v3/index.json';

/**
 * Sent with every request. Names the tool and where it lives, nothing about the
 * person or machine running it.
 */
const USER_AGENT =
    'ezclassify-dependency-age-check (+https://github.com/ezclassify/sdk-dotnet)';

const repositoryRoot = resolve(dirname(fileURLToPath(import.meta.url)), '..');

const SOLUTION = 'EzClassify.sln';

const DEFAULT_TOOL_MANIFESTS = [
    '.config/dotnet-tools.json',
    'docs/.config/dotnet-tools.json',
];

const USAGE =
    'Usage: check-dependency-age.mjs [--days=N] [--assets=PATH,PATH,...] [--tool-manifest=PATH,PATH,...] [--registration=URL]';

function parseArguments(argv) {
    const options = {};
    for (const argument of argv) {
        const match = /^--(days|assets|tool-manifest|registration)=(.+)$/.exec(argument);
        if (!match) {
            throw new Error(`Unrecognized argument "${argument}". ${USAGE}`);
        }
        options[match[1]] = match[2];
    }
    return options;
}

function resolveQuarantineDays(options) {
    const raw =
        options.days ??
        process.env.EZCLASSIFY_DEPENDENCY_AGE_DAYS ??
        String(DEFAULT_QUARANTINE_DAYS);
    const days = Number(raw);
    if (!Number.isFinite(days) || days < 0) {
        throw new Error(
            `Quarantine window must be a non-negative number of days, got "${raw}".`,
        );
    }
    return days;
}

function resolveOverallTimeout() {
    const raw = process.env.EZCLASSIFY_DEPENDENCY_AGE_OVERALL_TIMEOUT_MS;
    if (raw === undefined) return DEFAULT_OVERALL_TIMEOUT_MS;
    const value = Number(raw);
    if (!Number.isInteger(value) || value <= 0) {
        throw new Error(
            `EZCLASSIFY_DEPENDENCY_AGE_OVERALL_TIMEOUT_MS must be a positive integer, got "${raw}".`,
        );
    }
    return value;
}

function splitPaths(raw) {
    return raw
        .split(',')
        .map((entry) => entry.trim())
        .filter(Boolean)
        .map((path) => resolve(path));
}

/**
 * The restore graphs to read: the flag or environment list, otherwise one
 * `obj/project.assets.json` per project the solution lists.
 */
function resolveAssetsPaths(options) {
    const raw = options.assets ?? process.env.EZCLASSIFY_DEPENDENCY_AGE_ASSETS;
    if (raw) return splitPaths(raw);
    const solution = readRequired(resolve(repositoryRoot, SOLUTION));
    const projects = [...solution.matchAll(/^Project\("[^"]*"\)\s*=\s*"[^"]*",\s*"([^"]+\.csproj)"/gm)]
        .map((match) => match[1].replace(/\\/g, '/'));
    if (projects.length === 0) {
        throw new Error(`${SOLUTION} lists no C# projects; cannot find the restore graphs.`);
    }
    return projects.map((project) =>
        resolve(repositoryRoot, dirname(project), 'obj/project.assets.json'),
    );
}

function resolveToolManifests(options) {
    const raw =
        options['tool-manifest'] ?? process.env.EZCLASSIFY_DEPENDENCY_AGE_TOOL_MANIFESTS;
    if (raw) return splitPaths(raw);
    return DEFAULT_TOOL_MANIFESTS.map((path) => resolve(repositoryRoot, path));
}

function resolveRegistration(options) {
    const raw =
        options.registration ??
        process.env.EZCLASSIFY_DEPENDENCY_AGE_NUGET_REGISTRATION ??
        DEFAULT_REGISTRATION;
    return raw.replace(/\/+$/, '');
}

function readRequired(path, hint = '') {
    if (!existsSync(path)) {
        throw new Error(`Dependency quarantine: ${path} does not exist.${hint}`);
    }
    return readFileSync(path, 'utf8');
}

function readJson(path, hint) {
    const text = readRequired(path, hint);
    try {
        return JSON.parse(text);
    } catch (cause) {
        throw new Error(
            `Dependency quarantine: ${path} is not valid JSON: ${cause instanceof Error ? cause.message : String(cause)}`,
        );
    }
}

function displayPath(path) {
    return relative(repositoryRoot, path) || path;
}

/**
 * Collect the packages every restore graph resolved, keyed by lowercase
 * `id@version` so a package two projects share is checked once and a package
 * resolved at two versions is checked at both.
 */
function collectRestoredPackages(assetsPaths) {
    const packages = new Map();
    const refused = [];
    let projectReferences = 0;
    for (const path of assetsPaths) {
        const where = displayPath(path);
        const assets = readJson(path, ' Run `dotnet restore EzClassify.sln` first.');
        if (!assets.libraries || typeof assets.libraries !== 'object') {
            throw new Error(`${where} has no "libraries" section; is it a project.assets.json?`);
        }
        for (const source of Object.keys(assets.project?.restore?.sources ?? {})) {
            if (!/^https?:\/\//i.test(source)) continue;
            if (source.replace(/\/+$/, '').toLowerCase() === NUGET_ORG_FEED) continue;
            refused.push({
                entry: { id: source, version: '', where },
                reason: `${where} was restored with ${source}, a feed this gate cannot date`,
            });
        }
        for (const [key, library] of Object.entries(assets.libraries)) {
            const slash = key.indexOf('/');
            if (slash <= 0 || slash === key.length - 1) {
                throw new Error(`${where} has a library "${key}" that is not an id/version pair.`);
            }
            const id = key.slice(0, slash);
            const version = key.slice(slash + 1);
            if (library?.type === 'project') {
                projectReferences += 1;
                continue;
            }
            if (library?.type !== 'package') {
                refused.push({
                    entry: { id, version, where },
                    reason: `${where} lists it as a "${library?.type}" library, which this gate does not know how to date`,
                });
                continue;
            }
            addPackage(packages, { id, version, where });
        }
    }
    return { packages, refused, projectReferences };
}

/** Add the exact tool versions the tool manifests pin. */
function collectTools(manifestPaths, packages) {
    let tools = 0;
    for (const path of manifestPaths) {
        const where = displayPath(path);
        const manifest = readJson(path);
        for (const [id, tool] of Object.entries(manifest.tools ?? {})) {
            if (typeof tool?.version !== 'string' || tool.version === '') {
                throw new Error(`${where} pins no version for the tool ${id}.`);
            }
            tools += 1;
            addPackage(packages, { id, version: tool.version, where });
        }
    }
    return tools;
}

function addPackage(packages, entry) {
    const key = `${entry.id.toLowerCase()}@${entry.version.toLowerCase()}`;
    if (!packages.has(key)) packages.set(key, entry);
}

/**
 * GET a JSON document with the per-request timeout and the run's overall
 * bound. Any failure is thrown with a message naming the package.
 */
async function fetchJson(url, what, deadline) {
    let response;
    try {
        response = await fetch(url, {
            headers: { accept: 'application/json', 'user-agent': USER_AGENT },
            signal: AbortSignal.any([AbortSignal.timeout(REQUEST_TIMEOUT_MS), deadline]),
        });
    } catch (cause) {
        throw new Error(
            `could not reach ${new URL(url).origin} for ${what}: ${cause instanceof Error ? cause.message : String(cause)}`,
        );
    }
    if (!response.ok) {
        throw new Error(`${new URL(url).origin} answered ${response.status} for ${what}`);
    }
    try {
        return await response.json();
    } catch (cause) {
        throw new Error(
            `${new URL(url).origin} sent an unreadable answer for ${what}: ${cause instanceof Error ? cause.message : String(cause)}`,
        );
    }
}

/**
 * The publish time nuget.org records for one package version. An unlisted
 * version keeps its registration leaf but carries a placeholder date, so it
 * counts as having no publish time.
 */
async function fetchPublishTime(registration, entry, deadline) {
    const what = `${entry.id}@${entry.version}`;
    const leaf = await fetchJson(
        `${registration}/${encodeURIComponent(entry.id.toLowerCase())}/${encodeURIComponent(entry.version.toLowerCase())}.json`,
        what,
        deadline,
    );
    if (leaf?.listed === false) {
        throw new Error(`${what} is unlisted, so the registry gives it no publish time`);
    }
    if (typeof leaf?.published !== 'string') {
        throw new Error(`the registry reports no publish time for ${what}`);
    }
    return leaf.published;
}

/**
 * Run `worker` over `items` with at most `limit` in flight. The worker must not
 * throw; it reports failure in its result.
 */
async function mapLimited(items, limit, worker) {
    const results = new Array(items.length);
    let next = 0;
    async function lane() {
        while (next < items.length) {
            const index = next++;
            results[index] = await worker(items[index]);
        }
    }
    await Promise.all(Array.from({ length: Math.min(limit, items.length) }, lane));
    return results;
}

function label(entry) {
    return entry.version ? `${entry.id}@${entry.version}` : entry.id;
}

async function main() {
    const options = parseArguments(process.argv.slice(2));
    const quarantineDays = resolveQuarantineDays(options);
    const overallTimeoutMs = resolveOverallTimeout();
    const registration = resolveRegistration(options);

    const restored = collectRestoredPackages(resolveAssetsPaths(options));
    const tools = collectTools(resolveToolManifests(options), restored.packages);
    const packages = [...restored.packages.values()];

    const summary =
        `${packages.length} third-party NuGet package(s), ${tools} of them pinned tools; ` +
        `skipped ${restored.projectReferences} project reference(s) as first-party`;

    const unknown = [...restored.refused];
    const violations = [];

    if (quarantineDays === 0) {
        console.log(`Dependency quarantine window is 0 days; nothing to check (${summary}).`);
    } else {
        const overall = new AbortController();
        const timer = setTimeout(
            () => overall.abort(new Error('overall time limit reached')),
            overallTimeoutMs,
        );
        timer.unref();
        const results = await mapLimited(packages, CONCURRENCY, async (entry) => {
            try {
                if (overall.signal.aborted) throw overall.signal.reason;
                return { entry, published: await fetchPublishTime(registration, entry, overall.signal) };
            } catch (cause) {
                const reason = overall.signal.aborted
                    ? `the run passed its ${overallTimeoutMs} ms bound before it was checked`
                    : cause instanceof Error
                      ? cause.message
                      : String(cause);
                return { entry, reason };
            }
        });
        clearTimeout(timer);

        const cutoff = Date.now() - quarantineDays * MILLISECONDS_PER_DAY;
        for (const result of results) {
            if (result.reason !== undefined) {
                unknown.push(result);
                continue;
            }
            const publishedAt = Date.parse(result.published);
            if (Number.isNaN(publishedAt)) {
                unknown.push({
                    entry: result.entry,
                    reason: `the registry gave an unreadable publish time "${result.published}"`,
                });
                continue;
            }
            if (publishedAt > cutoff) {
                violations.push({
                    ...result,
                    clears: new Date(publishedAt + quarantineDays * MILLISECONDS_PER_DAY).toISOString(),
                });
            }
        }
    }

    if (unknown.length > 0) {
        console.error(
            `Dependency quarantine: could not confirm the publish time of ${unknown.length} package(s), so the gate fails closed.`,
        );
        for (const { entry, reason } of unknown) {
            console.error(`  ${label(entry)}: ${reason}`);
        }
        process.exitCode = 1;
    }

    if (violations.length > 0) {
        console.error(
            `Dependency quarantine: ${violations.length} third-party package(s) were published less than ${quarantineDays} day(s) ago.`,
        );
        for (const violation of violations) {
            console.error(
                `  ${label(violation.entry)} (${violation.entry.where}) published ${violation.published}, clears ${violation.clears}`,
            );
        }
        console.error('Wait out the window, or pin an older version, before taking these in.');
        process.exitCode = 1;
    }

    if (process.exitCode) return;
    if (quarantineDays === 0) return;

    console.log(
        `Dependency quarantine passed: ${summary}. Every checked package is older than ${quarantineDays} day(s).`,
    );
}

main().catch((error) => {
    console.error(error instanceof Error ? error.message : String(error));
    process.exitCode = 1;
});
