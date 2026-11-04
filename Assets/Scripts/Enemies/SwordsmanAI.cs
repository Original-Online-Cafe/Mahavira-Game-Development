using UnityEngine;

// Swordsman enemy brain: Patrol -> Alert -> Chase -> Attack, and Return home when the player escapes.
[RequireComponent(typeof(Animator))]
public class SwordsmanAI : MonoBehaviour
{
    private enum State { Patrol, Alert, Chase, Attack, Return }

    [Header("Target")]
    [SerializeField] private Transform player;              // drag the hero here (or tag the hero "Player")

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float patrolDistance = 3f;     // how far he walks from home while patrolling

    [Header("Ranges")]
    [SerializeField] private float senseRange = 5f;         // notices the player
    [SerializeField] private float giveUpRange = 7f;        // stops chasing (keep bigger than senseRange)
    [SerializeField] private float combatRange = 1.2f;      // close enough to attack
    [SerializeField] private float maxHeightDifference = 2f;// ignores the player if far above or below

    [Header("Timing")]
    [SerializeField] private float alertDuration = 0.4f;    // pause after "!" before charging
    [SerializeField] private float attackDuration = 0.7f;   // match the length of the attack animation
    [SerializeField] private float attackCooldown = 0.6f;   // rest time between attacks

    [Header("Visuals")]
    [SerializeField] private GameObject alertIcon;          // optional "!" above his head
    [SerializeField] private string walkAnimation = "Swordsman_Walk";
    [SerializeField] private string attackAnimation = "Swordsman_Attack";

    private Animator animator;
    private Vector3 homePosition;
    private State state = State.Patrol;
    private int patrolDirection = 1;
    private float stateTimer;
    private float cooldownTimer;
    private string currentAnimation;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        homePosition = transform.position;

        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }

        if (alertIcon != null) alertIcon.SetActive(false);
        PlayAnimation(walkAnimation);
    }

    private void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        switch (state)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.Alert:  UpdateAlert();  break;
            case State.Chase:  UpdateChase();  break;
            case State.Attack: UpdateAttack(); break;
            case State.Return: UpdateReturn(); break;
        }
    }

    // ---------- States ----------

    private void UpdatePatrol()
    {
        if (CanSensePlayer(senseRange)) { EnterAlert(); return; }

        float offset = transform.position.x - homePosition.x;
        if (offset >= patrolDistance) patrolDirection = -1;
        else if (offset <= -patrolDistance) patrolDirection = 1;

        Move(patrolDirection, patrolSpeed);
    }

    private void UpdateAlert()
    {
        FacePlayer();
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            if (alertIcon != null) alertIcon.SetActive(false);
            state = State.Chase;
        }
    }

    private void UpdateChase()
    {
        if (!CanSensePlayer(giveUpRange)) { state = State.Return; return; }

        FacePlayer();

        if (HorizontalDistanceToPlayer() <= combatRange)
        {
            if (cooldownTimer <= 0f) EnterAttack();   // in range and rested: attack
            return;                                   // in range but resting: stand and wait
        }

        Move(DirectionToPlayer(), chaseSpeed);
    }

    private void UpdateAttack()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            cooldownTimer = attackCooldown;
            PlayAnimation(walkAnimation);
            state = State.Chase;
        }
    }

    private void UpdateReturn()
    {
        if (CanSensePlayer(senseRange)) { EnterAlert(); return; }

        float offset = homePosition.x - transform.position.x;
        if (Mathf.Abs(offset) < 0.1f) { state = State.Patrol; return; }

        Move(offset > 0f ? 1 : -1, patrolSpeed);
    }

    // ---------- State entry ----------

    private void EnterAlert()
    {
        state = State.Alert;
        stateTimer = alertDuration;
        if (alertIcon != null) alertIcon.SetActive(true);
    }

    private void EnterAttack()
    {
        state = State.Attack;
        stateTimer = attackDuration;
        PlayAnimation(attackAnimation, restart: true);
        Debug.Log($"{name} attacks!");   // placeholder until we add damage
    }

    // ---------- Helpers ----------

    private bool CanSensePlayer(float range)
    {
        if (player == null) return false;
        float heightDifference = Mathf.Abs(player.position.y - transform.position.y);
        return HorizontalDistanceToPlayer() <= range && heightDifference <= maxHeightDifference;
    }

    private float HorizontalDistanceToPlayer() => Mathf.Abs(player.position.x - transform.position.x);

    private int DirectionToPlayer() => player.position.x > transform.position.x ? 1 : -1;

    private void FacePlayer() => Face(DirectionToPlayer());

    private void Move(int direction, float speed)
    {
        transform.position += Vector3.right * direction * speed * Time.deltaTime;
        Face(direction);
    }

    // Same flipping method as player_script, so all characters behave the same.
    private void Face(int direction)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

    private void PlayAnimation(string animationName, bool restart = false)
    {
        if (!restart && currentAnimation == animationName) return;
        animator.Play(animationName, 0, 0f);
        currentAnimation = animationName;
    }

    private void OnValidate()
    {
        if (giveUpRange < senseRange) giveUpRange = senseRange;
        if (combatRange > senseRange) combatRange = senseRange;
    }

    // Shows the ranges in the Scene view: yellow = sense, orange = give up, red = combat.
    private void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position;
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(center, senseRange);
        Gizmos.color = new Color(1f, 0.5f, 0f); Gizmos.DrawWireSphere(center, giveUpRange);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(center, combatRange);
    }
}