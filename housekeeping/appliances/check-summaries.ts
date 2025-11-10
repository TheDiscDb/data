import rules from "../rules.json";
import { fileInReleaseFolder, getModifiedReleases } from "./files";

interface ChunkLine {
  index: number;
  content: string;
  lineNumber: number;
}

const errors: Record<string, (string | { line: number; content: string })[]> =
  {};

const ChunkTypes = [
  "MainMovie",
  "Episode",
  "Extra",
  "Trailer",
  "DeletedScene",
] as const;

const INT_RE = /^\d+$/;
const RANGE_RE = /^\d+(?:-\d+)?$/;

const releaseFolders = await getModifiedReleases();
if (releaseFolders.length !== 0) {
  console.log(
    `Validating ${releaseFolders.length} applicable folders:\n- ${releaseFolders.join("\n- ")}\n`,
  );
}

const files = new Bun.Glob("../data/**/disc*-summary.txt");
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

  const chunks: Record<string, ChunkLine>[] = [];
  let i = -1;
  let totalI = 0;

  const content = await Bun.file(file).text();
  for (const line of content.trim().split("\n")) {
    i += 1;
    totalI += 1;
    let chunk = chunks[chunks.length - 1];
    if (!chunk) {
      chunk = {};
      chunks.push(chunk);
      i = 0;
    } else if (!line.trim()) {
      chunk = {};
      chunks.push(chunk);
      i = -1;
      continue;
    }

    const [type, content] = line.trim().split(":");

    // we don't do any validation to chapters right now
    if (type === "Chapters" || line.trim().startsWith("-")) continue;

    if (!type || !content) {
      // Not sure if chapters without a leading hyphen should be considered valid
      // e.push(`Invalid line (${totalI}) in ${file}: ${line}`);
      continue;
    }
    chunk[type] = { index: i, content: content.trim(), lineNumber: totalI };
  }

  for (const chunk of chunks) {
    const largestIndex = Object.values(chunk).sort(
      (a, b) => b.index - a.index,
    )[0]?.index;
    if (largestIndex === undefined) {
      if (rules.empty_chunk) {
        e.push(`Found chunk with no lines, likely a double return`);
      }
      continue;
    }

    if (!chunk.Name) {
      e.push(`Missing name for an item`);
      continue;
    }
    if (!chunk.Type) {
      e.push(`Missing type for "${chunk.Name?.content}"`);
      continue;
    }

    for (const [key, { index, content, lineNumber: line }] of Object.entries(
      chunk,
    )) {
      switch (key) {
        case "Type": {
          // @ts-expect-error string does not overlap with this const, duh
          if (!ChunkTypes.includes(content)) {
            e.push({
              line,
              content: `Received invalid type "${content}" for "${chunk.Name?.content}"`,
            });
          }
          if (content === "Episode") {
            if (!chunk.Season) {
              e.push({
                line,
                content: `Missing season for "${chunk.Name?.content}"`,
              });
            }
            if (!chunk.Episode) {
              e.push({
                line,
                content: `Missing episode for "${chunk.Name?.content}"`,
              });
            }
          }
          break;
        }
        case "File name": {
          if (index !== largestIndex) {
            e.push({
              line,
              content: `File name "${content}" was not the last entry in its chunk`,
            });
          }
          break;
        }
        case "Season":
        case "Chapters count":
        case "Segment count": {
          if (!INT_RE.test(content)) {
            e.push({
              line,
              content: `${key} value "${content}" is not an integer`,
            });
          }
          break;
        }
        case "Episode": {
          if (!RANGE_RE.test(content)) {
            e.push({
              line,
              content: `Episode value "${content}" is not an integer or range`,
            });
          }
          break;
        }
      }
    }
  }
}

const filteredErrors = Object.fromEntries(
  Object.entries(errors)
    .filter(([, v]) => v.length !== 0)
    .map(([k, v]) => [k, v]),
);
if (Object.keys(filteredErrors).length !== 0) {
  for (const [file, es] of Object.entries(filteredErrors)) {
    if (es.length === 0) continue;

    console.log(`::group::${file.replace(/^data\//, "")}`);
    for (const e of es) {
      console.log(
        `::error file=${file}${
          typeof e === "string" ? "" : `,line=${e.line}`
        }::${typeof e === "string" ? e : e.content}`,
      );
    }
    console.log("::endgroup::");
  }
  process.exit(1);
} else {
  console.log("Everything looks good!");
  process.exit(0);
}
