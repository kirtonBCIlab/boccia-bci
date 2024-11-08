using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Rendering;


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

    private RampController rampController = new SimulatedRamp();
    private RampController _simulatedRamp = new SimulatedRamp();
    private HardwareRamp _hardwareRamp = new();

    public float RampRotation => rampController.Rotation;
    public float RampElevation => rampController.Elevation;
    public bool BarState => rampController.IsBarOpen;
    public bool IsRampMoving => rampController.IsMoving;
    
    public BocciaBallState BallState;

    // Expose the GameOptionsContainer via a property
    public GameOptionsContainer GameOptions => bocciaData.GameOptions;
    // Expose the options for ball colors as a read-only property
    public IReadOnlyDictionary<string, Color> BallColorOptionsDict => bocciaData.GameOptions.BallColorOptionsDict;

    // BCI
    // Access to the current BCI Paradigm
    public BocciaBciParadigm Paradigm => bocciaData.Paradigm;  // Read-only property

    // BCI Training state
    public bool IsTraining { get; private set; }

    // Paradigm agnostic tracker for whether BCI Training has been done
    // get is public, set is private
    public bool BciTrained { get; private set; }

    // Expose the entire P300SettingsContainer via a property
    public P300SettingsContainer P300Settings => bocciaData.P300Settings;

    // Ramp Hardware
    public HardwareSettingsContainer HardwareSettings => bocciaData.HardwareSettings;

    // Ramp settings
    public RampSettingsContainer RampSettings => bocciaData.RampSettings;

    // Fan settings
    public FanSettingsContainer FanSettings => bocciaData.FanSettings;

    // Change events
    public event System.Action WasChanged;  // Referring to the Ramp. Need to change this in other scripts (e.g. RampPresenter.cs) if we want to make the variable name more informative
    public event System.Action NavigationChanged;
    public event System.Action BciChanged;
    public event System.Action NewRandomJack;
    public event System.Action BallResetChanged;
    public event System.Action BallFallingChanged;

    // Hardware interface
    // TODO - create this based on game mode (live or sim)
    public void SetRampControllerBasedOnMode()
    {
        switch (GameMode)
        {
            case BocciaGameMode.Play:  // Real ramp
                // Debug.Log("Switching to hardware ramp...");
                rampController.RampChanged -= SendRampChangeEvent;
                rampController = _hardwareRamp;
                rampController.RampChanged += SendRampChangeEvent;
                break;
            case BocciaGameMode.Virtual:  // Simulated ramp
                // Debug.Log("Switching to simulated ramp...");
                rampController.RampChanged -= SendRampChangeEvent;
                rampController = _simulatedRamp;
                rampController.RampChanged += SendRampChangeEvent;
                break;     
        }   
    }

    public void Start()
    {
        // If the model is uninitialized, set it up
        // Note: This will not run if a model is being loaded from an existing save state
        // e.g. Given how saving is setup, it will only run once unless something happens to the save file.
        if (!bocciaData.WasInitialized)
        {
            Debug.Log("Initializing BocciaData...");
            ResetNavigationState();
            ResetGameState();
            ResetBciState();
            ResetRampHardwareState();

            bocciaData.WasInitialized = true;
        }


        // These will actually run each time the software starts

        // Initialize the list of possible ball colors
        InitializeBallColorOptions();

        SendRampChangeEvent();

        // Initialize controller to _simulatedRamp
        rampController = _simulatedRamp;
        SetRampControllerBasedOnMode();

        // Set default hardware options
        SetDefaultHardwareOptions();
        // Set Ramp Settings
        SetRampSettings();
        // Set Fan settings
        SetFanSettings();
    }

    private void OnDisable()
    {
        rampController.RampChanged -= SendRampChangeEvent;
    }

    // Setting default values for Hardware options
    // This is triggered each time the application starts
    private void SetDefaultHardwareOptions()
    {
        bocciaData.HardwareSettings.COMPort = "";
        bocciaData.HardwareSettings.BaudRate = 9600;
        bocciaData.HardwareSettings.Serial = null;  // Need to have so that serial port connection to hardware persists even if we switch away from the hardware ramp
        bocciaData.HardwareSettings.IsHardwareRampMoving = false;
        bocciaData.HardwareSettings.IsSerialPortConnected = false;
        bocciaData.HardwareSettings.IsRampCalibrationDone = new Dictionary<string, bool>
        {
            {"Drop", false},
            {"Elevation", false},
            {"Rotation", false}
        };
    }

    // Set RampSettings
    private void SetRampSettings()
    {
        // Ramp movement limits
        bocciaData.RampSettings.ElevationLimitMin = 0;
        bocciaData.RampSettings.ElevationLimitMax = 100;
        bocciaData.RampSettings.RotationLimitMin = -85;
        bocciaData.RampSettings.RotationLimitMax = 85;

        // Speeds
        // NOTE: These are values for the hardware that controls the ramp
        // As pulses/movements per second
        // !!!!!!!!!!!!!!
        // A translation will need to be done later for
        // matching movements in the game world
        // and for presenting a physical measurement to the user
        // (e.g. cm/s or deg/s)
        bocciaData.RampSettings.ElevationSpeedMin = 1;
        bocciaData.RampSettings.ElevationSpeedMax = 255;
        bocciaData.RampSettings.RotationSpeedMin = 1;
        bocciaData.RampSettings.RotationSpeedMax = 1000;
    }

    // Set FanSettings
    private void SetFanSettings()
    {
        // ElevationRange
        bocciaData.FanSettings.ElevationRangeMin = 1;
        bocciaData.FanSettings.ElevationRangeMax = 100;

        // ElevationPrecision
        bocciaData.FanSettings.ElevationPrecisionMin = 1;
        bocciaData.FanSettings.ElevationPrecisionMax = 7;

        // RotationRange
        // Also sets limits on Theta for Fan generation
        bocciaData.FanSettings.RotationRangeMin = 5;
        bocciaData.FanSettings.RotationRangeMax = 180;

        // RotationPrecision
        bocciaData.FanSettings.RotationPrecisionMin = 1;
        bocciaData.FanSettings.RotationPrecisionMax = 1;
    }

    // MARK: Game options
    // Setting default values for Game Options
    // This is triggered by the ResetGameOptionsToDefaults() public method below whenever the "Reset to Default" button is pressed
    // within the Game Options menu, via the OnResetDefaultsClicked() method in GameOptionsMenuPresenter.cs
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

        // User values
        bocciaData.GameOptions.ElevationPrecision = 3;
        bocciaData.GameOptions.ElevationRange = 20;
        bocciaData.GameOptions.RotationPrecision = 3;
        bocciaData.GameOptions.RotationRange = 20;

        // Operator values
        bocciaData.GameOptions.ElevationSpeed = 5;
        bocciaData.GameOptions.RotationSpeed = 5;

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

    // MARK: Game control
    public void RotateBy(float degrees) => rampController.RotateBy(degrees);
    public void RotateTo(float degrees) => rampController.RotateTo(degrees);
    public void ElevateBy(float elevation) => rampController.ElevateBy(elevation);
    public void ElevateTo(float elevation) => rampController.ElevateTo(elevation);

    public void ResetRampPosition() => rampController.ResetRampPosition();

    public void DropBall() => rampController.DropBall();

    public bool ConnectToSerialPort(string comPort, int baudRate) => _hardwareRamp.ConnectToSerialPort(comPort, baudRate);
    public bool DisconnectFromSerialPort() => _hardwareRamp.DisconnectFromSerialPort();
    public string ReadSerialCommand() => _hardwareRamp.ReadSerialCommand();
    public void AddSerialCommandToList(string command) => _hardwareRamp.AddSerialCommandToList(command);
    public void SendSerialCommandList() => _hardwareRamp.SendSerialCommandList();
    public void ResetSerialCommands() => _hardwareRamp.ResetSerialCommands();
    public Task<string> ReadSerialCommandAsync() => _hardwareRamp.ReadSerialCommandAsync();
    public void RandomBallDrop(int randomRotation, int randomElevation) => _hardwareRamp.RandomBallDrop(randomRotation, randomElevation);

    // Method to reset the state of the bar after the ball has been dropped
    public void ResetBar()
    {
        rampController.ResetBar();
        SendRampChangeEvent();
    }

    public void SetBallStateReady()
    {
        BallState = BocciaBallState.ReadyToRelease;
    }

    public void SetBallStateReleased()
    {
        BallState = BocciaBallState.Released;
    }

    public void HandleBallFalling()
    {
        SendBallFallingEvent();
    }

    public void ResetVirtualBalls()
    {
        SendBallResetEvent();
    }

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

    public bool SetRampMoving(bool isMoving)
    {
        return rampController.IsMoving = isMoving;
    }

    // MARK: Navigation control
    public void StartMenu()
    {
        GameMode = BocciaGameMode.StopPlay;
        ShowScreen(BocciaScreen.StartMenu);
    }

    public void PlayMenu()
    {
        GameMode = BocciaGameMode.StopPlay;
        ShowScreen(BocciaScreen.PlayMenu);
    }

    public void Train()
    {
        GameMode = BocciaGameMode.Train;
        ShowScreen(BocciaScreen.TrainingScreen);
        // start training, hamburger -> menu = stop?
    }

    public void Play()
    {
        GameMode = BocciaGameMode.Play;
        ShowScreen(BocciaScreen.Play);
    }

    public void VirtualPlay()
    {
        GameMode = BocciaGameMode.Virtual;
        ShowScreen(BocciaScreen.VirtualPlay);
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
    public void ShowRampSetup() => ShowScreen(BocciaScreen.RampSetup);

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

    public void TrainingStarted()
    {
        IsTraining = true;
        SendBciChangeEvent();
    }

    // Update training status when training is complete
    public void TrainingComplete()
    {
        IsTraining = false;
        BciTrained = true;
        SendBciChangeEvent();
    }

    // MARK: BCI Setting Defaults
    // These is triggered for the active BCI paradigm by the ResetBciSettingsToDefaults() public method below whenever the
    // "Reset to Default" button is pressed/ within the BCI Options menu, via the OnResetDefaultsClicked() method in BciOptionsMenuPresenter.cs
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

    private void SendBallResetEvent()
    {
        BallResetChanged?.Invoke();
    }

    private void SendBallFallingEvent()
    {
        BallFallingChanged?.Invoke();
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
        BallState = BocciaBallState.ReadyToRelease;

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
        if (bocciaData.HardwareSettings.Serial != null && bocciaData.HardwareSettings.Serial.IsOpen)
        {
            bocciaData.HardwareSettings.Serial.Close();
        }
        bocciaData.HardwareSettings.IsSerialPortConnected = false;
        bocciaData.HardwareSettings.COMPort = "";
    }
}

