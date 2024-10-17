using UnityEngine;
using UnityEngine.UI;

public class VirtualPlayPresenter : MonoBehaviour
{
    [Header("UI Elements")]
    public Button resetRampButton;
    public Button randomJackButton;

    [Header("Ramp Control Fan")]
    public FanPresenter rampControlFan;

    private BocciaModel _model;



    void Start()
    {
        _model = BocciaModel.Instance;

        // connect buttons to model
        resetRampButton.onClick.AddListener(_model.ResetRampPosition);
        randomJackButton.onClick.AddListener(_model.RandomJackBall);
    }
}





