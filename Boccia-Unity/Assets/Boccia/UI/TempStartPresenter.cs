using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempHamburgerPresenter : MonoBehaviour
{
    public Button playButton;
    public Button quitButton;
    public Button backButton;

    private BocciaModel model;

    // Start is called before the first frame update
    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        // BocciaModel.WasChanged += ModelChanged;

        // connect buttons to model
        playButton.onClick.AddListener(model.PlayMenuPressed);
        quitButton.onClick.AddListener(model.QuitPressed);
        backButton.onClick.AddListener(model.BackPressed);
    }
}
