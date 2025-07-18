using UnityEngine;
using UnityEngine.EventSystems; // Required for hover events

public class ActionButtonInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Description")]
    [TextArea(3, 5)]
    public string actionDescription;

    // This is called when the mouse cursor enters the button's area
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (BattleSystem.instance != null && BattleSystem.instance.actionFeedbackText != null)
        {
            BattleSystem.instance.actionFeedbackText.text = actionDescription;
            BattleSystem.instance.actionFeedbackText.gameObject.SetActive(true);
        }
    }

    // This is called when the mouse cursor leaves the button's area
    public void OnPointerExit(PointerEventData eventData)
    {
        if (BattleSystem.instance != null && BattleSystem.instance.actionFeedbackText != null)
        {
            BattleSystem.instance.actionFeedbackText.gameObject.SetActive(false);
        }
    }
}