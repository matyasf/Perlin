using System.Collections.Generic;
using System.Numerics;
using Perlin.Display;
using Veldrid;
using Point = Perlin.Geom.Point;

namespace Perlin
{
    /// <summary>
    /// Helper class for easily managing keyboard input.
    /// </summary>
    public static class KeyboardInput
    {
        private static readonly HashSet<Key> CurrentlyPressedKeys = new HashSet<Key>();
        private static readonly HashSet<Key> NewKeysThisFrame = new HashSet<Key>();

        private static readonly HashSet<MouseButton> CurrentlyPressedMouseButtons = new HashSet<MouseButton>();
        private static readonly HashSet<MouseButton> NewMouseButtonsThisFrame = new HashSet<MouseButton>();
        private static (double timeSinceStart, Vector2 mouseCoords) _mouseDownData;

        /// <summary>
        /// Returns if the given key is pressed down currently.
        /// </summary>
        public static bool IsKeyDown(Key key)
        {
            return CurrentlyPressedKeys.Contains(key);
        }

        /// <summary>
        /// Returns if the given keyboard key was pressed this frame.
        /// </summary>
        public static bool IsKeyPressedThisFrame(Key key)
        {
            return NewKeysThisFrame.Contains(key);
        }

        private static DisplayObject _lastMouseDownObject;
        internal static void UpdateFrameInput(InputSnapshot snapshot, double elapsedTimeSinceStart)
        {
            NewKeysThisFrame.Clear();
            NewMouseButtonsThisFrame.Clear();
            for (int i = 0; i < snapshot.KeyEvents.Count; i++)
            {
                KeyEvent ke = snapshot.KeyEvents[i];
                if (ke.Down)
                {
                    KeyDown(ke.Key);
                }
                else
                {
                    KeyUp(ke.Key);
                }
            }
            var mousePosition = snapshot.MousePosition;
            for (int i = 0; i < snapshot.MouseEvents.Count; i++)
            {
                MouseEvent me = snapshot.MouseEvents[i];
                if (me.Down)
                {
                    if (CurrentlyPressedMouseButtons.Add(me.MouseButton))
                    {
                        NewMouseButtonsThisFrame.Add(me.MouseButton);
                        _lastMouseDownObject = PerlinApp.Stage.DispatchMouseDownInternal(me.MouseButton, mousePosition);
                        if (me.MouseButton == MouseButton.Left)
                        {
                            _mouseDownData = (elapsedTimeSinceStart, mousePosition);   
                        }
                    }
                }
                else
                {
                    CurrentlyPressedMouseButtons.Remove(me.MouseButton);
                    NewMouseButtonsThisFrame.Remove(me.MouseButton);
                    var lastMouseUpObject = PerlinApp.Stage.DispatchMouseUpInternal(me.MouseButton, mousePosition);
                    if (me.MouseButton == MouseButton.Left && 
                        elapsedTimeSinceStart -_mouseDownData.timeSinceStart < 0.3 &&
                        _lastMouseDownObject == lastMouseUpObject)
                    {
                        //Console.WriteLine("CLICK " + mousePosition + " " + lastMouseUpObject);
                        lastMouseUpObject.DispatchMouseClick(new Point(mousePosition.X, mousePosition.Y));
                    }
                    _lastMouseDownObject = null;
                }
            }
            PerlinApp.Stage.OnMouseMoveInternal(mousePosition.X, mousePosition.Y);
        }

        private static void KeyUp(Key key)
        {
            CurrentlyPressedKeys.Remove(key);
            NewKeysThisFrame.Remove(key);
        }

        private static void KeyDown(Key key)
        {
            if (CurrentlyPressedKeys.Add(key))
            {
                NewKeysThisFrame.Add(key);
            }
        }
    }
}
