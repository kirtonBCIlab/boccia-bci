using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;
using BCIEssentials.StimulusObjects;

public class TargetElementLSLStream : MonoBehaviour
{
    [Header("LSL Stream")]
    private StreamOutlet _outlet;
    public string StreamName = "TargetElementStream";
    private string StreamType = "Text";
    public string StreamId = "target_element_stream_01";
    private StreamInfo _streamInfo;
    private string[] _sample;
    private BocciaModel _model;
    // Start is called before the first frame update
    void Start()
    {
        // cache model
        _model = BocciaModel.Instance;

        if (_outlet == null)
        {
            InitializeStream();
        }   
    }

    private bool InitializeStream()
    {
        if (_outlet != null)
        {
            return false;
        }

        _streamInfo = new StreamInfo(StreamName, StreamType, 2, 0.0, LSL.channel_format_t.cf_string, StreamId);
        _outlet = new StreamOutlet(_streamInfo);

        _sample = new string[2];

        return true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            // Get and send the object ID of the target element when stimulus starts
            SendTargetElementObjectID();
        }
    }

    private void SendTargetElementObjectID()
    {
        if (_outlet == null)
        {
            Debug.Log("Stream not initialized");
            return;
        }

        // Get the target element SPO currently stored in the model
        SPO targetSPO = _model.TargetElementSPO;

        if (targetSPO == null)
        {
            return;
        }

        // Make sure to wait for the object ID to be set
        float timer = 0;
        while (targetSPO.ObjectID == -100 && timer < 5)
        {
            timer += Time.deltaTime;
        }
        if (targetSPO.ObjectID == -100)
        {
            // Show warning if object ID still not set after 5 seconds
            Debug.LogWarning("Object ID not set yet.");
        }

        // Get the object ID and selectable pool index of the target element
        string objectID = targetSPO.ObjectID.ToString();
        string selectablePoolIndex = targetSPO.SelectablePoolIndex.ToString();
        
        _sample[0] = "ObjectID: " + objectID;
        _sample[1] = "iSPO: " + selectablePoolIndex;

        // Send the sample to the LSL stream
        _outlet.push_sample(_sample);
        // Debug.Log("Target element object ID sent to LSL stream: " + _sample[0]);
        // Debug.Log("Target element selectable pool index sent to LSL stream: " + _sample[1]);

        // Clear the target element SPO in the model
        _model.ClearTargetElement();
    }
}
