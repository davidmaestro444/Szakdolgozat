using UnityEngine;

public class TrainingArea : MonoBehaviour
{
    public Fighter player1;
    public Fighter player2;
    public Transform p1Spawn;
    public Transform p2Spawn;
    public Transform leftWall;
    public Transform rightWall;
    public float cameraHalfWidth = 8.88f;

    private int p1WeaponCycle = 0;
    private int p2WeaponCycle = 0;

    void Start()
    {
        player1.GetComponent<DamageBox>().OnDeath += () => PlayerDied(player1);
        player2.GetComponent<DamageBox>().OnDeath += () => PlayerDied(player2);

        ResetArena();
    }

    void Update()
    {
        float midX = (player1.transform.position.x + player2.transform.position.x) / 2f;
        if (leftWall != null && rightWall != null)
        {
            leftWall.position = new Vector3(midX - cameraHalfWidth, leftWall.position.y, 0);
            rightWall.position = new Vector3(midX + cameraHalfWidth, rightWall.position.y, 0);
        }
    }

    public void ResetArena()
    {
        player1.transform.position = p1Spawn.position;
        player2.transform.position = p2Spawn.position;

        Rigidbody2D rb1 = player1.GetComponent<Rigidbody2D>();
        Rigidbody2D rb2 = player2.GetComponent<Rigidbody2D>();
        if (rb1 != null) rb1.linearVelocity = Vector2.zero;
        if (rb2 != null) rb2.linearVelocity = Vector2.zero;

        player1.GetComponent<DamageBox>().ResetCharacter();
        player2.GetComponent<DamageBox>().ResetCharacter();

        if (player1.weaponManager != null) player1.weaponManager.EquipWeaponByIndex(p1WeaponCycle);
        if (player2.weaponManager != null) player2.weaponManager.EquipWeaponByIndex(p2WeaponCycle);
    }

    public void PlayerDied(Fighter victim)
    {
        Fighter winner = (victim == player1) ? player2 : player1;
        winner.AddReward(0.5f);

        if (victim == player1) p1WeaponCycle = (p1WeaponCycle + 1) % 2;
        else p2WeaponCycle = (p2WeaponCycle + 1) % 2;

        player1.EndEpisode();
        player2.EndEpisode();
        ResetArena();
    }

    public void EndMatch(Fighter winner)
    {
        winner.AddReward(2.0f);
        Fighter loser = (winner == player1) ? player2 : player1;
        loser.AddReward(-1.0f);

        player1.EndEpisode();
        player2.EndEpisode();
        ResetArena();
    }
}
