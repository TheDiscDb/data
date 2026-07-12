import { Octokit } from "octokit";

// https://docs.github.com/en/actions/reference/workflows-and-actions/variables
const event = process.env.GITHUB_EVENT_NAME;
const repoFull = process.env.GITHUB_REPOSITORY;

/**
 * Resolve the pull request number for the current run.
 *
 * Reads it from the event payload (`pull_request.number`) which is reliable
 * even when the PR has already been merged/closed by the time the run starts.
 * In that case the checkout falls back to the base branch and
 * `GITHUB_REF_NAME` becomes e.g. `main` (not `<pr_number>/merge`), so parsing
 * the ref name yields `NaN` and the API call blows up on `pulls/NaN/files`.
 * Falls back to parsing `GITHUB_REF_NAME` for local testing.
 */
const getPullRequestNumber = async (): Promise<number | undefined> => {
  const eventPath = process.env.GITHUB_EVENT_PATH;
  if (eventPath) {
    try {
      const payload = await Bun.file(eventPath).json();
      const num = payload?.pull_request?.number;
      if (typeof num === "number" && Number.isInteger(num)) return num;
    } catch {
      // fall through to ref-name parsing
    }
  }

  // > For pull requests, the format is <pr_number>/merge.
  const refNumber = Number(process.env.GITHUB_REF_NAME?.split("/")[0]);
  return Number.isInteger(refNumber) ? refNumber : undefined;
};

export const getModifiedFiles = async (): Promise<string[]> => {
  if (event !== "pull_request") return [];

  const prNumber = await getPullRequestNumber();
  if (prNumber === undefined) {
    console.warn(
      "Could not determine the pull request number; skipping file scoping.",
    );
    return [];
  }

  // GITHUB_TOKEN not actually required while in an action; this is for local
  // testing with private repos
  const octokit = new Octokit({ auth: process.env.GITHUB_TOKEN });
  // biome-ignore lint/style/noNonNullAssertion: value from GH
  const [owner, repo] = repoFull!.split("/") as [string, string];

  const response = await octokit.rest.pulls.listFiles({
    owner,
    repo,
    pull_number: prNumber,
    // max allowed
    per_page: 3000,
  });
  return response.data
    .filter((d) => d.status !== "renamed" && d.status !== "unchanged")
    .map((d) => d.filename);
};

/**
 * Get modified release folders, no individual files
 */
export const getModifiedReleases = async (): Promise<string[]> => {
  const files = await getModifiedFiles();
  if (files.length === 0) return [];

  const releases = new Set<string>();
  for (const file of files) {
    const parts = file.split("/");
    if (parts[4]) {
      releases.add(parts.slice(0, 4).join("/"));
    }
  }

  return Array.from(releases);
};

export const fileInReleaseFolder = (
  path: string,
  folders: string[],
): boolean => {
  const releaseFolder = path
    .replace(/^\.\.\/data/, "data")
    .split("/")
    .slice(0, 4)
    .join("/");
  return folders.includes(releaseFolder);
};
