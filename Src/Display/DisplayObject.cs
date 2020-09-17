using System;
using System.Collections.Generic;
using Perlin.Geom;
using Veldrid;
using Point = Perlin.Geom.Point;
using Rectangle = Perlin.Geom.Rectangle;

namespace Perlin.Display
{
    /// <summary>
    /// DisplayObject is the base class for every renderable object.
    /// </summary>
    public abstract class DisplayObject
    {
        public delegate void EnterFrame(DisplayObject target, float elapsedTimeSecs);
        public delegate void UiEvent(DisplayObject target);
        /// <summary>
        /// Mouse Event
        /// </summary>
        /// <param name="target">The DisplayObject where the event occured</param>
        /// <param name="coords">The global mouse coordinates</param>
        /// <param name="button">the associated button</param>
        public delegate void MouseEvent(DisplayObject target, Point coords, MouseButton button);
        public delegate void MouseMoveEvent(DisplayObject target, Point coords);
        
        /// <summary>
        /// If false this object or its children cannot receive touch/click/move events.
        /// </summary>
        public bool MouseOrTouchEnabled = true;
        /// <summary>
        /// This event will get called on every frame while this object is on the Stage.
        /// </summary>
        public event EnterFrame EnterFrameEvent;
        /// <summary>
        /// Called when an object is added to the Stage.
        /// </summary>
        public event UiEvent AddedToStage;
        /// <summary>
        /// Called when an object is removed from the Stage.
        /// </summary>
        public event UiEvent RemovedFromStage;
        /// <summary>
        /// Called when a mouse button is pressed in this object
        /// </summary>
        public event MouseEvent MouseDown;
        /// <summary>
        /// Called when a pressed mouse button released in this object
        /// </summary>
        public event MouseEvent MouseUp;
        /// <summary>
        /// Called when the mouse moves after you press down a mouse on this object and it moves OR
        /// if the mouse moves over this object and mouse coordinates change.
        /// </summary>
        public event MouseMoveEvent MouseMoved;
        /// <summary>
        /// Called when the mouse moves over this object and mouse coordinates change.
        /// </summary>
        public event MouseMoveEvent MouseHover;
        /// <summary>
        /// Called once when the mouse enters this object's bounding box.
        /// </summary>
        public event MouseMoveEvent MouseEnter;
        /// <summary>
        /// Called once when the mouse leaves this object's bounding box.
        /// </summary>
        public event MouseMoveEvent MouseExit;
        /// <summary>
        /// Called when this object is clicked
        /// </summary>
        public event MouseEvent MouseClick;

        /// <summary>
        /// You can use this property to give an identifier to this object for debugging.
        /// </summary>
        public string Name = "DisplayObject";
        
        /// <summary>
        /// Sets the visibility of an object and its children. If something is not visible it will not render, but if
        /// its on the Stage it will still fire the `EnterFrameEvent` event.
        /// </summary>
        public bool Visible = true;
        
        /// <summary>
        /// The region of this object's texture to render. Shaders need this in 0..1 coordinates.
        /// </summary>
        internal Rectangle TextureSubRegionNormalized = new Rectangle(0,0,1,1);
        
        private float _pivotX;
        private float _pivotY;
        private float _scaleX = 1.0f;
        private float _scaleY = 1.0f;

        /// <summary>
        /// The GPU resource set for this object. In a way it holds a reference to the texture that is on the GPU
        /// and this object will use for rendering. This contains the following:
        /// <list type="bullet">
        /// <item><description>Its ResourceLayout, this holds the shader uniform names and positions</description></item>
        /// <item><description>The resources that its shader can access (stuff like buffers, textures)</description></item>
        /// </list>
        /// This is used for rendering, if its null, the object cannot be rendered.
        /// </summary>
        public ResourceSet ResSet;
        private readonly Matrix2D _transformationMatrix;
        private bool _transformationMatrixChanged = true;

        protected DisplayObject()
        {
            _transformationMatrix = Matrix2D.Create();
            Alpha = 1;
        }
        
        internal void DispatchMouseDown(MouseButton button, Point mousePosition)
        {
            MouseDown?.Invoke(this, mousePosition, button);
        }
        
        internal void DispatchMouseUp(MouseButton button, Point mousePosition)
        {
            MouseUp?.Invoke(this, mousePosition, button);
        }
        
