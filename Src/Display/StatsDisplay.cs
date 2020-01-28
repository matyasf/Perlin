using System.Diagnostics;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;

namespace Perlin.Display
{
    /// <summary>
    /// Simple overlay that displays some performance statistics. It is used internally by the engine.
    /// </summary>
    internal sealed class StatsDisplay : Sprite
    {
        private static readonly float UPDATE_INTERVAL = 0.5f;
        private static readonly float B_TO_MB = 1.0f / (1024f * 1024f); // convert from bytes to MB
        private readonly TextField _values;
        private int _frameCount;
        private float _totalTime;
        private const uint ComponentWidth = 90;
        private const uint ComponentHeight = 48;
        
        public float Fps;
        public float Memory;
        public float GpuMemory;
        //private int _skipCount;

        /// <summary>
        /// Creates a new Statistics Box.
        /// </summary>
        public StatsDisplay() : base(ComponentWidth, ComponentHeight, new Rgba32(200, 0, 0, 250))
        {
            const string gpuLabel = "\ngpu memory:";
            const string labels = "frames/sec:\nstd memory:" + gpuLabel + "\ndraw calls:";
            var font = PerlinApp.FontRobotoMono.CreateFont(9);

            var leftText = new TextField(font, labels, false)
            {
                Width = ComponentWidth - 2,
                Height = ComponentHeight,
                HorizontalAlign = HorizontalAlignment.Left,
                X = 2
            };

            _values = new TextField(font, null, false)
            {
                Width = ComponentWidth - 1,
                Height = ComponentHeight,
                HorizontalAlign = HorizontalAlignment.Right
            };
            AddChild(leftText);
            AddChild(_values);

            AddedToStage += OnAddedToStage;
            RemovedFromStage += OnRemovedFromStage;
        }

        private void OnAddedToStage(DisplayObject target)
        {
            EnterFrameEvent += OnEnterFrame;
            _totalTime = _frameCount = 0;
            //_skipCount = 0;
            Update();
        }

        private void OnRemovedFromStage(DisplayObject target)
        {
            EnterFrameEvent -= OnEnterFrame;
        }

        private void OnEnterFrame(DisplayObject target, float elapsedTimeSecs)
        {
            _totalTime += elapsedTimeSecs;
            _frameCount++;
            if (_totalTime > UPDATE_INTERVAL)
            {
                Update();
                _frameCount = 0;
                //_skipCount = 0;
                _totalTime = 0;
            }
        }

        /// <summary>
        /// Updates the displayed values.
        /// </summary>
        public void Update()
        {
            // _background.Tint = _skipCount > (_frameCount / 2) ? (uint)0x003F00 : 0x0;
            Fps = _totalTime > 0 ? _frameCount / _totalTime : 0;
            Process currentProc = Process.GetCurrentProcess();
            Memory = currentProc.PrivateMemorySize64 * B_TO_MB;
            GpuMemory = GetGPUMemory();

            string fpsText = Fps < 100 ? Fps.ToString("N1") : Fps.ToString("N0");
            string memText = Memory < 100 ? Memory.ToString("N1") : Memory.ToString("N0");
            string gpuMemText = GpuMemory < 100 ? GpuMemory.ToString("N1") : GpuMemory.ToString("N0");
            string drwText = PerlinApp.Renderer.DrawCount > 2 ? (PerlinApp.Renderer.DrawCount - 3).ToString() : "0"; // ignore self

            _values.Text = fpsText + "\n" + memText + "\n" +
                           (GpuMemory >= 0 ? gpuMemText + "\n" : "") + drwText;
        }

        /// <summary>
        /// Returns the currently used GPU memory in bytes. Might not work in all platforms!
        /// </summary>
        private int GetGPUMemory()
        {
            /* needs to figure out how to do this in Veldrid
            if (GLExtensions.DeviceSupportsOpenGLExtension("GL_NVX_gpu_memory_info"))
            {
                // this returns in Kb, Nvidia only extension
                int dedicated;
                Gl.Get(Gl.GPU_MEMORY_INFO_DEDICATED_VIDMEM_NVX, out dedicated);

                int available;
                Gl.Get(Gl.GPU_MEMORY_INFO_CURRENT_AVAILABLE_VIDMEM_NVX, out available);

                return (dedicated - available) / 1024;
            }
            */
            return 0;
        }
    }
}