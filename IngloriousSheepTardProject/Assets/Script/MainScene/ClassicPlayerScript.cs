using UnityEngine;
using System.Collections;

public class ClassicPlayerScript : APlayerScript {

	[SerializeField]
	public NavMeshAgent Agent;

	[SerializeField]
    Transform _transform;

	[SerializeField]
	float _shootDistance;
    [SerializeField]
    float _shootImpulse;
    [SerializeField]
    float _speed;
    [SerializeField]
    public float Range;

    public Transform myTransform
    {
        get { return _transform; }
    }

	float _squaredShootDistance;

    public Vector3 ActualObjective;
    public bool WantToPlay;
    public bool isPlaying;
    public PointStepManager nextStep;

	// Use this for initialization
	void Start () {
        ActualObjective = _transform.position;
		_squaredShootDistance = Mathf.Pow (_shootDistance, 2);
        isAttacking = false;
	}
	
	// Update is called once per frame
	void Update () {
        var step = _speed * Time.deltaTime;

        // Move our position a step closer to the target.
        //transform.position = Vector3.MoveTowards(transform.position,ActualObjective, step);
	}

    #region implemented abstract members of APlayerScript
    public override void tryToShoot()
    {
    }
    public override bool tryToAttack()
    {
        isAttacking = true;
        _transform.FindChild("Player Graphics").renderer.material.color = new Color(0f, 0f, 0f);
        Random a = new Random();
        bool success =  Mathf.Floor(UnityEngine.Random.Range(0, 1)) == 0;
        success = true; 
        return success;
    }

    public override void tryToMoveTo(Vector3 pos)
    {
        Debug.Log("moving");
        Agent.SetDestination(pos);
    }
    public override void stopAttack()
    {
        isAttacking = false;
        switch (_transform.name)
        {
            case "Player1": { _transform.FindChild("Player Graphics").renderer.material.color = new Color(0f, 0f, 255); break; }
            case "Player2": { _transform.FindChild("Player Graphics").renderer.material.color = new Color(255, 0f, 0f); break; }
            default: break;
        }

    }
	#endregion
}