        internal void DispatchMouseMoved(Point mousePosition)
        {
            MouseMoved?.Invoke(this, mousePosition);
        }
        
        internal void DispatchMouseHover(Point mousePosition)
        {
            MouseHover?.Invoke(this, mousePosition);
        }
        
        internal void DispatchMouseEnter(Point mousePosition)
        {
            MouseEnter?.Invoke(this, mousePosition);
        }
        
        internal void DispatchMouseExit(Point mousePosition)
        {
            MouseExit?.Invoke(this, mousePosition);
        }
        
        internal void DispatchMouseClick(Point mousePosition)
        {
            MouseClick?.Invoke(this, mousePosition, MouseButton.Left);
        }

        protected bool _isOnStage;
        /// <summary>
        /// Whether this instance is connected to the Stage (= itself or one its parent/grandparent/.. is on the Stage).
        /// If something is not on the Stage, it will not render.
        /// </summary>
        public bool IsOnStage
        {
            get => _isOnStage;
            internal set
            {
                if (value != _isOnStage)
                {
                    _isOnStage = value;
                    if (value)
                    {
                        AddedToStage?.Invoke(this);
                    }
                    else
                    {
                        RemovedFromStage?.Invoke(this);
                    }
                }
            }
        }
        
        /// <summary>
        /// Renders the object and its children. Do not call this, the engine calls it on every frame while
        /// this is on the Stage.
        /// </summary>
        /// <param name="elapsedTimeSecs">The elapsed time in seconds since the last render call.</param>
        public virtual void Render(float elapsedTimeSecs)
        {
            if (_isOnStage)
            {
                InvokeEnterFrameEvent(elapsedTimeSecs);
            }
            if (!Visible)
            {
                return;
            }
            PerlinApp.Renderer.PushRenderState(this);
            foreach (var child in Children)
            {
                child.Render(elapsedTimeSecs);
            }
            PerlinApp.Renderer.PopRenderState();
        }

        /// <summary>
        /// The parent of this object. You cannot set this manually, call <code>AddChild</code> on the object you want to add
        /// this to.
        /// </summary>
        public virtual DisplayObject Parent { get; internal set; }

        private float _x;
        /// <summary>
        /// The X coordinate of this object. The actual position is offset by the PivotX and PivotY values.
        /// </summary>
        public virtual float X
        {
            get => _x;
            set
            {
                if (value == _x)
                {
                    return;
                }
                _transformationMatrixChanged = true;
                _x = value;
            }
        }

        private float _y;
        /// <summary>
        /// The Y coordinate of this object. The actual position is offset by the PivotX and PivotY values.
        /// </summary>
        public virtual float Y
        {
            get => _y;
            set
            {
                if (value == _y)
                {
                    return;
                }
                _transformationMatrixChanged = true;
                _y = value;
            }
        }

        protected float OriginalWidth;
        /// <summary>
        /// The width of the object without transformations.
        /// </summary>
        public virtual float Width => OriginalWidth;

        protected float OriginalHeight;
        /// <summary>
        /// The height of the object without transformations.
        /// </summary>
        public virtual float Height => OriginalHeight;

        /// <summary>
        /// The width of the object after applying scaling
        /// </summary>
        public virtual float WidthScaled
        {
            get => OriginalWidth * _scaleX;
            set => ScaleX = value / OriginalWidth;
        }

        /// <summary>
        /// The height of the object after applying scaling
        /// </summary>
        public virtual float HeightScaled
        {
            get => OriginalHeight * _scaleY;
            set => ScaleY = value / OriginalHeight;
        }
        
        private float _rotation;
        /// <summary>
        /// Rotation in Degrees.
        /// </summary>
        public virtual float Rotation
        {
            get => _rotation;
            set
            {
                if (value == _rotation)
                {
                    return;
                }
                // move to equivalent value in range [0 deg, 360 deg]
                value = value % 360;
                // move to [-180 deg, +180 deg]
                if (value < -180)
                {
                    value += 360;
                }
                else if (value > 180)
                {
                    value -= 360;
                }
                _transformationMatrixChanged = true;
                _rotation = value;
            }
        }

        /// <summary>
        /// The transparency value of this object. 0=fully transparent, 1=not transparent.
        /// </summary>
        public float Alpha;
        
        internal void InvokeEnterFrameEvent(float elapsedTimeSecs)
        {
            EnterFrameEvent?.Invoke(this, elapsedTimeSecs);
        }

