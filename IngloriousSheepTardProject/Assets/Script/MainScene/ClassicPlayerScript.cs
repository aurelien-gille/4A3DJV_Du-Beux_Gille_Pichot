using UnityEngine;
using System.Collections;

public class ClassicPlayerScript : APlayerScript
{

    #region Fields
    [SerializeField]
    Transform _transform;

    [SerializeField]
    float _speed;
    #endregion

    #region Properties
    [SerializeField]
    public NavMeshAgent Agent;
    [SerializeField]
    public float Range;
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
    public bool ReceiveAttack(MonsterScript from)
    {
        bool success = Mathf.Floor(UnityEngine.Random.Range(0, 1)) == 0;
        success = true;
        if (success)
        {
            Skill.life -= from.dammage;
            Vector3 tmp = myTransform.FindChild("life").localScale;
            myTransform.FindChild("life").localScale = new Vector3(tmp.x - 1, tmp.y, tmp.z);
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

        bool success =  Mathf.Floor(UnityEngine.Random.Range(0, 1)) == 0;
        success = true; 
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
	#endregion

    #region Private Methods
    // Use this for initialization
    void Start()
    {
        ActualObjective = _transform.position;
        isAttacking = false;
        isBot = true;
        switch (type)
        {
            case TypePlayer.dps: { this.Skill = Skills.dps; break; }
            case TypePlayer.healer: { this.Skill = Skills.healer; break; }
            case TypePlayer.tank: { this.Skill = Skills.tank; break; }
            default : break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        var step = _speed * Time.deltaTime;
    }
    #endregion





}
