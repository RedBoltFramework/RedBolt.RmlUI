// Copyright (c) 2026 RedBolt and contributors.
// This work is licensed under the terms of the MIT license.
// For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

namespace RedBolt.Sample.HelloRmlUI;

using System;
using System.Runtime.CompilerServices;

using RedBolt;
using RedBolt.Audio;
using RedBolt.Bindings.RmlUi;
using RedBolt.Container;
using RedBolt.Core;
using RedBolt.Engine;
using RedBolt.Graphics;
using RedBolt.Input;
using RedBolt.Math;
using RedBolt.RenderAPI;
using RedBolt.RenderPipeline;
using RedBolt.Resource;
using RedBolt.RmlUI;
using RedBolt.Scene;
using CoreVariant = RedBolt.Core.Variant;
using RmlUISubsystem = RedBolt.RmlUI.RmlUI;

internal static class HelloRmlUIProgram
{
    private static int Main()
    {
        try
        {
            using var application = new HelloRmlUI();
            var exitCode = application.Run();
            if (exitCode == 0)
                Console.WriteLine("RedBolt HelloRmlUI sample finished.");
            return exitCode;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }
}

/// <summary>A 2D UI window, managed by main UI instance returned by GetSubsystem&lt;RmlUI&gt;().</summary>
[Reflected]
public partial class SimpleWindow : RmlUIComponent
{
    /// <summary>Construct.</summary>
    public SimpleWindow()
    {
        SetResource("UI/HelloRmlUI.rml");
        _variant.Value = new CoreVariant(new ResourceRef("Model", "Models/Box.mdl"));
        _variantVector.Add(new CoreVariant(42));
        _variantVector.Add(new CoreVariant("vector text"));
        _variantMap["int"] = new CoreVariant(24);
        _variantMap["str"] = new CoreVariant("map text");
    }

    /// <summary>Destruct.</summary>
    public override void Dispose()
    {
        _variant.Value.Dispose();
        _variantVector.Dispose();
        _variantMap.Dispose();
        base.Dispose();
    }

    /// <summary>Callback function invoked from rml template.</summary>
    public void CountClicks(VariantVector arguments)
    {
        // Increase counter and notify model of it's update.
        ++_counter;
        DirtyVariable("counter");
    }

    /// <summary>Process 'CloseWindow' event.</summary>
    public void OnCloseWindow(ref RmlCustomEvent args)
    {
        if (args.EventType != new StringHash("CloseWindow"))
            return;

        var element = Element.CreateManagedWrapper(args.Data["_Element"].GetVoidPtr(), false);
        var ownerDocument = element.GetOwnerDocument();
        if (ownerDocument is not null && GetDocument() is { } document && ownerDocument.Handle == document.Handle)
            Remove();
    }

    /// <summary>Update model and animate progressbars.</summary>
    public override void Update(float timeStep)
    {
        base.Update(timeStep);

        // Animate progressbars
        _progress = (MathDefs.Sin(GetSubsystem<Time>()!.GetElapsedTime() * 1.4f) + 1.0f) / 2.0f;
        DirtyVariable("progress");
    }

    /// <summary>Initialize document model.</summary>
    protected override void OnDataModelInitialized()
    {
        // Create a data model for connecting UI with state kept in this class.
        // Important: there can only be one data model with given name per unique RmlUI subsystem!
        BindDataModelProperty("font_size",
            (ref CoreVariant value) => value = new CoreVariant(GetEmSize()),
            (in CoreVariant value) => SetEmSize(value.GetFloat()));
        BindDataModelProperty("slider_value",
            (ref CoreVariant value) => value = new CoreVariant(_sliderValue),
            (in CoreVariant value) => _sliderValue = value.GetInt());
        BindDataModelProperty("counter",
            (ref CoreVariant value) => value = new CoreVariant(_counter),
            (in CoreVariant value) => _counter = value.GetInt());
        BindDataModelProperty("progress",
            (ref CoreVariant value) => value = new CoreVariant(_progress),
            (in CoreVariant value) => _progress = value.GetFloat());
        BindDataModelVariant("variant", _variant);
        BindDataModelVariantVector("variant_vector", _variantVector);
        BindDataModelVariantMap("variant_map", _variantMap);

        BindDataModelEvent("count", CountClicks);

        // Act on pressing window close button.
        SubscribeToEvent<RmlCustomEvent>(GetUI(), OnCloseWindow);
    }

