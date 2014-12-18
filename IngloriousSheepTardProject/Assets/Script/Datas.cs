using UnityEngine;
using System.Collections;

public class Datas {
    public static Datas Instance = new Datas();

    public bool isServer = true;
    public string serverIP = "127.0.0.1";
    public int port = 4430;
    public int maxPlayer = 3;
    private Datas() {}
}
