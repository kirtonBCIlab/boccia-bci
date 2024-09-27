using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuPresenter : MonoBehaviour
{
    public Button startButton;
    public Button gameOptionsButton;
    public Button bciOptionsButton;
    public Button quitButton;

    private BocciaModel model;

    // Start is called before the first frame update
    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;

        //connect buttons to model
        startButton.onClick.AddListener(model.PlayMenu);
        gameOptionsButton.onClick.AddListener(model.ShowGameOptions);
        bciOptionsButton.onClick.AddListener(model.ShowBciOptions);
        quitButton.onClick.AddListener(model.QuitGame);
    }
}
