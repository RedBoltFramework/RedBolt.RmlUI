# RedBolt.RmlUI workspace instructions

This repository contains the optional RmlUI runtime and its developer-only editor companion extracted from the sibling RedBolt repository.

- RedBolt is expected at `../RedBolt` unless `RedBoltRoot` is supplied.
- Tests remain in `../RedBolt.Tests`.
- Preserve the public `RedBolt.RmlUI` and `RedBolt.Bindings.RmlUi` assemblies and namespaces.
- Keep `RedBolt.RmlUI.Editor` separate from runtime game dependencies.
- RmlUI references `RedBolt.Audio` directly; Audio remains in the core repository and does not require an abstraction layer.
- Use generated SharpCpp bindings for RmlUi. Do not add manual native bindings.
- Keep desktop, WebAssembly, and Android builds working.
- Follow the monorepo `doc/VERIFICATION.md`: build affected projects and run focused tests; validate affected platform samples and packages when their behavior or integration changes. Run the full supported matrix for release validation or when requested.
