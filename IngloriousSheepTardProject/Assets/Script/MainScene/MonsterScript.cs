using UnityEngine;
using System.Collections;

public class MonsterScript : MonoBehaviour {

    [SerializeField]
    public Transform myself;
    public int life = 15;

    void Start() { life = 15;}
    public bool ReceiveAttack()
    {
        Random a = new Random();
        bool success = Mathf.Floor(UnityEngine.Random.Range(0, 1)) == 0;
        success = true;
        if (success)
        {
            life -= 5;
            Vector3 tmp = myself.FindChild("life").localScale;
            myself.FindChild("life").localScale = new Vector3(tmp.x-1, tmp.y, tmp.z);
        }
        return success;
    }
}
