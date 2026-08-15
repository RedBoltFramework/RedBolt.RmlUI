#:project ../../../RedBolt/src/RedBolt/Build/Packaging/RedBolt.ExternalModule.Packaging.csproj

using System.Runtime.CompilerServices;
using RedBolt.ExternalModule.Packaging;

return ExternalModulePackager.Run(args, new ExternalModulePackageManifest
{
    Name = "RedBolt.RmlUI",
    BasePackages = ["RedBolt.Core", "RedBolt.Audio", "SharpCpp.Std", "SharpCpp.Support"],
    RequireBaseRuntime = true,
    SupportedRids = ["linux-x64", "win-x64", "android-x64", "android-arm64", "browser-wasm"],
    DefaultRids = ["linux-x64"],
    BuildSteps =
    [
        new("Native/RedBolt.RmlUI.Native.csproj", BuildWhen.Linux),
        new("Native/RmlUi/RedBolt.Bindings.RmlUi.csproj", BuildWhen.Linux, true),
        new("RmlUI/RedBolt.RmlUI.csproj", BuildWhen.Linux, true, "-p:BuildProjectReferences=false"),
        new("RmlUI/RedBolt.RmlUI.csproj", BuildWhen.Windows, false,
            "-p:RedBoltPlatform=win-x64", "-p:RedBoltNativePreset=win-x64"),
        new("Sample/Web/RedBolt.Sample.Web.csproj", BuildWhen.Web, false,
            "-p:RedBoltEnablePlatforms=web"),
        new("Sample/Android/RedBolt.Sample.Android.csproj", BuildWhen.Android, false,
            "--framework", "net10.0-android", "-p:RedBoltEnablePlatforms=android",
            "-p:RedBoltEnableArchitectures=all")
    ],
    ResourcePackages =
    [
        new("RedBolt.RmlUI.CoreData", "CoreData", "bin/CoreData"),
        new("RedBolt.RmlUI.Data", "Data", "bin/Data")
    ],
    ManagedPackages =
    [
        new(
            "RedBolt.RmlUI",
            "RmlUi document rendering with data models plus events and scene components for RedBolt.",
            [
                new("build/bin/Release/linux-x64/core/RedBolt.RmlUI.dll", "lib/net10.0/RedBolt.RmlUI.dll"),
                new("build/bin/Release/linux-x64/extern/RedBolt.Bindings.RmlUi.dll", "lib/net10.0/RedBolt.Bindings.RmlUi.dll")
            ],
            [
                new("RedBolt.Core", PackageVersions.RedBolt),
                new("RedBolt.Audio", PackageVersions.RedBolt),
                new("SharpCpp.Std", PackageVersions.RedBolt),
                new("SharpCpp.Support", PackageVersions.RedBolt),
                new("RedBolt.RmlUI.CoreData", PackageVersions.Module),
                new("RedBolt.RmlUI.Data", PackageVersions.Module)
            ])
    ],
    RuntimePackage = new(
        "RedBolt.RmlUI.Runtime",
        "RmlUi",
        "*RedBolt.Native.RmlUi*",
        "build/native/{rid}/lib/Release/wasm/RedBolt.RmlUI.Native.Web.a",
        [
            new("RedBolt.RmlUI", PackageVersions.Module),
            new("RedBolt.Core.Runtime.{rid}", PackageVersions.RedBolt)
        ]),
    Consumer = new(
        "RedBolt.RmlUI.Runtime.linux-x64",
        """
        using System;
        Console.WriteLine(typeof(RedBolt.RmlUI.RmlUI).FullName);
        """,
        OnlyWithLinuxRuntime: true)
}, SourcePath());

static string SourcePath([CallerFilePath] string path = "") => path;
