using UnityEngine;

public class PlayerController : MonoBehaviour
{
    PlayerStateList pState;

    [SerializeField] MovementHandler movementHandler = new();
    [SerializeField] JumpHandler jumpHandler = new();
    [SerializeField] AttackHandler attackHandler = new();
    [SerializeField] RecoilHandler recoilHandler = new();
    [SerializeField] CollisionChecker groundCheck = new();
    [SerializeField] CollisionChecker ceilingCheck = new();
    [SerializeField] DodgeHandler dodgeHandler = new();
    [SerializeField] GravityHandler gravityWarper = new();
    
    float facing = 1;

    Controls c;

    Rigidbody2D rb;
    [SerializeField] Animator anim;

    private void Awake()
    {
        c = new();
        c.Enable();
    }

    // Use this for initialization
    public void Start()
    {
        pState = GetComponent<PlayerStateList>();
        rb = GetComponent<Rigidbody2D>();

        gravityWarper.Init(this);
        jumpHandler.Init(this);
        attackHandler.Init(this);
        movementHandler.Init(this);
        recoilHandler.Init(this);
        dodgeHandler.Init(this);

        groundCheck.Init(Vector2.down);
        ceilingCheck.Init(Vector2.up);
    }

    // Update is called once per frame
    public void Update()
    {
        groundCheck.Update();
        ceilingCheck.Update();

        jumpHandler.Update();

        if (!(attackHandler.Attacking || dodgeHandler.Dodging))
        {
            UpdateFacing();
            movementHandler.Update();
        }

        recoilHandler.Update();

        if (!attackHandler.Attacking) dodgeHandler.Update();
        if (!dodgeHandler.Dodging) attackHandler.Update();

        anim.SetBool("Grounded", groundCheck.Succeeded);
        anim.SetFloat("YVelocity", rb.velocity.y);
    }

    public void FixedUpdate()
    {
        if (!attackHandler.Attacking) jumpHandler.FixedUpdate();
    }

    void UpdateFacing()
    {
        float x = c.Player.DPad.ReadValue<Vector2>().x;
        if (x != 0)
        {
            facing = Mathf.Sign(x);
            transform.localScale = new Vector2(facing, transform.localScale.y);
        }
    }

    public void OnDrawGizmos()
    {
        attackHandler.OnGizmos();
        groundCheck.OnGizmos();
        ceilingCheck.OnGizmos();
    }

    [System.Serializable]
    class JumpHandler
    {
        [SerializeField] float jumpSpeed = 45;
        [SerializeField] float fallSpeed = 45;
        [SerializeField] float maxJumpDuration = 20;
        [SerializeField] float minJumpDuration = 7;

        PlayerController script;

        float jumpTime = 0;

        public bool Jumping { get; private set; }

        public void Init(PlayerController script)
        {
            this.script = script;
        }

        public void Update()
        {
            if (Jumping) jumpTime += Time.deltaTime;

            bool jump = script.c.Player.Jump.IsPressed();

            //Jumping
            if (jump && script.groundCheck.Succeeded)
            {
                Jumping = true;
                jumpTime = 0;
            }

            if (Jumping && !jump)
            {
                bool pastMin = jumpTime > minJumpDuration;
                bool beforeMin = jumpTime <= minJumpDuration;
                bool midJump = jumpTime < maxJumpDuration;

                if (midJump && pastMin)
                {
                    StopJumpQuick();
                }
                else if (beforeMin)
                {
                    StopJumpSlow();
                }
            }
        }

        public void FixedUpdate()
        {
            if (Jumping)
            {
                if (jumpTime < maxJumpDuration && !script.ceilingCheck.Succeeded)
                {
                    script.rb.velocity *= Vector2.right;
                    script.rb.velocity += Vector2.up * jumpSpeed;
                }
                else
                {
                    StopJumpSlow();
                }
            }

            //This limits how fast the player can fall
            //Since platformers generally have increased gravity, you don't want them to fall so fast they clip trough all the floors.
            if (script.rb.velocity.y < -Mathf.Abs(fallSpeed))
            {
                script.rb.velocity *= Vector2.right;
                script.rb.velocity += Vector2.down * fallSpeed;
            }
        }

        void StopJumpQuick()
        {
            //Stops The player jump immediately, causing them to start falling as soon as the button is released.
            Jumping = false;
            script.rb.velocity *= Vector2.right;
        }

        void StopJumpSlow()
        {
            //stops the jump but lets the player hang in the air for awhile.
            Jumping = false;
        }
    }

