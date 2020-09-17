using System;
using System.Diagnostics;
using Perlin.Display;
using Perlin.Rendering;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Memory;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Perlin
{
    /// <summary>
    /// <para>
    /// The main static class of the Perlin engine. It holds global variables and starts the engine.
    /// </para>
    /// To start your application call <code>PerlinApp.Start()</code>. This method has a function type parameter that
    /// gets called when the engine finished initializing. You should put here your initialization code, do not try to
    /// use the engine before this function is called.
    /// </summary>
    public static class PerlinApp
    {
        /// <summary>
        /// The engine's image manager. You can use this to load images to your project.
        /// </summary>
        public static readonly ImageManager ImageManager = new ImageManager();
        
        /// <summary>
        /// Stage is the root of the display. Anything that you want to render must be added with
        /// <code>AddChild</code> to the Stage.
        /// </summary>
        public static Stage Stage { get; private set; }
        internal static PerlinPipeline Pipeline { get; private set; }
        internal static BatchRenderer Renderer { get; private set; }
        internal static GraphicsDevice DefaultGraphicsDevice { get; private set; }
        internal static Sdl2Window Window { get; private set; }
        
        private static StatsDisplay _statsDisplay;
        internal static CommandList CommandList { get; private set; }
        
        /// <summary>
        /// The default font collection for the engine. You should create fonts with this instance.
        /// </summary>
        public static readonly FontCollection Fonts = new FontCollection();

        private static FontFamily _fontRobotoMono;
        /// <summary>
        /// Built-in Roboto Mono Regular font. Call <code>CreateFont</code> on the returned object to create a Font of
        /// this type to be used e.g. with a <code>TextField</code>.
        /// </summary>
        /// <see cref="TextField"/> 
        public static FontFamily FontRobotoMono
        {
            get
            {
                // workaround, was `_fontRobotoMono == null` see https://github.com/SixLabors/Fonts/issues/143
                if (ReferenceEquals(_fontRobotoMono, null))
                {
                    _fontRobotoMono = Fonts.Install(PerlinUtils.LoadEmbeddedResourceAsString("RobotoMono-Regular.ttf"));
                }
                return _fontRobotoMono;
            }
        }

        /// <summary>
        /// Starts you application. Call this method once when you want to start your app.
        /// </summary>
        /// <param name="width">The app window's width</param>
        /// <param name="height">The app window's height</param>
        /// <param name="windowTitle">The app window's title</param>
        /// <param name="onInit">Method to call when the app started. At this point the app is ready,
        /// you can add things to the stage, add event listeners etc.</param>
        public static void Start(int width, int height, string windowTitle, Action onInit)
        {
            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                true,
                null,
                true,
                ResourceBindingModel.Improved);
            if (VeldridStartup.GetPlatformDefaultBackend() == GraphicsBackend.Vulkan)
            {
                options.PreferStandardClipSpaceYDirection = true;
            }
            Start(width, height, windowTitle, onInit, options);
        }
        
        /// <summary>
        /// Starts you application. Call this method once when you want to start your app.
        /// </summary>
        /// <param name="width">The app window's width</param>
        /// <param name="height">The app window's height</param>
        /// <param name="windowTitle">The app window's title</param>
        /// <param name="onInit">Method to call when the app started. At this point the app is ready,
        /// you can add things to the stage, add event listeners etc.</param>
        /// <param name="options">Specify your own graphics device options.</param>
        public static void Start(int width, int height, string windowTitle, Action onInit, GraphicsDeviceOptions options)
        {
            Configuration.Default.MemoryAllocator = new SimpleGcMemoryAllocator();
            Window = new Sdl2Window(windowTitle, 50, 50, width, height, SDL_WindowFlags.OpenGL, false);
            DefaultGraphicsDevice = VeldridStartup.CreateGraphicsDevice(Window, options);
            Console.WriteLine("Starting Perlin using " + DefaultGraphicsDevice.BackendType);
            CommandList = DefaultGraphicsDevice.ResourceFactory.CreateCommandList();
            Window.Resized += () => DefaultGraphicsDevice.ResizeMainWindow((uint)Window.Width, (uint)Window.Height);
            Pipeline = new PerlinPipeline(DefaultGraphicsDevice);
            Renderer = new BatchRenderer();
            Stopwatch sw = Stopwatch.StartNew();
            Stage = new Stage(width, height);
            onInit.Invoke();
            
            long previousFrameTicks = 0;
            double desiredFrameLengthSeconds = 1.0 / 60.0;
            // The main loop. This gets repeated every frame.
            while (Window.Exists)
            {
                long currentFrameTicks = sw.ElapsedTicks;
                double deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;
                while (deltaSeconds < desiredFrameLengthSeconds)
                {
                    currentFrameTicks = sw.ElapsedTicks;
                    deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;
                }
                previousFrameTicks = currentFrameTicks;
                
                InputSnapshot snapshot = Window.PumpEvents();
                KeyboardInput.UpdateFrameInput(snapshot, sw.Elapsed.TotalSeconds);
                if (Window.Exists)
                {
                    CommandList.Begin();
                    CommandList.SetFramebuffer(DefaultGraphicsDevice.MainSwapchain.Framebuffer);
                    CommandList.ClearColorTarget(0, new RgbaFloat(
                        Stage.BackgroundColor.R/255f,
                        Stage.BackgroundColor.G/255f,
                        Stage.BackgroundColor.B/255f,
                        1));
                    // Adds elements to the render queue using a helper render state stack
                    Stage.Render((float)deltaSeconds);
                    // Iterates the render queue and sends it to the GPU for rendering
                    Renderer.RenderQueue();
                    CommandList.End();
                    DefaultGraphicsDevice.SubmitCommands(CommandList);
                    DefaultGraphicsDevice.SwapBuffers(DefaultGraphicsDevice.MainSwapchain);
                }
            }
            DefaultGraphicsDevice.Dispose();
            Console.WriteLine("program end");
        }
        
        /// <summary>
        /// Shows a small debug statistic overlay on one of the corners.
        /// </summary>
        public static void ShowStats(HorizontalAlignment horizontalAlign = HorizontalAlignment.Left, 
                                     VerticalAlignment verticalAlign = VerticalAlignment.Top, float scale = 1f)
        {
            float stageWidth  = Stage.Width;
            float stageHeight = Stage.Height;

            if (_statsDisplay == null)
            {
                _statsDisplay = new StatsDisplay();
                //_statsDisplay.Touchable = false;
            }
            
            Stage.AddChild(_statsDisplay);// TODO now things can be added above it, prevent this
            _statsDisplay.ScaleX = _statsDisplay.ScaleY = scale;

            if (horizontalAlign == HorizontalAlignment.Left) _statsDisplay.X = 0f;
            else if (horizontalAlign == HorizontalAlignment.Right) _statsDisplay.X = stageWidth - _statsDisplay.WidthScaled;
            else if (horizontalAlign == HorizontalAlignment.Center) _statsDisplay.X = (stageWidth - _statsDisplay.WidthScaled) / 2;

            if (verticalAlign == VerticalAlignment.Top) _statsDisplay.Y = 0f;
            else if (verticalAlign == VerticalAlignment.Bottom) _statsDisplay.Y = stageHeight - _statsDisplay.HeightScaled;
            else if (verticalAlign == VerticalAlignment.Center) _statsDisplay.Y = (stageHeight - _statsDisplay.HeightScaled) / 2;
        }

    }
}