        /// <summary>
        /// Returns this or one of its children (or grandchildren..) that is found topmost on the given point in local coordinates.
        /// If there is nothing, it returns the Stage if its on the Stage otherwise <c>null</c>.
        /// </summary>
        public virtual DisplayObject HitTest(Point p)
        {
            if (!Visible || !MouseOrTouchEnabled)
            {
                return null;
            }
            // check every child recursively
            for (var i = Children.Count - 1; i >= 0; --i) // front to back!
            {
                DisplayObject child = Children[i];
                if (child.Visible)
                {
                    Matrix2D transformationMatrix = Matrix2D.Create();
                    transformationMatrix.CopyFromMatrix(child.TransformationMatrix);
                    transformationMatrix.Invert();

                    Point transformedPoint = transformationMatrix.TransformPoint(p);
                    DisplayObject target = child.HitTest(transformedPoint);
                    if (target != null)
                    {
                        return target;
                    }
                }
            }
            // check self
            return GetBounds(this).Contains(p) ? this : null;
        }

        /// <summary>
        /// Returns the bounds of this object after transformations
        /// </summary>
        public virtual Rectangle GetBounds()
        {
            return GetBounds(Parent);
        }
        
        /// <summary>
        /// Returns the bounding box rectangle relative to the given DisplayObject.
        /// It will take transformations into account, but the returned rectangle is not rotated.
        /// </summary>
        /// <param name="targetSpace">The bounding box relative to this DisplayObject</param>
        public virtual Rectangle GetBounds(DisplayObject targetSpace)
        {
            Rectangle outRect = new Rectangle();
            if (targetSpace == this) // Optimization
            {
                outRect.Width = OriginalWidth;
                outRect.Height = OriginalHeight;
            }
            else if (targetSpace == Parent && _rotation == 0.0) // Optimization
            {
                outRect = new Rectangle(_x - _pivotX * _scaleX,
                                        _y - _pivotY * _scaleY,
                                        OriginalWidth * _scaleX,
                                        OriginalHeight * _scaleY);
                if (_scaleX < 0.0f)
                {
                    outRect.Width *= -1.0f;
                    outRect.X -= outRect.Width;
                }
                if (_scaleY < 0.0f)
                {
                    outRect.Height *= -1.0f;
                    outRect.Top -= outRect.Height;
                }
            }
            else
            {
                outRect.Width = OriginalWidth;
                outRect.Height = OriginalHeight;
                Matrix2D sMatrix = GetTransformationMatrix(targetSpace);
                outRect = outRect.GetBounds(sMatrix);
            }
            return outRect;
        }

        /// <summary>
        /// Returns the bounding box of this object in with its own and transformation and its children
        /// bounds included.
        /// </summary>
        public virtual Rectangle GetBoundsWithChildren()
        {
            return GetBoundsWithChildren(Parent);
        }
        public virtual Rectangle GetBoundsWithChildren(DisplayObject targetSpace)
        {
            var ownBounds = GetBounds(targetSpace);
            float minX = ownBounds.X, maxX = ownBounds.Right;
            float minY = ownBounds.Y, maxY = ownBounds.Bottom;
            foreach (DisplayObject child in Children)
            {
                Rectangle childBounds = child.GetBoundsWithChildren(targetSpace);
                minX = Math.Min(minX, childBounds.X);
                maxX = Math.Max(maxX, childBounds.X + childBounds.Width);
                minY = Math.Min(minY, childBounds.Top);
                maxY = Math.Max(maxY, childBounds.Top + childBounds.Height);
            }
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }
        
        /// <summary>
        /// The transformation matrix of the object relative to its parent. This is used internally, you likely
        /// do not need it.
        /// </summary>
        /// <returns>Not a copy, but the actual object!</returns>
        public Matrix2D TransformationMatrix
        {
            get
            {
                if (!_transformationMatrixChanged)
                {
                    return _transformationMatrix;
                }
                _transformationMatrix.Identity();
                _transformationMatrix.Scale(_scaleX, _scaleY);
                _transformationMatrix.Rotate(_rotation);
                _transformationMatrix.Translate(_x, _y);

                if (_pivotX != 0.0f || _pivotY != 0.0f)
                {
                    // prepend pivot transformation
                    _transformationMatrix.Tx = _x - _transformationMatrix.A * _pivotX
                                                  - _transformationMatrix.C * _pivotY;
                    _transformationMatrix.Ty = _y - _transformationMatrix.B * _pivotX
                                                  - _transformationMatrix.D * _pivotY;
                }
                _transformationMatrixChanged = false;
                return _transformationMatrix;
            }
        }
        
