using System.Collections.Generic;
using UnityEngine;

public class BrakeLever : MonoBehaviour
{
    public Sprite leverLeft;
    public Sprite leverRight;
    public float brakeForce = 40f;
    public float stunDuration = 1.0f;
    private bool isActivated = false;
    private SpriteRenderer spriteRenderer;
    private List<PlayerMovement> playersInZone = new List<PlayerMovement>();

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        }
    }

    public void ActivateBrake()
    {
        isActivated = true;

        if (spriteRenderer != null && leverRight != null) spriteRenderer.sprite = leverRight;

        ApplyBrakeForceToAll();
    }
    public void TryActivate(PlayerMovement pm)
    {
        if (playersInZone.Contains(pm) && !isActivated)
        {
            ActivateBrake();
        }
    }

    private void ApplyBrakeForceToAll()
    {
        CharacterBase[] allCharacters = FindObjectsByType<CharacterBase>(FindObjectsSortMode.None);
        foreach (CharacterBase charBase in allCharacters)
        {
            if (charBase.Grounded && !charBase.IsOnHighGround)
            {
                charBase.ApplyKnockback(new Vector2(brakeForce, 1f), stunDuration);
            }
        }
    }
}
