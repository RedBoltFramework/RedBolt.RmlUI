// Copyright (c) 2026 RedBolt contributors.
// This work is licensed under the terms of the MIT license.

using RedBolt.Plugins;
using RedBolt.RmlUI.Editor;

[assembly: PluginModule("RedBolt.RmlUI.Editor", typeof(RmlUIEditorPlugin),
    ReloadPolicy = PluginReloadPolicy.ProcessResident, AutoLoad = true)]

namespace RedBolt.RmlUI.Editor;

using System;
using System.Collections.Generic;

using RedBolt.Core;
using RedBolt.Editor.Core;
using RedBolt.Editor.Foundation;
using RedBolt.Graphics;

using EditorProject = RedBolt.Editor.Project.Project;
using RmlUISubsystem = RedBolt.RmlUI.RmlUI;

/// <summary>Developer-only RmlUI integration discovered by the standard RedBolt Editor.</summary>
public sealed class RmlUIEditorPlugin : PluginApplication
{
    protected override void Load()
    {
        _reflection = Context.RegisterAssemblyScoped(typeof(RmlUISubsystem).Assembly);
        _rmlUI = Context.GetSubsystem<RmlUISubsystem>();
        if (_rmlUI is null)
        {
            _rmlUI = new RmlUISubsystem();
            Context.RegisterSubsystem(_rmlUI);
            _ownsSubsystem = true;
        }

        var editorPlugins = GetSubsystem<EditorPluginManager>();
        _registrations.Add(editorPlugins.AddScopedPlugin<EditorProject>(
            "RmlUI.ReloadFonts", ApplyProject));
        _registrations.Add(editorPlugins.AddScopedPlugin<GameViewTab>(
            "RmlUI.GameView", ApplyGameView));
    }

    protected override void Unload()
    {
        for (var index = _registrations.Count - 1; index >= 0; --index)
            _registrations[index].Dispose();
        _registrations.Clear();

        _rmlUI?.SetRenderTarget(null);
        if (_ownsSubsystem && _rmlUI is not null)
        {
            Context.RemoveSubsystem<RmlUISubsystem>();
            _rmlUI.Dispose();
        }
        _rmlUI = null;
        _ownsSubsystem = false;
        _reflection?.Dispose();
        _reflection = null;
    }

    private IDisposable ApplyProject(EditorProject project)
    {
        void ReloadFonts(EditorProject _) => _rmlUI?.ReloadFonts();
        project.OnInitialized += ReloadFonts;
        return new CallbackRegistration(() => project.OnInitialized -= ReloadFonts);
    }

    private IDisposable ApplyGameView(GameViewTab gameView)
    {
        void SetRenderTarget(GameViewTab _, RenderSurface? target) => _rmlUI?.SetRenderTarget(target);
        gameView.RenderTargetChanged += SetRenderTarget;
        _rmlUI?.SetRenderTarget(gameView.RenderTarget);
        return new CallbackRegistration(() =>
        {
            gameView.RenderTargetChanged -= SetRenderTarget;
            _rmlUI?.SetRenderTarget(null);
        });
    }

    private sealed class CallbackRegistration(Action callback) : IDisposable
    {
        private Action? _callback = callback;

        public void Dispose()
        {
            _callback?.Invoke();
            _callback = null;
        }
    }

    private readonly List<IDisposable> _registrations = [];
    private AssemblyRegistration? _reflection;
    private RmlUISubsystem? _rmlUI;
    private bool _ownsSubsystem;
}
