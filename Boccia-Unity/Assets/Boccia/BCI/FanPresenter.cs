using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FanPresenter : MonoBehaviour
{
    public FanGenerator fanGenerator;

    [Header("Fan Parameters")]
    public float theta;         // Angle in degrees
    public float r1;            // Inner radius
    public float r2;            // Outer radius
    public float columnSpacing; // Spacing between columns
    public float rowSpacing;    // Spacing between rows;
    public int nColumns;        // Number of columns
    public int nRows;           // Number of rows
    
    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateFan()
    {
        fanGenerator.theta = theta;
        fanGenerator.r1 = r1;
        fanGenerator.r2 = r2;
        fanGenerator.columnSpacing = columnSpacing;
        fanGenerator.rowSpacing = rowSpacing;
        fanGenerator.nColumns = nColumns;
        fanGenerator.nRows = nRows;
        fanGenerator.GenerateFanShape();
    }
}