    [System.Serializable]
    class AttackHandler
    {
        [SerializeField] float airAttackGravityMultiplier = 0.1f;
        [SerializeField] float timeBetweenAttack = 0.4f;
        [SerializeField] float attackDuration = 0.5f;
        [SerializeField] Transform attackTransform; // this should be a transform childed to the player but to the right of them, where they attack from.
        [SerializeField] float attackRadius = 1;
        [SerializeField] Transform downAttackTransform;//This should be a transform childed below the player, for the down attack.
        [SerializeField] float downAttackRadius = 1;
        [SerializeField] Transform upAttackTransform;//Same as above but for the up attack.
        [SerializeField] float upAttackRadius = 1;
        [SerializeField] LayerMask attackableLayer;
        [SerializeField] float attackStepVelocity = 2;

        public bool Attacking { get; private set; }
        public bool LastAttackLanded { get; private set; }
        public bool WasAirAttack { get; private set; }

        float timeSinceAttack;

        PlayerController script;

        public void Init(PlayerController script)
        {
            this.script = script;
        }

        public void Update()
        {
            Vector2 dinput = script.c.Player.DPad.ReadValue<Vector2>();
            bool attack = script.c.Player.Attack.WasPressedThisFrame();

            timeSinceAttack += Time.deltaTime;
            Attacking = timeSinceAttack <= attackDuration;
            bool cooldownFinished = timeSinceAttack >= timeBetweenAttack;

            // Freeze position for air attacks;
            if (Attacking && LastAttackLanded)
            {
                script.gravityWarper.SetWarp(airAttackGravityMultiplier);
            }
            else
            {
                script.gravityWarper.Reset();
            }

            if (attack && cooldownFinished)
            {
                timeSinceAttack = 0;

                //Attack Side
                if (dinput.y == 0 || dinput.y < 0 && script.groundCheck.Succeeded)
                {
                    script.UpdateFacing();
                    script.rb.velocity = new Vector2(script.facing * attackStepVelocity, script.rb.velocity.y);

                    script.anim.SetTrigger("Attack Side");
                    Collider2D[] objectsToHit = Physics2D.OverlapCircleAll(attackTransform.position, attackRadius, attackableLayer);
                    LastAttackLanded = objectsToHit.Length > 0;
                    WasAirAttack = !script.groundCheck.Succeeded;
                    if (LastAttackLanded)
                    {
                        if (WasAirAttack)
                        {
                            script.rb.velocity = Vector2.zero;
                        }
                        script.pState.recoilingX = true;
                    }
                    for (int i = 0; i < objectsToHit.Length; i++)
                    {
                        //Here is where you would do whatever attacking does in your script.
                        //In my case its passing the Hit method to an Enemy script attached to the other object(s).
                    }
                }
                //Attack Up
                else if (dinput.y > 0)
                {
                    //anim.SetTrigger("2");
                    Collider2D[] objectsToHit = Physics2D.OverlapCircleAll(upAttackTransform.position, upAttackRadius, attackableLayer);
                    if (objectsToHit.Length > 0)
                    {
                        script.pState.recoilingY = true;
                    }
                    for (int i = 0; i < objectsToHit.Length; i++)
                    {
                        //Here is where you would do whatever attacking does in your script.
                        //In my case its passing the Hit method to an Enemy script attached to the other object(s).
                    }
                }
                //Attack Down
                else if (dinput.y < 0 && !script.groundCheck.Succeeded)
                {
                    //anim.SetTrigger("3");
                    Collider2D[] objectsToHit = Physics2D.OverlapCircleAll(downAttackTransform.position, downAttackRadius, attackableLayer);
                    if (objectsToHit.Length > 0)
                    {
                        script.pState.recoilingY = true;
                    }
                    for (int i = 0; i < objectsToHit.Length; i++)
                    {

                        //Here I commented out the actual script I use, in case you wanted to see it.


                        /*Instantiate(slashEffect1, objectsToHit[i].transform);
                        if (!(objectsToHit[i].GetComponent<EnemyV1>() == null))
                        {
                            objectsToHit[i].GetComponent<EnemyV1>().Hit(damage, 0, -YForce);
                        }

                        if (objectsToHit[i].tag == "Enemy")
                        {
                            Mana += ManaGain;
                        }*/
                    }
                }

            }
        }

