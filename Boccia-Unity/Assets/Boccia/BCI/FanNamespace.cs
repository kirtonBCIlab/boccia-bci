using UnityEngine;

namespace FanNamespace
{
    public enum BackButtonPositioningMode
    {
        None,
        Left,
        Right
    }

    public enum FanPositioningMode
    {
        None,
        CenterToRails,
        CenterToBase,
        CenterNorth,
    }

    [CreateAssetMenu(fileName = "FanSettings", menuName = "Fan/FanSettings")]
    public class FanSettings : ScriptableObject
    {
        [Header("Fan Parameters")]
        public float columnSpacing; // Spacing between columns
        public float rowSpacing;    // Spacing between rows;

        private int _maxColumns = 7;            // Max number of columns
        public int MaxColumns { get { return _maxColumns; } }

        private int _maxRows = 7;               // Max number of rows
        public int MaxRows { get { return _maxRows; } }

        private int _minRadiusDifference = 1;   // Minimum difference between inner and outer radius

        [SerializeField]
        private float _theta;               // Angle in degrees
        public float Theta
        {
            get { return _theta; }
            set { _theta = Mathf.Clamp(value, 5, 180); }
        }

        [SerializeField]
        private float _outerRadius;          // Outer radius
        public float OuterRadius
        {
            get { return _outerRadius; }
            set 
            {
                _outerRadius = Mathf.Clamp(value, _minRadiusDifference, float.MaxValue); 
                _innerRadius = Mathf.Clamp(_innerRadius, 0, _outerRadius - _minRadiusDifference);
            }
        }

        [SerializeField]
        private float _innerRadius;          // Inner radius
        public float InnerRadius
        {
            get { return _innerRadius; }
            set { _innerRadius = Mathf.Clamp(value, 0, OuterRadius-_minRadiusDifference); }
        }
    
        [SerializeField]
        private int _nColumns;        // Number of columns
        public int NColumns
        {
            get { return _nColumns; }
            set { _nColumns = Mathf.Clamp(value, 1, _maxColumns); }
        }

        [SerializeField]
        private int _nRows;           // Number of rows
        public int NRows
        {
            get { return _nRows; }
            set { _nRows = Mathf.Clamp(value, 1, _maxRows); }
        }

        [SerializeField]
        private float _elevationRange; // Elevation range [%]
        public float ElevationRange
        {
            get { return _elevationRange; } //There is some bug where the value is not being clamped. Going to do this on return now.
            set { _elevationRange = Mathf.Clamp(value, 1, 100); }
        }

        [Header("Additional Button Parameters")]
        [SerializeField]
        private float _backButtonWidth; // Width of the back button
        public float BackButtonWidth
        {
            get { return _backButtonWidth; }
            set { _backButtonWidth = Mathf.Clamp(value, 1f, float.MaxValue); }
        }

        [SerializeField]
        private float _dropButtonHeight; // Width of the back button
        public float DropButtonHeight
        {
            get { return _dropButtonHeight; }
            set { _dropButtonHeight = Mathf.Clamp(value, 0.5f, float.MaxValue); }
        }

        [Header("Annotation options")]
        public int annotationFontSize;

        private void OnValidate()
        {
            Theta = _theta;
            NColumns = _nColumns;
            NRows = _nRows;
            InnerRadius = _innerRadius;
            OuterRadius = _outerRadius;
            BackButtonWidth = _backButtonWidth;
            DropButtonHeight = _dropButtonHeight;
            ElevationRange = _elevationRange;
        }
    }
}