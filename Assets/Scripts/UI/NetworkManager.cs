using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }
    //private PokemonParty scene1;
    private void Awake() 
    {
        if (Instance != null && Instance != this)
        {
            gameObject.SetActive(false);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected!");
    }

    public void CreateRoom (string nameRoom)
    {
        PhotonNetwork.CreateRoom(nameRoom);
    }

    public void EnterRoom (string nameRoom)
    {
        PhotonNetwork.JoinRoom(nameRoom);
    }

    public void ChangeNick (string nickName)
    {
        PhotonNetwork.NickName = nickName;
    }

    public string PokemonsList()
    {
        var list = "Charmander"+"\n"+"Pikachu"+"\n";
        return list;
    }

    //  void Add () 
    //  {
    //      scene1 = GameObject.Find("Script").GetComponent<Scene1>();
    //      scene1.TestCall();
    //  }

    public bool OwnerRoom()
    {
        return PhotonNetwork.IsMasterClient;
    }

    public void ExitBattle()
    {
        PhotonNetwork.LeaveRoom();
    }

    [PunRPC]
    public void StartGame(string nameScene)
    {
        PhotonNetwork.LoadLevel(nameScene);
    }
}
