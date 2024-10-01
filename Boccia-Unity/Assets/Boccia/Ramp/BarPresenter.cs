using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// THIS SCRIPT HAS BEEN COMBINED INTO BallPresenter.cs

public class BarPresenter : MonoBehaviour
{
    private Animator barAnimation;
    public GameObject dropBar;

    private BocciaModel model;

    // Start is called before the first frame update
    void Start()
    {
        // cache model and subscribe for changed event
        model = BocciaModel.Instance;
        model.WasChanged += ModelChanged;

        // Initialize bar animation
        barAnimation = dropBar.GetComponent<Animator>();

        ModelChanged();
    }

    void OnDisable()
    {
        model.WasChanged -= ModelChanged;
    }

    private void ModelChanged()
    {
        if (model.BarState)
        {
             StartCoroutine(BarAnimation()); // Start the bar movement animation
        }
    }

    private IEnumerator BarAnimation()
    {
        barAnimation.SetBool("isOpening", true);

        yield return new WaitForSecondsRealtime(1f);

        barAnimation.SetBool("isOpening", false);

        yield return null;
    }

}
