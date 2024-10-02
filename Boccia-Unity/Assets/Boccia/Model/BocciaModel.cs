using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;


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
    private BocciaScreen PreviousScreen;

    // Game
    public BocciaGameMode GameMode;
    public float RampRotation => rampController.Rotation;
    public float RampElevation => rampController.Elevation;
    public bool BarState => rampController.IsBarOpen;
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
    public event System.Action WasChanged;
    public event System.Action NavigationChanged;

    // Hardware interface
    // TODO - create this based on game mode (live or sim)
    private RampController rampController = new SimulatedRamp();

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

        SendRampChangeEvent();

        // For now, just emit change event if ramp changes
        rampController.RampChanged += SendRampChangeEvent;
    }

    private void OnDisable()
    {
        rampController.RampChanged -= SendRampChangeEvent;
    }



    // MARK: Game control
    public void RotateBy(float degrees) => rampController.RotateBy(degrees);
    public void ElevateBy(float elevation) => rampController.ElevateBy(elevation);

    public void ResetRampPosition() => rampController.ResetRampPosition();

    public void DropBall() => rampController.DropBall();

    // Method to reset the state of the bar after the ball has been dropped
    public void ResetBar()
    {
        rampController.ResetBar();
        SendRampChangeEvent();
    }

    public void RandomColor()
    {
        bocciaData.BallColor = UnityEngine.Random.ColorHSV();
        SendRampChangeEvent();
    }

    public void ChangeBallColor(string colorString)
    {
        Color color = GetColorFromName(colorString);
        bocciaData.BallColor = color;
        SendRampChangeEvent();
    }

    public void SetElevationPrecision(float elevationPercent)
    {
        bocciaData.ElevationPrecision = elevationPercent;
        SendRampChangeEvent();
    }

    public void SetElevationRange(float elevationRange)
    {
        bocciaData.ElevationRange = elevationRange;
        SendRampChangeEvent();
    }

    public void SetRotationPrecision(float rangeDegree)
    {
        bocciaData.RotationPrecision = rangeDegree;
        SendRampChangeEvent();
    }
    public void SetRotationRange(float rotationRange)
    {
        bocciaData.RotationRange = rotationRange;
        SendRampChangeEvent();
    }

    public void SetElevationSpeed(float elevationSpeed)
    {
        bocciaData.ElevationSpeed = elevationSpeed;
        SendRampChangeEvent();
    }

    public void SetRotationSpeed(float rotationSpeed)
    {
        bocciaData.RotationSpeed = rotationSpeed;
        SendRampChangeEvent();
    }
    

    // MARK: Navigation control
    public void StartMenu()
    {
        ShowScreen(BocciaScreen.StartMenu);
        // GameMode = BocciaGameMode.Stop;
    }

    public void PlayMenu()
    {
        ShowScreen(BocciaScreen.PlayMenu);
        // GameMode = BocciaGameMode.Stop;
    }

    public void Train()
    {
        ShowScreen(BocciaScreen.Train);
        // start training, hamburger -> menu = stop?
        // GameMode = BocciaGameMode.Train;
    }

    public void Play()
    {
        ShowScreen(BocciaScreen.Play);
        // GameMode = BocciaGameMode.Play;
    }

    public void VirtualPlay()
    {
        ShowScreen(BocciaScreen.VirtualPlay);
        // GameMode = BocciaGameMode.Virtual;
    }

    public void ShowHamburgerMenu()
    {
        if (CurrentScreen == BocciaScreen.HamburgerMenu)
        {
            ShowPreviousScreen();
        }
        else
        {
            ShowScreen(BocciaScreen.HamburgerMenu);
        }
    }   
    
    public void ShowGameOptions() => ShowScreen(BocciaScreen.GameOptions);
    public void ShowBciOptions() => ShowScreen(BocciaScreen.BciOptions);
    public void ShowRampOptions() => ShowScreen(BocciaScreen.RampOptions);

    private void ShowScreen(BocciaScreen screen)
    {
        PreviousScreen = CurrentScreen;
        CurrentScreen = screen;
        SendNavigationChangeEvent();
    }

    private void ShowPreviousScreen()
    {
        CurrentScreen = PreviousScreen;
        SendNavigationChangeEvent();
    }

    public void QuitGame()
    {
        UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }


    // MARK: BCI control


    // MARK: Persistence
    public void Bind(BocciaData gameData)
    {
        // Bind replaces the current gameData with another one.  This is used
        // to provide the model with a BocciaData loaded from disk, etc.
        this.bocciaData = gameData;
        SendRampChangeEvent();
    }


    // MARK: Helpers
    private void SendRampChangeEvent()
    {
        WasChanged?.Invoke();
    }

    private void SendNavigationChangeEvent()
    {
        NavigationChanged?.Invoke();
    }

    private void ResetNavigationState()
    {
        CurrentScreen = BocciaScreen.StartMenu;
        PreviousScreen = CurrentScreen;
    }

    private void ResetGameState()
    {
        GameMode = BocciaGameMode.StopPlay;
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

    Color GetColorFromName(string colorName)
    {
        switch (colorName.ToLower())
        {
            case "red":
                return Color.red;
            case "green":
                return Color.green;
            case "blue":
                return Color.blue;
            case "yellow":
                return Color.yellow;
            case "white":
                return Color.white;
            case "black":
                return Color.black;
            default:
                return Color.clear;  // Returns a transparent color if not found
        }
    }
}