        public void OnGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackTransform.position, attackRadius);
            Gizmos.DrawWireSphere(downAttackTransform.position, downAttackRadius);
            Gizmos.DrawWireSphere(upAttackTransform.position, upAttackRadius);
        }
    }

    [System.Serializable]
    class CollisionChecker
    {
        [SerializeField] Transform checkTransform; //This is supposed to be a transform childed to the player just under their collider.
        [SerializeField] float checkHeight = 0.2f; //How far on the Y axis the groundcheck Raycast goes.
        [SerializeField] float checkHalfWidth = 1;//Same as above but for X.
        [SerializeField] LayerMask layerMask;

        Vector2 checkUnitVector;

        public bool Succeeded { get; private set; }

        public void Init(Vector2 checkUnitVector)
        {
            this.checkUnitVector = checkUnitVector;
        }

        bool Raycast(float xOffset)
        {
            return Physics2D.Raycast(checkTransform.position + Vector3.right * xOffset, checkUnitVector, checkHeight, layerMask);
        }

        public void Update()
        {
            Succeeded = Raycast(0) || Raycast(checkHalfWidth) || Raycast(-checkHalfWidth);
        }

        public void OnGizmos() {
            Gizmos.DrawLine(checkTransform.position, checkTransform.position + new Vector3(0, -checkHeight));
            Gizmos.DrawLine(checkTransform.position + new Vector3(-checkHalfWidth, 0), checkTransform.position + new Vector3(-checkHalfWidth, -checkHeight));
            Gizmos.DrawLine(checkTransform.position + new Vector3(checkHalfWidth, 0), checkTransform.position + new Vector3(checkHalfWidth, -checkHeight));
        }
    }

    [System.Serializable]
    class GravityHandler
    {
        [SerializeField] float baseGravityScale;

        PlayerController script;

        public void Init(PlayerController script)
        {
            this.script = script;
        }

        public void SetWarp(float multiplier)
        {
            script.rb.gravityScale = baseGravityScale * multiplier;
        }

        public void Reset() => SetWarp(1);
    }

    [System.Serializable]
    class MovementHandler
    {
        [SerializeField] float walkSpeed = 25f;

        PlayerController script;

        public void Init(PlayerController script)
        {
            this.script = script;
        }

        public void Update()
        {
            Vector2 dinput = script.c.Player.DPad.ReadValue<Vector2>();

            if (!script.pState.recoilingX)
            {
                script.rb.velocity *= Vector2.up;
                script.rb.velocity += dinput.x * walkSpeed * Vector2.right;

                bool walking = Mathf.Abs(script.rb.velocity.x) > 0;
                script.anim.SetBool("Walking", walking);

                script.pState.walking = walking;

                if (dinput.x != 0)
                {
                    script.pState.lookingRight = dinput.x > 0;
                }
            }
        }
    }

    [System.Serializable]
    class RecoilHandler
    {
        [SerializeField] float xRecoilDuration = 4;
        [SerializeField] float yRecoilDuration = 10;
        [SerializeField] float recoilXSpeed = 45;
        [SerializeField] float recoilYSpeed = 45;

        float xRecoilTime;
        float yRecoilTime;

        PlayerController script;

        public void Init(PlayerController script)
        {
            this.script = script;
        }

        public void Update()
        {
            Vector2 dinput = script.c.Player.DPad.ReadValue<Vector2>();

            if (script.pState.recoilingX)
            {
                xRecoilTime += Time.deltaTime;
                if (xRecoilTime > xRecoilDuration) StopRecoilX();

                int looking = script.pState.lookingRight ? -1 : 1;
                script.rb.velocity = looking * recoilXSpeed * Vector2.right;
            }

            if (script.pState.recoilingY)
            {
                yRecoilTime += Time.deltaTime;
                if (yRecoilTime > yRecoilDuration || script.groundCheck.Succeeded) StopRecoilY();

                float dir = -Mathf.Sign(dinput.y);
                script.rb.velocity *= Vector2.right;
                script.rb.velocity += dir * recoilYSpeed * Vector2.up;
                script.gravityWarper.SetWarp(0);
            }
            else
            {
                script.gravityWarper.Reset();
            }
        }

        void StopRecoilX()
        {
            xRecoilTime = 0;
            script.pState.recoilingX = false;
        }

        void StopRecoilY()
        {
            yRecoilTime = 0;
            script.pState.recoilingY = false;
        }
    }

    [System.Serializable]
    class DodgeHandler
    {
        [SerializeField] float dodgeForce = 10;
        [SerializeField] float dodgeInvincibilityDuration = 0.5f;
        [SerializeField] float timeBetweenDodges = 1f;

        public bool Dodging { get; private set; }
        bool dodgeInvincible;
        bool dodgedInAir;
        float dodgeTimer = 0;

        PlayerController script;

        public void Init(PlayerController script)
        {
            this.script = script;
        }

        public void Update()
        {
            if (Dodging)
            {
                dodgeTimer += Time.deltaTime;
                dodgeInvincible = dodgeTimer < dodgeInvincibilityDuration;
                Dodging = dodgeTimer < timeBetweenDodges;
            }
            else
            {
                if (script.c.Player.Roll.WasPressedThisFrame())
                {
                    Dodging = true;
                    dodgedInAir = !script.groundCheck.Succeeded;
                    dodgeTimer = 0;
                    script.anim.SetTrigger("Roll");
                }
            }
        }
    }
}