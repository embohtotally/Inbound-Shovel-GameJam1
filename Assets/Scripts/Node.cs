using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// An enum defines a set of named constants. Easy to use in the inspector.
public enum NodeType { Path, Encounter, Treasure, Story, Rest }

public class Node : MonoBehaviour
{
    [Header("Node Settings")]
    [Tooltip("The type of event that triggers when landing on this node.")]
    public NodeType nodeType = NodeType.Path; // <-- NEW
    public GameObject enemyPrefab; // <-- ADD THIS LINE
    [Header("Node Connections")]
    public List<Node> adjacentNodes = new List<Node>();
    public List<DialogueLine> dialogueLines;
    [Header("Visuals")]
    public SpriteRenderer nodeSprite;
    public Color defaultColor = Color.white;
    public Color reachableColor = Color.green;
    public Color hoverColor = Color.yellow;

    [Header("Hover Effect")]
    public float hoverScaleMultiplier = 1.2f;
    public float scaleDuration = 0.1f;

    // Private variables
    private Vector3 originalScale;
    private bool isCurrentlyReachable = false;
    private Coroutine scaleCoroutine;

    public static event Action<Node> OnNodeClicked;

    void Start()
    {
        originalScale = transform.localScale;
        SetColor(defaultColor);
    }

    public void SetReachable(bool isReachable)
    {
        isCurrentlyReachable = isReachable;
        if (nodeSprite != null)
        {
            nodeSprite.color = isReachable ? reachableColor : defaultColor;
        }
    }

    private void OnMouseEnter()
    {
        if (isCurrentlyReachable)
        {
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(AnimateScale(originalScale * hoverScaleMultiplier));
            SetColor(hoverColor);
        }
    }



    private void OnMouseExit()
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(AnimateScale(originalScale));

        if (isCurrentlyReachable)
        {
            SetColor(reachableColor);
        }
    }

    private IEnumerator AnimateScale(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float timeElapsed = 0;

        while (timeElapsed < scaleDuration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, timeElapsed / scaleDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
    }

    private void OnMouseDown()
    {
        if (isCurrentlyReachable)
        {
            OnNodeClicked?.Invoke(this);
        }
    }

    private void SetColor(Color color)
    {
        if (nodeSprite != null)
        {
            nodeSprite.color = color;
        }
    }

    // Gizmos and OnValidate can remain the same
    void OnValidate()
    {
        if (nodeSprite == null)
        {
            nodeSprite = GetComponent<SpriteRenderer>();
        }
    }

    private void OnDrawGizmos()
    {
        // To avoid clutter, let's comment this out since we have the new Line Renderer
        /*
        Gizmos.color = Color.blue;
        foreach (Node adjacentNode in adjacentNodes)
        {
            if (adjacentNode != null)
            {
                Gizmos.DrawLine(transform.position, adjacentNode.transform.position);
            }
        }
        */
    }
}