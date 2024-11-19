using BCIEssentials.StimulusEffects;
using BCIEssentials.StimulusObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashingSPOButtonManager : MonoBehaviour
{
    public enum StimulusCategoryType { Training, Testing }

    public GameObject[] Buttons;
    public StimulusCategoryType StimulusCategory;

    private BocciaModel _model;
    private List<ColourFlashStimulusEffect> _flashEffects;

    void Start()
    {
        _model = BocciaModel.Instance;
        _model.BciChanged += BCIChanged;
        _flashEffects = new();

        foreach (GameObject button in Buttons)
            InitializeButton(button);

        ApplyFlashColourSetting();
    }

    void InitializeButton(GameObject button)
    {
        // Add Flashing effects component
        ColourFlashStimulusEffect flashEffect = button.AddComponent<ColourFlashStimulusEffect>();
        _flashEffects.Add(flashEffect);

        // Add SPO component to make segment selectable with BCI
        button.tag = "BCI";
        SPO spo = button.AddComponent<SPO>();
        spo.Selectable = true;

        spo.StartStimulusEvent.AddListener(flashEffect.SetOn);
        spo.StopStimulusEvent.AddListener(flashEffect.SetOff);

        spo.OnSelectedEvent.AddListener(flashEffect.Play);

        if (StimulusCategory == StimulusCategoryType.Training)
        {
            spo.StartTrainingStimulusEvent.AddListener(spo.OnTrainTarget);
            spo.StopTrainingStimulusEvent.AddListener(spo.OffTrainTarget);
        }
    }

    void BCIChanged()
    {
        ApplyFlashColourSetting();
    }

    void ApplyFlashColourSetting()
    {
        Color flashColour = (StimulusCategory == StimulusCategoryType.Training) ?
            _model.P300Settings.Train.FlashColour : _model.P300Settings.Test.FlashColour;
        
        foreach (ColourFlashStimulusEffect flashEffect in _flashEffects)
            flashEffect.OnColor = flashColour;
    }
}
