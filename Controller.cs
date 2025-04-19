using UnityEngine;
using Unity.Burst;
using System.Threading.Tasks;
using System.Collections;



//? Normaldekinin aksine, pozisyon temelli
[BurstCompile]
public class Controller : MonoBehaviour
{
    #region Variables

        [SerializeField] private float GRAVITY = 9.81f;
        [SerializeField] private float SPEED;

        [SerializeField] private Transform position;
        [SerializeField] private Vector3 input;

        [SerializeField] private float accelerationOfG;


        [SerializeField] private Transform rightSidePos;
        [SerializeField] private Transform leftSidePos;

        [SerializeField] private Vector2 rightSideCollisionChecker;
        [SerializeField] private Vector2 leftSideCollisionChecker;

        [SerializeField] private Vector2 topCollisionChecker;
        [SerializeField] private Transform topCollisionCheckerTransform;



        #region Dash Variables
        [Header("Dash Variables")]
        [SerializeField] private bool isDashing;
        public bool isDashKeyPressed;
        [SerializeField] private int dir;

        [SerializeField] private float DASH_COOLDOWN;
        [SerializeField] private float DASH_SPEED;
        [SerializeField] private float DASH_TIME;
        [SerializeField] private float DASH_PRE_WAIT = 0.1f;
        #endregion Dash variables


        #region Jump Variables
        [Header("Jump Variables")]
        [SerializeField] private bool isJumping;
        public bool isJumpKeyPressed;
        [SerializeField] private bool isTouchingGround;
        [SerializeField] private uint jumpCount = 0;

        [SerializeField] private float JUMP_TIME;
        [SerializeField] private float JUMP_SPEED;

        [SerializeField] private Transform  groundCheckTransform;
        [SerializeField] private Vector2 groundCheckPointScale;
        #endregion Jump Variables

    #endregion Variables



    #region LifeCycle Methods
    private void Update()
    {
        TakeInput();

        if(isTouchingGround)
        {
            jumpCount = 0;
        }

        if(isJumpKeyPressed && !isJumping && jumpCount < 2 )
        {
           StartCoroutine(Jump());
        }

        if(isDashKeyPressed && !isDashing)
        {   
           StartCoroutine(Dash());
        }      

    }

    private void FixedUpdate()
    {

        if(input.x > 0)
        {
            if(!CheckForCollisions(rightSidePos.position, rightSideCollisionChecker))
            {
                Advance(input.x * SPEED, Vector2.right);
            }
        }
        if (input.x < 0)
        {
            if(!CheckForCollisions(leftSidePos.position, leftSideCollisionChecker))
            {
                Advance(input.x * SPEED, Vector2.right);
            }   
        }

        Gravity();

        isTouchingGround = CheckForCollisions(groundCheckTransform.position, groundCheckPointScale);
    }
    #endregion LifeCycle Methods


    #region Input
    protected void TakeInput()//TODO İleride ayrı bir sınıf olabilir
    {
        input.x = Input.GetAxisRaw("Horizontal");
        //input.y = Input.GetAxisRaw("Vertical");

        if(Input.GetKey(KeyCode.UpArrow))
        {
            isJumpKeyPressed = true;
        }
        else
        {
            isJumpKeyPressed = false;
        }


        if(Input.GetKey(KeyCode.LeftShift))
        {
            isDashKeyPressed = true;
        }
        else
        {
            isDashKeyPressed = false;
        }
        if(Input.GetKey(KeyCode.RightArrow))
        {
            dir = 1;
        }
        else if(Input.GetKey(KeyCode.LeftArrow))
        {
            dir = -1;
        }
    }
    #endregion Input

    protected void Gravity()
    {
        if(!isTouchingGround && !isJumping)
        {
            accelerationOfG += GRAVITY * Time.deltaTime;
        }
        else
        {
            accelerationOfG = 0;
            return;
        }

        accelerationOfG = Mathf.Clamp(accelerationOfG, 0, GRAVITY);

        if(!CheckForCollisions(groundCheckTransform.position, groundCheckPointScale))
        {
            Advance(accelerationOfG, Vector2.down);
        }
        else
        {
            jumpCount = 0;
        }
    }

    /// <summary>
    /// İlerle
    /// </summary>
    /// <param name="amount">İlerleme miktarı</param>
    /// <param name="dir">Yön</param>
    protected void Advance(float amount, Vector2 dir)
    {
        transform.position += (Vector3)(dir * amount * Time.deltaTime);
    }


    #region Collision Checking
    /// <summary>
    /// Çarpışma kontrolü
    /// </summary>
    /// <param name="pos">Çarpışma kontrol noktası</param>
    /// <param name="scale">Çarpışma kontrol alan büyüklüğü</param>
    /// <returns>Çarpışma var mı?</returns>
    protected bool CheckForCollisions(Vector2 pos, Vector2 scale)
    {   
        return Physics2D.OverlapBox(pos, scale, 0f);
    }

    
    #endregion Collision Checking



    #region Jump
    protected IEnumerator Jump()
    {
        isJumping = true;
            
        jumpCount++;

        float verticalAcceleration = JUMP_SPEED;
        float t = 0;
        
        while(t <= JUMP_TIME)
        {
            if(!CheckForCollisions(topCollisionCheckerTransform.position, topCollisionChecker))
            {
                Advance(verticalAcceleration, Vector2.up);
            }
            else
            {
                break;
            }

            verticalAcceleration -= JUMP_TIME * verticalAcceleration * .1f;

            t += Time.deltaTime;
            yield return null;;
        }

        isJumping = false;

        yield return null;
    }
    #endregion Jump


    #region Dash
    protected IEnumerator Dash()
    {
        isDashing = true;

            float t = 0;
            while(t <= DASH_PRE_WAIT)
            {
                if(Input.GetAxis("Horizontal") > 0)
                {
                    dir = 1;
                    break;
                }
                if(Input.GetAxis("Horizontal") < 0)
                {
                    dir = -1;
                    break;
                }

                t += Time.deltaTime;
                yield return null;
            }

            Vector2 v2 = new Vector2(dir, 0);
            
            t = 0;
            while(t <= DASH_TIME)
            {
                if(dir == 1)
                {
                    if(!CheckForCollisions(rightSidePos.position, rightSideCollisionChecker))
                    {
                        Advance(DASH_SPEED, v2);
                    }
                }
                else if(dir == -1)
                {
                    if(!CheckForCollisions(leftSidePos.position, leftSideCollisionChecker))
                    {
                        Advance(DASH_SPEED, v2);
                    }
                }     

                t += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(DASH_COOLDOWN);
            isDashing = false;
    }
    #endregion Dash



    #region Debug
    protected void OnDrawGizmos()
    {
        Gizmos.color = isTouchingGround ? Color.green : Color.red;

        Gizmos.DrawCube(groundCheckTransform.position, groundCheckPointScale);

        Gizmos.color = CheckForCollisions(rightSidePos.position, rightSideCollisionChecker) ? Color.red : Color.cyan;
        Gizmos.DrawCube(rightSidePos.position, rightSideCollisionChecker);

        Gizmos.color = CheckForCollisions(leftSidePos.position, leftSideCollisionChecker) ? Color.red : Color.cyan;
        Gizmos.DrawCube(leftSidePos.position, leftSideCollisionChecker);

        Gizmos.color = CheckForCollisions(topCollisionCheckerTransform.position, topCollisionChecker) ? Color.red : Color.yellow;
        Gizmos.DrawCube(topCollisionCheckerTransform.position, topCollisionChecker);
        
    }
    #endregion Debug


}
