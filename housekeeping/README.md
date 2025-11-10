# housekeeping

Collection of CI scripts (appliances) to validate data from new commits. Each script returns exit code `1` if there were errors, `0` otherwise.

- `summaries`: validates `discXX-summary.txt` files for missing values, data types, and line order
- `finalize`: makes a rough guess as to whether the summary has been finalized, emits a warning for missing content hashes, and verifies a matching `Index` in each disc JSON file

## Rules

`rules.json` contains a few rules that can be modified to adjust strictness:

```jsonc
{
  // Raise an error for double returns in summary files
  "empty_chunk": true,
}
```

## Local testing

- Install [Bun](https://bun.com)
- `cd housekeeping`
- `bun install`
- Set up your `.env` if you would like to test a specific case (examples below)
- Run a script with `bun <script>` - descriptions above

**Pull request .env**

When in a pull request context, the scripts only scan the files changed in the pull request. Remember to clone the PR so you actually have the changes locally.

```
GITHUB_EVENT_NAME=pull_request
GITHUB_REPOSITORY=TheDiscDb/data
GITHUB_REF_NAME=152/merge
```
