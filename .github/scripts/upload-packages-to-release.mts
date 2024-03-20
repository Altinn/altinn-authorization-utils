import * as c from "std/fmt/colors.ts";
import { expandGlob } from "std/fs/mod.ts";
import { Octokit } from "octokit";

const ghToken = Deno.env.get("GITHUB_TOKEN");
const filesGlob = Deno.env.get("FILES_GLOB");
const releaseId = Deno.env.get("RELEASE_ID");

if (!ghToken || !filesGlob || !releaseId) {
  console.error("Missing required environment variables");
  Deno.exit(1);
}

const github = new Octokit({
  auth: ghToken,
});

const release = await github.rest.repos.getRelease({
  release_id: Number.parseInt(releaseId, 10),
  owner: "Altinn",
  repo: "altinn-authorization-utils",
});

console.log(`Uploading files to release ${c.cyan(release.data.tag_name)}`);

for await (const file of expandGlob(filesGlob)) {
  const name = file.name;
  const path = file.path;

  console.log(`Uploading ${c.yellow(name)}`);
  const fs = await Deno.open(path, { read: true });
  const stat = await fs.stat();

  try {
    await github.rest.repos.uploadReleaseAsset({
      url: release.data.upload_url,
      name,
      data: fs.readable as any,
      release_id: Number.parseInt(releaseId, 10),
      owner: "Altinn",
      repo: "altinn-authorization-utils",
      headers: {
        "Content-Length": `${stat.size}`,
      },
    });
  } finally {
    try {
      fs.close();
    } catch {
      // ignore
    }
  }
}
