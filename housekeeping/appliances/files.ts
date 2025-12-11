import { Octokit } from "octokit";

// https://docs.github.com/en/actions/reference/workflows-and-actions/variables
const event = process.env.GITHUB_EVENT_NAME;
const repoFull = process.env.GITHUB_REPOSITORY;

export const getModifiedFiles = async (): Promise<string[]> => {
  if (event !== "pull_request") return [];

  // GITHUB_TOKEN not actually required while in an action; this is for local
  // testing with private repos
  const octokit = new Octokit({ auth: process.env.GITHUB_TOKEN });
  // biome-ignore lint/style/noNonNullAssertion: value from GH
  const [owner, repo] = repoFull!.split("/") as [string, string];

  // > For pull requests, the format is <pr_number>/merge.
  // biome-ignore lint/style/noNonNullAssertion: value from GH
  const prNumber = Number(process.env.GITHUB_REF_NAME!.split("/")[0]);

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
