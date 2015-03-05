using UnityEngine;
using System.Collections;

public class ClassicPlayerScript : APlayerScript
{

    #region Fields
    [SerializeField]
    Transform _transform;
    [SerializeField]
    GameObject _SkinDamage;
    [SerializeField]
    GameObject _SkinTank;
    [SerializeField]
    GameObject _SkinHeal;

    [SerializeField]
    float _speed;
    Vector3 maxLifeLength;

    #endregion

    #region Properties
    public Vector3 MaxLifeLength
    {
        get { return maxLifeLength; }
    }
    [SerializeField]
    public NavMeshAgent Agent;
    [SerializeField]
    public float Range;
    public float Life;
    public Skills Skill;
    public Transform myTransform
    {
        get { return _transform; }
    }
    public Vector3 ActualObjective;
    public bool WantToPlay;
    public bool isPlaying;
    public bool isBot;
    public PointStepManager nextStep;
    #endregion

    #region Public Methods

    public bool isOnRange(GameObject o)
    {
        bool result = false;
        float tmpDist = Mathf.Pow((o.transform.position.x - myTransform.position.x), 2) + Mathf.Pow((o.transform.position.y - myTransform.position.y), 2);
        if (tmpDist <= Range * Range)
            result = true;
        return result;
    }

    public bool ReceiveAttack(MonsterScript from)
    {
        bool success = Mathf.Floor(UnityEngine.Random.Range(0, 1)) == 0;
        success = true;
        if (success)
        {
            Life -= from.dammage;
            myTransform.FindChild("life").localScale = new Vector3(MaxLifeLength.x*Life/Skill.life, MaxLifeLength.y, MaxLifeLength.z);
        }
        return success;
    }
    public override bool tryToAttack()
    {
        isAttacking = true;
        Transform tmpchild = _transform.FindChild("Player Graphics");
        if (tmpchild == null || !tmpchild.gameObject.activeSelf)
            _transform.FindChild("CPU Graphics");

        tmpchild.renderer.material.color = new Color(0f, 0f, 0f);

        bool success = true;
        //success =  Mathf.Floor(UnityEngine.Random.Range(0, 1)) == 0; 
        return success;
    }

    public override void tryToMoveTo(Vector3 pos)
    {
        Agent.SetDestination(pos);
    }
    public override void stopAttack()
    {
        isAttacking = false;
        switch (_transform.name)
        {
            case "Player1":
                {
                    Transform tmpchild = _transform.FindChild("Player Graphics");
                    if (tmpchild == null || !tmpchild.gameObject.activeSelf)
                        _transform.FindChild("CPU Graphics");

                    tmpchild.renderer.material.color = new Color(0f, 0f, 255);
                    break;
                }
            case "Player2":
                {
                    Transform tmpchild = _transform.FindChild("Player Graphics");
                    if (tmpchild == null || !tmpchild.gameObject.activeSelf)
                        _transform.FindChild("CPU Graphics");

                    tmpchild.renderer.material.color = new Color(255, 0f, 0f);
                    break;
                }
            case "Player3":
                {
                    Transform tmpchild = _transform.FindChild("Player Graphics");
                    if (tmpchild == null || !tmpchild.gameObject.activeSelf)
                        _transform.FindChild("CPU Graphics");

                    tmpchild.renderer.material.color = new Color(0f, 255, 0f);
                    break;
                }
            default: break;
        }

    }
    public void ChangeType(TypePlayer newType)
    {
        Debug.Log(newType);
        type = newType;
        this._SkinDamage.SetActive(false);
        this._SkinHeal.SetActive(false);
        this._SkinTank.SetActive(false);
        switch (type)
        {
            case TypePlayer.dps: { this.Skill = Skills.dps; this.Life = Skills.dps.life; this._SkinDamage.SetActive(true); break; }
            case TypePlayer.healer: { this.Skill = Skills.healer; this.Life = Skills.healer.life; this._SkinHeal.SetActive(true); break; }
            case TypePlayer.tank: { this.Skill = Skills.tank; this.Life = Skills.tank.life; this._SkinTank.SetActive(true); break; }
            default: break;
        }
        Range = Skill.range;
        myTransform.FindChild("life").localScale = new Vector3(MaxLifeLength.x * Life / Skill.life, MaxLifeLength.y, MaxLifeLength.z);
    }
	#endregion

    #region Private Methods
    // Use this for initialization
    void Start()
    {

        ActualObjective = _transform.position;
        isAttacking = false;
        isBot = true;
        maxLifeLength= myTransform.FindChild("life").localScale;
        switch (type)
        {
            case TypePlayer.dps: { this.Skill = Skills.dps; this.Life = Skills.dps.life;  break; }
            case TypePlayer.healer: { this.Skill = Skills.healer; this.Life = Skills.healer.life; break; }
            case TypePlayer.tank: { this.Skill = Skills.tank; this.Life = Skills.tank.life; break; }
            default : break;
        }
        Range = Skill.range;
        myTransform.FindChild("life").localScale = new Vector3(MaxLifeLength.x * Life / Skill.life, MaxLifeLength.y, MaxLifeLength.z);

    }

    // Update is called once per frame
    void Update()
    {
        var step = _speed * Time.deltaTime;
    }
    #endregion





}
