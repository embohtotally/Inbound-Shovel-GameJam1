using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
public class PlayerController : MonoBehaviour
{
    private Animator myAnimator;

    [Header("Connections")]
    public BattleSystem battleSystem;
    private SpriteRenderer mySpriteRenderer;
    [Header("Movement Settings")]
    public Node startingNode;
    public float moveSpeed = 5f;
    public Sprite mapIconSprite;
    [Header("Line Visuals")]
    public GameObject linePrefab;

    [Header("Transition Effects")]
    public Camera mainCamera;
    public Light2D globalLight;
    public float zoomOutDuration = 1.0f;
    public float zoomInDuration = 0.5f;

    private Node currentNode;
    public bool isMoving = false;
    private List<Node> allNodesInScene;
    private List<GameObject> activeLines = new List<GameObject>();
    public GameObject winPanel;
    void OnEnable() { Node.OnNodeClicked += HandleNodeClicked; }
    void OnDisable() { Node.OnNodeClicked -= HandleNodeClicked; }

    void Start()
    {
        myAnimator = GetComponent<Animator>();
        myAnimator.enabled = false; // Start with animator OFF
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        allNodesInScene = new List<Node>(FindObjectsOfType<Node>());
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        if (startingNode == null)
        {
            Debug.LogError("FATAL: Starting Node is not set on the Player Controller!");
            return;
        }

        currentNode = startingNode;
        transform.position = currentNode.transform.position;

        StartCoroutine(InitialNodeUpdate());
    }
    void Update()
    {
        // If the animator is OFF (we are on the map)
        if (myAnimator != null && !myAnimator.enabled)
        {
            // Make sure the sprite is set to the map icon.
            mySpriteRenderer.sprite = mapIconSprite;
        }
        // If the animator is ON, we don't do anything,
        // because the animator is controlling the sprite.
    }
    private IEnumerator InitialNodeUpdate()
    {
        yield return new WaitForEndOfFrame();
        UpdateReachableNodes();
        UpdateLineVisuals();
        TriggerNodeEvent(currentNode.nodeType);
    }

    private void HandleNodeClicked(Node clickedNode)
    {
        if (isMoving) return; // This will now also prevent movement during dialogue

        if (currentNode.adjacentNodes.Contains(clickedNode))
        {
            StartCoroutine(MoveToNode(clickedNode));
        }
    }

    private IEnumerator MoveToNode(Node targetNode)
    {
        isMoving = true;
        ClearAllReachableNodes();
        ClearLineVisuals();
        AudioManager.instance.PlaySFX("PlayerMove");
        Vector3 startPosition = transform.position;
        Vector3 endPosition = targetNode.transform.position;

        float timeElapsed = 0;
        float duration = Vector3.Distance(startPosition, endPosition) / moveSpeed;

        while (timeElapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        currentNode = targetNode;

        UpdateReachableNodes();
        UpdateLineVisuals();

        TriggerNodeEvent(currentNode.nodeType);
    }

    private void TriggerNodeEvent(NodeType type)
    {
        switch (type)
        {
            case NodeType.Encounter:
                StartCoroutine(EnterBattleTransition());
                break;

            // For all non-battle nodes, we allow the player to move again immediately.
            case NodeType.Path:
                isMoving = false;
                break;
            case NodeType.Treasure:
                // --- THIS IS THE NEW LOGIC ---
                Debug.Log("You reached the treasure! YOU WIN!");
                // Load the Main Menu scene.
                StartCoroutine(WinGameSequence());// Make sure your scene is named "MainMenu"
                break;
            case NodeType.Story:
                Debug.Log("A STORY event unfolds...");
                // Check if the node has dialogue and a manager exists
                if (currentNode.dialogueLines.Count > 0 && DialogueManager.instance != null)
                {
                    DialogueManager.instance.StartDialogue(currentNode.dialogueLines, this);
                }
                else
                {
                    isMoving = false; // No dialogue, so allow movement
                }
                break;
            case NodeType.Rest:
                if (type == NodeType.Rest)
                {
                    CharacterStats playerStats = GetComponent<CharacterStats>();
                    if (playerStats != null)
                    {
                        playerStats.currentHealth = playerStats.maxHealth;
                        playerStats.UpdateHealthUI();
                    }
                }
                isMoving = false;
                break;
        }
    }

    IEnumerator WinGameSequence()
    {
        isMoving = true; // Lock player movement

        // Show the "You Win" panel
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        // Wait for 3 seconds
        yield return new WaitForSeconds(3f);

        // Load the Main Menu scene
        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator EnterBattleTransition()
    {
        isMoving = true;

        // ... (The two-phase transition code is correct)
        float originalCamSize = mainCamera.orthographicSize;
        float originalLightIntensity = globalLight.intensity;
        float zoomOutTargetSize = 10f;
        float timeElapsed = 0;
        while (timeElapsed < zoomOutDuration)
        {
            mainCamera.orthographicSize = Mathf.Lerp(originalCamSize, zoomOutTargetSize, timeElapsed / zoomOutDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        mainCamera.orthographicSize = zoomOutTargetSize;
        timeElapsed = 0;
        while (timeElapsed < zoomInDuration)
        {
            mainCamera.orthographicSize = Mathf.Lerp(zoomOutTargetSize, originalCamSize, timeElapsed / zoomInDuration);
            globalLight.intensity = Mathf.Lerp(originalLightIntensity, 0f, timeElapsed / zoomInDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        mainCamera.orthographicSize = originalCamSize;
        globalLight.intensity = 0f;

        if (myAnimator != null) myAnimator.enabled = true;

        // --- THIS IS THE CORRECTED LINE ---
        CharacterStats playerStats = GetComponent<CharacterStats>();
        if (battleSystem != null && playerStats != null)
        {
            battleSystem.StartBattle(playerStats, currentNode.enemyPrefab);
        }

        globalLight.intensity = originalLightIntensity;
        isMoving = false;
    }

    // NOTE: The OnBattleConcluded method has been removed.

    private void UpdateLineVisuals()
    {
        ClearLineVisuals();
        if (linePrefab == null) return;
        foreach (Node adjacentNode in currentNode.adjacentNodes)
        {
            GameObject lineObject = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
            LineRenderer line = lineObject.GetComponent<LineRenderer>();
            line.SetPosition(0, currentNode.transform.position);
            line.SetPosition(1, adjacentNode.transform.position);
            activeLines.Add(lineObject);
        }
    }

    private void ClearLineVisuals()
    {
        foreach (GameObject line in activeLines) Destroy(line);
        activeLines.Clear();
    }

    private void UpdateReachableNodes()
    {
        if (allNodesInScene == null) return;
        foreach (Node node in allNodesInScene)
        {
            node.SetReachable(currentNode.adjacentNodes.Contains(node));
        }
    }

    private void ClearAllReachableNodes()
    {
        if (currentNode == null) return;
        foreach (Node node in currentNode.adjacentNodes)
        {
            if (node != null) node.SetReachable(false);
        }
    }
}