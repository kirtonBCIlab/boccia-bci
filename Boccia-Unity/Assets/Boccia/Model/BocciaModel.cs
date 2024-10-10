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
    // Access to the current BCI Paradigm
    // public BocciaBciParadigm Paradigm => bocciaData.Paradigm;
    public BocciaBciParadigm Paradigm  // Test new
    {
        get => bocciaData.Paradigm;
        set
        {
            if (bocciaData.Paradigm != value)
            {
                bocciaData.Paradigm = value;
                SendBciChangeEvent();  // Notify listeners of the paradigm change
            }
        }
    }

    // Paradigm agnostic tracker for whether BCI Training has been done
    // get is public, set is private
    public bool BciTrained { get; private set; }  // Is there a need to have a paradigm-specific state tracker, e.g. BciTrainedP300, BciTrainedSSVEP

    // Expose the entire P300SettingsContainer via a property
    public P300SettingsContainer P300Settings => bocciaData.P300Settings;

    // Example if adding SSVEP
    // public SSVEPSettingsContainer SSVEPSettings => bocciaData.SSVEPSettings;

    // Ramp Hardware
    public bool RampHardwareConnected;
    public string SerialPortName => bocciaData.SerialPortName;

    // Change events
    public event System.Action WasChanged;  // Referring to the Ramp. Need to change this in other scripts (e.g. RampPresenter.cs) if we want to make the variable name more informative
    public event System.Action NavigationChanged;
    public event System.Action BciChanged;

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

    public void ChangeBallColor(Color colorString)
    {
        bocciaData.BallColor = colorString;
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

    public void ShowPreviousScreen()
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
    // // Example setters for P300 Training and Testing settings, if we wanted to make one per setting
    // public void SetP300TrainNumFlashes(int numFlashes)
    // {
    //     bocciaData.P300Settings.Train.NumFlashes = numFlashes;
    //     // Additional logic or events can be triggered here if needed
    // }

    // public void SetP300TestStimulusOnDuration(double duration)
    // {
    //     bocciaData.P300Settings.Test.StimulusOnDuration = duration;
    //     // Additional logic or events can be triggered here if needed
    // }
    public void SetBciOption<T>(ref T settingField, T newValue)
    // Generic setter for changing any BCI setting (train or test, P300 or SSVEP)
    {
        settingField = newValue;
        SendBciChangeEvent();
    }

    // MARK: BCI Setting Defaults
    private void SetDefaultP300Settings()
    // Set default values for P300 settings
    {
        // Reset P300 Training settings
        bocciaData.P300Settings.Train.NumFlashes = 5;
        bocciaData.P300Settings.Train.NumTrainingWindows = 3;
        bocciaData.P300Settings.Train.TargetAnimation = BocciaAnimation.Default;
        bocciaData.P300Settings.Train.ShamSelectionFeedback = false;
        bocciaData.P300Settings.Train.ShamSelectionAnimation = BocciaAnimation.Default;
        bocciaData.P300Settings.Train.StimulusOnDuration = 2.0f;
        bocciaData.P300Settings.Train.StimulusOffDuration = 2.0f;
        bocciaData.P300Settings.Train.FlashColour = Color.red;

        // Reset P300 Testing settings
        bocciaData.P300Settings.Test.NumFlashes = 5;
        bocciaData.P300Settings.Test.TargetSelectionFeedback = true;
        bocciaData.P300Settings.Test.TargetSelectionAnimation = BocciaAnimation.Default;
        bocciaData.P300Settings.Test.StimulusOnDuration = 2.0f;
        bocciaData.P300Settings.Test.StimulusOffDuration = 2.0f;
        bocciaData.P300Settings.Test.FlashColour = Color.red;  
    }

    // Placeholder for SSVEP paradigm  default settings (future expansion)
    // private void SetDefaultSSVEPSettings()
    // {
    //     // Add SSVEP-specific defaults here
    // }

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

    private void SendBciChangeEvent()
    {
        BciChanged?.Invoke();
    }

    // MARK: Resetting states to Defaults
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
    // Reset BCI to untrained state with active-paradigm-specific default settings
    {
        BciTrained = false;  // Reset the training state
        
        // Call the public method to reset the settings
        ResetBciSettingsToDefaults();
        // SendBciChangeEvent() trigged within ResetBciSettingsToDefaults();
    }

    // This method resets the BCI settings to default without touching the BciTrained state
    public void ResetBciSettingsToDefaults()
    {
        // Reset settings based on the active paradigm
        switch (bocciaData.Paradigm)
        {
            case BocciaBciParadigm.P300:
                SetDefaultP300Settings();
                break;
            // Placeholder for other paradigms like SSVEP, MI, etc.
            // case BocciaBciParadigm.SSVEP:
            //     SetDefaultSSVEPSettings();
            //     break;
        }

        // Notify the UI of the change
        SendBciChangeEvent();
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

