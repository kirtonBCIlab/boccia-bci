using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JackPresenter : MonoBehaviour
{
    private BocciaModel _model;
    public GameObject jackBall; // The ball prefab to use as the jack
    public GameObject spawnArea; // The area in which to spawn the jack

    public BocciaGameMode _gameMode;

    public event System.Action<GameObject> JackSpawned;

    // Start is called before the first frame update
    void Start()
    {
        _model = BocciaModel.Instance;
        _model.NewRandomJack += NewJack;
        _model.BallResetChanged += ResetJackBall;
        _model.NavigationChanged += NavigationChanged;

        _gameMode = _model.GameMode;
    }
 
    private void OnDisable()
    {
        _model.NewRandomJack -= NewJack;
        _model.BallResetChanged -= ResetJackBall;
        _model.NavigationChanged -= NavigationChanged;
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

        JackSpawned?.Invoke(newJack);
    }

    private void ResetJackBall()
    {
        GameObject currentJack = GameObject.FindWithTag("JackBall");
        if (currentJack != null)
        {
            Destroy(currentJack);

            // Call the method to remove tail
            _model.ResetBallTails();
        }
    }

    private void NavigationChanged()
    {
        // Reset jack if the game mode changes
        BocciaGameMode currentGameMode = _model.GameMode;
        if (currentGameMode != _gameMode)
        {
            _gameMode = currentGameMode;
            ResetJackBall();
        }
    }
}
