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

    // Expose the GameOptionsContainer via a property
    public GameOptionsContainer GameOptions => bocciaData.GameOptions;
    // Expose the options for ball colors as a read-only property
    public IReadOnlyDictionary<string, Color> BallColorOptionsDict => bocciaData.GameOptions.BallColorOptionsDict;

    // public Color BallColor => bocciaData.BallColor;
    // public float ElevationPrecision => bocciaData.ElevationPrecision;
    // public float ElevationRange => bocciaData.ElevationRange;
    // public float ElevationSpeed => bocciaData.ElevationSpeed;
    // public float RotationPrecision => bocciaData.RotationPrecision;
    // public float RotationRange => bocciaData.RotationRange;
    // public float RotationSpeed => bocciaData.RotationSpeed;

    // BCI
    // Access to the current BCI Paradigm
    public BocciaBciParadigm Paradigm => bocciaData.Paradigm;  // Read-only property

    // Paradigm agnostic tracker for whether BCI Training has been done
    // get is public, set is private
    public bool BciTrained { get; private set; }

    // Expose the entire P300SettingsContainer via a property
    public P300SettingsContainer P300Settings => bocciaData.P300Settings;

    // Ramp Hardware
    public bool RampHardwareConnected;
    public string SerialPortName => bocciaData.SerialPortName;

    // Change events
    public event System.Action WasChanged;  // Referring to the Ramp. Need to change this in other scripts (e.g. RampPresenter.cs) if we want to make the variable name more informative
    public event System.Action NavigationChanged;
    public event System.Action BciChanged;
    public event System.Action NewRandomJack;

    // Hardware interface
    // TODO - create this based on game mode (live or sim)
    private RampController rampController = new SimulatedRamp();

    public void Start()
    {
        // If the model is uninitialized, set it up
        if (!bocciaData.WasInitialized)
        {
            Debug.Log("Initializing BocciaData...");
            ResetNavigationState();
            ResetGameState();
            ResetBciState();
            ResetRampHardwareState();

            bocciaData.WasInitialized = true;
        }

        // Initialize the list of possible ball colors
        InitializeBallColorOptions();

        SendRampChangeEvent();

        // For now, just emit change event if ramp changes
        rampController.RampChanged += SendRampChangeEvent;
    }

    private void OnDisable()
    {
        rampController.RampChanged -= SendRampChangeEvent;
    }


    // MARK: Game options
    // Setting default values for Game Options
    private void SetDefaultGameOptions()
    {
        // Set BallColor to the first color in the BallColorOptionsDict dictionary
        if (BallColorOptionsDict.Count > 0)
        {
            GameOptions.BallColor = BallColorOptionsDict.First().Value;  // Set to the first color in the dictionary
        }
        else
        {
            GameOptions.BallColor = Color.red;  // Fallback if dictionary is empty (shouldn't happen)
        }

        bocciaData.GameOptions.ElevationPrecision = 0.0f;
        bocciaData.GameOptions.ElevationRange = 0.0f;
        bocciaData.GameOptions.ElevationSpeed = 0.0f;
        bocciaData.GameOptions.RotationPrecision = 0.0f;
        bocciaData.GameOptions.RotationRange = 0.0f;
        bocciaData.GameOptions.RotationSpeed = 0.0f;

        // Note: SendRampChangeEvent() trigged within ResetGameOptionsToDefaults();
    }

    // Generic setter method for changing any game option
    public void SetGameOption<T>(ref T settingField, T newValue)
    {
        settingField = newValue;
        SendRampChangeEvent();
    }


    // MARK: Methods for managing ball color
    // Initialize and populate the PossibleBallColors dictionary
    private void InitializeBallColorOptions()
    {
        bocciaData.GameOptions.BallColorOptionsDict = new Dictionary<string, Color>
        {
            {"Red", Color.red },
            {"Blue", Color.blue },
            {"Green", Color.green },
            {"Yellow", Color.yellow },
            {"Black", Color.black },
            {"Magenta", Color.magenta},
            {"Grey", Color.grey},
            {"Cyan", Color.cyan}
        };
    }

    // Get the current ball color
    public Color GetCurrentBallColor()
    {
        return GameOptions.BallColor;
    }

    // Set a new ball color
    public void SetBallColor(Color newColor)
    {
        GameOptions.BallColor = newColor;
        SendRampChangeEvent();
    }

    // public void ChangeBallColor(Color colorString)
    // {
    //     bocciaData.GameOptions.BallColor = colorString;
    //     SendRampChangeEvent();
    // }

    // public void SetElevationPrecision(float elevationPercent)
    // {
    //     bocciaData.ElevationPrecision = elevationPercent;
    //     SendRampChangeEvent();
    // }

    // public void SetElevationRange(float elevationRange)
    // {
    //     bocciaData.ElevationRange = elevationRange;
    //     SendRampChangeEvent();
    // }

    // public void SetRotationPrecision(float rangeDegree)
    // {
    //     bocciaData.RotationPrecision = rangeDegree;
    //     SendRampChangeEvent();
    // }
    // public void SetRotationRange(float rotationRange)
    // {
    //     bocciaData.RotationRange = rotationRange;
    //     SendRampChangeEvent();
    // }

    // public void SetElevationSpeed(float elevationSpeed)
    // {
    //     bocciaData.ElevationSpeed = elevationSpeed;
    //     SendRampChangeEvent();
    // }

    // public void SetRotationSpeed(float rotationSpeed)
    // {
    //     bocciaData.RotationSpeed = rotationSpeed;
    //     SendRampChangeEvent();
    // }

    // MARK: Game control
    public void RotateBy(float degrees) => rampController.RotateBy(degrees);
    public void RotateTo(float degrees) => rampController.RotateTo(degrees);
    public void ElevateBy(float elevation) => rampController.ElevateBy(elevation);
    public void ElevateTo(float elevation) => rampController.ElevateTo(elevation);

    public void ResetRampPosition() => rampController.ResetRampPosition();

    public void DropBall() => rampController.DropBall();

    // Method to reset the state of the bar after the ball has been dropped
    public void ResetBar()
    {
        rampController.ResetBar();
        SendRampChangeEvent();
    }

    // public void RandomBallColor()
    // {
    //     bocciaData.GameOptions.BallColor = UnityEngine.Random.ColorHSV();
    //     SendRampChangeEvent();
    // }

    public void RandomBallColor()
    {
        // Get all the keys (color names) from the BallColorOptionsDict
        List<string> colorKeys = new List<string>(bocciaData.GameOptions.BallColorOptionsDict.Keys);

        // Check if there are any colors in the dictionary
        if (colorKeys.Count > 0)
        {
            // Pick a random index
            int randomIndex = UnityEngine.Random.Range(0, colorKeys.Count);

            // Get the randomly selected color name
            string randomColorKey = colorKeys[randomIndex];

            // Set the GameOptions.BallColor to the corresponding color from the dictionary
            bocciaData.GameOptions.BallColor = bocciaData.GameOptions.BallColorOptionsDict[randomColorKey];

            // Trigger the change event
            SendRampChangeEvent();
        }
        else
        {
            Debug.LogError("BallColorOptionsDict is empty. Cannot assign a random ball color.");
        }
    }

    public void RandomJackBall()
    {
        SendRandomJackEvent();
    }

    
    public float GetRampOrientation()
    {
        return rampController.Rotation;
    }

    public float GetRampElevation()
    {
        return rampController.Elevation;
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

    // Set the BCI paradigm
    public void SetBciParadigm(BocciaBciParadigm newParadigm)
    {
        if (bocciaData.Paradigm != newParadigm)
        {
            bocciaData.Paradigm = newParadigm;
            SendBciChangeEvent();  // Notify listeners of the paradigm change
        }
    }

    public void SetBciOption<T>(ref T settingField, T newValue)
    // Generic setter for changing any BCI setting for a given paradigm given an existing data container for the paradigm
    // e.g. settingField = bocciaModel.P300Settings.Train.NumFlashes; newValue = 10;
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

        // Note: SendBciChangeEvent() trigged within ResetBciOptionsToDefaults();
    }

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

    private void SendRandomJackEvent()
    {
        NewRandomJack?.Invoke();
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

        ResetGameOptionsToDefaults();
        // Note: SendRampChangeEvent() trigged within ResetGameOptionsToDefaults();
    }

    public void ResetGameOptionsToDefaults()
    {
        SetDefaultGameOptions();
        SendRampChangeEvent();
    }

    private void ResetBciState()
    // Reset BCI to untrained state with active-paradigm-specific default settings
    {
        BciTrained = false;  // Reset the training state
        
        // Call the public method to reset the settings
        ResetBciSettingsToDefaults();
        // Note: SendBciChangeEvent() trigged within ResetBciSettingsToDefaults();
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
        }

        // Notify the UI of the change
        SendBciChangeEvent();
    }

    private void ResetRampHardwareState()
    {
        RampHardwareConnected = false;
        bocciaData.SerialPortName = "COM1";
    }

    // Color GetColorFromName(string colorName)
    // {
    //     switch (colorName.ToLower())
    //     {
    //         case "red":
    //             return Color.red;
    //         case "green":
    //             return Color.green;
    //         case "blue":
    //             return Color.blue;
    //         case "yellow":
    //             return Color.yellow;
    //         case "white":
    //             return Color.white;
    //         case "black":
    //             return Color.black;
    //         default:
    //             return Color.clear;  // Returns a transparent color if not found
    //     }
    // }
}

