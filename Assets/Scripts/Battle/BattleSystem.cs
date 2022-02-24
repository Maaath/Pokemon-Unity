using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


public enum BattleState {Start, ActionSelection, MoveSelection, RunningTurn, Busy, BattleOver}
public enum BatteAction { Move, UseItem, Run }

public class BattleSystem : MonoBehaviourPunCallbacks
{
    [SerializeField] BattleUnit playerUnit;
    //[SerializeField] BattleHud playerHud;
    [SerializeField] BattleUnit enemyUnit;
    //[SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] Pokemon wildPokemon; // Nao temos um mapa (sem wildPokemons)
    [SerializeField] PokemonParty playerParty;
    [SerializeField] Image player1Image;
    [SerializeField] Image player2Image;

    BattleState state;
    BattleState prevState;
    int currentAction;
    int currentMove;

    private void Start() 
    {
        StartCoroutine(SetupBattle());
    }

    // public void setPokemon (PokemonParty newPokemonParty) {
    //     playerParty = newPokemonParty;
    // }

    public IEnumerator SetupBattle() 
    {   
        Debug.Log(PhotonNetwork.NickName);
        //playerUnit.gameObject.SetActive(false);
        //startando as condições de status
        ConditionsDB.Init();


        wildPokemon.Init(); // Remover esta linha quando adicionar player 2

        player1Image.gameObject.SetActive(true);
        player2Image.gameObject.SetActive(true);
        yield return dialogBox.TypeDialog($"Vamos batalhar!");
        playerUnit.ActiveHud();
        enemyUnit.ActiveHud();
        player1Image.gameObject.SetActive(false);
        player2Image.gameObject.SetActive(false);
        playerUnit.gameObject.SetActive(true);
        enemyUnit.gameObject.SetActive(true);
        

        playerUnit.Setup(playerParty.GetHelthyPokemon());
        enemyUnit.Setup(wildPokemon);

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        ActionSelection();
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        //OnBattleOver(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        StartCoroutine(dialogBox.TypeDialog("Escolha uma opção:"));
        dialogBox.EnableActionSelector(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator RunTurns(BatteAction playerAction)
    {
        state = BattleState.RunningTurn;

        if (playerAction == BatteAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

            // Checar quem vai atacar primeiro
            bool playerGoesFirst = true;
            if(enemyMovePriority > playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else if (enemyMovePriority == playerMovePriority)
            {
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
            } 

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;
        
            var secondPokemon = secondUnit.Pokemon;

            // Primeiro turno
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            if (secondPokemon.HP > 0) {
                // Segundo turno
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }
        }
        else {
            var enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }

        if (state != BattleState.BattleOver)
        {
            ActionSelection();
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget)
    {
        //Boosts
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);

        }
        //Status Conditions
        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status); 
        }

        //Volatile Status Conditions
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus); 
        }
        
        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit) 
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        // Poison and Burn fere os pokemons depois do turno
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateHP();

        if (sourceUnit.Pokemon.HP <=0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} desmaiou!");
            yield return new WaitForSeconds(1f);
            

            //essa parte ficou diferente 
            if (sourceUnit.IsPlayerUnit){
                var nextPokemon = playerParty.GetHelthyPokemon();
                currentMove = 0;
                CheckForBattleOver(sourceUnit, nextPokemon);
                if(nextPokemon != null)
                {
                yield return dialogBox.TypeDialog($"Vai {nextPokemon.Base.Name}!");
                }
                
            }else{
                CheckForBattleOver(sourceUnit);
            }
        }
    }

    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon  target)
    {
        if (move.Base.AlwaysHits){
            return true;
        }

        float moveAccruacy = move.Base.Accuracy;

        int accuracy =  source.StatBoosts[Stat.Accuracy];
        int evasion =  target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f , 3f, 3.5f, 4f };

        //accurracy
        if (accuracy > 0){
            moveAccruacy *= boostValues[accuracy];
        }else{
            moveAccruacy /= boostValues[-accuracy];
        }

        //evasion
        if (evasion > 0){
            moveAccruacy /= boostValues[evasion];
        }else{
            moveAccruacy *= boostValues[-evasion];
        }


        return UnityEngine.Random.Range(1,101) <= moveAccruacy;
    }

    IEnumerator ShowStatusChanges( Pokemon pokemon )
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator RunMove (BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        //Tem que ter cuidado de resetar isso daqui depois
        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} usou {move.Base.Name}.");

        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon)){
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);

            targetUnit.PlayHitAnimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects( move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target );
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if ( move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0){
                foreach (var secondary in move.Base.Secondaries){
                    var rnd = UnityEngine.Random.Range(1,101);
                    if (rnd <= secondary.Chance){
                        yield return RunMoveEffects( secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target );
                    }
                }
            }

            if (targetUnit.Pokemon.HP <=0)
            {
                yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} desmaiou!");
                yield return new WaitForSeconds(1f);
                

                //essa parte ficou diferente 
                if (targetUnit.IsPlayerUnit){
                    var nextPokemon = playerParty.GetHelthyPokemon();
                    currentMove = 0;
                    CheckForBattleOver(targetUnit, nextPokemon);
                    if(nextPokemon != null)
                    {
                        yield return dialogBox.TypeDialog($"Vai {nextPokemon.Base.Name}!");
                    }
                }else{
                    CheckForBattleOver(targetUnit);
                }
            }
        }else{
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} errou o ataque");
        }
    }

    void CheckForBattleOver ( BattleUnit faintedUnit,  Pokemon nextPokemon=null) 
    {
        //aqui ficou um pouco diferente, ja que não tem o hud para troca de pokemon, apenas pega o proximo
        if (faintedUnit.IsPlayerUnit)
        {
            if (nextPokemon != null) // Quando por 3 pokemons verificar se o pokemon não está setado
            {
                
                playerUnit.Setup(nextPokemon);
                
                dialogBox.SetMoveNames(nextPokemon.Moves);

                RunAfterTurn(playerUnit);                
            }
            else{
                playerUnit.PlayFaintAnimation();
                BattleOver(true);
            }
        }
        else {
            faintedUnit.PlayFaintAnimation();
            BattleOver(true);
        }
    }

    IEnumerator ShowDamageDetails ( DamageDetails damageDetails )
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("Ataque crítico!!");
        
        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("Ataque super efetivo.");

        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("Ataque não surtiu efeito.");
    }

    private void Update() 
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 1)
                ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 0 )
                --currentAction;
        }

        dialogBox.UpdateActionSelection(currentAction); 

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                // Fight
                MoveSelection();
            }
        
            else if (currentAction == 0)
            {
                // Surrender
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 1)
                ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove > 0 )
                --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 2)
                currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 1 )
                currentMove -= 2;
        }

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var move = playerUnit.Pokemon.Moves[currentMove];
            if (move.PP == 0) return;
            
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BatteAction.Move));
        }
    }
}
