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
        _model = BocciaModel.Instance;
        
        _markerStream = GetComponent<LSLMarkerStream>();
        string streamName = _markerStream.StreamName;
        string streamType = _markerStream.StreamType;
        string streamID = _markerStream.StreamId;
        LSL.StreamInfo streamInfo = new LSL.StreamInfo(streamName, streamType, 1, 0.0, LSL.channel_format_t.cf_string, streamID);

        _streamInlet = new StreamInlet(streamInfo);
    }

    // Update is called once per frame
    void Update()
    {
        string[] sample = new string[1];
        double timestamp = _streamInlet.pull_sample(sample, 0.0f);

        if (timestamp != 0.0f)
        {
            if (sample[0].Contains("Training Complete"))
            {
                _model.SetBciTrained();
            }
        }
    }
}
