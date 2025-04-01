using UnityEngine;

namespace CaptionForge
{
    public class SubtitleSection
    {
        public bool Resizing { get; private set; }
        
        // Properties
        public Rect SectionRect;
        public string Text;
        public Color SectionColor;
        
        // Internal
        private bool _selected;
        private readonly Color _selectedColor;
        private readonly Color _notSelectedColor;
        
        public SubtitleSection(Rect rect, string text, Color color)
        {
            this.SectionRect = rect;
            this.Text = text;
            this.SectionColor = color;
            _selectedColor = color;
            _notSelectedColor = color;
            _notSelectedColor.a = 0.1f;
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;

            if (!selected)
            {
                IsResizing(false);
            }

            SectionColor = _selected ? _selectedColor : _notSelectedColor;
        }

        public void IsResizing(bool resizing)
        {
            Resizing = resizing;
        }
    }
}