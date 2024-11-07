using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallFalling : MonoBehaviour
{
    public GameObject rampBase;

    // Start is called before the first frame update
    void Start()
    {
        if (rampBase != null)
        {
            transform.position = rampBase.transform.position;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

