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
        GameOptionsMenu,
    }

    [CreateAssetMenu(fileName = "FanSettings", menuName = "Fan/FanSettings")]
    public class FanSettings : ScriptableObject
    {
        // Ranges for the fan settings
        BocciaModel _model; // Reference to the model
        private readonly int _minColumns = 1;
        private readonly int _maxColumns = 7;
        
        private readonly int _minRows = 1;
        private readonly int _maxRows = 7;
        
        private readonly int _minCoarseTheta = 5 ;
        private readonly int _maxCoarseTheta = 170;

        private readonly int _minFineTheta = 5;
        private readonly int _maxFineTheta = 50;
        
        private readonly int _minElevationRange = 1;
        private readonly int _maxElevationRange = 100;        

        private readonly int _minRadiusDifference = 1;   // Minimum difference between inner and outer radius

        // Public getters to send to model
        public int MinColumns { get { return _minColumns; } }
        public int MaxColumns { get { return _maxColumns; } }

        public int MinRows { get { return _minRows; } }
        public int MaxRows { get { return _maxRows; } }

        public int MinCoarseTheta { get { return _minCoarseTheta; } }
        public int MaxCoarseTheta { get { return _maxCoarseTheta; } }

        public int MinFineTheta { get { return _minFineTheta; } }
        public int MaxFineTheta { get { return _maxFineTheta; } }

        public int MinElevationRange { get { return _minElevationRange; } }
        public int MaxElevationRange { get { return _maxElevationRange; } }

        [Header("Fan Parameters")]
        public float columnSpacing; // Spacing between columns
        public float rowSpacing;    // Spacing between rows;
       
        // Conditional sets to stay within ranges
        [SerializeField]
        private float _theta;               // Angle in degrees
        public float Theta
        {
            get { return _theta; }
            set { _theta = Mathf.Clamp(value, _minCoarseTheta, _maxCoarseTheta); }
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