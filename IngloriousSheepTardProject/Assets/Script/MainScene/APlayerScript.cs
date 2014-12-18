using UnityEngine;
using System.Collections;

public abstract class APlayerScript : MonoBehaviour {

    public bool isAttacking;
    public float timeAttack;
	// Use this for initialization
	void Start () {
	
	}
	


	abstract public void tryToMoveTo (Vector3 pos);

    abstract public void tryToShoot();
    abstract public bool tryToAttack();
    abstract public void stopAttack();


}
