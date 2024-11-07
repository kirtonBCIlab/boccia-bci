using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallFalling : MonoBehaviour
{
    public GameObject rampBase;

    private BocciaModel _model;

    // Start is called before the first frame update
    void Start()
    {
        _model = BocciaModel.Instance;

        if (rampBase != null)
        {
            transform.position = rampBase.transform.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_model.BallState == BocciaBallState.ReadyToRelease && other.gameObject.CompareTag("BocciaBall"))
        {
            _model.HandleBallFalling();
        }
    }
}

