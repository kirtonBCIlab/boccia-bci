using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayScreenPresenter : MonoBehaviour
{
    public Button resetRampButton;
    //public Button randomBallButton;

    private BocciaModel _model;

    // Start is called before the first frame update
    void Start()
    {
        _model = BocciaModel.Instance;
        _model.WasChanged += ModelChanged;

        // connect buttons to model
        resetRampButton.onClick.AddListener(_model.ResetRampPosition);
        // TODO: see task BOC-90 for what the random ball button should do
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ModelChanged()
    {

    }
}
