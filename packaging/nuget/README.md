# RedBolt.RmlUI packages

Create packages after producing matching RedBolt Release packages. `RedBolt.RmlUI` depends directly on `RedBolt.Audio`; there is no optional-Audio bridge package.

```sh
dotnet run --file packaging/nuget/package.cs -- create --rids linux-x64
```

Use `--rids linux-x64,win-x64,android-x64,android-arm64,browser-wasm` for the complete runtime set.

The command produces the managed RmlUI package, `CoreData` and `Data` resource packages, and one runtime package per selected RID. Validate package contents and a package-only consumer with:

```sh
dotnet run --file packaging/nuget/package.cs -- validate --rids linux-x64
```
