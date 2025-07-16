using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Node : MonoBehaviour
{
    [Header("Node Connections")]
    [Tooltip("Drag other Node objects here to define connections.")]
    public List<Node> adjacentNodes = new List<Node>();

    [Header("Visuals")]
    [Tooltip("The SpriteRenderer for this node's visual.")]
    public SpriteRenderer nodeSprite;
    public Color defaultColor = Color.white;
    public Color reachableColor = Color.green;
    public Color hoverColor = Color.yellow;

    [Header("Hover Effect")]
    [Tooltip("How much the node scales up when hovered over.")]
    public float hoverScaleMultiplier = 1.2f;
    [Tooltip("How long the smooth scaling animation takes in seconds.")]
    public float scaleDuration = 0.1f;

    // Private variables to store state
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
            // Stop any existing scaling animation to start a new one
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);

            // Start the scale-up animation
            scaleCoroutine = StartCoroutine(AnimateScale(originalScale * hoverScaleMultiplier));
            SetColor(hoverColor);
        }
    }

    private void OnMouseExit()
    {
        // Stop any existing scaling animation to start a new one
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);

        // Start the scale-down animation
        scaleCoroutine = StartCoroutine(AnimateScale(originalScale));

        if (isCurrentlyReachable)
        {
            SetColor(reachableColor);
        }
    }

    // This coroutine animates the scale smoothly over time
    private IEnumerator AnimateScale(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float timeElapsed = 0;

        while (timeElapsed < scaleDuration)
        {
            // Lerp (Linear Interpolation) calculates the point between start and end
            transform.localScale = Vector3.Lerp(startScale, targetScale, timeElapsed / scaleDuration);
            timeElapsed += Time.deltaTime;
            yield return null; // Wait for the next frame before continuing
        }

        // Ensure the scale is set exactly to the target at the end
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

    // --- The rest of your script (OnValidate, OnDrawGizmos) remains the same ---

    void OnValidate()
    {
        if (nodeSprite == null)
        {
            nodeSprite = GetComponent<SpriteRenderer>();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        foreach (Node adjacentNode in adjacentNodes)
        {
            if (adjacentNode != null)
            {
                Gizmos.DrawLine(transform.position, adjacentNode.transform.position);
            }
        }
    }
}