    /// <summary>Value of UI slider.</summary>
    protected int _sliderValue = 32;
    /// <summary>Value of button click counter.</summary>
    protected int _counter = 7;
    /// <summary>Value of progressbar progress.</summary>
    protected float _progress;
    /// <summary>Value of Urho3D::Variant type.</summary>
    protected readonly StrongBox<CoreVariant> _variant = new();
    /// <summary>Value of Urho3D::VariantVector type.</summary>
    protected readonly VariantVector _variantVector = [];
    /// <summary>Value of Urho3D::VariantMap type.</summary>
    protected readonly VariantMap _variantMap = [];
}

/// <summary>A compact RmlUI document rendered to a texture on a 3D object.</summary>
[Reflected]
public partial class WorldStatusPanel : RmlUIComponent
{
    /// <summary>Raised when the world-space button requests the full showcase.</summary>
    public event Action? ShowcaseRequested;

    /// <summary>Construct.</summary>
    public WorldStatusPanel()
    {
        SetResource("UI/HelloRmlUI_World.rml");
    }

    /// <summary>Update the live signal indicator.</summary>
    public override void Update(float timeStep)
    {
        base.Update(timeStep);

        _progress = 0.72f + MathDefs.Sin(GetSubsystem<Time>()!.GetElapsedTime() * 1.8f) * 0.18f;
        DirtyVariable("progress");
    }

    /// <summary>Open the showcase from the button remapped onto the 3D surface.</summary>
    public void OpenShowcase(VariantVector arguments)
    {
        ShowcaseRequested?.Invoke();
    }

    /// <summary>Initialize the world-space document model.</summary>
    protected override void OnDataModelInitialized()
    {
        BindDataModelProperty("progress",
            (ref CoreVariant value) => value = new CoreVariant(_progress),
            (in CoreVariant value) => _progress = value.GetFloat());
        BindDataModelEvent("open_showcase", OpenShowcase);
    }

    private float _progress = 0.72f;
}

/// <summary>A RmlUI demonstration.</summary>
public sealed class HelloRmlUI : Application
{
    /// <summary>Construct.</summary>
    public HelloRmlUI()
    {
    }

    /// <summary>Setup after engine initialization and before running main loop.</summary>
    protected override void Start()
    {
        _sampleInput = new SampleInputMap(movement: true);
        _sampleInput.Map.MapKeyboardKey("Showcase", Scancode.Space);
        _sampleInput.Map.MapControllerButton("Showcase", ControllerButton.A);
        _sampleInput.Map.MapKeyboardKey("Debugger", Scancode.F9);
        _sampleInput.Map.MapControllerButton("Debugger", ControllerButton.Y);
        // Register custom components.
        Audio.RegisterAudioSubsystem(Context);
        RmlUISubsystem.RegisterRmlUILibrary(Context);
        Context.RegisterAssembly(typeof(SimpleWindow).Assembly);
        _ui = new RmlUISubsystem();
        Context.RegisterSubsystem(_ui);
        if (Engine.Parameters.LoadFonts)
            _ui.ReloadFonts();
        // Execute base class startup
        base.Start();

        // Initialize Scene
        InitScene();

        // Initialize Window
        InitWindow();

        // Set the mouse mode to use in the sample
        Input.SetMouseMode(MouseMode.Free);
        Input.SetMouseVisible(true);
    }

