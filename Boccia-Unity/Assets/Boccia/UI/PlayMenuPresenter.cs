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
        BocciaModel.WasChanged += ModelChanged;
        
        // connect buttons to model
        //trainingButton.onClick.AddListener();
        //playBocciaButton.onClick.AddListener();
        //virtualPlayButton.onClick.AddListener();
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
