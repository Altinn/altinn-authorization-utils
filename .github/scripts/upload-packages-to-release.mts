import { Chalk } from "chalk";
import { globby } from "globby";
import { Octokit } from "@octokit/action";
import path from "node:path";
import fs from "node:fs/promises";

const c = new Chalk({ level: 3 });

const ghToken = process.env.GITHUB_TOKEN;
const filesGlob = process.env.FILES_GLOB;
const releaseId = process.env.RELEASE_ID;

if (!ghToken || !filesGlob || !releaseId) {
  console.error("Missing required environment variables");
  process.exit(1);
}

const releaseInfo = {
  release_id: Number.parseInt(releaseId, 10),
  owner: "Altinn",
  repo: "altinn-authorization-utils",
} as const;

const github = new Octokit({
  auth: ghToken,
});

const release = await github.rest.repos.getRelease(releaseInfo);

console.log(`Uploading files to release ${c.cyan(release.data.tag_name)}`);

for (const file of await globby(filesGlob)) {
  const name = path.basename(file);
  const fullPath = path.resolve(file);

  console.log(`Uploading ${c.yellow(name)}`);
  let handle: fs.FileHandle | null = null;
  try {
    handle = await fs.open(fullPath, "r");
    const stat = await handle.stat();

    await retry(() =>
      github.rest.repos.uploadReleaseAsset({
        ...releaseInfo,
        url: release.data.upload_url,
        name,
        data: handle!.createReadStream({
          start: 0,
          emitClose: false,
          autoClose: false,
        }) as any,
        headers: {
          "content-type": "binary/octet-stream",
          "content-length": stat.size,
        },
      })
    );
  } finally {
    if (handle) {
      await handle.close();
    }
  }
}

async function retry(fn: () => Promise<any>, retries = 3) {
  for (let i = 0; i < retries; i++) {
    try {
      return await fn();
    } catch (err) {
      if (i === retries - 1) {
        throw err;
      }
      console.error(`Retrying after error: ${c.red(err.message)}`);
    }
  }
}
