// import rules from "../rules.json";
import { fileInReleaseFolder, getModifiedReleases } from "./files";

const errors: Record<
  string,
  { line?: number; content: string; type?: "error" | "warning" }[]
> = {};

const releaseFolders = await getModifiedReleases();
if (releaseFolders.length !== 0) {
  console.log(
    `Validating ${releaseFolders.length} applicable folders:\n- ${releaseFolders.join("\n- ")}\n`,
  );
}

const files = new Bun.Glob("../data/**/disc*.json");
for await (const file of files.scan()) {
  if (
    releaseFolders.length !== 0 &&
    !fileInReleaseFolder(file, releaseFolders)
  ) {
    continue;
  }

  const absFile = file.replace(/^\.\.\/data/, "data");
  errors[absFile] = [];
  const e = errors[absFile];

  // biome-ignore lint/style/noNonNullAssertion: the path will have at least one slash
  const filename = file.split("/").slice(-1)[0]!;
  const match = filename.match(/^disc(\d+)\.json$/);
  let fileIndex = -1;
  if (match) {
    fileIndex = Number(match[1]);
  }

  const content = await Bun.file(file).json();
  if (!content.Index || Number.isNaN(content.Index)) {
    e.push({ line: 2, content: "Missing or invalid `Index`" });
  } else if (fileIndex !== -1 && fileIndex !== content.Index) {
    e.push({
      line: 2,
      content: `\`Index\` does not match index of disc according to filename (${content.Index} != ${fileIndex})`,
    });
  }

  if (!content.ContentHash) {
    e.push({
      content: "Missing `ContentHash` in JSON file",
      type: "warning",
    });
  }

  if (!content.Titles?.length) {
    e.push({
      content:
        "No titles present in JSON file; this release may not have been finalized",
    });
  }
}

const filteredErrors = Object.fromEntries(
  Object.entries(errors)
    .filter(([, v]) => v.length !== 0)
    .map(([k, v]) => [k, v]),
);
if (Object.keys(filteredErrors).length !== 0) {
  let code = 0;

  for (const [file, es] of Object.entries(errors)) {
    console.log(`::group::${file.replace(/^data\//, "")}`);
    for (const e of es) {
      if (e.type === "warning") {
        console.log(
          `::warning file=${file}${
            e.line === undefined ? "" : `,line=${e.line}`
          }::${e.content}`,
        );
      } else {
        // Exit with 1 if there is any error entry
        code = 1;
        console.log(
          `::error file=${file}${
            e.line === undefined ? "" : `,line=${e.line}`
          }::${e.content}`,
        );
      }
    }
    console.log("::endgroup::");
  }
  process.exit(code);
} else {
  console.log("Everything looks good!");
  process.exit(0);
}
