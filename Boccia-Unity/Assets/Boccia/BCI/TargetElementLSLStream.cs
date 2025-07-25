using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;
using BCIEssentials.StimulusObjects;
using BCIEssentials.ControllerBehaviors;

public class TargetElementLSLStream : MonoBehaviour
{
    public string StreamName = "TargetElementStream";
    public string StreamId = "target_element_stream_01";

    [Header("LSL Stream")]
    private StreamOutlet _outlet;
    private readonly string _StreamType = "Text";
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

        _streamInfo = new StreamInfo(StreamName, _StreamType, 2, 0.0, LSL.channel_format_t.cf_string, StreamId);
        _outlet = new StreamOutlet(_streamInfo);

        _sample = new string[2];

        return true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            // Get and send the segment ID and selectable pool index of the target element
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

        // Get the index of this SPO in the selectable objects list - this is the same
        // index used by the P300ControllerBehavior for LSL markers during stimulus presentation
        string selectablePoolIndex = "Unknown";
        string segmentID = "Unknown";
        
        // Try to find the BCI controller to get the selectable SPO index
        var bciController = FindObjectOfType<CustomP300ControllerBehavior>();
        if (bciController != null)
        {
            var selectableSPOs = bciController.GetCameraVisibleSPOs();
            int index = selectableSPOs.IndexOf(targetSPO);
            if (index >= 0)
            {
                selectablePoolIndex = index.ToString();
            }
        }
        
        // Also try to get the FanSegmentIdentifier ID if it exists (for fan segments)
        var segmentIdentifier = targetSPO.GetComponent<FanSegmentIdentifier>();
        if (segmentIdentifier != null)
        {
            segmentID = segmentIdentifier.SegmentID.ToString();
        }
        
        _sample[0] = "SegmentID: " + segmentID;
        _sample[1] = "iSPO: " + selectablePoolIndex;

        // Send the sample to the LSL stream
        _outlet.push_sample(_sample);
        Debug.Log("Target element segment ID sent to LSL stream: " + _sample[0]);
        Debug.Log("Target element selectable pool index sent to LSL stream: " + _sample[1]);

        // Clear the target element SPO in the model
        _model.ClearTargetElement();
    }
}
