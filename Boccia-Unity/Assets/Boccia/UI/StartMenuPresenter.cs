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
        //startButton.onClick.AddListener();
        //gameOptionsButton.onClick.AddListener();
        //bciOptionsButton.onClick.AddListener();
        //quitButton.onClick.AddListener();
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
}