    /// <summary>Initialize 3D scene.</summary>
    private void InitScene()
    {
        var cache = Resources;

        _scene = new Scene();
        _scene.CreateComponent<Octree>();
        var zone = _scene.CreateComponent<Zone>();
        zone.SetBoundingBox(new BoundingBox(-1000.0f, 1000.0f));
        zone.SetAmbientColor(new Color(0.08f, 0.13f, 0.16f));
        zone.SetFogColor(new Color(0.025f, 0.055f, 0.07f));
        zone.SetFogStart(100.0f);
        zone.SetFogEnd(300.0f);

        // Build the display from a solid casing and a separate front-facing plane.
        // Only the plane receives the UI texture, so the casing edges remain stable.
        _environmentMaterial = new Material();
        _environmentMaterial.SetTechnique(0, cache.Get<Technique>("Techniques/DiffUnlit.xml"));
        _environmentMaterial.SetShaderParameter("MatDiffColor", new Vector4f(0.055f, 0.14f, 0.16f, 1.0f));

        var displayNode = _scene.CreateChild("Display");
        displayNode.SetPosition(new Vector3f(3.4f, 0.0f, 0.0f));
        displayNode.SetRotation(new Quaternion(2.0f, -12.0f, 0.0f));

        var casingNode = displayNode.CreateChild("DisplayCasing");
        casingNode.SetScale(new Vector3f(4.32f, 3.52f, 0.18f));
        var casingModel = casingNode.CreateComponent<StaticModel>();
        casingModel.SetModel(cache.Get<Model>("Models/Box.mdl"));
        casingModel.SetMaterial(_environmentMaterial);

        var screenNode = displayNode.CreateChild("Screen");
        screenNode.SetPosition(new Vector3f(0.0f, 0.0f, -0.095f));
        screenNode.SetRotation(new Quaternion(-90.0f, 0.0f, 0.0f));
        screenNode.SetScale(new Vector3f(4.2f, 1.0f, 3.4f));
        var screenModel = screenNode.CreateComponent<StaticModel>();
        screenModel.SetModel(cache.Get<Model>("Models/Plane.mdl"));

        // Mount the display on a simple pedestal for clear scene context.

        var standNode = _scene.CreateChild("DisplayStand");
        standNode.SetPosition(new Vector3f(3.4f, -2.05f, 0.35f));
        standNode.SetScale(new Vector3f(0.28f, 0.9f, 0.32f));
        var standModel = standNode.CreateComponent<StaticModel>();
        standModel.SetModel(cache.Get<Model>("Models/Box.mdl"));
        standModel.SetMaterial(_environmentMaterial);

        var baseNode = _scene.CreateChild("DisplayBase");
        baseNode.SetPosition(new Vector3f(3.4f, -2.55f, 0.35f));
        baseNode.SetScale(new Vector3f(1.8f, 0.16f, 0.9f));
        var baseModel = baseNode.CreateComponent<StaticModel>();
        baseModel.SetModel(cache.Get<Model>("Models/Box.mdl"));
        baseModel.SetMaterial(_environmentMaterial);

        // Create a camera.
        _cameraNode = _scene.CreateChild("Camera");
        _cameraNode.CreateComponent<Camera>();
        _cameraController = _cameraNode.CreateComponent<FreeFlyController>();
        _cameraController.SetInputMap(_sampleInput!.Map);
        _cameraController.SetSpeed(3.0f);
        _cameraController.SetAcceleratedSpeed(8.0f);
        _cameraController.SetEnabled(false);

        // Begin farther away and centered on the world-space display. The intro
        // moves toward it before handing camera control to the user.
        _cameraNode.SetPosition(new Vector3f(3.4f, 0.0f, -16.0f));

        // Set up a viewport so 3D scene can be visible.
        Renderer.SetViewport(0, new Viewport(_scene, _cameraNode.GetComponent<Camera>()!));
    }

    /// <summary>Initialize the world-space UI. The full showcase opens on demand.</summary>
    private void InitWindow()
    {
        // Node that will get UI rendered on it.
        var screenNode = _scene!.GetChild("Display")!.GetChild("Screen")!;

        // Create a texture we will render to.
        _texture = new Texture2D();

        // Create a material that will display UI texture on a cube.
        _material = new Material();
        _material.SetTechnique(0, Resources.Get<Technique>("Techniques/DiffUnlit.xml"));
        _material.SetTexture("Albedo", _texture);
        _material.SetDepthBias(new BiasParameters(-0.001f, 0.0f));

        // Create a component that sets up UI rendering.
        var canvas = screenNode.CreateComponent<RmlCanvasComponent>();
        canvas.SetTexture(_texture);
        canvas.SetUISize(new Vector2i(512, 512));
        canvas.SetRemapMousePos(true);

        // Create a compact document that will be rendered on the 3D display.
        var windowOnCube = screenNode.CreateComponent<WorldStatusPanel>();
        windowOnCube.ShowcaseRequested += RequestShowcase;
        _windowOnCube = new WeakReference<WorldStatusPanel>(windowOnCube);

        // Create a StaticModel for our cube.
        var model = screenNode.GetComponent<StaticModel>()!;

        // And set a material to it so UI would be rendered on to cube.
        model.SetMaterial(_material);
    }

    /// <summary>Begin the camera reveal that makes room for the full UI showcase.</summary>
    private void RequestShowcase()
    {
        if (_showcaseRequested)
            return;

        _showcaseRequested = true;
        _showcaseTransitionTime = 0.0f;
        _showcaseTransitionStart = _cameraNode!.GetPosition();
        _cameraController?.SetEnabled(false);
    }

    /// <summary>Create the full screen-space UI after the world-space interaction.</summary>
    private void OpenShowcase()
    {
        var window = _scene!.CreateComponent<SimpleWindow>();
        _window = new WeakReference<SimpleWindow>(window);
        _showcaseOpened = true;
        _cameraController?.SetEnabled(true);
    }

