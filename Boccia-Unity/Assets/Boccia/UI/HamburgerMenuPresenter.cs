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
    public Button rampSetupButton;
    public Button quitButton;

    [Header("Canvas")]
    public GameObject hamburgerMenuOptions;

    private BocciaModel model;

    void Awake()
    {
        // Cache the model
        model = BocciaModel.Instance;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Subscribe for changed event
        model.BciChanged += BciChanged;

        // Connect buttons to model
        hamburgerButton.onClick.AddListener(model.ShowHamburgerMenu);
        playMenuButton.onClick.AddListener(model.PlayMenu);
        gameOptionsButton.onClick.AddListener(model.ShowGameOptions); // Need to change this to show game options
        bciOptionsButton.onClick.AddListener(model.ShowBciOptions);  // Need to change this to show BCI options
        rampSetupButton.onClick.AddListener(RampSetupClicked);
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

    private void RampSetupClicked()
    {
        // Navigate to the ramp setup screen which displays in play menu
        model.ShowRampSetup();
    }
}
