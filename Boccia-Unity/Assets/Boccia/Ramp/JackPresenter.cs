using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JackPresenter : MonoBehaviour
{
    private BocciaModel model;
    public GameObject jackBall; // The ball prefab to use as the jack
    public GameObject spawnArea; // The area in which to spawn the jack

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

        if (spawnArea == null)
        {
            spawnArea = GameObject.FindWithTag("JackSpawnArea");
        }

        Vector3 randomJackPosition = spawnArea.GetComponent<SpawnArea>().ReturnRandomPosition();
        //Debug.Log(randomJackPosition);

        GameObject newJack = Instantiate(jackBall, randomJackPosition, Quaternion.identity, transform);
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
}
