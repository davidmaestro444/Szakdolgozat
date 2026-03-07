using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
public class Fighter : Agent
{
    public CharacterBase movement;
    public PlayerMovement playerMovement;
    public WeaponManager weaponManager;
    public TrainingArea area;
    public bool isPlayerOne;

    public Transform myTargetGoal;
    public Transform enemyBody;

    public override void OnEpisodeBegin()
    {

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float distanceToGoal = transform.position.x - myTargetGoal.position.x;
        sensor.AddObservation(distanceToGoal / 20f);
        Vector3 dirToEnemy = enemyBody.position - transform.position;
        sensor.AddObservation(dirToEnemy.normalized);
        sensor.AddObservation(dirToEnemy.magnitude / 20f);
        sensor.AddObservation(weaponManager.HasBow() ? 1 : 0);
        sensor.AddObservation(weaponManager.HasWeapon() ? 1 : 0);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        movement.AIMove(moveX);

        if (actions.DiscreteActions[0] == 1) movement.AIJump();

        if (actions.DiscreteActions[1] == 1) movement.PerformAttack();

        if (actions.DiscreteActions[2] == 1) movement.PerformThrow();

        if (actions.DiscreteActions[3] == 1 && playerMovement != null)
        {
            playerMovement.aiInteractTriggered = true;

            BrakeLever[] levers = FindObjectsByType<BrakeLever>(FindObjectsSortMode.None);
            foreach (var lever in levers)
            {
                if (Vector2.Distance(transform.position, lever.transform.position) < 1.5f)
                {
                    lever.TryActivate(playerMovement);
                }
            }
        }

        AddReward(-0.0005f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == (isPlayerOne ? "PlayerGoal" : "OpponentGoal"))
        {
            AddReward(2.0f);
            area.EndMatch(this);
        }

        if (other.CompareTag("DeathZone"))
        {
            AddReward(-1.0f);
            area.PlayerDied(this);
        }
    }
}
