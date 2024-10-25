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
        model.BciChanged += BciChanged;

        // Connect buttons to model
        hamburgerButton.onClick.AddListener(model.ShowHamburgerMenu);
        playMenuButton.onClick.AddListener(model.PlayMenu);
        gameOptionsButton.onClick.AddListener(model.ShowGameOptions); // Need to change this to show game options
        bciOptionsButton.onClick.AddListener(model.ShowBciOptions);  // Need to change this to show BCI options
        quitButton.onClick.AddListener(model.QuitGame);
    }

    void OnEnable()
    {
        if (model != null)
        {
            model.BciChanged += BciChanged;
        }
    }

    void OnDisable()
    {
        if (model != null)  
        {
            model.BciChanged -= BciChanged;
        }
    }

    private void BciChanged()
    {
        if (model.IsTraining == true)
        {
            hamburgerButton.interactable = false;
        }
        else 
        {
            hamburgerButton.interactable = true;
        }
    }
}
