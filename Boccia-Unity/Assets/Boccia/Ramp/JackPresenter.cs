using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JackPresenter : MonoBehaviour
{
    private BocciaModel model;
    public GameObject jackBall; // The ball prefab to use as the jack
    public GameObject bocciaCourt;

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
        Instantiate(jackBall, model.jackPosition, Quaternion.identity, bocciaCourt.transform);
    }

}
