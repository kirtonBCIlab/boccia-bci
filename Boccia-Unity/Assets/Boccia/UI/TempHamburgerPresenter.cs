using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempStartPresenter : MonoBehaviour
{
    public Button startButton;
    public Button quitButton;

    private BocciaModel model;

    // Start is called before the first frame update
    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        // BocciaModel.WasChanged += ModelChanged;

        // connect buttons to model
        startButton.onClick.AddListener(model.StartPressed);
        quitButton.onClick.AddListener(model.QuitPressed);
    }
}
