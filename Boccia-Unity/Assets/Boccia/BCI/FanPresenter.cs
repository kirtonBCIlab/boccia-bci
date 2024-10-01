using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FanPresenter : MonoBehaviour
{
    public FanGenerator fanGenerator;

    [Header("Positioning")]
    public bool useRampPosition;
    
    private BocciaModel model;
    
    // Start is called before the first frame update
    void Start()
    {
        model = BocciaModel.Instance;        

        if (useRampPosition) { setFanPositionToRamp(); } 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setFanPositionToRamp()
    {
        float shaftOrientation = model.GetRampOrientation();
        Quaternion currentRotation = transform.rotation;
        Quaternion newRotation = Quaternion.Euler(currentRotation.eulerAngles.x, shaftOrientation-90, currentRotation.eulerAngles.z);
        transform.rotation = newRotation;
    }


    public void GenerateFan()
    {
        fanGenerator.GenerateFanShape();
    }
}
