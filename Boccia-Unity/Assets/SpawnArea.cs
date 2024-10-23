using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnArea : MonoBehaviour
{

    [SerializeField] private Vector3 center;
    [SerializeField] private Vector3 size;

    // Update is called once per frame

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.1f);
        Gizmos.DrawCube(center, size);
       
    }

    public Vector3 ReturnRandomPosition()
    {
        Vector3 pos = center + new Vector3(Random.Range(-size.x / 2, size.x / 2), 0, Random.Range(-size.z / 2, size.z / 2));
        Debug.Log(pos);

        return pos;
    }
}
