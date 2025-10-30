using UnityEngine;
using UnityEditor;
using System;

namespace UniGLTF.MeshUtility
{
    [Serializable]
    public abstract class Splitter
    {
        internal enum SplitMode
        {
            Horizontal,
            Vertical,
        }

        protected float _value;

        private EditorWindow _window;
        private SplitMode _lockMode;
        private float _lockValues;
        private MouseCursor _mouseCursor;
        protected float _barSize;

        private bool _isResize;
        private Rect _resizeRect;
        private float _ratio = 0.5f;

        private static readonly Color SPLITTER_COLOR = new Color(0, 0, 0, 1.0f);
        private static readonly Color SPLITTER_COLOR_FREE = new Color(0.5f, 0.5f, 0.5f, 1.0f);

        internal Splitter(EditorWindow window, float defaultValue, SplitMode lockMode, float minValue, float barSize = 16f)
        {
            _window = window;
            _value = defaultValue;
            _lockMode = lockMode;
            _lockValues = minValue;
            _mouseCursor = lockMode == SplitMode.Vertical ? MouseCursor.ResizeHorizontal : MouseCursor.ResizeVertical;
            _barSize = barSize;
        }

        protected abstract RectOffset RectOffset();

        protected abstract Rect MainRect(Rect rect);

        protected abstract Rect SubRect(Rect rect);

        protected abstract Rect BarRect(Rect rect);

        public void OnGUI(Rect rect, Action<Rect> mainDelegate, Action<Rect> subDelegate)
        {
            var current = Event.current;

            mainDelegate(MainRect(rect));
            subDelegate(SubRect(rect));

            _resizeRect = BarRect(rect);
            EditorGUIUtility.AddCursorRect(_resizeRect, _mouseCursor);

            var clampMax = _lockMode == SplitMode.Vertical ? rect.width - _lockValues : rect.height - _lockValues;
            var targetSplitterValue = _lockMode == SplitMode.Vertical ? rect.width : rect.height;

            _ratio = _lockMode == SplitMode.Vertical ? _value / rect.width : _value / rect.height;

            if (current.type == EventType.MouseDown && _resizeRect.Contains(current.mousePosition))
                _isResize = true;
            if (current.type == EventType.MouseUp)
                _isResize = false;

            if (_isResize && current.type == EventType.MouseDrag)
            {
                var targetValue = _lockMode == SplitMode.Vertical
                    ? current.mousePosition.x
                    : current.mousePosition.y;
                var diffValue = _lockMode == SplitMode.Vertical ? rect.width : rect.height;
                _ratio = targetValue / diffValue;
            }
            else if (current.type != EventType.Layout && Event.current.type != EventType.Used)
            {
                _ratio = targetSplitterValue * _ratio / targetSplitterValue;
            }

            _value = Mathf.Clamp(targetSplitterValue * _ratio, _lockValues, clampMax);

            EditorGUI.DrawRect(RectOffset().Remove(_resizeRect), SPLITTER_COLOR_FREE);

            if (_isResize)
                _window.Repaint();
        }
    }

    [Serializable]
    public class HorizontalSplitter : Splitter
    {
        private static readonly RectOffset RECT_OFFSET = new RectOffset(0, 0, 7, 8);

        public HorizontalSplitter(EditorWindow window, float defaultValue, float minValue, float barSize = 16)
        : base(window, defaultValue, SplitMode.Horizontal, minValue, barSize)
        {
        }

        protected override RectOffset RectOffset()
        {
            return RECT_OFFSET;
        }

        protected override Rect MainRect(Rect rect)
        {
            return new Rect(rect)
            {
                x = 0,
                y = 0,
                height = _value,
            };
        }

        protected override Rect SubRect(Rect rect)
        {
            return new Rect(rect)
            {
                x = 0,
                y = _value,
                height = rect.height - _value,
            };
        }

        protected override Rect BarRect(Rect rect)
        {
            return new Rect(rect)
            {
                x = 0,
                y = _value - _barSize / 2,
                height = _barSize,
            };
        }

    }

    [Serializable]
    public class VerticalSplitter : Splitter
    {
        private static readonly RectOffset RECT_OFFSET = new RectOffset(0, 0, 7, 8);

        public VerticalSplitter(EditorWindow window, float defaultValue, float minValue, float barSize = 4)
        : base(window, defaultValue, SplitMode.Vertical, minValue, barSize)
        {
        }

        protected override RectOffset RectOffset()
        {
            return RECT_OFFSET;
        }

        protected override Rect MainRect(Rect rect)
        {
            return new Rect(rect)
            {
                x = rect.x + 0,
                y = rect.y + 0,
                width = _value,
            };
        }

        protected override Rect SubRect(Rect rect)
        {
            return new Rect(rect)
            {
                x = rect.x + _value,
                y = rect.y + 0,
                width = rect.width - _value,
            };
        }

        protected override Rect BarRect(Rect rect)
        {
            return new Rect(rect)
            {
                x = rect.x + _value - _barSize / 2,
                y = rect.y + 0,
                width = _barSize,
            };
        }
    }
}