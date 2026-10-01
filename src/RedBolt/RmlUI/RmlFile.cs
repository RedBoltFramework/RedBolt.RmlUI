// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.RmlUI;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using RedBolt.Bindings.RmlUi;
using RedBolt.Core;
using Context = RedBolt.Core.Context;
using RedBolt.IO;
using RedBolt.Resource;

public class RmlFile : FileInterface
{
    /// <summary>Construct.</summary>
    public RmlFile()
    {
    }

    /// <summary>Opens a file.</summary>
    public override nuint Open(Std.String path)
    {
        var cache = Context.Instance.GetSubsystem<ResourceCache>();

        if (cache!.GetFile(path.ToString()) is { } file)
        {
            _loadedResources.Add(file.GetName());
            return (nuint)GCHandle.ToIntPtr(GCHandle.Alloc(file));
        }
        return 0;
    }

    /// <summary>Closes a previously opened file.</summary>
    public override void NativeClose(nuint file)
    {
        var handle = GCHandle.FromIntPtr((nint)file);
        ((AbstractFile)handle.Target!).Dispose();
        handle.Free();
    }

    /// <summary>Reads data from a previously opened file.</summary>
    public override unsafe nuint Read(nint buffer, nuint size, nuint file)
    {
        var handle = GCHandle.FromIntPtr((nint)file);
        return ((AbstractFile)handle.Target!).Read(new Span<byte>((void*)buffer, checked((int)size)));
    }

    /// <summary>Seeks to a point in a previously opened file.</summary>
    public override bool Seek(nuint file, nint offset, int origin)
    {
        var handle = GCHandle.FromIntPtr((nint)file);
        var fp = (AbstractFile)handle.Target!;
        var fileOffset = (long)offset;
        if (origin == (int)System.IO.SeekOrigin.Current)
            fileOffset = fp.Tell() + fileOffset;
        else if (origin == (int)System.IO.SeekOrigin.End)
            fileOffset = fp.GetSize() - fileOffset;
        if (fileOffset < 0 || fileOffset > uint.MaxValue)
            return false;
        return fp.Seek((uint)fileOffset) == fileOffset;
    }

    /// <summary>Returns the current position of the file pointer.</summary>
    public override nuint Tell(nuint file)
    {
        var handle = GCHandle.FromIntPtr((nint)file);
        return ((AbstractFile)handle.Target!).Tell();
    }

    /// <summary>Returns the length of the file.</summary>
    public override nuint Length(nuint file)
    {
        var handle = GCHandle.FromIntPtr((nint)file);
        return ((AbstractFile)handle.Target!).GetSize();
    }

    /// <summary>Returns true if file was opened since last call to ClearOpenedFiles().</summary>
    public bool IsResourceLoaded(string path)
    {
        return _loadedResources.Contains(path);
    }

    /// <summary>Add resource to the set of opened files.</summary>
    public void AddResourceLoaded(string resourceName)
    {
        _loadedResources.Add(resourceName);
    }

    /// <summary>Clear a set of opened files.</summary>
    public void ClearLoadedResources() => _loadedResources.Clear();

    /// <summary>A set of loaded files. Used to trigger UI reloads when resource cache reloads a modified file.</summary>
    private readonly HashSet<string> _loadedResources = [];
}
