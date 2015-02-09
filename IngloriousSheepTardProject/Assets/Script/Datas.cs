using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Datas {
    #region Properties
    public static Datas Instance = new Datas();
    public bool isServer = true;
    public string serverIP = "127.0.0.1";
    public int port = 4430;
    public int maxPlayer = 3;
    #endregion

    #region Private Methods
    private Datas() {}
    #endregion
}

public enum TypePlayer {
    dps, 
    tank,
    healer
}
public class Skills {
    public int dammage, hates, heal, life, range;
    public static Skills dps = new Skills(8,7,0,10, 15);
    public static Skills tank = new Skills(2, 10, 0, 20, 5);
    public static Skills healer = new Skills(0, 8, 5, 10, 15);
    
    private Skills(){}
    private Skills(int dam, int hat, int hea, int lif, int ran){
        dammage = dam;
        hates = hat;
        heal = hea;
        life = lif;
        range = ran;
    }
}
