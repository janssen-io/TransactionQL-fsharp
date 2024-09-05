# Contributing

Feel free to open issues and merge requests!

## Building
The app can be build with FAKE (F#'s make) using the command: 
`dotnet run --project src/TransactionQL.Build/TransactionQL.Build.fsproj`

This command supports the following parameters:

| Parameter                 | Description |
|---------------------------|-------------|
| `-e SkipTests=1`          | Do not run the test suite |
| `-e VERSION=1.0.0`        | Set the version of the assemblies and apps |
| `-e CONFIGURATION=DEBUG`  | Compile the apps in Debug rather than Release |
| `-e parallel-jobs=n`      | Use `n` workers to run the build in parallel | 
| `-t <target>`             | Run the specific target (e.g. `Test`) |

The default target (`Complete`) runs the entire build and copies the
artifacts to the `ci/dist` directory.

### Artifacts
One assembly per plugin. These assemblies are _not_ selfcontained and require
cli or gui to load them. The cli and gui do contain their necessary
dependencies.

The cli is built as a single executable that is published as a single file, but
not self contained. It still requires the .NET runtime to run.

The gui is built as a single executable alongside a few DLLs required for rendering.
It also requires the .NET runtime to run.

### Supported Targets

* **Clean:** Empties the `ci` directory to clean up after previous builds.
* **Restore:** Restores dependencies for the entire solution.
* **Test:** Runs all unit tests.
* **Publish:** Ensures that the `ci/build` folder exists.
* **Publish Plugins:** Builds and copies the plugin assemblies to `ci/build/plugins/`.
* **Publish CLI:** Builds and copies the CLI executable to `ci/build/console/`.
* **Publish GUI:** Builds and copies the GUI files to `ci/build/desktop/`.
* **Stage Artifacts:** Builds and copies just the required application files to `ci/staging/`.
* **Dist:** Ensures that the `ci/dist` folder exists.
* **Setup:** (Windows only) Creates a installer using InnoSetup.
* **Archive:** Bundles the application files into `ci/dist/tql-<platform>-x64.zip`.
* **Vim:** Copies the vim syntax highlighting file to `ci/dist/tql.vim`.
* **Complete:** Runs all of the above.

## Conventions

1. Code must be formatted using `dotnet format` before merging into `main`.
