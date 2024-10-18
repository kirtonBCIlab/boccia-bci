using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JackPresenter : MonoBehaviour
{
    private BocciaModel model;
    public GameObject jackBall; // The ball prefab to use as the jack

    private Vector2 xRange = new Vector2(-6f, 6f);
    private float yPosition = 0f;
    private Vector2 zRange = new Vector2(6f, 18f);

    // Start is called before the first frame update
    void Start()
    {
        model = BocciaModel.Instance;
        model.NewRandomJack += NewJack;
        model.BallResetChanged += ResetJackBall;
    }
 
    private void OnDisable()
    {
        model.NewRandomJack -= NewJack;
        model.BallResetChanged -= ResetJackBall;
    }

    private void NewJack()
    {
        GameObject currentJack = GameObject.FindWithTag("JackBall");
        if (currentJack != null)
        {
            Destroy(currentJack);
        }

        Vector3 randomJackPosition = RandomLocation();
        //Debug.Log(randomJackPosition);

        GameObject newJack = Instantiate(jackBall, transform.position + randomJackPosition, Quaternion.identity, transform);
        newJack.name = "JackBall";
    }

    private void ResetJackBall()
    {
        GameObject currentJack = GameObject.FindWithTag("JackBall");
        if (currentJack != null)
        {
            Destroy(currentJack);
        }
    }

    private Vector3 RandomLocation()
    {
        float xPosition = Random.Range(xRange.x, xRange.y);
        float zPosition = Random.Range(zRange.x, zRange.y);
        
        return new Vector3(xPosition, yPosition, zPosition);
    }

    // If the jack rolls out of bounds, reset it
    // Note: the jack should not instantiate outside the boundary, but the Boccia ball could
    // knock the jack out of bounds
    /*
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("JackBall"))
        {
            Debug.Log("Jack out of bounds");
            ResetJackBall();
        }
    }
    */
}
