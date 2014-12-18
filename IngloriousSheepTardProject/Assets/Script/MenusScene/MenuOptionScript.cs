using UnityEngine;
using System.Collections;

public class MenuOptionScript : MonoBehaviour {

    [SerializeField]
    GameObject ipField;
    [SerializeField]
    GameObject portField;



    public void LoadLevel(string levelName)
	{
		Application.LoadLevel(levelName);
	}
	public void ExitApplication()
	{
		ExitApplication();
	}

	public void setOptimalFullScreen(){
		Screen.SetResolution (Screen.currentResolution.width, Screen.currentResolution.height, true);
	}
	public void setWindowedMode(){
		Screen.SetResolution (1024, 768, false);
	}

    public void OnClickApply()
    {
        UnityEngine.UI.Text ip = (UnityEngine.UI.Text)ipField.GetComponent("Text");
        UnityEngine.UI.Text port = (UnityEngine.UI.Text)ipField.GetComponent("Text");

        int.TryParse(port.text, out Datas.Instance.port);
        Datas.Instance.serverIP = ip.text;

        LoadLevel("Menu");
    }
}
