using System.Numerics;

namespace Perlin.Rendering
{
    /// <summary>
    /// The internal data structure that stores a single Vertex on the GPU.
    /// </summary>
    public struct QuadVertex
    {
        public const uint VertexSize = 40; // in bytes; 1 float = 4 bytes.
        /// <summary>
        /// The absolute position in the screen
        /// </summary>
        public Vector2 Position;
        /// <summary>
        /// The absolute width and height on the screen.
        /// </summary>
        public Vector2 Size;
        /// <summary>
        /// The transparency (0=fully transparent, 1=fully opaque)
        /// </summary>
        public float Alpha;
        /// <summary>
        /// Rotation in Radians
        /// </summary>
        public float Rotation;
        /// <summary>
        /// The rectangular sub-region of the texture to render.
        /// </summary>
        public Vector4 TextureSubRegion;
    }
}