    /// <summary>Animate cube, handle keys.</summary>
    protected override void Update(float timeStep)
    {
        if (Input.ExitRequested || _sampleInput!.Pressed("Exit"))
        {
            Exit();
            return;
        }

        // Keyboard fallback for accessibility and automated sample runs. The
        // primary interaction remains the button rendered on the 3D surface.
        if (!_showcaseRequested && _sampleInput.Pressed("Showcase"))
            RequestShowcase();

        // Keep the world-space display moving subtly while it remains readable.
        var node = _scene!.GetChild("Display")!;
        var elapsedTime = GetSubsystem<Time>()!.GetElapsedTime();
        node.SetRotation(new Quaternion(
            2.0f + MathDefs.Sin(elapsedTime * 0.7f) * 1.5f,
            -12.0f + MathDefs.Sin(elapsedTime * 0.45f) * 4.0f,
            MathDefs.Sin(elapsedTime * 0.55f)));

        if (!_showcaseRequested)
        {
            _introTime = MathF.Min(_introTime + timeStep, IntroDuration);
            var progress = _introTime / IntroDuration;
            var easedProgress = MathDefs.SmoothStep(0.0f, 1.0f, progress);
            _cameraNode!.SetPosition(new Vector3f(
                3.4f,
                0.0f,
                -16.0f + 6.0f * easedProgress));
            if (_introTime >= IntroDuration)
                _cameraController?.SetEnabled(true);
        }
        else if (!_showcaseOpened)
        {
            _showcaseTransitionTime = MathF.Min(_showcaseTransitionTime + timeStep, ShowcaseTransitionDuration);
            var progress = _showcaseTransitionTime / ShowcaseTransitionDuration;
            var easedProgress = MathDefs.SmoothStep(0.0f, 1.0f, progress);
            _cameraNode!.SetPosition(new Vector3f(
                _showcaseTransitionStart.X * (1.0f - easedProgress),
                _showcaseTransitionStart.Y * (1.0f - easedProgress),
                _showcaseTransitionStart.Z + (-10.0f - _showcaseTransitionStart.Z) * easedProgress));
            if (_showcaseTransitionTime >= ShowcaseTransitionDuration)
                OpenShowcase();
        }

        if (_sampleInput.Pressed("Debugger"))
            _ui!.SetDebuggerVisible(!_ui.IsDebuggerVisible());
    }

    protected override void Setup(EngineParameters parameters)
    {
        parameters.ApplicationName = "RedBolt.Sample.HelloRmlUI";
        parameters.Borderless = false;
        parameters.FullScreen = false;
        parameters.ResourcePaths = "CoreData.RmlUI;SampleData.RmlUI;CoreData;SampleData";
        parameters.ResourcePrefixPaths = ".;..";
        parameters.WindowMaximize = false;
#if !REDBOLT_PLATFORM_BROWSER
            parameters.WindowHeight = 720;
            parameters.WindowWidth = 1280;
#endif
        parameters.WindowTitle = "RedBolt Hello RmlUI";
    }

    protected override void Stop()
    {
        _sampleInput?.Dispose();
        _sampleInput = null;
        Renderer.SetNumViewports(0);
        _window = null;
        _windowOnCube = null;
        _cameraNode = null;
        _cameraController = null;
        _scene?.Dispose();
        _scene = null;
        _material?.Dispose();
        _material = null;
        _environmentMaterial?.Dispose();
        _environmentMaterial = null;
        _texture?.Dispose();
        _texture = null;
        Context.RemoveSubsystem<RmlUISubsystem>();
        _ui?.Dispose();
        _ui = null;
        base.Stop();
    }

    public override void Dispose()
    {
        if (_scene is not null)
            Renderer.SetNumViewports(0);
        _window = null;
        _windowOnCube = null;
        _cameraNode = null;
        _cameraController = null;
        _scene?.Dispose();
        _scene = null;
        _material?.Dispose();
        _material = null;
        _environmentMaterial?.Dispose();
        _environmentMaterial = null;
        _texture?.Dispose();
        _texture = null;
        Context.RemoveSubsystem<RmlUISubsystem>();
        _ui?.Dispose();
        _ui = null;
        base.Dispose();
    }

    /// <summary>Window which will be rendered into backbuffer.</summary>
    private WeakReference<SimpleWindow>? _window;
    /// <summary>Window which will be rendered onto a side of a cube.</summary>
    private WeakReference<WorldStatusPanel>? _windowOnCube;
    /// <summary>Texture to which windowOnCube_ will render.</summary>
    private Texture2D? _texture;
    /// <summary>Material which will apply windowOnCube_ on to a cube.</summary>
    private Material? _material;
    private Material? _environmentMaterial;
    private RmlUISubsystem? _ui;
    private Scene? _scene;
    private Node? _cameraNode;
    private FreeFlyController? _cameraController;
    private SampleInputMap? _sampleInput;
    private Vector3f _showcaseTransitionStart;
    private float _introTime;
    private float _showcaseTransitionTime;
    private bool _showcaseRequested;
    private bool _showcaseOpened;
    private const float IntroDuration = 2.4f;
    private const float ShowcaseTransitionDuration = 0.8f;
}
