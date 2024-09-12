using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public enum BocciaScreen
{
    Start,
    GameOptions,
    BciOptions,
    RampSetup,
    PlayMenu,
    TrainView,
    RampView,
    QuitGame,
}

public enum BocciaGameMode
{
    Start,
    Train,
    Live,
    Simulated,
}

public enum BocciaBallState
{
    Ready,
    Released,
}

// BocciaModel implements the "business logic" for the game.  The BocciaModel 
// exposes state to the application via read only properties.  The state is changed
// through functions, ex: BocciaModel.doSomething().  Persistent state is stored a 
// BocciaData object for easy serialization.
public class BocciaModel : Singleton<BocciaModel>
{
    // Create a default BocciaData, this may be replaced by Bind()
    private BocciaData bocciaData = new BocciaData();

    // Navigation
    public BocciaScreen CurrentScreen;

    // Game
    public BocciaGameMode GameMode;
    public Double RampRotation;
    public Double RampHeight;
    public BocciaBallState BallState;

    public Color BallColor => bocciaData.BallColor;
    public Double ElevationPrecision => bocciaData.ElevationPrecision;
    public Double ElevationRange => bocciaData.ElevationRange;
    public Double ElevationSpeed => bocciaData.ElevationSpeed;
    public Double RotationPrecision => bocciaData.RotationPrecision;
    public Double RotationRange => bocciaData.RotationRange;
    public Double RotationSpeed => bocciaData.RotationSpeed;

    // BCI
    public bool BciTrained;

    public BocciaBciParadigm Paradigm => bocciaData.Paradigm;
    public int NumFlashes => bocciaData.NumFlashes;
    public int NumTrainingWindows => bocciaData.NumTrainingWindows;
    public BocciaAnimation TargetAnimation => bocciaData.TargetAnimation;
    public bool ShamFeedback => bocciaData.ShamFeedback;
    public Double StimulusOnDuration => bocciaData.StimulusOnDuration;
    public Double StimulusOffDuration => bocciaData.StimulusOffDuration;

    // Ramp Hardware
    public bool RampHardwareConnected;
    public string SerialPortName => bocciaData.SerialPortName;

    // Change events
    public static event System.Action WasChanged;


    // TODO - remove, these are here just for example
    public bool IsRotating => bocciaData.IsRotating;
    public Vector3 RotationRates => bocciaData.RotationRates;


    public void Start()
    {
        // If the model is uninitialized, set it up
        if (!bocciaData.WasInitialized)
        {
            // TODO - remove
            ResetRotations();

            ResetNavigationState();
            ResetGameState();
            ResetBciState();
            ResetRampHardwareState();
            
            bocciaData.WasInitialized = true;
        }
        SendChangeEvent();
    }


    // Model mutators
    public void RandomColor()
    {
        bocciaData.BallColor = UnityEngine.Random.ColorHSV();
        SendChangeEvent();
    }

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
        bocciaData.RotationRates = UnityEngine.Random.rotation.eulerAngles;
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

    private void ResetNavigationState()
    {
        CurrentScreen = BocciaScreen.Start;
    }

    private void ResetGameState()
    {
        GameMode = BocciaGameMode.Start;
        RampRotation = 0.0;
        RampHeight = 0.0;
        BallState = BocciaBallState.Ready;

        bocciaData.BallColor = Color.blue;
        bocciaData.ElevationPrecision = 0.0;
        bocciaData.ElevationRange = 0.0;
        bocciaData.ElevationSpeed = 0.0;
        bocciaData.RotationPrecision = 0.0;
        bocciaData.RotationRange = 0.0;
        bocciaData.RotationSpeed = 0.0;

    }

    private void ResetBciState()
    {
        BciTrained = false;

        bocciaData.Paradigm = BocciaBciParadigm.P300;
        bocciaData.NumFlashes = 5;
        bocciaData.NumTrainingWindows = 3;
        bocciaData.TargetAnimation = BocciaAnimation.Default;
        bocciaData.ShamFeedback = false;
        bocciaData.StimulusOnDuration = 2.0;
        bocciaData.StimulusOffDuration = 2.0;

    }

    private void ResetRampHardwareState()
    {
        RampHardwareConnected = false;
        bocciaData.SerialPortName = "COM1";
    }
}

