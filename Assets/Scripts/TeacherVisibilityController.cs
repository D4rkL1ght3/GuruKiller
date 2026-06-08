using UnityEngine;

public class TeacherVisibilityController : MonoBehaviour
{
    [Header("Visuals")]
    public SpriteRenderer[] spriteRenderers;
    public Animator animator;

    private bool isVisible = true;

    void Awake()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    public void SetVisible(bool visible)
    {
        isVisible = visible;

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = visible;
            }
        }

        if (animator != null)
        {
            animator.enabled = visible;
        }
    }

    public bool IsVisible()
    {
        return isVisible;
    }
}