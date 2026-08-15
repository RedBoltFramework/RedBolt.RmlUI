# RedBolt.RmlUI workspace instructions

This repository contains the optional RmlUI runtime and its developer-only editor companion extracted from the sibling RedBolt repository.

- RedBolt is expected at `../RedBolt` unless `RedBoltRoot` is supplied.
- Tests remain in `../RedBolt.Tests`.
- Preserve the public `RedBolt.RmlUI` and `RedBolt.Bindings.RmlUi` assemblies and namespaces.
- Keep `RedBolt.RmlUI.Editor` separate from runtime game dependencies.
- RmlUI references `RedBolt.Audio` directly; Audio remains in the core repository and does not require an abstraction layer.
- Use generated SharpCpp bindings for RmlUi. Do not add manual native bindings.
- Keep desktop, WebAssembly, and Android builds working.
- At completion, build the solution and platform samples, create packages, run focused RmlUI tests, and review a captured sample screenshot.
