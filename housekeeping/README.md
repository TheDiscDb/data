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
