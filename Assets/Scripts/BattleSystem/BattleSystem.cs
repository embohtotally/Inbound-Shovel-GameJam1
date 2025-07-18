using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
public enum BattleState { INACTIVE, START, PLAYERTURN, ENEMYTURN, WON, LOST }

public class BattleSystem : MonoBehaviour
{
    public BattleState state;
    public static BattleSystem instance;
    [Header("Scene References")]
    public GameObject mapStage;
    public GameObject battleStage;
    public GameObject playerCharacter;

    [Header("Battle Setup")]
    //public GameObject enemyPrefab;
    public Transform playerPosition;
    public Transform enemyPosition;
    public TMP_Text actionFeedbackText;
    [Header("UI Connections")]
    public Button[] actionButtons;
    public Image[] enemyHeartIcons;
    public TMP_Text outcomeText;

    private CharacterStats playerStats;
    private CharacterStats enemyStats;
    private Vector3 playerOriginalMapPosition;

    void Start()
    {
        // Corrected: The battle should be INACTIVE when the game starts.
        state = BattleState.INACTIVE;
    }

    public void StartBattle(CharacterStats player, GameObject enemyToSpawn)
    {
        this.playerStats = player;
        playerOriginalMapPosition = playerCharacter.transform.position;
        actionFeedbackText.gameObject.SetActive(false);
        outcomeText.gameObject.SetActive(false);
        mapStage.SetActive(false);
        battleStage.SetActive(true);

        state = BattleState.START;
        StartCoroutine(SetupBattle(enemyToSpawn));
    }
    void Awake()
    {
        instance = this;
    }
    void PlayerTurn()
    {
        // NEW: Check if the player is supposed to miss their turn at the START of their turn.
        if (playerStats.willMissNextTurn)
        {
            playerStats.willMissNextTurn = false; // Reset the flag
            Debug.Log("You tripped and are recovering! You miss your turn.");
            SetActionButtons(false); // Make sure buttons are disabled
            StartCoroutine(EnemyTurn()); // Immediately skip to the enemy's turn
            return; // Stop the rest of this method from running
        }

        Debug.Log("Player's turn! Choose a Trick.");
        SetActionButtons(true);
    }

