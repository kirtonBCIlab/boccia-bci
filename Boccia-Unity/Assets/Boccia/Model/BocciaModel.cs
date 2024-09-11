using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

// BocciaModel implements the "business logic" for the game.  It stores
// state in a BocciaData object for easy serialization.  The BocciaModel exposes
// state to the application via read only properties.  The state is changed
// through functions, ex: BocciaModel.doSomething()
public class BocciaModel : Singleton<BocciaModel>
{
    // Create a default BocciaData, this may be replaced by Bind()
    private BocciaData bocciaData = new BocciaData();

    // BocciaData is accessed using read only properties and mutated with functions
    public Color CubeColor => bocciaData.CubeColor;
    public bool IsRotating => bocciaData.IsRotating;
    public Vector3 RotationRates => bocciaData.RotationRates;

    // Change events
    public static event System.Action WasChanged;

    public void Start()
    {
        // If the model is uninitialized, set it up
        if (!bocciaData.WasInitialized)
        {
            SetCubeColor("Green");
            ResetRotations();
            bocciaData.WasInitialized = true;
        }
        SendChangeEvent();
    }


    // cube setup
    public string CubeColorName => CubeColors.FirstOrDefault(item => item.Value == CubeColor).Key;

    public List<string> CubeColorNames => CubeColors.Keys.ToList<string>();

    public void SetCubeColor(string colorName)
    {
        if (CubeColors.ContainsKey(colorName))
        {
            bocciaData.CubeColor = CubeColors[colorName];
            SendChangeEvent();
        }
    }

    private static readonly Dictionary<string, Color> CubeColors = new Dictionary<string, Color>
    {
        {"Blue", Color.blue},
        {"Green", Color.green},
        {"Purple", Color.magenta}
    };


    // game control
    public void StartRotation()
    {
        bocciaData.IsRotating = true;
    }

    public void StopRotation()
    {
        bocciaData.IsRotating = false;
    }

    public void ResetRotations()
    {
        bocciaData.RotationRates = Random.rotation.eulerAngles;
    }


    // Bind replaces the current gameData with another one.  This is used
    // to provide the model with a BocciaData loaded from disk, etc.
    public void Bind(BocciaData gameData)
    {
        this.bocciaData = gameData;
        SendChangeEvent();
    }

    private void SendChangeEvent()
    {
        WasChanged?.Invoke();
    }
}

