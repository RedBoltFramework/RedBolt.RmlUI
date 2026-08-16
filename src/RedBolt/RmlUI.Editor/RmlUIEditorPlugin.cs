// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

using RedBolt.Plugins;
using RedBolt.RmlUI.Editor;

[assembly: PluginModule("RedBolt.RmlUI.Editor", typeof(RmlUIEditorPlugin),
    ReloadPolicy = PluginReloadPolicy.ProcessResident, AutoLoad = true)]

namespace RedBolt.RmlUI.Editor;

using System;

using RedBolt.Core;
using RedBolt.Editor.Core;
using RedBolt.Editor.Foundation;
using RedBolt.Graphics;

using EditorProject = RedBolt.Editor.Project.Project;
using RmlUISubsystem = RedBolt.RmlUI.RmlUI;

/// <summary>Developer-only RmlUI integration discovered by the standard RedBolt Editor.</summary>
public sealed class RmlUIEditorPlugin : EditorPluginApplication
{
    protected override void LoadEditorPlugin()
    {
        _rmlUI = Context.GetSubsystem<RmlUISubsystem>();
        if (_rmlUI is null)
        {
            _rmlUI = new RmlUISubsystem();
            Context.RegisterSubsystem(_rmlUI);
            _ownsSubsystem = true;
        }
    }

    protected override void UnloadEditorPlugin()
    {
        _rmlUI?.SetRenderTarget(null);
        if (_ownsSubsystem && _rmlUI is not null)
        {
            Context.RemoveSubsystem<RmlUISubsystem>();
            _rmlUI.Dispose();
        }
        _rmlUI = null;
        _ownsSubsystem = false;
    }

    private RmlUISubsystem? _rmlUI;
    private bool _ownsSubsystem;
}

[EditorExtension("RmlUI.ReloadFonts")]
internal static class RmlUIReloadFontsExtension
{
    public static IDisposable Apply(EditorProject project)
    {
        var rmlUI = Context.Instance.GetSubsystem<RmlUISubsystem>();
        void ReloadFonts(EditorProject _) => rmlUI?.ReloadFonts();
        project.OnInitialized += ReloadFonts;
        return new CallbackRegistration(() => project.OnInitialized -= ReloadFonts);
    }
}

[EditorExtension("RmlUI.GameView")]
internal static class RmlUIGameViewExtension
{
    public static IDisposable Apply(GameViewTab gameView)
    {
        var rmlUI = Context.Instance.GetSubsystem<RmlUISubsystem>();
        void SetRenderTarget(GameViewTab _, RenderSurface? target) => rmlUI?.SetRenderTarget(target);
        gameView.RenderTargetChanged += SetRenderTarget;
        rmlUI?.SetRenderTarget(gameView.RenderTarget);
        return new CallbackRegistration(() =>
        {
            gameView.RenderTargetChanged -= SetRenderTarget;
            rmlUI?.SetRenderTarget(null);
        });
    }
}

internal sealed class CallbackRegistration(Action callback) : IDisposable
{
    private Action? _callback = callback;

    public void Dispose()
    {
        _callback?.Invoke();
        _callback = null;
    }
}
