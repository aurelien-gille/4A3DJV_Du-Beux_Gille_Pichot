using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterScript : MonoBehaviour
{
    #region Fields
    private Dictionary<ClassicPlayerScript, int> hating;
    private float maxLife;
    private Vector3 maxLifeLength;
    #endregion
    #region Properties
    [SerializeField]
    public Transform myself;
    [SerializeField]
    public NavMeshAgent Agent;
    public Vector3 ActualObjective;
    public PointStepManager nextStep;


    public float life = 15;
    public int range = 5;
    public int speed = 10;
    public int dammage = 3;
    public float timeAttack;
    public bool isAttacking;
    #endregion

    #region Public Methods
    public bool ReceiveAttack(ClassicPlayerScript from)
    {
        bool success = true;
        //success = Mathf.Floor(UnityEngine.Random.Range(0, 1)) == 0;
        if (success)
        {
            life -= from.Skill.dammage;
            hating[from] = hating[from] + ((int)from.Skill.dammage * (int)from.Skill.hates);
            myself.FindChild("life").localScale = new Vector3(maxLifeLength.x*life/maxLife, maxLifeLength.y, maxLifeLength.z);
        }
        return success;
    }

    public ClassicPlayerScript GetTarget() {
        KeyValuePair<ClassicPlayerScript, int> result = new KeyValuePair<ClassicPlayerScript,int>();
        foreach(KeyValuePair<ClassicPlayerScript, int> val in hating){
            if (result.Value == 0 || val.Value > result.Value)
                result = val;
        }
        return result.Key;
    }

    public void TryToMove(Vector3 to) 
    {
        Agent.SetDestination(to);
        Agent.Resume();
    }


    public bool tryToAttack()
    {
        isAttacking = true;
        return  Mathf.Floor(UnityEngine.Random.Range(0, 1)) == 0;
    }
    public void stopAttack()
    {
        isAttacking = false;
    }
    public void AddRage(ClassicPlayerScript from, int value)
    {
        hating[from] += value;
    }
	
    #endregion

    #region Private Methods
    void Start() {
        hating = new Dictionary<ClassicPlayerScript, int>();
        life = 15;
        range = 5;
        speed = 10;
        dammage = 3;
        maxLifeLength = myself.FindChild("life").localScale;
        maxLife = life;
        hating.Add(((ClassicPlayerScript)GameObject.Find("Player1").GetComponentInChildren<ClassicPlayerScript>()), 0);
        hating.Add(((ClassicPlayerScript)GameObject.Find("Player2").GetComponentInChildren<ClassicPlayerScript>()), 0);
        hating.Add(((ClassicPlayerScript)GameObject.Find("Player3").GetComponentInChildren<ClassicPlayerScript>()), 0);
    }
    #endregion  

}
