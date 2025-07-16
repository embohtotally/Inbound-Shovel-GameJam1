using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Required for List

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("The first node the player will start on.")]
    public Node startingNode;
    [Tooltip("How fast the player moves from node to node.")]
    public float moveSpeed = 5f;

    private Node currentNode;
    private bool isMoving = false;
    private List<Node> allNodesInScene; // Store a reference to all nodes

    void OnEnable()
    {
        Node.OnNodeClicked += HandleNodeClicked;
    }

    void OnDisable()
    {
        Node.OnNodeClicked -= HandleNodeClicked;
    }

    void Start()
    {
        // --- DEBUG: Find all nodes and store them immediately ---
        allNodesInScene = new List<Node>(FindObjectsOfType<Node>());
        Debug.Log("Found " + allNodesInScene.Count + " nodes in the scene.");

        if (startingNode == null)
        {
            Debug.LogError("FATAL: Starting Node is not set on the Player Controller!");
            return;
        }

        // --- DEBUG: Confirming start-up ---
        Debug.Log("PlayerController starting at: " + startingNode.gameObject.name);

        currentNode = startingNode;
        transform.position = currentNode.transform.position;

        // A short delay to ensure all other scripts have run their Start() methods
        StartCoroutine(InitialNodeUpdate());
    }

    // Coroutine to handle the initial update with a tiny delay
    private IEnumerator InitialNodeUpdate()
    {
        // Wait for the end of the frame to ensure all other objects are initialized
        yield return new WaitForEndOfFrame();
        Debug.Log("Coroutine Started: Updating reachable nodes for the first time.");
        UpdateReachableNodes();
    }

    private void HandleNodeClicked(Node clickedNode)
    {
        if (isMoving) return;

        if (currentNode.adjacentNodes.Contains(clickedNode))
        {
            StartCoroutine(MoveToNode(clickedNode));
        }
    }

    private IEnumerator MoveToNode(Node targetNode)
    {
        isMoving = true;
        ClearAllReachableNodes();

        Vector3 startPosition = transform.position;
        Vector3 endPosition = targetNode.transform.position;

        // Using a simple Lerp over time instead of distance calculation
        float timeElapsed = 0;
        float duration = 1f / moveSpeed; // Simple duration based on speed

        while (timeElapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition; // Ensure it snaps to the final position

        currentNode = targetNode;
        isMoving = false;

        UpdateReachableNodes();
    }

    private void UpdateReachableNodes()
    {
        // --- DEBUG: Announce what the function is doing ---
        Debug.Log("UpdateReachableNodes called. Current Node: " + currentNode.gameObject.name);
        Debug.Log(currentNode.gameObject.name + " has " + currentNode.adjacentNodes.Count + " adjacent nodes.");

        foreach (Node node in allNodesInScene)
        {
            if (currentNode.adjacentNodes.Contains(node))
            {
                // --- DEBUG: Confirm which node is being set to reachable ---
                Debug.Log("Setting " + node.gameObject.name + " as REACHABLE.");
                node.SetReachable(true);
            }
            else
            {
                node.SetReachable(false);
            }
        }
    }

    private void ClearAllReachableNodes()
    {
        if (currentNode == null) return;
        foreach (Node node in currentNode.adjacentNodes)
        {
            if (node != null)
            {
                node.SetReachable(false);
            }
        }
    }
}