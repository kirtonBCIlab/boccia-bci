using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FanPresenter : MonoBehaviour
{
    public FanGenerator fanGenerator;

    [Header("Positioning")]
    public bool useRampPosition;
    
    private BocciaModel _model;
    private Quaternion _originalRotation;
    
    // Start is called before the first frame update
    void Start()
    {
        _model = BocciaModel.Instance; 
        _originalRotation = transform.rotation;       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setFanPositionToRamp()
    {
        float shaftOrientation = _model.GetRampOrientation();
        Quaternion newRotation = Quaternion.Euler(
            _originalRotation.eulerAngles.x,
            _originalRotation.eulerAngles.y + shaftOrientation,
            _originalRotation.eulerAngles.z
            );
        transform.rotation = newRotation;
    }


    public void GenerateFan()
    {
        if (useRampPosition) { setFanPositionToRamp(); }

        fanGenerator.DestroyFanSegments();
        fanGenerator.GenerateFanShape();
    }
}
