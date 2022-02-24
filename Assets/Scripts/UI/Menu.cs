using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Menu : MonoBehaviourPunCallbacks
{
    [SerializeField] private EnterMenu enterMenu;    
    [SerializeField] private MenuLobby menuLobby;    

    private void Start()
    {
        enterMenu.gameObject.SetActive(false);
        menuLobby.gameObject.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        enterMenu.gameObject.SetActive(true);
        
    }
    public override void OnJoinedRoom()
    {
        ChangeMenu(menuLobby.gameObject);
        menuLobby.photonView.RPC("AttPokemonList", RpcTarget.All);
    }

    public void ChangeMenu(GameObject menu)
    {
        enterMenu.gameObject.SetActive(false);
        menuLobby.gameObject.SetActive(false);

        menu.SetActive(true);
    }

    // public override void OnPlayerLeftRoom(Player otherPlayer)
    // {
    //     menuLobby.AttPokemonList();
    // }

    public void ExitBattle()
    {
        NetworkManager.Instance.ExitBattle();
        ChangeMenu(enterMenu.gameObject);
    }

    public void StartGame(string nameScene)
    {
        NetworkManager.Instance.photonView.RPC("StartGame", RpcTarget.All, nameScene);
    }
    public void PlayerPokemon(string pokemonName)
    {   
        //Debug.Log(pokemonName);
        NetworkManager.Instance.PokemonsSelected(pokemonName);
    }
}