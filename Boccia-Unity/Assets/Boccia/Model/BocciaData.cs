using System;
using UnityEngine;

public enum BocciaBciParadigm
{
    P300,
    // Future paradigms can be added here
    // SSVEP,
    // MI
}

public enum BocciaAnimation
{
    Default,
}

// BocciaData contains state used by BocciaModel.  This is just a dumb container
// that's easy to serialize to/from disk.  Busines logic should go in the model.
[System.Serializable]
public class BocciaData
{
    public bool WasInitialized;

    // Game
    public Color BallColor;
    public float ElevationPrecision;
    public float ElevationRange;
    public float ElevationSpeed;
    public float RotationPrecision;
    public float RotationRange;
    public float RotationSpeed;

    // hotkeys?

    // Active BCI Paradigm for switching between paradigms
    public BocciaBciParadigm Paradigm;  // Tracks the currently active paradigm

    // public int NumFlashes;
    // public int NumTrainingWindows;
    // public BocciaAnimation TargetAnimation;
    // public bool ShamFeedback;
    // public Double StimulusOnDuration;
    // public Double StimulusOffDuration;

    // // bci testing?

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
    public string SerialPortName;
}

/// The P300SettingsContainer class contains training and testing settings specific to the P300 paradigm.
[System.Serializable]
public class P300SettingsContainer
{
    public TrainSettings Train = new TrainSettings();  // Settings related to training
    public TestSettings Test = new TestSettings();    // Settings related to testing

    // Nested class for P300 training settings
    [System.Serializable]
    public class TrainSettings
    {
        public int NumFlashes;  // Number of flashes during training
        public int NumTrainingWindows;  // Number of training windows
        public BocciaAnimation TargetAnimation;  // "Training Target Animation"
        public bool ShamSelectionFeedback;  // Sham selection feedback toggle
        public BocciaAnimation ShamSelectionAnimation;  // "Sham Selection Animation". 
        public double StimulusOnDuration;  // Stimulus on duration
        public double StimulusOffDuration;  // Stimulus off duration
        public Color FlashColour;
        // Add more P300 training-specific parameters here if needed
    }

    // Nested class for P300 testing settings
    [System.Serializable]
    public class TestSettings
    {
        public int NumFlashes;  // Number of flashes during testing
        public bool TargetSelectionFeedback;  // Test target selection feedback toggle
        public BocciaAnimation TargetSelectionAnimation;  // "Target Selection Animation"
        public double StimulusOnDuration;  // Stimulus on duration
        public double StimulusOffDuration;  // Stimulus off duration
        public Color FlashColour;
        // Add more P300 testing-specific parameters here if needed
    }
}


// Example of SSVEP Settings class
// [System.Serializable]
// public class SSVEPSettings
// {
//     public TrainSettings Train = new TrainSettings();  // Training settings for SSVEP
//     public TestSettings Test = new TestSettings();    // Testing settings for SSVEP

//     // Nested class for SSVEP training settings
//     [System.Serializable]
//     public class TrainSettings
//     {
//         public int NumStimulations;  // Number of stimulations during training
//         public int NumTrainingWindows;  // Number of training windows
//         public double Frequency;  // Frequency for the SSVEP stimulus
//         // Other training-specific parameters
//     }

//     // Nested class for SSVEP testing settings
//     [System.Serializable]
//     public class TestSettings
//     {
//         public int NumStimulations;  // Number of stimulations during testing
//         public double Frequency;  // Frequency for the SSVEP stimulus
//         // Other testing-specific parameters
//     }
// }
