using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TailPresenter : MonoBehaviour
{
    private BocciaModel _model;

    [Header("References")]
    public GameObject tailPrefab;
    public Material ballTailMaterial;
    public Material jackTailMaterial;

    [Header("Spawn Parameters")]
    [Tooltip("Seconds after ball is released to spawn a tail on it")]
    public float tailSpawnDelay = 3.0f;
    [Tooltip("Length in seconds of a tail's spawn animation")]
    public float tailGrowPeriod = 1.0f;

    private AttachedTail _jackTail = null;
    private List<AttachedTail> _activeTails = new();

    private AttachedTail _lastSpawnedBallTail = null;
    private GameObject _activeBall = null;


    void Start()
    {
        _model = BocciaModel.Instance;
        GetComponent<BallPresenter>().BallSpawned += BallSpawned;
        GetComponent<BallPresenter>().BallDropped += BallDropped;
        GetComponent<JackPresenter>().JackSpawned += JackSpawned;
        _model.WasChanged += ModelChanged;
        _model.BallResetChanged += ResetTails;
    }


    void Update()
    {
        _jackTail?.Update();
        foreach (AttachedTail tail in _activeTails)
            tail.Update();
    }

    void BallSpawned(GameObject newBall)
    {
        _activeBall = newBall;
    }

    void BallDropped(GameObject droppedBall)
    {
        _activeBall = droppedBall;
        if (droppedBall)
            StartCoroutine(SpawnBallTailAfterDelay(droppedBall));
    }

    void JackSpawned(GameObject newJack)
    {
        if (newJack != null)
        {
            if (_jackTail == null)
                _jackTail = SpawnTail(newJack, jackTailMaterial);
            else
                _jackTail.SetTarget(newJack);
        }
    }

    void ModelChanged()
    {
        if (_lastSpawnedBallTail != null && _lastSpawnedBallTail.IsTargetting(_activeBall))
        {
            Color ballColour = _model.GetCurrentBallColor();
            _lastSpawnedBallTail.SetColour(ballColour);
        }
    }

    void ResetTails()
    {
        foreach (AttachedTail tail in _activeTails)
            tail.DestroyTail();
        _activeTails.Clear();

        if (_jackTail != null)
        {
            _jackTail.DestroyTail();
            _jackTail = null;
        }

        _activeBall = null;
        _lastSpawnedBallTail = null;
    }

    private IEnumerator SpawnBallTailAfterDelay(GameObject target)
    {
        Color ballColour = _model.GetCurrentBallColor();
        yield return new WaitForSecondsRealtime(tailSpawnDelay);
        if (target != null)
            SpawnColouredTail(target, ballTailMaterial, ballColour);
    }

    AttachedTail SpawnColouredTail(GameObject target, Material tailMaterial, Color colour)
    {
        AttachedTail newTail = SpawnTail(target, tailMaterial);
        newTail.SetColour(colour);
        _lastSpawnedBallTail = newTail;
        return newTail;
    }

    AttachedTail SpawnTail(GameObject target, Material tailMaterial)
    {
        AttachedTail newTail = new(tailPrefab, target, tailMaterial);
        newTail.SetParent(transform);
        newTail.StartSpawnAnimation(tailGrowPeriod);
        _activeTails.Add(newTail);
        return newTail;
    }

    class AttachedTail
    {
        private GameObject _tailObject;
        private Rigidbody _linkedBody;

        private Material _instancedTailMaterial;

        private float _lifetime = 0;
        private float _growPeriod = 0;
        private float _finalHeight;


        public AttachedTail(GameObject prefab, GameObject target, Material sourceMaterial)
        {
            _tailObject = Object.Instantiate(prefab, target.transform.position, Quaternion.identity);
            SetTarget(target);

            MeshRenderer tailRenderer = _tailObject.GetComponent<MeshRenderer>();
            tailRenderer.material = sourceMaterial;
            _instancedTailMaterial = tailRenderer.material;

            _finalHeight = _instancedTailMaterial.GetFloat("_Height");
            _instancedTailMaterial.SetFloat("_Height", 0);
        }

        public void Update()
        {
            if (_lifetime < _growPeriod)
                ProcessAnimation();
            MoveToBall();
        }

        public void StartSpawnAnimation(float period)
        {
            _lifetime = 0;
            _growPeriod = period;
            _instancedTailMaterial.SetFloat("_Height", 0);
        }

        void ProcessAnimation()
        {
            _lifetime += Time.deltaTime;
            float normalizedAnimationTime = (_lifetime / _growPeriod);
            normalizedAnimationTime = Mathf.Clamp01(normalizedAnimationTime);

            float height = _finalHeight * Mathf.Sin(normalizedAnimationTime * Mathf.PI / 2);
            _instancedTailMaterial.SetFloat("_Height", height);
        }

        void MoveToBall()
        {
            if (_linkedBody != null)
            {
                _tailObject.transform.position = _linkedBody.worldCenterOfMass;
            }
        }

        public void SetParent(Transform parent)
        {
            _tailObject.transform.parent = parent;
        }

        public void SetTarget(GameObject newTarget)
        {
            _linkedBody = newTarget.GetComponent<Rigidbody>();
            MoveToBall();
        }

        public bool IsTargetting(GameObject target)
        {
            return target != null && target == _linkedBody.gameObject;
        }

        public void SetColour(Color colour)
        {
            _instancedTailMaterial.SetColor("_BottomColour", colour);
        }

        public void DestroyTail()
        {
            Destroy(_tailObject);
        }
    }
}
