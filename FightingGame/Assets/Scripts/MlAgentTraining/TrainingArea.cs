using UnityEngine;

public class TrainingArea : MonoBehaviour
{
    public Fighter player1;
    public Fighter player2;

    public Transform p1Spawn;
    public Transform p2Spawn;

    private int p1WeaponCycle = 0;
    private int p2WeaponCycle = 0;

    void Start()
    {
        ResetArea();
    }

    public void ResetArea()
    {
        player1.transform.position = p1Spawn.position;
        player2.transform.position = p2Spawn.position;
        Rigidbody2D rb1 = player1.GetComponent<Rigidbody2D>();
        Rigidbody2D rb2 = player2.GetComponent<Rigidbody2D>();
        rb1.linearVelocity = Vector2.zero;
        rb2.linearVelocity = Vector2.zero;
        player1.GetComponent<DamageBox>().ResetCharacter();
        player2.GetComponent<DamageBox>().ResetCharacter();
        WeaponManager wm1 = player1.GetComponent<WeaponManager>();
        wm1.EquipWeaponByIndex(p1WeaponCycle);
        WeaponManager wm2 = player2.GetComponent<WeaponManager>();
        wm2.EquipWeaponByIndex(p2WeaponCycle);
        BrakeLever[] levers = GetComponentsInChildren<BrakeLever>();
    }

    public void PlayerDied(Fighter victim)
    {
        Fighter winner = (victim == player1) ? player2 : player1;
        winner.AddReward(0.5f);

        if (victim == player1) p1WeaponCycle = (p1WeaponCycle + 1) % 2;
        else p2WeaponCycle = (p2WeaponCycle + 1) % 2;

        player1.EndEpisode();
        player2.EndEpisode();
        ResetArea();
    }

    public void EndMatch(Fighter winner)
    {
        winner.AddReward(2.0f);
        Fighter loser = (winner == player1) ? player2 : player1;
        loser.AddReward(-1.0f);

        player1.EndEpisode();
        player2.EndEpisode();
        ResetArea();
    }
}
