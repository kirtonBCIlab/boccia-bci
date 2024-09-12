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
    public float RampRotation;
    public float RampElevation;
    public BocciaBallState BallState;

    public Color BallColor => bocciaData.BallColor;
    public float ElevationPrecision => bocciaData.ElevationPrecision;
    public float ElevationRange => bocciaData.ElevationRange;
    public float ElevationSpeed => bocciaData.ElevationSpeed;
    public float RotationPrecision => bocciaData.RotationPrecision;
    public float RotationRange => bocciaData.RotationRange;
    public float RotationSpeed => bocciaData.RotationSpeed;

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


    public void Start()
    {
        // If the model is uninitialized, set it up
        if (!bocciaData.WasInitialized)
        {
            ResetNavigationState();
            ResetGameState();
            ResetBciState();
            ResetRampHardwareState();
            
            bocciaData.WasInitialized = true;
        }
        SendChangeEvent();
    }


    // Game control
    public void RandomColor()
    {
        bocciaData.BallColor = UnityEngine.Random.ColorHSV();
        SendChangeEvent();
    }

    public void RotateLeft()
    {
        RampRotation -= 10.0f;
    }

    public void RotateRight()
    {
        RampRotation += 10.0f;
    }


    // Bind replaces the current gameData with another one.  This is used
    // to provide the model with a BocciaData loaded from disk, etc.
    public void Bind(BocciaData gameData)
    {
        this.bocciaData = gameData;
        SendChangeEvent();
    }


    // Helpers
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
        RampRotation = 0.0f;
        RampElevation = 0.0f;
        BallState = BocciaBallState.Ready;

        bocciaData.BallColor = Color.blue;
        bocciaData.ElevationPrecision = 0.0f;
        bocciaData.ElevationRange = 0.0f;
        bocciaData.ElevationSpeed = 0.0f;
        bocciaData.RotationPrecision = 0.0f;
        bocciaData.RotationRange = 0.0f;
        bocciaData.RotationSpeed = 0.0f;

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

