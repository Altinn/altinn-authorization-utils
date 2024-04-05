import { Chalk } from "chalk";
import { globby } from "globby";
import { $ } from "zx";
import path from "node:path";
import { Octokit } from "@octokit/action";

const c = new Chalk({ level: 3 });

const ghToken = process.env.GITHUB_TOKEN;
const filesGlob = process.env.FILES_GLOB;

const github = new Octokit({
  auth: ghToken,
});

if (!ghToken || !filesGlob) {
  console.error("Missing required environment variables");
  process.exit(1);
}

for (const file of await globby(filesGlob)) {
  const name = path.basename(file);
  const fullPath = path.resolve(file);

  console.log(`Publishing ${c.yellow(name)}`);
  await $`dotnet nuget push "${fullPath}" --api-key "${ghToken}" --source "github"`

  console.log(`Published ${c.green(name)}`);
  const pkg = await retry(() => github.rest.packages.getPackageForAuthenticatedUser({
    package_type: "nuget",
    package_name: path.basename(file, ".nupkg"),
  }));

  console.log(`Package visibility: ${visibility(pkg.data.visibility)}`);
}

function visibility(visibility: "public" | "private") {
  if (visibility === "public") {
    return c.green(visibility);
  }

  return c.red(visibility);
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
