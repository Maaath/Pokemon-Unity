using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] private List <Pokemon> pokemons;
    //private List <Pokemon> pokemons;

    private void Start()
    {
        foreach (var pokemon in pokemons)
        {
            pokemon.Init(); 
        }
    }

    public Pokemon GetHelthyPokemon()
    {
        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
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