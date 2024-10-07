using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JackPresenter : MonoBehaviour
{
    private BocciaModel model;
    public GameObject jackBall; // The ball prefab to use as the jack

    private Vector2 xRange = new Vector2(-8f, 8f);
    private float yPosition = 0f;
    private Vector2 zRange = new Vector2(4f, 12f);

    // Start is called before the first frame update
    void Start()
    {
        model = BocciaModel.Instance;
        model.NewRandomJack += ModelChanged;
    }
 
    private void OnDisable()
    {
        model.NewRandomJack -= ModelChanged;
    }

    private void ModelChanged()
    {
        NewJack();
    }

    private void NewJack()
    {
        GameObject currentJack = GameObject.FindWithTag("JackBall");
        if (currentJack != null)
        {
            Destroy(currentJack);
        }

        Vector3 randomJackPosition = RandomLocation();
        Debug.Log(randomJackPosition);

        Instantiate(jackBall, transform.position + randomJackPosition, Quaternion.identity, transform);

    }

    private Vector3 RandomLocation()
    {
        float xPosition = Random.Range(xRange.x, xRange.y);
        float zPosition = Random.Range(zRange.x, zRange.y);
        
        return new Vector3(xPosition, yPosition, zPosition);
    }

}
