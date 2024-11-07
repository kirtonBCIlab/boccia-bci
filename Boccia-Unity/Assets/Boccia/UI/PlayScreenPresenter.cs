using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayScreenPresenter : MonoBehaviour
{
    public Button resetRampButton;
    public Button randomBallButton;

    private BocciaModel _model;

    private int _randomRotation;
    private int _randomElevation;

    // Start is called before the first frame update
    void Start()
    {
        _model = BocciaModel.Instance;

        // connect buttons to model
        resetRampButton.onClick.AddListener(_model.ResetRampPosition);
        randomBallButton.onClick.AddListener(SetRandomBallDropPosition);
        
        // TODO: see task BOC-90 for what the random ball button should do
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        if (_model == null)
        {
            _model = BocciaModel.Instance;
        }
        _model.WasChanged += ModelChanged;
    }

    void OnDisable()
    {
        _model.WasChanged -= ModelChanged;
    }

    private void ModelChanged()
    {

    }

    private void SetRandomBallDropPosition()
    {
        // Generate random values for the ball drop position
        // add +1 to max to maxe it inclusive
        // TODO: The min and max values should come from the model, not hard coded
        _randomRotation = Random.Range(-85, 85+1);
        _randomElevation = Random.Range(0, 100+1);

        _model.RandomBallDrop(_randomRotation, _randomElevation);

        StartCoroutine(WaitForStopBeforeRampReset());
    }

    private IEnumerator WaitForStopBeforeRampReset()
    {  
        while (_model.IsRampMoving)
        {
            yield return null;
        }

        _model.ResetRampPosition();

    }
}