    IEnumerator SetupBattle(GameObject enemyToSpawn)
    {
        playerCharacter.transform.position = playerPosition.position;
        playerStats.UpdateHealthUI();

        // Use the passed-in enemy prefab instead of the old one.
        GameObject enemyGO = Instantiate(enemyToSpawn, enemyPosition);
        enemyStats = enemyGO.GetComponent<CharacterStats>();

        enemyStats.hearts = this.enemyHeartIcons;
        enemyStats.UpdateHealthUI();

        yield return new WaitForSeconds(1f);
        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    IEnumerator EndPlayerTurn()
    {
        SetActionButtons(false);
        yield return new WaitForSeconds(1.5f);

        if (enemyStats.currentHealth <= 0)
        {
            state = BattleState.WON;
            StartCoroutine(EndBattle(true));
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    // --- PLAYER ACTION METHODS ---
    public void OnBonkHeadButton() { if (state == BattleState.PLAYERTURN) StartCoroutine(ResolvePlayerAction("BonkHead")); }
    public void OnTripLegsButton() { if (state == BattleState.PLAYERTURN) StartCoroutine(ResolvePlayerAction("TripLegs")); }
    public void OnPokeClawsButton() { if (state == BattleState.PLAYERTURN) StartCoroutine(ResolvePlayerAction("PokeClaws")); }
    public void OnScramButton()
    {
        if (state != BattleState.PLAYERTURN) return;
        Debug.Log("You try to Scram!");

        if (Random.Range(0f, 1f) <= 0.2f) // 20% chance
        {
            Debug.Log("POOF! You successfully ran away!");
            // CHANGED: We now call a dedicated coroutine for fleeing.
            StartCoroutine(FleeBattle());
        }
        else
        {
            Debug.Log("You tripped while trying to run! You lose your turn.");
            playerStats.willMissNextTurn = true;
            StartCoroutine(EndPlayerTurn());
        }
    }

    IEnumerator FleeBattle()
    {
        SetActionButtons(false);

        // Show the success message
        actionFeedbackText.text = "POOF! You got away safely!";
        actionFeedbackText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        actionFeedbackText.gameObject.SetActive(false);

        // Return to map logic
        playerCharacter.transform.position = playerOriginalMapPosition;
        playerCharacter.GetComponent<Animator>().enabled = false;
        battleStage.SetActive(false);
        mapStage.SetActive(true);
        if (enemyStats != null) Destroy(enemyStats.gameObject);
        state = BattleState.INACTIVE;
    }

    IEnumerator ResolvePlayerAction(string action)
    {
        SetActionButtons(false);
        string feedbackMessage = ""; // We'll store the feedback here

        if (action != "Scram") playerStats.animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f);

        switch (action)
        {
            case "BonkHead":
                enemyStats.TakeDamage(1);
                feedbackMessage = "You used Bonk Head, dealing 1 damage.";
                if (Random.Range(0f, 1f) <= 0.3f)
                {
                    enemyStats.willMissNextTurn = true;
                    feedbackMessage += "\nIt succeeded in making the enemy dizzy!";
                }
                else
                {
                    feedbackMessage += "\nIt failed to make the enemy dizzy.";
                }
                break;
            case "TripLegs":
                feedbackMessage = "You used Trip Legs.";
                if (Random.Range(0f, 1f) <= 0.7f)
                {
                    enemyStats.willMissNextTurn = true;
                    feedbackMessage += "\nSuccess! The enemy stumbled.";
                }
                else
                {
                    feedbackMessage += "\nIt failed to trip the enemy.";
                }
                break;
            case "PokeClaws":
                enemyStats.bigPinchDisabled = true;
                feedbackMessage = "You poked the enemy's claws, disabling its Big Pinch!";
                break;

            // --- THIS IS THE MODIFIED CASE ---
            case "Scram":
                if (Random.Range(0f, 1f) <= 0.8f)
                {
                    // On success, we call the FleeBattle coroutine and exit
                    StartCoroutine(FleeBattle());
                    yield break; // Exit this coroutine immediately
                }
                else
                {
                    // On failure, set the status and the feedback message
                    playerStats.willMissNextTurn = true;
                    feedbackMessage = "You tripped while trying to run and lost your turn!";
                }
                break;
        }

        // Show the generated feedback message for all actions except a successful scram
        actionFeedbackText.text = feedbackMessage;
        actionFeedbackText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f); // Time to read the feedback
        actionFeedbackText.gameObject.SetActive(false);

        if (enemyStats.currentHealth <= 0)
        {
            state = BattleState.WON;
            StartCoroutine(EndBattle(true));
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator EnemyTurn()
    {
        Debug.Log("--- Enemy's Turn ---");
        string feedbackMessage = ""; // To store the enemy's action text

        if (enemyStats.willMissNextTurn)
        {
            feedbackMessage = "The Enemy is dizzy and misses its turn!";
            enemyStats.willMissNextTurn = false;
        }
        else
        {
            // Simple Enemy AI
            if (!enemyStats.bigPinchDisabled && Random.Range(0f, 1f) <= 0.6f)
            {
                feedbackMessage = "The Enemy uses Big Pinch!";
                enemyStats.animator.SetTrigger("Attack");
                yield return new WaitForSeconds(0.5f); // Wait for animation impact
                playerStats.TakeDamage(1);
            }
            else
            {
                feedbackMessage = "The Enemy blows some defensive tactics.";
                // This is a non-damaging move, so no attack animation is needed
            }
        }

        // --- NEW: Display the feedback ---
        actionFeedbackText.text = feedbackMessage;
        actionFeedbackText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f); // Wait for the player to read the message
        actionFeedbackText.gameObject.SetActive(false);
        // --------------------------------

        enemyStats.bigPinchDisabled = false;

        if (playerStats.currentHealth <= 0)
        {
            state = BattleState.LOST;
            StartCoroutine(EndBattle(false));
        }
        else
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }

    IEnumerator EndBattle(bool playerWon)
    {
        SetActionButtons(false);

        if (playerWon)
        {
            outcomeText.text = "YOU WON!";
            if (enemyStats != null) enemyStats.animator.SetTrigger("Die"); // Added: Trigger enemy Die animation
        }
        else
        {
            outcomeText.text = "Wiped Out!";
            if (playerStats != null) playerStats.animator.SetTrigger("Die"); // Added: Trigger player Die animation
        }

        outcomeText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f); // Added: Wait for death animation to play

        if (playerWon == false)
        {
            playerStats.currentHealth = playerStats.maxHealth;
            playerStats.UpdateHealthUI();
        }

        playerCharacter.transform.position = playerOriginalMapPosition;
        playerCharacter.GetComponent<Animator>().enabled = false; // Added: Turn player animator OFF

        battleStage.SetActive(false);
        mapStage.SetActive(true);

        if (enemyStats != null) Destroy(enemyStats.gameObject);

        state = BattleState.INACTIVE;
    }

    IEnumerator ShowFeedbackAndEndBattle(string message, bool playerWon)
    {
        actionFeedbackText.text = message;
        actionFeedbackText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        actionFeedbackText.gameObject.SetActive(false);
        StartCoroutine(EndBattle(playerWon));
    }

    void SetActionButtons(bool interactable)
    {
        foreach (Button button in actionButtons)
        {
            button.interactable = interactable;
        }
    }
}