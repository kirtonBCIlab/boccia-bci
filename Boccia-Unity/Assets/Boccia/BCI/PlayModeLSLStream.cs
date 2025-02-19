using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;

public class PlayModeLSLStream : MonoBehaviour
{
    private StreamOutlet _outlet;
    public string StreamName = "PlayModeLSLStream";
    public string StreamType = "Text";
    public string StreamId = "data_collection_01";
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
            Debug.LogWarning("Stream already initialized");
            return false;
        }

        _streamInfo = new StreamInfo(StreamName, StreamType, 2, 0.0, LSL.channel_format_t.cf_string, StreamId);
        _outlet = new StreamOutlet(_streamInfo);

        _sample = new string[2];

        return true;
    }

    private void SendData()
    {
        if (_outlet == null)
        {
            Debug.Log("Stream not initialized");
            return;
        }

        _sample[0] = _model.TargetNumber;
        _sample[1] = _model.RampLocation;
        // Debug.Log("Sample: " + _sample[0] + ", " + _sample[1]);

        _outlet.push_sample(_sample);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            // Debug.Log("Sending data: " + _model.TargetNumber + ", " + _model.RampLocation);
            SendData();
        }
    }
}
