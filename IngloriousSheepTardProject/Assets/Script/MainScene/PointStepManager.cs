using UnityEngine;
using System.Collections;

public class PointStepManager : MonoBehaviour
{

    [SerializeField]
    public PointStepManager NextPoint;

    [SerializeField]
    public Transform myself;

    public bool isAttacking;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void DestroySelf()
    {
        Destroy(myself.gameObject);
    }
    public void DestroyChild()
    {
        if (NextPoint)
        {
            NextPoint.DestroyChild();
            NextPoint.DestroySelf();
        }
    }
}
