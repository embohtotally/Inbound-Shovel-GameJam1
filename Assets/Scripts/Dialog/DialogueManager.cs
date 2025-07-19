using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("Dialogue UI Elements")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image speakerImageA;
    [SerializeField] private Image speakerImageB;

    [Header("Typing Settings")]
    [SerializeField] private float typingSpeed = 0.03f;

    private Queue<DialogueLine> dialogueQueue;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private string currentFullLine = "";
    private Coroutine typingCoroutine;

    private PlayerController activePlayerController; // <-- NEW: To remember the player

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        dialogueQueue = new Queue<DialogueLine>();
    }

    void Start()
    {
        dialogueUI.SetActive(false);
    }

    void Update()
    {
        if (isDialogueActive && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            if (isTyping)
            {
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                dialogueText.text = currentFullLine;
                isTyping = false;
            }
            else
            {
                DisplayNextSentence();
            }
        }
    }

    public void StartDialogue(List<DialogueLine> lines, PlayerController player)
    {
        if (lines.Count == 0) return;

        activePlayerController = player; // <-- Store the player reference
        isDialogueActive = true;
        activePlayerController.isMoving = true; // Lock the stored player

        dialogueQueue.Clear();
        foreach (var line in lines)
        {
            dialogueQueue.Enqueue(line);
        }

        dialogueUI.SetActive(true);
        DisplayNextSentence();
    }

    private void DisplayNextSentence()
    {
        if (dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        var currentLine = dialogueQueue.Dequeue();

        speakerText.text = currentLine.speakerName;
        currentFullLine = currentLine.dialogueText;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(currentFullLine));

        Color bright = Color.white;
        Color dim = new Color(0.7f, 0.7f, 0.7f, 1f);

        speakerImageA.sprite = currentLine.speakerImageA;
        speakerImageB.sprite = currentLine.speakerImageB;

        speakerImageA.preserveAspect = true;
        speakerImageB.preserveAspect = true;

        speakerImageA.gameObject.SetActive(currentLine.speakerImageA != null);
        speakerImageB.gameObject.SetActive(currentLine.speakerImageB != null);

        speakerImageA.color = currentLine.isSpeakerAActive ? bright : dim;
        speakerImageB.color = !currentLine.isSpeakerAActive ? bright : dim;
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        dialogueUI.SetActive(false);

        // --- THIS IS THE FIX ---
        // Use the stored reference to unlock the player.
        if (activePlayerController != null)
        {
            activePlayerController.isMoving = false;
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }
}