        /// <summary>
        /// Creates a matrix that represents the transformation from the local coordinate system to another.
        /// </summary>
        public Matrix2D GetTransformationMatrix(DisplayObject targetSpace)
        {
            DisplayObject currentObject;
            Matrix2D outMatrix = Matrix2D.Create();
            outMatrix.Identity();
            if (targetSpace == this)
            {
                return outMatrix;
            }
            if (targetSpace == Parent || (targetSpace == null && Parent == null))
            {
                outMatrix.CopyFromMatrix(TransformationMatrix);
                return outMatrix;
            }
            if (targetSpace == null || targetSpace == Root)
            {
                // if targetSpace 'null', we assume that we need it in the space of the Base object.
                // -> move up from this to base
                currentObject = this;
                while (currentObject != targetSpace)
                {
                    outMatrix.AppendMatrix(currentObject.TransformationMatrix);
                    currentObject = currentObject.Parent;
                }
                return outMatrix;
            }
            if (targetSpace.Parent == this)
            {
                outMatrix = targetSpace.GetTransformationMatrix(this);
                outMatrix.Invert();
                return outMatrix;
            }
            // targetSpace is not an ancestor
            // 1.: Find a common parent of this and the target coordinate space.
            var commonParent = FindCommonParent(this, targetSpace);

            // 2.: Move up from this to common parent
            currentObject = this;
            while (currentObject != commonParent)
            {
                outMatrix.AppendMatrix(currentObject.TransformationMatrix);
                currentObject = currentObject.Parent;
            }

            if (commonParent == targetSpace)
            {
                return outMatrix;
            }

            // 3.: Now move up from target until we reach the common parent
            var sHelperMatrix = Matrix2D.Create();
            sHelperMatrix.Identity();
            currentObject = targetSpace;
            while (currentObject != commonParent)
            {
                sHelperMatrix.AppendMatrix(currentObject.TransformationMatrix);
                currentObject = currentObject.Parent;
            }

            // 4.: Combine the two matrices
            sHelperMatrix.Invert();
            outMatrix.AppendMatrix(sHelperMatrix);

            return outMatrix;
        }
        
        private static readonly List<DisplayObject> CommonParentHelper = new List<DisplayObject>();
        private static DisplayObject FindCommonParent(DisplayObject object1, DisplayObject object2)
        {
            DisplayObject currentObject = object1;
            while (currentObject != null)
            {
                CommonParentHelper.Add(currentObject);
                currentObject = currentObject.Parent;
            }
            currentObject = object2;
            while (currentObject != null && CommonParentHelper.Contains(currentObject) == false)
            {
                currentObject = currentObject.Parent;
            }
            CommonParentHelper.Clear();
            if (currentObject != null)
            {
                return currentObject;
            }
            throw new ArgumentException("Object not connected to target");
        }

        /// <summary>
        /// The topmost object in the display tree the object is part of.
        /// </summary>
        public DisplayObject Root
        {
            get
            {
                DisplayObject currentObject = this;
                while (currentObject.Parent != null)
                {
                    currentObject = currentObject.Parent;
                }
                return currentObject;
            }
        }

        /// <summary>
        /// The horizontal scale of the object. 1 (the default) represents no scaling, 2 is scaled to 2x,
        /// negative values mirror the object
        /// </summary>
        public float ScaleX
        {
            get => _scaleX;
            set
            {
                if (value == _scaleX)
                {
                    return;
                }
                _transformationMatrixChanged = true;
                _scaleX = value;
            }
        }

        /// <summary>
        /// The vertical scale of the object. 1 (the default) represents no scaling, 2 is scaled to 2x,
        /// negative values mirror the object.
        /// </summary>
        public float ScaleY
        {
            get => _scaleY;
            set
            {
                if (value == _scaleY)
                {
                    return;
                }
                _transformationMatrixChanged = true;
                _scaleY = value;
            }
        }
        
