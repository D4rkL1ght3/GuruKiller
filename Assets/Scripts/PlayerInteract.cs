using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("UI")]
    public GameObject interactText;

    private List<Interactable> nearbyInteractables = new List<Interactable>();

    private Interactable currentInteractable;

    void Start()
    {
        interactText.SetActive(false);
    }

    void Update()
    {
        FindClosestInteractable();

        // Interact
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentInteractable != null && Time.timeScale != 0f)
            {
                currentInteractable.Interact();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && Time.timeScale != 0f)
        {
            if (currentInteractable != null)
            {
                currentInteractable.CloseUI();
            }
        }
    }

    void FindClosestInteractable()
    {
        // Remove null entries
        nearbyInteractables.RemoveAll(item => item == null);

        if (nearbyInteractables.Count == 0)
        {
            ClearCurrentInteractable();
            return;
        }

        Interactable closest = null;

        float closestDistance = Mathf.Infinity;

        foreach (Interactable interactable in nearbyInteractables)
        {
            MonoBehaviour interactableMono = interactable as MonoBehaviour;

            if (interactableMono == null)
                continue;

            float distance =
                Vector2.Distance(
                    transform.position,
                    interactableMono.transform.position
                );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = interactable;
            }
        }

        // Change highlight
        if (closest != currentInteractable)
        {
            if (currentInteractable != null)
            {
                currentInteractable.RemoveHighlight();
            }

            currentInteractable = closest;

            if (currentInteractable != null)
            {
                currentInteractable.Highlight();
            }
        }

        interactText.SetActive(currentInteractable != null);
    }

    void ClearCurrentInteractable()
    {
        if (currentInteractable != null)
        {
            currentInteractable.RemoveHighlight();
        }

        currentInteractable = null;

        interactText.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Interactable interactable = other.GetComponent<Interactable>();

        if (interactable != null &&
            !nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Add(interactable);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Interactable interactable = other.GetComponent<Interactable>();

        if (interactable != null)
        {
            nearbyInteractables.Remove(interactable);
        }
    }
}