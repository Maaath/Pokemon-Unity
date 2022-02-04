using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class MenuLobby : MonoBehaviourPunCallbacks
{
    [SerializeField] private Text pokemonsList;
    [SerializeField] private Button startBattle;

    [PunRPC] 
    public void AttPokemonList()
    {
        pokemonsList.text = NetworkManager.Instance.PokemonsList();
        startBattle.interactable = NetworkManager.Instance.OwnerRoom();
    }
}
