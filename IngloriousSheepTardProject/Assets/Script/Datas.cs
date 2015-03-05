using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Datas {
    #region Properties
    public static Datas Instance = new Datas();
    public bool isServer = true;
    public string serverIP = "127.0.0.1";
    public int port = 4430;
    public int maxPlayer = 3;
    public TypePlayer selectedType = TypePlayer.dps;
    #endregion

    #region Private Methods
    private Datas() { }
    #endregion
}

public static class ToolBox {
    #region Public Methods
    public static GameObject PickNearest(this GameObject from, System.Type typeOf)
    {
        GameObject result = null;
        float actualDist = 0;
        foreach (GameObject o in GameObject.FindObjectsOfType(typeOf))
        {
            if (o != from)
            {
                float tmpDist = Mathf.Pow((o.transform.position.x - from.transform.position.x), 2) + Mathf.Pow((o.transform.position.y - from.transform.position.y), 2);
                if (result == null || actualDist > tmpDist)
                {
                    result = o;
                    actualDist = tmpDist;
                }
            }
        }
        return result;
    }
    public static Component PickNearestContains(this GameObject from, System.Type typeOf)
    {
        GameObject result = null;
        float actualDist = 0;
        foreach (GameObject o in GameObject.FindObjectsOfType(typeof(GameObject)))
        {
            if (o != from && o.GetComponent(typeOf.ToString()) != null)
            {
                float tmpDist = Mathf.Pow((o.transform.position.x - from.transform.position.x), 2) + Mathf.Pow((o.transform.position.y - from.transform.position.y), 2);
                if (result == null || actualDist > tmpDist)
                {
                    result = o;
                    actualDist = tmpDist;
                }
            }
        }
        return result.GetComponent(typeOf.ToString());
    }
    public static bool isInRange(this GameObject from, GameObject o, float range)
    {
        bool result = false;
        float tmpDist = Mathf.Pow((o.transform.position.x - from.transform.position.x), 2) + Mathf.Pow((o.transform.position.y - from.transform.position.y), 2);
        if (tmpDist <= range * range)
            result = true;
        return result;
    }

    public static void FadeIn(this GameObject from, float seconds, bool DestroyAfterFade)
    {
        throw new NotImplementedException("The requested feature is not implemented.");
    }
    #endregion
}

public enum TypePlayer {
    dps, 
    tank,
    healer
}
public class Skills {
    public float dammage, hates, heal, life, range;
    public static Skills dps = new Skills(8,7,0,10, 15);
    public static Skills tank = new Skills(2, 10, 0, 20, 5);
    public static Skills healer = new Skills(0, 8, 5, 10, 15);
    
    private Skills(){}
    private Skills(float dam, float hat, float hea, float lif, float ran)
    {
        dammage = dam;
        hates = hat;
        heal = hea;
        life = lif;
        range = ran;
    }
}
