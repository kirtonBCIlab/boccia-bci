using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public enum BocciaBciParadigm
{
    P300,
    // Future paradigms can be added here
}

public enum BocciaAnimation
{
    None,
    SizeChange,
    ColorChange
}

// BocciaData contains state used by BocciaModel.  This is just a dumb container
// that's easy to serialize to/from disk.  Busines logic should go in the model.
[System.Serializable]
public class BocciaData
{
    public bool WasInitialized;

    // Game
    // Game options container for game-related settings
    public GameOptionsContainer GameOptions = new GameOptionsContainer();

    // hotkeys?

    // Active BCI Paradigm for switching between paradigms
    public BocciaBciParadigm Paradigm;  // Tracks the currently active paradigm

    // Paradigm-specific data container for P300
    public P300SettingsContainer P300Settings = new P300SettingsContainer();
    // ================================
    // Documentation for Accessing P300SettingsContainer Variables within BocciaData:
    // --------------------------------
    // 1. Create an instance of BocciaData:
    //    private BocciaData bocciaData = new BocciaData();
    //
    // 2. To access training variables, such as NumFlashes in training:
    //    int trainNumFlashes = bocciaData.P300Settings.Train.NumFlashes;
    //
    // 3. To modify training variables, for example, changing NumFlashes:
    //    bocciaData.P300Settings.Train.NumFlashes = 10;
    //
    // 4. To access testing variables, such as NumFlashes in testing:
    //    int testNumFlashes = bocciaData.P300Settings.Test.NumFlashes;
    //
    // 5. To modify testing variables, for example, changing NumFlashes:
    //    bocciaData.P300Settings.Test.NumFlashes = 5;
    // ================================

    // // Hardware
    public HardwareSettingsContainer HardwareSettings = new();

    // Ramp Settings
    public RampSettingsContainer RampSettings = new();

    // Fan settings
    public FanSettingsContainer FanSettings = new();
}

/// Container for hardware-related settings
[System.Serializable]
public class HardwareSettingsContainer
{
    public string COMPort;
    public int BaudRate;
    public SerialPort Serial; 
    public bool IsSerialPortConnected;
    public bool IsHardwareRampMoving;
    public Dictionary<string, bool> IsRampCalibrationDone = new();
}

/// Container for game-related settings
[System.Serializable]
public class GameOptionsContainer
{
    public Color BallColor;
    public int ElevationPrecision;
    public int ElevationRange;
    public int ElevationSpeed;
    public int RotationPrecision;
    public int RotationRange;
    public int RotationSpeed;

    // Empty dictionary to hold possible ball colors
    // Define this list within BocciaModel
    public Dictionary<string, Color> BallColorOptionsDict = new Dictionary<string, Color>();
}

/// Container for ramp and fan related settings
[System.Serializable]
public class RampSettingsContainer
{
    // Origin setting – what rotation and elevation the ramp goes to as default, e.g. on initialization or resetting
    public float ElevationOrigin;
    public float RotationOrigin;

    // Ramp elevation
    // Used by to set limits of Ramp elevation in Hardwware and Simulated ramp
    // Don't confuse similar name with that in FanSettingsContainer
    // This is for the hardware and simulated ramp
    public int ElevationLimitMin;
    public int ElevationLimitMax;

    // Ramp rotation
    // Used by to set limits of Ramp rotation in Hardwware and Simulated ramp
    public int RotationLimitMin;
    public int RotationLimitMax;

    // Ramp speeds
    // These are values for the hardware that controls the ramp
    // As pulses/movements per second
    public int ElevationSpeedMin;
    public int ElevationSpeedMax;
    public int RotationSpeedMin;
    public int RotationSpeedMax;
}

[System.Serializable]
public class FanSettingsContainer
{
    // Elevation range
    public int ElevationRangeMin;  // Used to set lower limit of GameOptions.ElevationRange
    public int ElevationRangeMax;  // Used to set upper limit of GameOptions.ElevationRange

    // ElevationPrecision – Also used to limit the number of rows of the fine fan
    public int ElevationPrecisionMin;  // Used to set lower limit of GameOptions.ElevationPrecision
    public int ElevationPrecisionMax;  // Used to set upper limit of GameOptions.ElevationPrecision

    // RotationRange
    // Also used to set limits of Theta in FanNamespace
    public int RotationRangeMin;  // Used to set lower limit of GameOptions.RotationRange
    public int RotationRangeMax;  // Used to set upper limit of GameOptions.RotationRange

    // RotationPrecision – Also used to limit the number of columns of the fine fan
    public int RotationPrecisionMin;  // Used to set lower limit of GameOptions.RotationPrecision
    public int RotationPrecisionMax;  // Used to set upper limit of GameOptions.RotationPrecision
}

/// The P300SettingsContainer class contains training and testing settings specific to the P300 paradigm.
[System.Serializable]
public class P300SettingsContainer
{
    public TrainSettings Train = new TrainSettings();  // Settings related to training
    public TestSettings Test = new TestSettings();    // Settings related to testing
    public bool SeparateButtons;

    // Nested class for P300 training settings
    [System.Serializable]
    public class TrainSettings
    {
        public int NumFlashes;  // Number of flashes during training
        public int NumTrainingSelections;  // Number of training selections
        public BocciaAnimation TargetAnimation;  // "Training Target Animation"
        public bool ShamSelectionFeedback;  // Sham selection feedback toggle
        public BocciaAnimation ShamSelectionAnimation;  // "Sham Selection Animation". 
        public float StimulusOnDuration;  // Stimulus on duration
        public float StimulusOffDuration;  // Stimulus off duration
        public Color FlashColour;  // Stimulus flash colour
        // Add more P300 training-specific parameters here if needed
    }

    // Nested class for P300 testing settings
    [System.Serializable]
    public class TestSettings
    {
        public int NumFlashes;  // Number of flashes during testing
        public bool TargetSelectionFeedback;  // Test target selection feedback toggle
        public BocciaAnimation TargetSelectionAnimation;  // "Target Selection Animation"
        public float StimulusOnDuration;  // Stimulus on duration
        public float StimulusOffDuration;  // Stimulus off duration
        public Color FlashColour;  // Stimulus flash colour
        // Add more P300 testing-specific parameters here if needed
    }
}
