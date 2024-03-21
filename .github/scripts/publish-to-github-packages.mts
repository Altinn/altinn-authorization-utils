import { Chalk } from "chalk";
import { globby } from "globby";
import { $ } from "zx";
import path from "node:path";

const c = new Chalk({ level: 3 });

const ghToken = process.env.GITHUB_TOKEN;
const filesGlob = process.env.FILES_GLOB;

if (!ghToken || !filesGlob) {
  console.error("Missing required environment variables");
  process.exit(1);
}

for (const file of await globby(filesGlob)) {
  const name = path.basename(file);
  const fullPath = path.resolve(file);

  console.log(`Publishing ${c.yellow(name)}`);
  await $`dotnet nuget push "${fullPath}" --api-key "${filesGlob}" --source "github"`
}
