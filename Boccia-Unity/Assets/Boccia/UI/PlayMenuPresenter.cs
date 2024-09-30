using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayMenuPresenter : MonoBehaviour
{
    public Button trainingButton;
    public Button playBocciaButton;
    public Button virtualPlayButton;

    private BocciaModel model;

    // Start is called before the first frame update
    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;

        // connect buttons to model
        // Note - need to connect to real model function, start menu is just for test
        trainingButton.onClick.AddListener(model.Train);
        playBocciaButton.onClick.AddListener(model.Play);
        virtualPlayButton.onClick.AddListener(model.VirtualPlay);
    }
}
