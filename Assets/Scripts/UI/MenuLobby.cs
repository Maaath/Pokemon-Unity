using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class MenuLobby : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button startBattle;

    [PunRPC] 
    public void AttPokemonList()
    {
        startBattle.interactable = NetworkManager.Instance.OwnerRoom();
    }
}
