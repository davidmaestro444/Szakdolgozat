using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
public class Fighter : Agent
{
    public CharacterBase movement;
    public PlayerMovement playerMovement;
    public WeaponManager weaponManager;
    private DamageBox damageBox;

    public bool isPlayerOne;

    private Transform myTargetGoal;
    private Transform enemyBody;
    private int lastJumpAction = 0;

    public override void Initialize()
    {
        damageBox = GetComponent<DamageBox>();
        if (damageBox != null) damageBox.OnDeath += OnAgentDeath;

        if (playerMovement != null) playerMovement.isAI = true;

        if (isPlayerOne)
            myTargetGoal = GameObject.Find("PlayerGoal").transform;
        else
            myTargetGoal = GameObject.Find("OpponentGoal").transform;
    }

    void OnAgentDeath()
    {
        AddReward(-1.0f);
        EndEpisode();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (enemyBody == null)
        {
            string enemyTag = isPlayerOne ? "Enemy" : "Player";
            GameObject enemyObj = GameObject.FindGameObjectWithTag(enemyTag);
            if (enemyObj != null) enemyBody = enemyObj.transform;
        }

        if (enemyBody == null || myTargetGoal == null)
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            return;
        }

        float distanceToGoal = transform.position.x - myTargetGoal.position.x;
        sensor.AddObservation(distanceToGoal / 20f);
        Vector3 dirToEnemy = enemyBody.position - transform.position;
        sensor.AddObservation(dirToEnemy.normalized);
        sensor.AddObservation(dirToEnemy.magnitude / 20f);
        sensor.AddObservation(weaponManager.HasBow() ? 1f : 0f);
        sensor.AddObservation(weaponManager.HasWeapon() ? 1f : 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (damageBox != null && damageBox.isDead) return;
        if (!movement.enabled) return;

        float moveX = actions.ContinuousActions[0];
        movement.AIMove(moveX);

        int currentJump = actions.DiscreteActions[0];
        if (currentJump == 1 && lastJumpAction == 0)
        {
            movement.AIJump();
        }
        lastJumpAction = currentJump;

        if (actions.DiscreteActions[1] == 1) movement.PerformAttack();
        if (actions.DiscreteActions[2] == 1) movement.PerformThrow();

        if (actions.DiscreteActions[3] == 1 && playerMovement != null)
        {
            playerMovement.aiInteractTriggered = true;
        }

        AddReward(-0.0005f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (myTargetGoal != null && other.gameObject == myTargetGoal.gameObject)
        {
            AddReward(2.0f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        var discreteActions = actionsOut.DiscreteActions;

        continuousActions[0] = 0f;
        discreteActions[0] = 0;
        discreteActions[1] = 0;
        discreteActions[2] = 0;
        discreteActions[3] = 0;

        if (isPlayerOne)
        {
            if (Input.GetKey(KeyCode.D)) continuousActions[0] = 1f;
            if (Input.GetKey(KeyCode.A)) continuousActions[0] = -1f;
            if (Input.GetKey(KeyCode.W)) discreteActions[0] = 1;
            if (Input.GetKey(KeyCode.Space)) discreteActions[1] = 1;
            if (Input.GetKey(KeyCode.Q)) discreteActions[2] = 1;
            if (Input.GetKey(KeyCode.F)) discreteActions[3] = 1;
        }
        else
        {
            if (Input.GetKey(KeyCode.RightArrow)) continuousActions[0] = 1f;
            if (Input.GetKey(KeyCode.LeftArrow)) continuousActions[0] = -1f;
            if (Input.GetKey(KeyCode.UpArrow)) discreteActions[0] = 1;
            if (Input.GetKey(KeyCode.Keypad1)) discreteActions[1] = 1;
            if (Input.GetKey(KeyCode.Keypad3)) discreteActions[2] = 1;
            if (Input.GetKey(KeyCode.Keypad2)) discreteActions[3] = 1;
        }
    }
}
