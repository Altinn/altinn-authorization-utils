[private]
@default:
  just --list

# Install node packages required to run scripts - uses pnpm to install the packages
@install-script-packages:
  #!pwsh
  pushd .github/scripts
  pnpm install --frozen-lockfile

# Run the script to update solution files
@update-sln-files: install-script-packages
  #!pwsh
  ./.github/scripts/node_modules/.bin/tsx ./.github/scripts/update-sln-files.mts
