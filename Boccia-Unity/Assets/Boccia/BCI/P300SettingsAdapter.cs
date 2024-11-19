using BCIEssentials.ControllerBehaviors;
using BCIEssentials.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class P300SettingsAdapter : MonoBehaviour
{
    private BocciaModel _model;
    private P300ControllerBehavior _controller;

    void Start()
    {
        _model = BocciaModel.Instance;
        _model.BciChanged += BCIChanged;
        _model.NavigationChanged += NavigationChanged;
    }

    void BCIChanged()
    {
        if (!TestControllerReference())
            return;

        ApplyGeneralSettings();
    }

    void NavigationChanged()
    {
        if (!TestControllerReference())
            return;

        if (_model.CurrentScreen == BocciaScreen.TrainingScreen)
            ApplyTrainingSettings();
        else
            ApplyTestingSettings();
    }

    void ApplyGeneralSettings()
    {
        P300SettingsContainer settings = _model.P300Settings;
        _controller.numTrainWindows = settings.Train.NumTrainingWindows;
        _controller.shamFeedback = settings.Train.ShamSelectionFeedback;    
    }

    void ApplyTrainingSettings()
    {
        var settings = _model.P300Settings.Train;
        _controller.numFlashesLowerLimit = settings.NumFlashes;
        _controller.onTime = settings.StimulusOnDuration;
        _controller.offTime = settings.StimulusOffDuration;
    }

    void ApplyTestingSettings()
    {
        var settings = _model.P300Settings.Test;
        _controller.numFlashesLowerLimit = settings.NumFlashes;
        _controller.onTime = settings.StimulusOnDuration;
        _controller.offTime = settings.StimulusOffDuration;
    }

    bool TestControllerReference()
    {
        if (_controller != null)
            return true;

        _controller = BCIController.Instance.ActiveBehavior as P300ControllerBehavior;
        if (_controller != null)
        {
            ApplyGeneralSettings();
            return true;
        }
        return false;
    }
}
