using System.Collections;
using UnityEngine;

namespace BCIEssentials.StimulusEffects
{
    /// <summary>
    /// Assign or Flash a renderers material color.
    /// </summary>
    public class CanvasColorFlashEffect : StimulusEffect
    {
        private BocciaModel _model;

        [SerializeField]
        [Tooltip("The renderer to assign the material color to")]
        private CanvasRenderer _renderer;

        [Header("Flash Settings")]
        [SerializeField]
        [Tooltip("Material Color to assign while flashing is on")]
        private Color _flashOnColor = Color.red;
        
        [SerializeField]
        [Tooltip("Material Color to assign while flashing is off")]
        private Color _flashOffColor = Color.white;

        [SerializeField]
        [Tooltip("Gradient material to use for gradient stimulus type")]
        private Material _gradientMaterial;

        [SerializeField]
        [Tooltip("Sprite to use for FaceSprite stimulus type")]
        private GameObject _spriteObject;

        private BocciaStimulusType _stimulusType;

        [SerializeField]
        [Tooltip("If the flash on color is applied on start or the flash off color.")]
        private bool _startOn;
        
        [SerializeField]
        [Min(0.05f)]

        private float _flashDurationSeconds = 0.2f;

        [SerializeField]
        [Min(1)]
        private int _flashAmount = 3;

        public bool IsPlaying => _effectRoutine != null;


        private Coroutine _effectRoutine;

        private void Awake()
        {
            _model = BocciaModel.Instance;

            if (_renderer == null && !gameObject.TryGetComponent(out _renderer))
            {
                Debug.LogWarning($"No Renderer component found for {gameObject.name}");
                return;
            }

            AssignMaterialColor(_startOn ? _flashOnColor: _flashOffColor);
        }

        private void OnEnable()
        {
            if (_model != null)
            {
                if (_renderer == null && !gameObject.TryGetComponent(out _renderer))
                {
                    Debug.LogWarning($"No Renderer component found for {gameObject.name}");
                    return;
                }

                setFlashOnColor();
                setStimulusType();
            }
        }

        private void setFlashOnColor()
        {
            if (_model.GameMode == BocciaGameMode.Train)
            {
                _flashOnColor = _model.P300Settings.Train.FlashColour;
            }

            else if (_model.GameMode == BocciaGameMode.Play || _model.GameMode == BocciaGameMode.Virtual)
            {
                _flashOnColor = _model.P300Settings.Test.FlashColour;
            } 
        }

        private void setStimulusType()
        {
            if (_model.GameMode == BocciaGameMode.Train)
            {
                _stimulusType = _model.P300Settings.Train.StimulusType;
            }

            else if (_model.GameMode == BocciaGameMode.Play || _model.GameMode == BocciaGameMode.Virtual)
            {
                _stimulusType = _model.P300Settings.Test.StimulusType;
            }
        }

        public override void SetOn()
        {
            if (_renderer == null)
            {
                return;
            }

            if ((_stimulusType == BocciaStimulusType.Gradient) && _gradientMaterial != null)
            {
                _gradientMaterial.SetColor("_GradientColor", _flashOnColor); // Set gradient color to flashOnColor
                AssignMaterial(_gradientMaterial);
            }

            else if (_stimulusType == BocciaStimulusType.FaceSprite && _spriteObject != null)
            {
                _spriteObject.SetActive(true);
            }

            else
            {
                AssignMaterialColor(_flashOnColor);
            }

            IsOn = true;
        }

        public override void SetOff()
        {
            if (_renderer == null)
            {
                return;
            }

            if (_stimulusType == BocciaStimulusType.Gradient)
            {
                Material defaultUIMaterial = new Material(Shader.Find("UI/Default")); // Get Unity's default UI Material
                AssignMaterial(defaultUIMaterial);
            }

            else if (_stimulusType == BocciaStimulusType.FaceSprite)
            {
                _spriteObject.SetActive(false);
            }
            
            else
            {
                AssignMaterialColor(_flashOffColor);
            }
            
            IsOn = false;
        }

        public void Play()
        {
            Stop();
            _effectRoutine = StartCoroutine(RunEffect());
        }

        private void Stop()
        {
            if (!IsPlaying)
            {
                return;
            }

            SetOff();
            StopCoroutine(_effectRoutine);
            _effectRoutine = null;
        }

        private IEnumerator RunEffect()
        {
            if (_renderer != null)
            {
                IsOn = true;
                
                for (var i = 0; i < _flashAmount; i++)
                {
                    //Deliberately not using SetOn and SetOff here
                    //to avoid excessive null checking
                    
                    AssignMaterialColor(_flashOnColor);
                    yield return new WaitForSecondsRealtime(_flashDurationSeconds);

                    AssignMaterialColor(_flashOffColor);
                    yield return new WaitForSecondsRealtime(_flashDurationSeconds);
                }
            }

            SetOff();
            _effectRoutine = null;
        }

        private void AssignMaterialColor(Color color)
        {
            _renderer.SetColor(color);
        }

        private void AssignMaterial(Material material)
        {
            _renderer.SetMaterial(material, 0);
        }
    }
}