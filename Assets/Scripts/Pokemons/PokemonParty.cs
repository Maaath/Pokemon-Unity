using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] private List <Pokemon> pokemons;
    [SerializeField] private List <Pokemon> wildPokemons;
    //private List <Pokemon> pokemons;

    private void Start()
    {   
        foreach (var pokemon in pokemons)
        {   
            if(PhotonNetwork.NickName == pokemon.Base.Name)
            {
            pokemon.Init();
            } 
        }
        var i = UnityEngine.Random.Range(0,6);
        wildPokemons[i].Init();
    }

    public Pokemon GetHelthyPokemon()
    {
        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }

    public Pokemon GetHelthyWildPokemon()
    {
        return wildPokemons.Where(x => x.HP > 0).FirstOrDefault();
    }

    public List <Pokemon> ListPokemon ()
    {
        return pokemons;
    }

    // public void AddPokemon (Pokemon pokemon)
    // {
    //     pokemons.Add(pokemon);
    // }

    // public void RemovePokemon (Pokemon pokemon)
    // {
    //     pokemons.Remove(pokemon);
    // }
}
