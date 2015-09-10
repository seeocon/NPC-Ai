using UnityEngine;
using System.Collections;

public class NPCAi : MonoBehaviour {

    // assign public vars in inspector menu
    public Transform player;
    public float attackDistance = 10.0f;
    public bool showCurrentState;
    public int maxHealth = 100;
    public int curHealth = 100;

    private float walkSpeed;
    private float turnSpeed = 10.0f;
    private Animator anim;
    private Vector3 wayPoint;
    private float circleRadius = 15.0f;
    private Vector3 circleCenter;
    private float minDistance = 1.0f;
    private Vector3 currentTargetPosition;
    private float waitTime;
    private float waitTimer;
    private GameObject pHealth;
    private GameObject tSystem;
    private CharacterController playerController;
    private State currentState;

    enum State {
        WAITING,
        MOVING,
        ATTACKING
    }


    void Start() {
        playerController = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        waitTimer = 0.0f;
        circleCenter = transform.position;
        FindNewWaypoint();
        currentState = State.WAITING;
        pHealth = GameObject.Find("Player");
        tSystem = GameObject.Find("Player");
    }

    void Update() {
        if (curHealth > 0) {

            if (showCurrentState)
                Debug.Log(currentState);

            if (Vector3.Distance(transform.position, player.position) <= attackDistance && Vector3.Distance(transform.position, player.position) >= 2f) {
                currentState = State.ATTACKING;
                anim.SetTrigger("inDistance");
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(player.position - transform.position), turnSpeed * Time.deltaTime);

                Vector3 moveVector2 = (player.transform.position - transform.position).normalized * walkSpeed * Time.deltaTime;
                transform.position += moveVector2;
                playerController.Move(walkSpeed * moveVector2);

            } else if (Vector3.Distance(transform.position, player.position) <= attackDistance && Vector3.Distance(transform.position, player.position) <= 3f) {
                currentState = State.ATTACKING;
                anim.SetTrigger("attack");
                PlayerHealth p_Health = (PlayerHealth)pHealth.GetComponent(typeof(PlayerHealth));
                p_Health.AdjustCurrentHealth(2);

            }

            switch (currentState) {
                case State.WAITING:
                    anim.SetTrigger("idle");
                    waitTime = Random.Range(4, 17);
                    Wait();
                    break;
                case State.MOVING:
                    anim.SetTrigger("inDistance");
                    walkSpeed = 1.5f;
                    MoveTowardWaypoint();
                    break;
                case State.ATTACKING:
                    walkSpeed = 1.25f;
                    if (Vector3.Distance(transform.position, player.position) >= attackDistance) {
                        currentState = State.WAITING;
                    }
                    break;
            }
        } else {
            anim.SetTrigger("dead");
            TargetSystem t_System = (TargetSystem)tSystem.GetComponent(typeof(TargetSystem));
            t_System.killed();
            Destroy(this.gameObject, .9f);
        }

    }

    // Find a new waypoint
    void FindNewWaypoint() {
        currentTargetPosition = circleCenter + (OnUnitCircle() * circleRadius);
    }

    // Assigns the bounds
    Vector3 OnUnitCircle() {
		float angleInRadians = Random.Range(0, 2*Mathf.PI);
		float x = Mathf.Cos(angleInRadians);
		float z = Mathf.Sin(angleInRadians);
		return new Vector3(x, 0.0f, z);
	}

    // Move to the waypoint
    void MoveTowardWaypoint() {
        Vector3 direction = currentTargetPosition - transform.position;
        direction.y = 0.0f;
        if (direction.magnitude > minDistance + 10f && currentState != State.ATTACKING) {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
            Vector3 moveVector = direction.normalized * walkSpeed * Time.deltaTime;
            playerController.Move(walkSpeed * moveVector);
        } else if (currentState != State.ATTACKING) {
            FindNewWaypoint();
            currentState = State.WAITING;
        }
    }

    // Wait to walk to another waypoint
    void Wait() {
        waitTimer += Time.deltaTime;
        if (waitTimer > waitTime) {
            waitTimer = 0.0f;
            currentState = State.MOVING;
        }
    }

    // Method for applying damage, which is called in another script
    void ApplyDamage() {
        anim.SetTrigger("hit");
        curHealth -= 10;
        //Debug.Log(curHealth);
    }

    // Draw gizmos on the scene screen for the waypoint loc
    void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(circleCenter, circleRadius);
        Gizmos.DrawCube(currentTargetPosition, new Vector3(4, 4, 4));
    }
}
