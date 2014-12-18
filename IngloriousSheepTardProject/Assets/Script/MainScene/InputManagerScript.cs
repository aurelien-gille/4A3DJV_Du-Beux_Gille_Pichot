using UnityEngine;
using System.Collections;

public class InputManagerScript : MonoBehaviour {

	[SerializeField]
	Camera _gameCamera;

	[SerializeField]
	GameManagerScript _gameManager;

	[SerializeField]
	Collider _groundCollider;

	[SerializeField]
    float _groundDistance;

    [SerializeField]
    NetworkView _networkView;

    [SerializeField]
    GameObject _PathPointPrefab;


    GameObject lastBall;
    private bool _isMoving;
    private GameObject _playerCollider;

	// Use this for initialization
	void Start () {
	    _isMoving = false;
        _playerCollider = null;
	}

    public void OnClickAttakingButton()
    {

        PointStepManager pns = (PointStepManager)lastBall.GetComponent<PointStepManager>();
        pns.isAttacking = true;
        pns.myself.Find("pointPathGraphic").renderer.material.color = new Color(0f, 0f, 0f);
    }
    public void OnClickWantToLaunch()
    {
        _networkView.RPC("wantToLaunch", RPCMode.Server, Network.player);
    }


	// Update is called once per frame
	void Update ()
    {
        if (Network.isClient)
        {
            #region IsPlaying
            if (_gameManager.isPlaying)
            {
                ClassicPlayerScript playerScript = (ClassicPlayerScript)_playerCollider.GetComponent<ClassicPlayerScript>();
                PointStepManager actualStep = playerScript.nextStep;
                Debug.Log(playerScript.isAttacking +"&" + actualStep);
                if (actualStep != null)
                {
                    if (!playerScript.isAttacking)
                    {
                        if (Mathf.Pow((_playerCollider.transform.position.x - actualStep.myself.position.x), 2) + Mathf.Pow((_playerCollider.transform.position.z - actualStep.myself.position.z), 2) > 2)
                        {
                            Debug.Log("pouet1");
                            _networkView.RPC("wantToMove", RPCMode.Server, Network.player, playerScript.nextStep.myself.position);
                        }
                        else
                        {
                            if (!actualStep.isAttacking)
                            {
                                playerScript.nextStep = actualStep.NextPoint;
                                actualStep.DestroySelf();
                                Debug.Log("pouet2");
                                _networkView.RPC("wantToMove", RPCMode.Server, Network.player, playerScript.nextStep.myself.position);
                            }
                            else
                            {
                                _networkView.RPC("wantToAttack", RPCMode.Server, Network.player);
                                playerScript.nextStep = actualStep.NextPoint;
                                actualStep.DestroySelf();

                            }
                        }
                    }
                }
            }
            #endregion
            #region IsPreparing
            if (!_gameManager.isPlaying)
            {
                if (_playerCollider == null)
                {
                    int p = int.Parse(Network.player.ToString()) - 1;
                    Debug.Log(p + "/" + _gameManager.PlayerScript.Length);
                    if (_gameManager.PlayerScript.Length >= p)
                        _playerCollider = _gameManager.PlayerScript[(int.Parse(Network.player.ToString())) - 1].gameObject;
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    _networkView.RPC("wantToShoot", RPCMode.Server, Network.player);
                    //_gameManager.wantToShoot(0);
                }
                if (Input.GetMouseButtonDown(0))
                {
                    var ray = _gameCamera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hitInfo;

                    if (_playerCollider.collider.Raycast(ray, out hitInfo, _groundDistance))
                    {
                        ClassicPlayerScript cps = (ClassicPlayerScript)_playerCollider.GetComponent<ClassicPlayerScript>();
                        if (cps.nextStep != null)
                        {
                            cps.nextStep.DestroyChild();
                            cps.nextStep.DestroySelf();
                            lastBall = null;
                        }
                        _isMoving = true;
                    }
                    else if (Physics.Raycast(ray, out hitInfo))
                    {
                        if (hitInfo.transform.name == "pointStep(Clone)") {
                            PointStepManager pns = (PointStepManager)hitInfo.transform.gameObject.GetComponent<PointStepManager>();
                            if (pns.NextPoint != null)
                            {
                                pns.NextPoint.DestroyChild();
                                pns.NextPoint.DestroySelf();
                                lastBall = hitInfo.transform.gameObject;
                            }
                            _isMoving = true;

                        }
                    }


                }
                if (Input.GetMouseButtonUp(0))
                {
                    _isMoving = false;

                }
                if (_isMoving)
                {
                    var ray = _gameCamera.ScreenPointToRay(Input.mousePosition);

                    RaycastHit hitInfo;

                    if (_groundCollider.Raycast(ray, out hitInfo, _groundDistance))
                    {
                        Vector3 lastPos;
                        if (lastBall != null)
                            lastPos = lastBall.transform.position;
                        else
                        {
                            lastPos = _playerCollider.transform.position;
                        }
                        if (Mathf.Pow((hitInfo.point.x - lastPos.x), 2) + Mathf.Pow((hitInfo.point.z - lastPos.z), 2) > 10)
                        {
                            GameObject go = (GameObject)Instantiate(_PathPointPrefab);
                            PointStepManager ns = go.GetComponent<PointStepManager>();
                            go.transform.position = hitInfo.point;
                            go.transform.parent = this.gameObject.transform.FindChild("paths");
                            if (lastBall == null) {
                                ClassicPlayerScript lns = (ClassicPlayerScript)_playerCollider.GetComponent<ClassicPlayerScript>();
                                lns.nextStep = ns;
                            }
                            else
                            {
                                PointStepManager lns = (PointStepManager)lastBall.GetComponent<PointStepManager>();
                                lns.NextPoint = ns;
                            }
                            lastBall = go;
                        }
                    }
                }
            }
            #endregion
		}
	}

}