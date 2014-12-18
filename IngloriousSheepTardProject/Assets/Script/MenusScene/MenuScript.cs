using UnityEngine;
using System.Collections;

public class MenuScript : MonoBehaviour {
		public void LoadLevel(string levelName)
		{
			Application.LoadLevel(levelName);
		}
		public void ExitApplication()
		{
			ExitApplication();
		}
	
	// Update is called once per frame
	void Update () {
		}

    public void LaunchServer()
    {
        Datas.Instance.isServer = true;
        Application.LoadLevel("MainScene");
    }
    public void LaunchClient()
    {
        Datas.Instance.isServer = false;
        Application.LoadLevel("MainScene");
    }
	}
