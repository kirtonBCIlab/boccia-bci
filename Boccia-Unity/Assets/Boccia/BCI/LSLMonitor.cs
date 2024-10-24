using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;
using BCIEssentials.LSLFramework;

public class LSLMonitor : MonoBehaviour
{
    private BocciaModel _model;
    private LSLMarkerStream _markerStream;
    private StreamInlet _streamInlet;

    // Start is called before the first frame update
    void Start()
    {
        // cache model
        _model = BocciaModel.Instance;

        // Initialize the LSL marker stream
        _markerStream = GetComponent<LSLMarkerStream>();
        string streamName = _markerStream.StreamName;
        string streamType = _markerStream.StreamType;
        string streamID = _markerStream.StreamId;
        LSL.StreamInfo streamInfo = new LSL.StreamInfo(streamName, streamType, 1, 0.0, LSL.channel_format_t.cf_string, streamID);

        // Initialize the stream inlet for the LSL marker stream
        _streamInlet = new StreamInlet(streamInfo);
    }

    // Update is called once per frame
    void Update()
    {
        // Place to store the stream samples
        string[] sample = new string[1];

        // Pull the stream sample
        double streamSample = _streamInlet.pull_sample(sample, 0.0f);

        // Process the stream sample
        if (streamSample != 0.0f)
        {
            // Check for the Training Complete marker
            if (sample[0].Contains("Training Complete"))
            {
                // If training is complete, update BciTrained flag
                _model.SetBciTrained();
            }
        }
    }
}
