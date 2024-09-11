using System;
using UnityEngine;

// BocciaData contains game state used by GameModel.  This is just a dumb container
// that's easy to serialize to/from disk.  Busines logic should go in the model.
[System.Serializable]
public class BocciaData
{
    public bool WasInitialized;
    public Color CubeColor;
    public bool IsRotating;
    public Vector3 RotationRates;
}
