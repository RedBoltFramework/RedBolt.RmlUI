# RedBolt.RmlUI

Optional RmlUi integration for [RedBolt](https://github.com/RedBoltFramework/RedBolt). It contains the runtime assembly, generated native binding, module resources, two samples, and an editor companion executable.

The source build expects RedBolt in `../RedBolt` by default. Override `RedBoltRoot` when the repositories use a different layout.

```sh
dotnet build RedBolt.RmlUI.slnx --configuration Debug
```

`RedBolt.RmlUI` depends directly on the core-repository `RedBolt.Audio` module and uses its sound feature without an abstraction layer.

The core `RedBolt.Editor` has no RmlUI dependency. `RedBolt.RmlUI.Editor` composes RmlUI into the editor as a separate developer-only executable and is not a dependency of game assemblies.
