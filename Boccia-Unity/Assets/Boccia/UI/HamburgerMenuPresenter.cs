using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HamburgerMenuPresenter : MonoBehaviour
{
    [Header("Buttons")]
    public Button hamburgerButton;
    public Button playMenuButton;
    public Button gameOptionsButton;
    public Button bciOptionsButton;
    public Button quitButton;

    [Header("Canvas")]
    public GameObject hamburgerMenuOptions;

    private BocciaModel model;

    // Start is called before the first frame update
    void Start()
    {
        // Cache model and subscribe for changed event
        model = BocciaModel.Instance;
        BocciaModel.WasChanged += ModelChanged;

        // Connect buttons to model
        // TODO: connect to real model functions
        hamburgerButton.onClick.AddListener(model.ShowHamburgerMenu);
        playMenuButton.onClick.AddListener(model.ShowPlayMenu);
        gameOptionsButton.onClick.AddListener(model.ShowStartMenu); // Need to change this to show game options
        bciOptionsButton.onClick.AddListener(model.ShowStartMenu);  // Need to change this to show BCI options
        quitButton.onClick.AddListener(model.QuitGame);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ModelChanged()
    {

    }
}
