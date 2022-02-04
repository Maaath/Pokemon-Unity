using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB 
{
    public static void Init()
    {
        foreach (var kvp in Conditions )
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions {get; set;} = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "foi envenenado!",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} sofreu dano do veneno!");
                }
            }
        },

        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "está queimando!",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} sofreu dano das chamas!");
                }
            }
        },

        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "está paralizado!",
                OnBeforeMove = (Pokemon pokemon) => 
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} está paralizado e não consegue se mover!");
                        return false;
                    }

                    return true;
                }
            }
        },

        {
            ConditionID.frz,
            new Condition()
            {
                Name = "Freeze",
                StartMessage = "foi congelado!",
                OnBeforeMove = (Pokemon pokemon) => 
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} não está mais congelado!");
                        return true;
                    }

                    return false;
                }
            }
        },

        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "adormeceu!",
                OnStart = (Pokemon pokemon) => 
                {
                    // Sleep por 1 - 3 turnos
                    pokemon.StatusTime = Random.Range(1, 4);
                    //Debug.Log($"Dormindo por {pokemon.StatusTime} movimentos"); 
                },
                OnBeforeMove = (Pokemon pokemon) => 
                {
                    if (pokemon.StatusTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} acordou!");
                        return true;
                    }
                    pokemon.StatusTime--;   
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} está dormindo!");
                    return false;
                }
            }
        },

        //Volatile Status Conditions

        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "ficou confuso!",
                OnStart = (Pokemon pokemon) => 
                {
                    // Confuso por 1 - 4 turnos
                    pokemon.VolatileStatusTime = Random.Range(1, 5);
                    Debug.Log($"Confuso por {pokemon.VolatileStatusTime} movimentos"); 
                },
                OnBeforeMove = (Pokemon pokemon) => 
                {
                    if (pokemon.VolatileStatusTime <= 0)
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} não está mais confuso!");
                        return true;
                    }
                    pokemon.VolatileStatusTime--;   

                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} está confuso!");
                    // 50% de chance de usar um golpe
                    if (Random.Range(1, 3) == 1){
                        return true;
                    }

                    pokemon.UpdateHP(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"Feriu a si mesmo devido a confusão!");
                    return false;
                }
            }
        }

    };
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz,
    confusion
}