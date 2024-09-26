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
        BocciaModel.WasChanged += ModelChanged;

        //connect buttons to model
        startButton.onClick.AddListener(model.PlayMenu);
        gameOptionsButton.onClick.AddListener(model.ShowGameOptions);
        bciOptionsButton.onClick.AddListener(model.ShowBciOptions);
        quitButton.onClick.AddListener(model.QuitGame);
    }


    private void OnDisable()
    {
        BocciaModel.WasChanged -= ModelChanged;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void ModelChanged()
    {

    }

    //These are all skeletons- the methods do not exist in BCIModel
    private void ToPlay()
    {
        // model.NavigateToPlay();
    }

    private void ToBCIOptions()
    {
        // model.NavigateToBCIOptions();
    }

    private void ToGameOptions()
    {
        //  model.NavigateToGameOptions();
    }

    private void Quit()
    {
        //model.Quit();
    }

}
