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
        GameOptionsMenu
    }

    [CreateAssetMenu(fileName = "FanSettings", menuName = "Fan/FanSettings")]
    public class FanSettings : ScriptableObject
    {
        [Header("Fan Parameters")]
        public float columnSpacing; // Spacing between columns
        public float rowSpacing;    // Spacing between rows;

        private int _minColumns;
        private int _maxColumns;
        public int MaxColumns { get { return _maxColumns; } }

        private int _minRows;
        private int _maxRows;
        public int MaxRows { get { return _maxRows; } }

        private int _minTheta;
        private int _maxTheta;

        private int _minElevationRange;
        private int _maxElevationRange;

        private int _minRadiusDifference = 1;   // Minimum difference between inner and outer radius

        [SerializeField]
        private float _theta;               // Angle in degrees
        public float Theta
        {
            get { return _theta; }
            set { _theta = Mathf.Clamp(value, _minTheta, _maxTheta); }
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
            set { _nColumns = Mathf.Clamp(value, _minColumns, _maxColumns); }
        }

        [SerializeField]
        private int _nRows;           // Number of rows
        public int NRows
        {
            get { return _nRows; }
            set { _nRows = Mathf.Clamp(value, _minRows, _maxRows); }
        }

        [SerializeField]
        private float _elevationRange; // Elevation range [%]
        public float ElevationRange
        {
            get { return _elevationRange; } 
            set { _elevationRange = Mathf.Clamp(value, _minElevationRange, _maxElevationRange); }
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

        public void Setup(BocciaModel model)
        {
            // Initialize min and max columns and rows based on centralized model settings
            _minColumns = model.FanSettings.RotationPrecisionMin;
            _maxColumns = model.FanSettings.RotationPrecisionMax;

            _minRows = model.FanSettings.ElevationPrecisionMin;
            _maxRows = model.FanSettings.ElevationPrecisionMax;

            _minTheta = model.FanSettings.RotationRangeMin;
            _maxTheta = model.FanSettings.RotationRangeMax;

            _minElevationRange = model.FanSettings.ElevationRangeMin;
            _maxElevationRange = model.FanSettings.ElevationRangeMax;
        }

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