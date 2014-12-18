using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManagerScript : MonoBehaviour {

	[SerializeField]
    public APlayerScript[] PlayerScript;

    bool _buildingServer;
    
    public bool BuildingServer
    {
        get { return Datas.Instance.isServer; }
        set { Datas.Instance.isServer = value; }
    }


    [SerializeField]
    int _playingTimeMax;

	[SerializeField]
    NetworkView _networkView;
    public bool isPlaying;


    private List<NetworkPlayer> players;
    private List<NetworkPlayer> playerWantToLaunch;
    private List<MonsterScript> monsters;
    private float launchTimer;

	// Use this for initialization
	void Start () {
		Application.runInBackground = true;
        monsters = new List<MonsterScript>();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Monsters")) {
            monsters.Add((MonsterScript) go.GetComponent<MonsterScript>());
        }
        Debug.Log("NB monstre : " + monsters.Count);
        if (Datas.Instance.isServer)
        {
			Network.InitializeSecurity ();
			Network.InitializeServer (Datas.Instance.maxPlayer, Datas.Instance.port, true);
            Debug.Log("Server created, waiting for players ...");
            players = new List<NetworkPlayer>();
            playerWantToLaunch = new List<NetworkPlayer>();
		} else {
            Network.Connect(Datas.Instance.serverIP, Datas.Instance.port); 
            Debug.Log("Connected");
		}
	}

    void Update() {
        int cpt = 0;
        foreach (APlayerScript ps in PlayerScript)
        {
            if (ps.timeAttack > 0)
            {
                ps.timeAttack -= Time.deltaTime;
                if (ps.timeAttack <= 0)
                {
                    //PlayerScript[cpt].stopAttack();
                    _networkView.RPC("wantToStopAttack", RPCMode.Server, players[cpt]);
                }
                
            }
            ++cpt;
        }
        if (launchTimer > 0)
        {
            launchTimer -= Time.deltaTime;
            if (launchTimer <= 0)
            {
                isPlaying = false;
                _networkView.RPC("PlaySequence", RPCMode.Others, false);
            }

        }
    }

    void attackNearest(ClassicPlayerScript aps)
    {
        float minDis = 0;
        MonsterScript nearest = null;
        int cpt =-1;
        monsters.ForEach(monst => {
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
        if (minDis <= Mathf.Pow(aps.Range, 2))
        {
            Debug.Log("POUET TOUCHER !");
            bool success = nearest.ReceiveAttack();
            if (success)
            {
                Debug.Log("POUET t'AS MAL !");
                _networkView.RPC("monsterLooseLife", RPCMode.Server, cpt, nearest.life, nearest.myself.FindChild("life").localScale);
                if (nearest.life <= 0)
                {
                    int aliveMob = monsters.Count;
                    monsters.ForEach(monst => { if (monst.life <= 0) --aliveMob; });
                    if (aliveMob <= 0)
                    {
                        _networkView.RPC("callGameOver", RPCMode.Server);
                        Application.LoadLevel("MenuVictory");
                    }

                }
            }
        }
    }

    void OnServerInitialized()
    {
        Debug.Log("Server is initialized");
    }

    void OnPlayerConnected(NetworkPlayer player)
    {
        Debug.Log("Player " + player + " connected from " + player.ipAddress + ":" + player.port);
        players.Add(player);
        if (players.Count <= 2)
        {
            for (int i = 0; i <= players.Count - 1; ++i)
            {
                GameObject go = PlayerScript[i].gameObject;
                go.SetActive(true);
            }
            _networkView.RPC("activeNewPlayer", RPCMode.Server, players.Count - 1);
        }
    }

    [RPC]
    public void callGameOver()
    {
        if (Network.isServer)
        {
            _networkView.RPC("callGameOver", RPCMode.Others);
            Application.LoadLevel("MenuVictory");
        }

        Application.LoadLevel("MenuVictory");
    }
    [RPC]
    public void activeNewPlayer(int playerI)
    {
        for (int i = 0; i <= playerI; ++i)
        {
            GameObject go = PlayerScript[i].gameObject;
            go.SetActive(true);
        }
        if (Network.isServer)
        {
            _networkView.RPC("activeNewPlayer", RPCMode.Others, playerI);
        }
    }
    [RPC]
    public void monsterLooseLife(int monsterI, int monsterLife, Vector3 monsterLifeBar)
    {
        if (Network.isServer)
        {
            _networkView.RPC("monsterLooseLife", RPCMode.Others, monsterI, monsterLife, monsterLifeBar);
        }
        monsters[monsterI].life = monsterLife;
        monsters[monsterI].myself.FindChild("life").localScale = monsterLifeBar;
    }
    [RPC]
    public void wantToAttack(NetworkPlayer player)
    {
        var playerId = int.Parse(player.ToString());
        bool isSucces =  PlayerScript[playerId - 1].tryToAttack();
        if (Network.isServer)
        {
            PlayerScript[playerId - 1].timeAttack = 3;
            _networkView.RPC("wantToAttack", RPCMode.Others, player);
            PlayerScript[playerId - 1].tryToAttack();
            if (isSucces)
            {
                Debug.Log("POUET ON ATTACK");
                attackNearest(PlayerScript[playerId - 1] as ClassicPlayerScript);
            }
        }
    }
    [RPC]
    public void wantToStopAttack(NetworkPlayer player)
    {
        Debug.Log("Server player wants to stop attack");
        var playerId = int.Parse(player.ToString());
        if (Network.isServer)
        {
            _networkView.RPC("wantToStopAttack", RPCMode.Others, player);
        }

        PlayerScript[playerId - 1].stopAttack();
    }
    [RPC]
    public void wantToMove(NetworkPlayer player, Vector3 pos)
    {
        var playerId = int.Parse(player.ToString());

        if (Network.isServer)
        {
            Debug.Log("player : " + PlayerScript[playerId - 1].transform.position + " to : " + pos);
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
            }
        }
    }
    [RPC]
    public void PlaySequence(bool launched)
    {
        isPlaying = launched;
    }
}