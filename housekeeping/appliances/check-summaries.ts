const rules = {
  empty_chunk: false,
};

interface ChunkLine {
  index: number;
  content: string;
}

const errors: Record<string, string[]> = {};

const ChunkTypes = [
  "MainMovie",
  "Episode",
  "Extra",
  "Trailer",
  "DeletedScene",
] as const;

const INT_RE = /^\d+$/;
const RANGE_RE = /^\d+(?:-\d+)?$/;

const files = new Bun.Glob("../data/**/disc*-summary.txt");
for await (const file of files.scan()) {
  const prettyFile = file.replace(/\.\.\/data\//, "");
  errors[prettyFile] = [];
  const e = errors[prettyFile];

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
    chunk[type] = { index: i, content: content.trim() };
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

    for (const [key, { index, content }] of Object.entries(chunk)) {
      switch (key) {
        case "Type": {
          // @ts-expect-error string does not overlap with this const, duh
          if (!ChunkTypes.includes(content)) {
            e.push(
              `Received invalid type "${content}" for "${chunk.Name?.content}"`,
            );
          }
          if (content === "Episode") {
            if (!chunk.Season) {
              e.push(`Missing season for "${chunk.Name?.content}"`);
            }
            if (!chunk.Episode) {
              e.push(`Missing episode for "${chunk.Name?.content}"`);
            }
          }
          break;
        }
        case "File name": {
          if (index !== largestIndex) {
            e.push(
              `File name "${content}" was not the last entry in its chunk`,
            );
          }
          break;
        }
        case "Season":
        case "Chapters count":
        case "Segment count": {
          if (!INT_RE.test(content)) {
            e.push(`${key} value "${content}" is not an integer`);
          }
          break;
        }
        case "Episode": {
          if (!RANGE_RE.test(content)) {
            e.push(`Episode value "${content}" is not an integer or range`);
          }
          break;
        }
      }
    }
  }
}

if (Object.keys(errors).length !== 0) {
  for (const [prettyFile, es] of Object.entries(errors)) {
    if (es.length === 0) continue;
    console.error([`${prettyFile}:`, `- ${es.join("\n- ")}`, ""].join("\n"));
  }
  process.exit(1);
} else {
  console.log("Everything looks good!");
  process.exit(0);
}
