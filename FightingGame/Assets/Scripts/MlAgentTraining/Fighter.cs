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
    private DamageBox enemyDamageBox;
    private int lastJumpAction = 0;
    private bool enemyEventsSubscribed = false;
    private float lastKnownDirection = 1f;

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
    public override void OnEpisodeBegin()
    {
        lastJumpAction = 0;
    }

    void OnAgentDeath()
    {
        AddReward(-1.0f);
        EndEpisode();
    }

    void OnEnemyKilled()
    {
        AddReward(0.5f);
    }

    void OnEnemyHit()
    {
        AddReward(0.1f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (enemyDamageBox == null)
        {
            string enemyTag = isPlayerOne ? "Enemy" : "Player";
            GameObject enemyObj = GameObject.FindGameObjectWithTag(enemyTag);
            if (enemyObj != null)
            {
                enemyBody = enemyObj.transform;
                enemyDamageBox = enemyObj.GetComponent<DamageBox>();

                if (enemyDamageBox != null && !enemyEventsSubscribed)
                {
                    enemyDamageBox.OnDeath += OnEnemyKilled;
                    enemyDamageBox.OnHit += OnEnemyHit;
                    enemyEventsSubscribed = true;
                }
            }
        }

        if (enemyBody == null || myTargetGoal == null)
        {
            for (int i = 0; i < 12; i++) sensor.AddObservation(0f);
            return;
        }

        bool isEnemyAlive = enemyBody.gameObject.activeInHierarchy && !enemyDamageBox.isDead;
        sensor.AddObservation(isEnemyAlive ? 1f : 0f);
        float distanceToGoal = transform.position.x - myTargetGoal.position.x;
        sensor.AddObservation(distanceToGoal / 20f);
        Vector3 dirToEnemy = enemyBody.position - transform.position;
        sensor.AddObservation(dirToEnemy.normalized);
        sensor.AddObservation(dirToEnemy.magnitude / 20f);
        sensor.AddObservation(weaponManager.HasBow() ? 1f : 0f);
        sensor.AddObservation(weaponManager.HasWeapon() ? 1f : 0f);
        sensor.AddObservation(movement.IsOnHighGround ? 1f : 0f);
        float myFacingDir = movement.transform.localScale.x > 0 ? 1f : -1f;
        sensor.AddObservation(myFacingDir);

        float distToNearestLever = 20f;
        BrakeLever[] levers = FindObjectsByType<BrakeLever>(FindObjectsSortMode.None);
        foreach (var lever in levers)
        {
            float d = Vector3.Distance(enemyBody.position, lever.transform.position);
            if (d < distToNearestLever) distToNearestLever = d;
        }
        sensor.AddObservation(distToNearestLever / 20f);

        /*float velocityX = movement.GetComponent<Rigidbody2D>().linearVelocity.x;
        if (Mathf.Abs(velocityX) > 0.1f)
        {
            lastKnownDirection = Mathf.Sign(velocityX);
        }

        Vector2 gapCheckPos = new Vector2(transform.position.x + (lastKnownDirection * 1.5f), transform.position.y);
        RaycastHit2D groundHit = Physics2D.Raycast(gapCheckPos, Vector2.down, 5f, LayerMask.GetMask("Ground"));
        bool isGapAhead = groundHit.collider == null;
        sensor.AddObservation(isGapAhead ? 1f : 0f);
        //Debug.DrawRay(gapCheckPos, Vector2.down * 5f, isGapAhead ? Color.red : Color.green);*/
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (damageBox != null && damageBox.isDead) return;
        if (!movement.enabled) return;

        float moveX = actions.ContinuousActions[0];
        movement.AIMove(moveX);

        int currentJump = actions.DiscreteActions[0];
        if (currentJump == 1 && lastJumpAction == 0) movement.AIJump();
        lastJumpAction = currentJump;

        bool isAttacking = actions.DiscreteActions[1] == 1;
        bool isThrowing = actions.DiscreteActions[2] == 1;

        if (isAttacking) movement.PerformAttack();
        if (isThrowing) movement.PerformThrow();
        if (actions.DiscreteActions[3] == 1 && playerMovement != null) playerMovement.aiInteractTriggered = true;

        //AddReward(0.0001f);

        if (weaponManager.HasWeapon())
        {
            AddReward(0.0005f);
        }

        if (enemyDamageBox != null && enemyDamageBox.isDead)
        {
            if (isAttacking || isThrowing)
            {
                AddReward(-0.01f);
            }

            float velocityTowardsGoal = (isPlayerOne ? 1f : -1f) * movement.GetComponent<Rigidbody2D>().linearVelocity.x;
            if (velocityTowardsGoal > 0.1f)
            {
                AddReward(0.005f);
            }
            else
            {
                AddReward(-0.002f);
            }
        }
        else if (enemyBody != null)
        {
            if (isAttacking || isThrowing)
            {
                AddReward(-0.0005f);
            }

            AddReward(-0.0002f);

            float distToEnemy = Vector3.Distance(transform.position, enemyBody.position);
            float dirToEnemySign = Mathf.Sign(enemyBody.position.x - transform.position.x);
            float myFacingDirection = movement.transform.localScale.x > 0 ? 1f : -1f;
            bool isFacingEnemy = (dirToEnemySign == myFacingDirection);
            float optimalAttackRange = weaponManager.HasBow() ? 7.0f : 2.0f;

            if (distToEnemy > optimalAttackRange)
            {
                float velocityTowardsEnemy = movement.GetComponent<Rigidbody2D>().linearVelocity.x * dirToEnemySign;
                if (velocityTowardsEnemy > 0.1f)
                {
                    AddReward(0.0005f);
                }
            }
            else
            {
                if (isFacingEnemy && (isAttacking || isThrowing))
                {
                    AddReward(0.01f);
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (myTargetGoal != null && other.gameObject == myTargetGoal.gameObject)
        {
            AddReward(1.0f);
            EndEpisode();
        }
    }

    void OnDestroy()
    {
        if (damageBox != null) damageBox.OnDeath -= OnAgentDeath;
        if (enemyDamageBox != null)
        {
            enemyDamageBox.OnDeath -= OnEnemyKilled;
            enemyDamageBox.OnHit -= OnEnemyHit;
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
