using Perlin.Display;
using Perlin.Geom;
using Veldrid;
using Rectangle = Perlin.Geom.Rectangle;

namespace Perlin.Rendering
{
    public class RenderState
    {
        
        public ResourceSet ResSet { get; private set; }
        /// <summary>
        /// The Vertex that will be uploaded to the GPU to render this.
        /// </summary>
        public ref QuadVertex GpuVertex
        {
            get
            {
                _gpuVertex.Position.X = _modelviewMatrix.Tx;
                _gpuVertex.Position.Y = _modelviewMatrix.Ty;
                _gpuVertex.Size.X = _originalWidth * _scaleX;
                _gpuVertex.Size.Y = _originalHeight * _scaleY;
                _gpuVertex.Rotation = _modelviewMatrix.Rotation;
                _gpuVertex.Alpha = _alpha;
                _gpuVertex.TextureSubRegion.X = _textureSubRegion.X;
                _gpuVertex.TextureSubRegion.Y = _textureSubRegion.Y;
                _gpuVertex.TextureSubRegion.Z = _textureSubRegion.Width;
                _gpuVertex.TextureSubRegion.W = _textureSubRegion.Height;
                return ref _gpuVertex;
            }
        }
        private QuadVertex _gpuVertex;
        private float _alpha;
        private float _scaleX;
        private float _scaleY;
        private float _originalWidth;
        private float _originalHeight;
        /// <summary>
        /// Used for translation and rotation.
        /// Scale cannot be known for sure from this, see https://stackoverflow.com/a/33539644/319473
        /// </summary>
        private Matrix2D _modelviewMatrix;
        private Rectangle _textureSubRegion;
        
        public RenderState()
        {
            Reset();
        }
        
        /// <summary>
        /// Resets the RenderState to the default settings.
        /// </summary>
        private void Reset()
        {
            _scaleX = _scaleY = 1.0f;
            _alpha = 1.0f;
            if (_modelviewMatrix != null) _modelviewMatrix.Identity();
            else _modelviewMatrix = Matrix2D.Create();
        }

        /// <summary>
        /// Sets this RenderState by taking the given render state as a base and applying the given
        /// DisplayObject's properties.
        /// This is used to calculate the absolute values for the given object (position, scale, rotation,..)
        /// </summary>
        /// <param name="parentState">The state of its parent object.</param>
        /// <param name="displayObject">The DisplayObject whose state needs to be calculated.</param>
        public void SetValues(RenderState parentState, DisplayObject displayObject)
        {
            _alpha = parentState._alpha * displayObject.Alpha;
            _scaleX = parentState._scaleX * displayObject.ScaleX;
            _scaleY = parentState._scaleY * displayObject.ScaleY;
            _modelviewMatrix.CopyFromMatrix(parentState._modelviewMatrix).PrependMatrix(displayObject.TransformationMatrix);
            _originalWidth = displayObject.Width;
            _originalHeight = displayObject.Height;
            _textureSubRegion = displayObject.TextureSubRegionNormalized;
            ResSet = displayObject.ResSet;
        }
    }
}