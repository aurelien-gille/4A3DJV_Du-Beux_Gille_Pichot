using UnityEngine;
using System.Collections;

public class MenuGameOverScript : MonoBehaviour {

    public void ReturnToMenu()
    {
        Application.LoadLevel("Menu");
    }
}
