using UnityEngine;

public class InteractableObject : MonoBehaviour, Interactable
{
    [Header("UI")]
    public GameObject objectUI;

    [Header("Highlight")]
    private SpriteRenderer spriteRenderer;

    private Color originalColor;

    public Color highlightColor = Color.yellow;

    [Header("Player")]
    public MonoBehaviour playerController;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        originalColor = spriteRenderer.color;
    }

    public void Interact()
    {
        objectUI.SetActive(true);
        playerController.enabled = false;
    }

    public void CloseUI()
    {
        objectUI.SetActive(false);
        playerController.enabled = true;
    }

    public void Highlight()
    {
        spriteRenderer.color = highlightColor;
    }

    public void RemoveHighlight()
    {
        spriteRenderer.color = originalColor;
    }
}