using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayMenuPresenter : MonoBehaviour
{
    public Button trainingButton;
    public Button playBocciaButton;
    public Button virtualPlayButton;

    private BocciaModel _model;

    // Start is called before the first frame update
    void Start()
    {
        // cache model and subscribe for changed event
        _model = BocciaModel.Instance;
        _model.BciChanged += BciChanged;

        // connect buttons to model
        // Note - need to connect to real model function, start menu is just for test
        trainingButton.onClick.AddListener(_model.Train);
        playBocciaButton.onClick.AddListener(_model.ShowRampSetup);
        virtualPlayButton.onClick.AddListener(_model.VirtualPlay);

        // Ensure Play Boccia and Virtual Play buttons cannot be clicked yet
        // TODO: Uncomment this when done development
        // playBocciaButton.interactable = false;
        // virtualPlayButton.interactable = false;
    }

    private void BciChanged()
    {
        // Turn on Play and Virtual Play buttons only when training is complete
        // TODO: Uncomment this when done development
        // if (_model.BciTrained == true)
        // {
        //     playBocciaButton.interactable = true;
        //     virtualPlayButton.interactable = true;
        // }

        // else 
        // {
        //     playBocciaButton.interactable = false;
        //     virtualPlayButton.interactable = false;
        // }
    }
}