        /// <summary>
        /// The pivot point of an object is the center of its rotation. By default its the top left corner. 
        /// </summary>
        public float PivotX
        {
            get => _pivotX;
            set
            {
                if (value == _pivotX)
                {
                    return;
                }
                _transformationMatrixChanged = true;
                _pivotX = value;
            }
        }

        /// <summary>
        /// The pivot point of an object is the center of its rotation. By default its the top left corner. 
        /// </summary>
        public float PivotY
        {
            get => _pivotY;
            set
            {
                if (value == _pivotY)
                {
                    return;
                }
                _transformationMatrixChanged = true;
                _pivotY = value;
            }
        }
        
        /// <summary>
        /// Transforms a point from the local coordinate system to global (stage) coordinates.
        /// </summary>
        public Point LocalToGlobal(Point localPoint)
        {
            Matrix2D matrix = GetTransformationMatrix(Root);
            return matrix.TransformPoint(localPoint);
        }

        /// <summary>
        /// Transforms a point from global (stage) coordinates to the local coordinate system.
        /// </summary>
        public Point GlobalToLocal(Point globalPoint)
        {
            Matrix2D matrix = GetTransformationMatrix(Root);
            matrix.Invert();
            return matrix.TransformPoint(globalPoint);
        }
        
        /// <summary>
        /// The List that stores the children of this DisplayObject. Use <code>AddChild(), RemoveChild()</code>,..
        /// to manage its contents!
        /// </summary>
        protected readonly List<DisplayObject> Children = new List<DisplayObject>();
        
        /// <summary>
        /// <para>
        /// Adds a child to this instance. The child is added to the front (top) of all other children in
        /// this instance. (To add a child to a specific index position, use the <code>AddChildAt()</code> method.)
        /// </para>
        /// <para>
        /// If you add a child object that already has a different display object container as a parent, the object
        /// is removed from the child list of the other display object container.
        /// </para>
        /// </summary>
        /// <param name="child">The child to add</param>
        public virtual void AddChild(DisplayObject child)
        {
            var pos = child.Parent == this ? NumChildren-1 : NumChildren;
            AddChildAt(child, pos);
        }

        /// <summary>
        /// <para>
        /// Adds a child to this instance. The child is added to the specified index, 0 represents the back (bottom)
        /// of this DisplayObject.
        /// </para>
        /// <para>
        /// If you add a child object that already has a different display object container as a parent, the object
        /// is removed from the child list of the other display object container.
        /// </para>
        /// </summary>
        /// <param name="child">The child to add</param>
        /// <param name="index">Index to add to.</param>
        public virtual void AddChildAt(DisplayObject child, int index)
        {
            if (child.Parent != null)
            {
                child.RemoveFromParent();
            }
            Children.Insert(index, child);
            if (_isOnStage)
            {
                child.IsOnStage = true;
            }
            child.Parent = this;
        }

        /// <summary>
        /// Removes a child from this object's list of children.
        /// </summary>
        /// <param name="child">The child to remove</param>
        public virtual void RemoveChild(DisplayObject child)
        {
            Children.Remove(child);
            child.IsOnStage = false;
            child.Parent = null;
        }
        
        /// <summary>
        /// Removes a child at the specified index.
        /// </summary>
        /// <param name="index"></param>
        public virtual void RemoveChildAt(int index)
        {
            RemoveChild(Children[index]);
        }
        
        /// <summary>
        /// Removes every child from this instance.
        /// </summary>
        public virtual void RemoveAllChildren()
        {
            while (NumChildren > 0)
            {
                RemoveChildAt(0);
            }
        }
        
        /// <summary>
        /// Removes this object from its Parent. Does nothing if it has no Parent.
        /// </summary>
        public virtual void RemoveFromParent()
        {
            if (Parent != null)
            {
                Parent.RemoveChild(this);
                Parent = null;
                IsOnStage = false;
            }
        }
        
        /// <summary>
        /// Swaps the rendering order of the 2 specified children.
        /// </summary>
        public virtual void SwapChildren(DisplayObject child1, DisplayObject child2)
        {
            var firstIndex = Children.FindIndex(o => o == child1);
            var secondIndex = Children.FindIndex(o => o == child2);
            DisplayObject tmp = Children[firstIndex];
            Children[firstIndex] = Children[secondIndex];
            Children[secondIndex] = tmp;
        }
        /// <summary>
        /// The number of children.
        /// </summary>
        public virtual int NumChildren => Children.Count;
    }
}