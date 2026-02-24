using System.Collections.Generic;
using UnityEngine;

public class BrakeLever : MonoBehaviour
{
    [Header("Beállítások")]
    public Sprite leverLeft;
    public Sprite leverRight;
    public float brakeForce = 40f;
    public float stunDuration = 1.0f;

    private bool isActivated = false;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private List<PlayerMovement> playersInZone = new List<PlayerMovement>();

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (spriteRenderer != null && leverLeft != null) spriteRenderer.sprite = leverLeft;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement pm = other.GetComponent<PlayerMovement>();
        if (pm != null && !playersInZone.Contains(pm))
        {
            playersInZone.Add(pm);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerMovement pm = other.GetComponent<PlayerMovement>();
        if (pm != null && playersInZone.Contains(pm))
        {
            playersInZone.Remove(pm);
        }
    }

    void Update()
    {
        if (isActivated) return;

        foreach (PlayerMovement pm in playersInZone)
        {
            if (pm != null && Input.GetKeyDown(pm.interactKey))
            {
                ActivateBrake();
                break;
            }
        }
    }

    private void ActivateBrake()
    {
        isActivated = true;

        if (spriteRenderer != null && leverRight != null) spriteRenderer.sprite = leverRight;
        if (audioSource != null) audioSource.Play();

        ApplyBrakeForceToAll();
    }

    private void ApplyBrakeForceToAll()
    {
        PlayerMovement[] allPlayers = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);

        foreach (PlayerMovement p in allPlayers)
        {
            if (p.Grounded)
            {
                p.ApplyKnockback(new Vector2(brakeForce, 1f), stunDuration);
            }
        }
    }
}
