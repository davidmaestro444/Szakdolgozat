using System.Collections;
using UnityEngine;

public class PlayerMovement : CharacterBase
{
    public string horizontalAxis = "Horizontal";
    public string jumpButton = "Vertical";
    public KeyCode attackKey = KeyCode.Space;
    public KeyCode throwKey = KeyCode.Q;
    public KeyCode interactKey = KeyCode.F;

    public bool aiInteractTriggered = false;
    public bool isAI = false;

    protected override void UpdateBehavior()
    {
        if (!isAI)
        {
            float xInput = Input.GetAxis(horizontalAxis);
            Move(xInput);

            if (Input.GetButtonDown(jumpButton)) TryJump();
            if (Input.GetKeyDown(attackKey)) PerformAttack();
            if (Input.GetKeyDown(throwKey)) PerformThrow();

            if (Input.GetKeyDown(interactKey))
            {
                if (weaponManager.TryPickUpWeapon()) knightControl.idle();
            }
        }

        if (aiInteractTriggered)
        {
            aiInteractTriggered = false;
            if (weaponManager.TryPickUpWeapon())
            {
                knightControl.idle();
            }
        }
    }
}