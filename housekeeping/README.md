# housekeeping

Collection of CI scripts (appliances) to validate data from new commits. Each script returns exit code `1` if there were errors, `0` otherwise.

- `summaries`: validates `discXX-summary.txt` files for missing values, data types, and line order
- `finalize`: (TODO) makes a rough guess as to whether the summary has been finalized
