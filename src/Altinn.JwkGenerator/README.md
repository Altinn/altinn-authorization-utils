# Altinn.JwkGenerator Console App

The `Altinn.JwkGenerator` console application provides a convenient way to generate Json Web Keys. 

## Installation

Clone repo and build solution using IDE or CLI.

## Usage

Use your favorite terminal and execute the `JwkGenerator.exe` from build output.

Here's an example of the `Altinn.JwkGenerator` console application's built in help page:

```
Description:
  Console app for creating Json Web Keys

Usage:
  JwkGenerator [command] [options]

Options:
  --keyName <keyName>        Name for the JWK. Used in the kid in the format: "{keyName}-{date}". [default: MyJwk-DEV]
  --keySize <keySize>        Key size in bits. [default: 2048]
  --alg <alg>                Algorithm to use. [default: RS256]
  --use <use>                Use for the JWK. [default: sig]
  --filePath <filePath>      Filepath for where the JWK files should be stored. [default: c:\jwks]
  --keySetName <keySetName>  Optional name for a keyset to include the key in. Setting a keyset name will collate all keys
                             with the same keyset name in a subfolder of the file output folder (see --filePath). All public
                             and private keys will also be added to separate collections stored in the base of the keyset
                             folder. Useful for when creating a new key for rotation, on an existing Maskinporten client. []
  --version                  Show version information
  -?, -h, --help             Show help and usage information

Commands:
  create  Creates a new JWK.
```

### Create command

To create a JWK using the default values simply execute the app with the `create` command:

```powershell
.\JwkGenerator.exe create
```

Output:
```powershell
Creating JWK with
kid: MyJwk-DEV-2024-06-21, keySize: 2048, alg: RS256, use: sig, filePath: c:\jwks
Success! Files written to c:\jwks\MyJwk-DEV-2024-06-21
```

Any of the `options` can be used to override defaults when executing the`create` command:

```powershell
.\JwkGenerator.exe create --keyName MyJwk-PROD --keySize 4096 --alg RS512 --use sig --filePath c:\myJwks --keySetName MyJwk-PROD
```

Output:
```powershell
Creating JWK with
kid: MyJwk-PROD-2024-06-21, keySize: 4096, alg: RS512, use: sig, filePath: c:\myJwks
Creating new KeySet: MyJwk-PROD
Success! Files written to c:\myJwks\MyJwk-PROD\MyJwk-PROD-2024-06-21
```

## Contributing

Contributions are welcome! If you find any issues or have suggestions for improvements, please feel free to open an issue or submit a pull request on [GitHub](https://github.com/your/repository).

## License

This library is licensed under the [MIT License](LICENSE).
