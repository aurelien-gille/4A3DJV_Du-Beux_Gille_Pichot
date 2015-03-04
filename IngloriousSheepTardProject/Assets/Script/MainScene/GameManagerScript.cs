using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManagerScript : MonoBehaviour
{

    #region Fields
    private Vector3 player2position;
    private Vector3 player3position;
    bool _buildingServer;
    [SerializeField]
    int _playingTimeMax;
	[SerializeField]
    NetworkView _networkView;
    private List<NetworkPlayer> players;
    private List<NetworkPlayer> playerWantToLaunch;
    private List<MonsterScript> monsters;
    private float launchTimer;
    #endregion

    #region Properties
    [SerializeField]
    GameObject _PathPointPrefab;
    [SerializeField]
    public APlayerScript[] PlayerScript;
    public bool BuildingServer
    {
        get { return Datas.Instance.isServer; }
        set { Datas.Instance.isServer = value; }
    }
    public bool isPlaying;
    #endregion

    #region Public Methods
    #region RPC
    [RPC]
    public void callGameOver(bool isVictory)
    {
        if (Network.isServer)
        {
            _networkView.RPC("callGameOver", RPCMode.Others);
            if (isVictory)
                Application.LoadLevel("MenuVictory");
            else
                Application.LoadLevel("MenuDefeat");
        }

        if (isVictory)
            Application.LoadLevel("MenuVictory");
        else
            Application.LoadLevel("MenuDefeat");
    }
    [RPC]
    public void activeNewPlayer(int playerI)
    {
        for (int i = 0; i <= playerI; ++i)
        {
            GameObject go = PlayerScript[i].gameObject;
            go.SetActive(true);
            if (i != 0)
            {
                go.transform.FindChild("CPU Graphics").gameObject.SetActive(false);
                go.transform.FindChild("Player Graphics").gameObject.SetActive(true);
                (go.GetComponent<ClassicPlayerScript>() as ClassicPlayerScript).isBot = false;
            }
        }
        if (Network.isServer)
        {
            _networkView.RPC("activeNewPlayer", RPCMode.Others, playerI);
        }
    }
    [RPC]
    public void monsterLooseLife(int monsterI, float monsterLife, Vector3 monsterLifeBar)
    {
        if (Network.isServer)
        {
            _networkView.RPC("monsterLooseLife", RPCMode.Others, monsterI, monsterLife, monsterLifeBar);
        }
        monsters[monsterI].life = monsterLife;
        monsters[monsterI].myself.FindChild("life").localScale = monsterLifeBar;
    }
    [RPC]
    public void monsterWantToAttack(int monster, bool isSucces = false)
    {
        if (Network.isServer)
        {
            monsters[monster - 1].timeAttack = 3;
            isSucces = monsters[monster - 1].tryToAttack();
            _networkView.RPC("monsterWantToAttack", RPCMode.Others, monster, isSucces);
            if (isSucces)
            {
                ClassicPlayerScript target = monsters[monster - 1].GetTarget();
                bool success = target.ReceiveAttack(monsters[monster - 1]);
                if (success)
                {

                    int cpt = 0;
                    foreach (ClassicPlayerScript i in PlayerScript)
                    {
                        if (i == target)
                            _networkView.RPC("playerLooseLife", RPCMode.Server, cpt, target.Life, target.myTransform.FindChild("life").localScale);
                        cpt++;
                    }

                    if (target.Life <= 0)
                    {
                        int alivePlayer = PlayerScript.Length;
                        foreach(ClassicPlayerScript player in  PlayerScript){ if (player.Life <= 0) --alivePlayer; };
                        if (alivePlayer <= 0)
                        {
                            _networkView.RPC("callGameOver", RPCMode.Server, false);
                            Application.LoadLevel("MenuDefeat");
                        }

                    }
                }

            }
        }
    }
    [RPC]
    public void monsterWantToStopAttack(int monster)
    {
        var monsterId = monster;
        if (Network.isServer)
        {
            _networkView.RPC("monsterWantToStopAttack", RPCMode.Others, monster);
        }

        monsters[monster -1].stopAttack();
    }
    [RPC]
    public void monsterWantToMove(int monster, Vector3 pos)
    {
        var monsterId = monster;

        if (Network.isServer)
        {
            _networkView.RPC("monsterWantToMove", RPCMode.Others, monster, pos);
        }
        monsters[monster - 1].TryToMove(pos);

    }
    [RPC]
    public void playerLooseLife(int player, float playerLife, Vector3 playerLifeBar)
    {
        if (Network.isServer)
        {
            _networkView.RPC("playerLooseLife", RPCMode.Others, player, playerLife, playerLifeBar);
        }
        (PlayerScript[player]as ClassicPlayerScript).Life = playerLife;
        (PlayerScript[player] as ClassicPlayerScript).myTransform.FindChild("life").localScale = playerLifeBar;
    }
    [RPC]
    public void wantToAttack(int player)
    {
        var playerId = player;
        bool isSucces = PlayerScript[playerId - 1].tryToAttack();
        if (Network.isServer)
        {
            PlayerScript[playerId - 1].timeAttack = 3;
            _networkView.RPC("wantToAttack", RPCMode.Others, player);
            PlayerScript[playerId - 1].tryToAttack();
            if (isSucces)
            {
                if(PlayerScript[playerId - 1].type != TypePlayer.healer)
                    attackNearest(PlayerScript[playerId - 1] as ClassicPlayerScript);
                else
                    HealNearest(PlayerScript[playerId - 1] as ClassicPlayerScript);
            }
        }
    }
    [RPC]
    public void wantToStopAttack(int player)
    {
        var playerId = player;
        if (Network.isServer)
        {
            _networkView.RPC("wantToStopAttack", RPCMode.Others, player);
        }

        PlayerScript[playerId - 1].stopAttack();
    }
    [RPC]
    public void wantToMove(int player, Vector3 pos)
    {
        var playerId = player;

        if (Network.isServer)
        {
            _networkView.RPC("wantToMove", RPCMode.Others, player, pos);
        }
        PlayerScript[playerId - 1].tryToMoveTo(pos);

    }
    [RPC]
    public void wantToLaunch(NetworkPlayer player)
    {
        if (Network.isServer)
        {
            if (!playerWantToLaunch.Contains(player))
                playerWantToLaunch.Add(player);
            if (playerWantToLaunch.Count == players.Count)
            {
                playerWantToLaunch = new List<NetworkPlayer>();
                isPlaying = true;
                launchTimer = _playingTimeMax;
                _networkView.RPC("PlaySequence", RPCMode.Others, true);
                #region Allies IA launcher
                foreach (ClassicPlayerScript ps in PlayerScript)
                {
                    if(ps.nextStep != null)
                        ps.nextStep.DestroySelf();
                    if (ps.isBot)
                    {
                        #region getnearest monster
                        float minDis = 0;
                        MonsterScript nearest = null;
                        monsters.ForEach(monst =>
                        {
                            if (monst.life > 0)
                            {
                                if (nearest == null)
                                {
                                    minDis = Mathf.Pow((monst.myself.position.x - ps.myTransform.position.x), 2) + Mathf.Pow((monst.myself.position.z - ps.myTransform.position.z), 2);
                                    nearest = monst;
                                }
                                else
                                {
                                    float tmpDist = Mathf.Pow((monst.myself.position.x - ps.myTransform.position.x), 2) + Mathf.Pow((monst.myself.position.z - ps.myTransform.position.z), 2);
                                    if (tmpDist < minDis)
                                    {
                                        minDis = tmpDist;
                                        nearest = monst;
                                    }
                                }
                            }
                        });
                        #endregion
                        switch (ps.type)
                        {
                            case TypePlayer.healer: {
                                ClassicPlayerScript nearPlayer = null;
                                foreach (APlayerScript p in PlayerScript) { 
                                    ClassicPlayerScript tmpPlayer = p as ClassicPlayerScript;
                                    if (tmpPlayer != ps && tmpPlayer.Life != tmpPlayer.Skill.life)
                                        if (tmpPlayer.isOnRange(ps.transform.gameObject))
                                            nearPlayer = tmpPlayer;
                                }
                                GameObject go = (GameObject)Instantiate(_PathPointPrefab);
                                PointStepManager ns = go.GetComponent<PointStepManager>();

                                ps.nextStep = ns;
                                if (nearPlayer)
                                {
                                    ps.nextStep.isAttacking = true;
                                    float x = (ps.transform.position.x + ((nearPlayer.transform.position.x + ps.transform.position.x) / 2)) / 2;
                                    float z = (ps.transform.position.z + ((nearest.transform.position.z + ps.transform.position.z) / 2)) / 2;
                                    ns.transform.position = new Vector3(x, 0, z);
                                }
                                else
                                {
                                    nearPlayer = (ClassicPlayerScript)ps.myTransform.gameObject.PickNearestContains(typeof(ClassicPlayerScript));
                                    float x = (nearPlayer.transform.position.x + ((nearPlayer.transform.position.x + ps.transform.position.x) / 2)) / 2;
                                    float z = (nearPlayer.transform.position.z + ((nearPlayer.transform.position.z + ps.transform.position.z) / 2)) / 2;
                                    ns.transform.position = new Vector3(x, 0, z);
                                    ps.nextStep.isAttacking = false;

                                }
                                break; }
                            default:
                                {
                                    GameObject go = (GameObject)Instantiate(_PathPointPrefab);
                                    PointStepManager ns = go.GetComponent<PointStepManager>();

                                    ps.nextStep = ns;
                                    ps.nextStep.isAttacking = true;

                                    if (minDis <= ps.Skill.range*ps.Skill.range)
                                    {
                                        float x = (ps.transform.position.x + ((nearest.transform.position.x + ps.transform.position.x) / 2)) / 2;
                                        float z = (ps.transform.position.z + ((nearest.transform.position.z + ps.transform.position.z) / 2)) / 2;
                                        ns.transform.position = new Vector3(x, 0, z);
                                    }
                                    else 
                                    {
                                        float x = (nearest.transform.position.x + ((nearest.transform.position.x + ps.transform.position.x) / 2)) / 2;
                                        float z = (nearest.transform.position.z + ((nearest.transform.position.z + ps.transform.position.z) / 2)) / 2;
                                        ns.transform.position = new Vector3(x, 0, z);
                                        ps.nextStep.isAttacking = false;
                                    }
                                    break;
                                };
                        }

                    }
                }
                #endregion

                #region Monster AI launcher
                monsters.ForEach(m => {
                    if(m.nextStep != null)
                        m.nextStep.DestroySelf();
                    ClassicPlayerScript nearest = m.GetTarget();
                    float minDis = 0;
                    minDis = Mathf.Pow((m.myself.position.x - nearest.myTransform.position.x), 2) + Mathf.Pow((m.myself.position.z - nearest.myTransform.position.z), 2);
                    
                    
                    GameObject go = (GameObject)Instantiate(_PathPointPrefab);
                    PointStepManager ns = go.GetComponent<PointStepManager>();

                    m.nextStep = ns;
                    m.nextStep.isAttacking = true;

                    if (minDis <= m.range * m.range)
                    {
                        ns.transform.position = ns.transform.position;
                    }
                    else
                    {

                        float x = (nearest.transform.position.x + ((nearest.transform.position.x + m.myself.position.x) / 2)) / 2;
                        float z = (nearest.transform.position.z + ((nearest.transform.position.z + m.myself.position.z) / 2)) / 2;
                        ns.transform.position = new Vector3(x, 0, z);
                    }

                });
                #endregion
            }
        }
    }
    [RPC]
    public void changeType( NetworkPlayer player, int type)
    {
        int cpt = 0;
        while (players[cpt] != player)
            cpt++;
        (PlayerScript[cpt] as ClassicPlayerScript).ChangeType((TypePlayer)type); ;
    }
    [RPC]
    public void PlaySequence(bool launched)
    {
        isPlaying = launched;
        
    }
    #endregion
    #endregion

    #region Private Methods
    void Start () {
		Application.runInBackground = true;
        monsters = new List<MonsterScript>();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Monsters")) {
            monsters.Add((MonsterScript) go.GetComponent<MonsterScript>());
        }
        if (Datas.Instance.isServer)
        {
			Network.InitializeSecurity ();
			Network.InitializeServer (Datas.Instance.maxPlayer, Datas.Instance.port, true);
            players = new List<NetworkPlayer>();
            playerWantToLaunch = new List<NetworkPlayer>();
		} else {
            Network.Connect(Datas.Instance.serverIP, Datas.Instance.port); 
		}
	}

    void Update()
    {
        #region Party
        if (launchTimer > 0)
        {
            launchTimer -= Time.deltaTime;
            if (launchTimer <= 0)
            {
                isPlaying = false;
                _networkView.RPC("PlaySequence", RPCMode.Others, false);
            }

        }
        #endregion
        
        #region Allies
        int cpt = 0;
        foreach (ClassicPlayerScript ps in PlayerScript)
        {
            if (!isPlaying)
                ps.Agent.Stop();
            if (ps.timeAttack > 0)
            {
                ps.timeAttack -= Time.deltaTime;
                if (ps.timeAttack <= 0)
                {
                    _networkView.RPC("wantToStopAttack", RPCMode.Server, cpt+1);
                }
            }
            ++cpt;
        }

        if (Network.isServer && isPlaying)
        {
            cpt = 0;
            foreach (ClassicPlayerScript playerScript in PlayerScript)
            {
                if (playerScript.isBot)
                {
                    PointStepManager actualStep = playerScript.nextStep;
                    if (actualStep != null)
                    {
                        if (!playerScript.isAttacking)
                        {
                            if (Mathf.Pow((playerScript.myTransform.position.x - actualStep.myself.position.x), 2) + Mathf.Pow((playerScript.myTransform.position.z - actualStep.myself.position.z), 2) > 2)
                            {
                                wantToMove(cpt + 1, playerScript.nextStep.myself.position);
                            }
                            else
                            {
                                if (!actualStep.isAttacking)
                                {
                                    playerScript.nextStep = actualStep.NextPoint;
                                    actualStep.DestroySelf();
                                    if (playerScript.nextStep != null)
                                        wantToMove(cpt + 1, playerScript.nextStep.myself.position);
                                }
                                else
                                {
                                    wantToAttack(cpt+1);
                                    playerScript.nextStep = actualStep.NextPoint;
                                    actualStep.DestroySelf();

                                }
                            }
                        }
                    }
                }
                cpt++;
            }
        }
        #endregion
        
        #region Enemies
        foreach (MonsterScript monsterScript in monsters)
        {
            if (!isPlaying)
                monsterScript.Agent.Stop();
            if (monsterScript.timeAttack > 0)
            {
                monsterScript.timeAttack -= Time.deltaTime;
                if (monsterScript.timeAttack <= 0)
                {
                    _networkView.RPC("monsterWantToStopAttack", RPCMode.Server, cpt + 1);
                }
            }
            ++cpt;
        }

        if (Network.isServer && isPlaying)
        {
            cpt = 0;
            foreach (MonsterScript monsterScript in monsters)
            {
                PointStepManager actualStep = monsterScript.nextStep;
                if (actualStep != null)
                {
                    if (!monsterScript.isAttacking)
                    {
                        if (Mathf.Pow((monsterScript.myself.position.x - actualStep.myself.position.x), 2) + Mathf.Pow((monsterScript.myself.position.z - actualStep.myself.position.z), 2) > 2)
                        {
                            monsterWantToMove(cpt + 1, monsterScript.nextStep.myself.position);
                        }
                        else
                        {
                            if (!actualStep.isAttacking)
                            {
                                monsterScript.nextStep = actualStep.NextPoint;
                                actualStep.DestroySelf();
                                monsterWantToMove(cpt + 1, monsterScript.nextStep.myself.position);
                            }
                            else
                            {
                                monsterWantToAttack(cpt + 1);
                                monsterScript.nextStep = actualStep.NextPoint;
                                actualStep.DestroySelf();

                            }
                        }
                    }
                }
                cpt++;
            }
        }
        #endregion
        
    }

    void attackNearest(ClassicPlayerScript aps)
    {
        float minDis = 0;
        MonsterScript nearest = null;
        int cpt = -1;
        monsters.ForEach(monst =>
        {
            ++cpt;
            if (monst.life > 0)
            {
                if (nearest == null)
                {
                    minDis = Mathf.Pow((monst.myself.position.x - aps.myTransform.position.x), 2) + Mathf.Pow((monst.myself.position.z - aps.myTransform.position.z), 2);
                    nearest = monst;
                }
                else
                {
                    float tmpDist = Mathf.Pow((monst.myself.position.x - aps.myTransform.position.x), 2) + Mathf.Pow((monst.myself.position.z - aps.myTransform.position.z), 2);
                    if (tmpDist < minDis)
                    {
                        minDis = tmpDist;
                        nearest = monst;
                    }
                }
            }
        });
        if (minDis <= Mathf.Pow(aps.Skill.range, 2))
        {
            bool success = nearest.ReceiveAttack(aps);
            if (success)
            {
                _networkView.RPC("monsterLooseLife", RPCMode.Server, cpt, nearest.life, nearest.myself.FindChild("life").localScale);
                if (nearest.life <= 0)
                {
                    int aliveMob = monsters.Count;
                    monsters.ForEach(monst => { if (monst.life <= 0) --aliveMob; });
                    if (aliveMob <= 0)
                    {
                        _networkView.RPC("callGameOver", RPCMode.Server, true);
                        Application.LoadLevel("MenuVictory");
                    }

                }
            }
        }
    }

    void HealNearest(ClassicPlayerScript aps)
    {
        ClassicPlayerScript maxPrior = null;
        int cpt = -1;
        int actual = -1;
        foreach(APlayerScript p in PlayerScript){
            ClassicPlayerScript player = p as ClassicPlayerScript;
            cpt++;

            Debug.Log("player" + cpt + " : " + player.Life / player.Skill.life);
            if (player.Life > 0)
            {
                float actualDist = Mathf.Pow((player.myTransform.position.x - aps.myTransform.position.x), 2) + Mathf.Pow((player.myTransform.position.z - aps.myTransform.position.z), 2);
                if (actualDist < Mathf.Pow(aps.Skill.range, 2))
                {
                    if (maxPrior == null || player.Life / player.Skill.life < maxPrior.Life / maxPrior.Skill.life)
                    {
                        actual = cpt;
                        maxPrior = player;
                    }

                }
            }
        }
        monsters.ForEach(monster => { monster.AddRage(aps, (int)(aps.Skill.heal * aps.Skill.heal)); });
        maxPrior.Life = maxPrior.Life + aps.Skill.heal <= maxPrior.Skill.life ? maxPrior.Life + aps.Skill.heal : maxPrior.Skill.life;
        Vector3 newScale = new Vector3(maxPrior.MaxLifeLength.x * maxPrior.Life / maxPrior.Skill.life, maxPrior.MaxLifeLength.y, maxPrior.MaxLifeLength.z);
        _networkView.RPC("playerLooseLife", RPCMode.Server, actual, maxPrior.Life, newScale);
    }

    void OnServerInitialized()
    {
    }

    void OnPlayerConnected(NetworkPlayer player)
    {
        players.Add(player);
        if (players.Count <= Datas.Instance.maxPlayer)
        {
            for (int i = 0; i <= players.Count - 1; ++i)
            {
                GameObject go = PlayerScript[i].gameObject;
                go.SetActive(true);
                if (i != 0)
                {
                    go.transform.FindChild("CPU Graphics").gameObject.SetActive(false);
                    go.transform.FindChild("Player Graphics").gameObject.SetActive(true);
                }
                (go.GetComponent<ClassicPlayerScript>() as ClassicPlayerScript).isBot = false;
                Debug.Log(
                    (go.GetComponent<ClassicPlayerScript>() as ClassicPlayerScript).Life +" sur "+
                    (go.GetComponent<ClassicPlayerScript>() as ClassicPlayerScript).Skill.life
                );
            }
            _networkView.RPC("activeNewPlayer", RPCMode.Server, players.Count - 1);
        }
    }

    void OnConnectedToServer()
    {
        _networkView.RPC("changeType", RPCMode.Server, Network.player, (int)Datas.Instance.selectedType);
    }

    #endregion

	// Use this for initialization

}