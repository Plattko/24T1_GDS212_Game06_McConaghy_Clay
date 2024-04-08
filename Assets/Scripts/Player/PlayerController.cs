using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Plattko
{
    public class PlayerController : MonoBehaviour
    {
        private Rigidbody2D rb;
        private Transform sprite;
        private Transform groundCheck;
        private Transform ledgeCheck;
        private Animator animator;
        private PlayerHearts playerHearts;

        // Collision check variables
        private int groundLayer = 6;
        private int wallLayer = 7;
        private LayerMask groundLayerMask;
        [HideInInspector] public LayerMask wallLayerMask;
        private Collider2D wallCollider;

        private bool canCheckLedge = true;
        [HideInInspector] public bool isLedgeDetected;

        [Header("Movement Variables")]
        private float moveInput;
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float acceleration = 7f;
        [SerializeField] private float deceleration = 7f;
        [SerializeField] private float velPower = 0.9f;
        [SerializeField] private float friction = 0.25f;

        private bool canFlip = true;

        [Header("Jump Variables")]
        [SerializeField] private float jumpPower = 8f;
        [SerializeField] private float coyoteTime = 0.15f;
        private float coyoteTimeCounter;

        [SerializeField] private float gravityScale = 1f;
        [SerializeField] private float gravityFallMultiplier = 1.9f;

        [Header("Wall-Slide Variables")]
        [SerializeField] private float wallSlideSpeed = 4f;
        private bool isWallSliding;

        [Header("Wall-Jump Variables")]
        [SerializeField] private Vector2 wallJumpPower;
        [SerializeField] private float wallJumpCoyoteTime = 0.15f;
        private float wallJumpCoyoteTimeCounter;

        [SerializeField] private float wallJumpTime = 0.75f;
        private float wallJumpEndTime;
        [SerializeField] private float wallJumpMoveLerp = 0.2f;
        private int wallJumpDirection;
        private bool isWallJumping;

        [Header("Mantle Variables")]
        [SerializeField] private Vector2 startOffset;
        [SerializeField] private Vector2 endOffset;

        private Vector2 mantleStartPosition;
        private Vector2 mantleEndPosition;

        private bool canMantle;

        private Transform ledge;
        private Vector2 ledgeStartPosition;

        [Header("Damage Bounce Variables")]
        [SerializeField] private float damageBounceForce = 25f;

        private bool isInDamageBounce;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            sprite = transform.GetChild(0).gameObject.GetComponent<Transform>();
            groundCheck = transform.GetChild(1).gameObject.GetComponent<Transform>();
            ledgeCheck = transform.GetChild(2).gameObject.GetComponent<Transform>();
            animator = GetComponent<Animator>();
            playerHearts = GetComponent<PlayerHearts>();

            groundLayerMask = 1 << groundLayer;
            wallLayerMask = 1 << wallLayer;
        }

        void Update()
        {
            if (IsGrounded())
            {
                coyoteTimeCounter = coyoteTime;
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            if (isWallSliding)
            {
                wallJumpCoyoteTimeCounter = wallJumpCoyoteTime;

                // Set the wall-jump direction
                if (wallCollider != null && wallCollider.transform.position.x < transform.position.x)
                {
                    wallJumpDirection = 1;
                }
                else if (wallCollider != null && wallCollider.transform.position.x > transform.position.x)
                {
                    wallJumpDirection = -1;
                }
            }
            else
            {
                wallJumpCoyoteTimeCounter -= Time.deltaTime;
            }

            if ((isWallJumping && Time.time > wallJumpEndTime) || IsGrounded() || (isWallSliding && rb.velocity.x == 0))
            {
                isWallJumping = false;
            }

            //Debug.Log("Ledge detected:" + isLedgeDetected);
            //Debug.Log(wallCollider);
        }

        private void FixedUpdate()
        {
            if (isWallJumping)
            {
                Move(wallJumpMoveLerp);
            }
            else
            {
                Move(1);
            }

            // Friction
            if (IsGrounded() && Mathf.Abs(moveInput) < 0.01f)
            {
                // Choose the smallest value between the player's current velocity and the friction amount
                float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(friction));
                // Set movement direction
                amount *= Mathf.Sign(rb.velocity.x);
                // Apply friction force opposite to the player's movement direction
                rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
            }

            // Fast fall
            if (rb.velocity.y < 0)
            {
                rb.gravityScale = gravityScale * gravityFallMultiplier;
            }
            else
            {
                rb.gravityScale = gravityScale;
            }

            WallSlide();

            Mantle();

            Flip();

            UpdateAnimationParameters();
        }

        // ---------------------------------
        // COLLISION CHECKS
        // ---------------------------------
        private bool IsGrounded()
        {
            Collider2D collider = Physics2D.OverlapBox(new Vector2(groundCheck.position.x, groundCheck.position.y), new Vector2(0.8f, 0.2f), 0f, groundLayerMask);
            if (Mathf.Abs(rb.velocity.y) <= 0.01f  && collider != null)
            {
                return true;
            }
            return false;
        }

        public bool IsWalled()
        {
            //wallCollider = Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y), new Vector2(1.0f, 0.4f), 0f, wallLayerMask); // Vector2 formerly 1.0f, 0.8f
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Mathf.Sign(ledgeCheck.localScale.x) * transform.right, 0.6f, wallLayerMask);

            if (hit)
            {
                wallCollider = hit.collider;
            }

            if (hit && !IsGrounded())
            {
                return hit;
            }
            return false;
        }

        private void AllowLedgeCheck()
        {
            canCheckLedge = true;
        }

        // ---------------------------------
        // MOVEMENT METHODS
        // ---------------------------------
        private void Move(float lerpAmount)
        {
            // Calculate the move direction and the desired velocity
            float targetSpeed = moveInput * moveSpeed;
            // Reduce player horizontal control during wall jump
            targetSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed, lerpAmount);

            // Calculate the difference between the current and desired velocity
            float speedDiff = targetSpeed - rb.velocity.x;
            // Change between acceleration or deceleration depending on whether the player is providing an input
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            // Apply acceleration to the speed difference, then raise to a set power so acceleration increases with higher speeds
            // Multiply by sign to reapply direction
            float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, velPower) * Mathf.Sign(speedDiff);

            // Apply movement force to the Rigidbody on the x axis
            rb.AddForce(movement * Vector2.right);
        }

        private void Jump()
        {
            rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        }

        private void WallSlide()
        {
            if (IsWalled() && !canMantle)
            {
                isWallSliding = true;
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlideSpeed, float.MaxValue));
            }
            else
            {
                isWallSliding = false;
            }
        }

        private void WallJump()
        {
            isWallJumping = true;
            wallJumpEndTime = Time.time + wallJumpTime;

            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(new Vector2(wallJumpPower.x * wallJumpDirection, wallJumpPower.y), ForceMode2D.Impulse);
        }

        private void Mantle()
        {
            if (isLedgeDetected && canCheckLedge && !isInDamageBounce)
            {
                Vector2 boxPos = new Vector2(transform.position.x + Mathf.Sign(ledgeCheck.localScale.x) * 1.08f, transform.position.y + 1.2f);
                Vector2 boxSize = new Vector2(0.64f, 1.21f);
                if (Physics2D.OverlapBox(boxPos, boxSize, 0f))
                {
                    return;
                }
                
                canCheckLedge = false;

                float ledgeSide;
                float ledgeTop = wallCollider.bounds.max.y;
                ledge = wallCollider.transform;
                ledgeStartPosition = ledge.position;

                if (ledgeCheck.localScale.x > 0)
                {
                    ledgeSide = wallCollider.bounds.min.x;
                    mantleStartPosition = new Vector2(ledgeSide + startOffset.x, ledgeTop + startOffset.y);
                    mantleEndPosition = new Vector2(ledgeSide + endOffset.x, ledgeTop + endOffset.y);
                }
                else if (ledgeCheck.localScale.x < 0)
                {
                    ledgeSide = wallCollider.bounds.max.x;
                    mantleStartPosition = new Vector2(ledgeSide - startOffset.x, ledgeTop + startOffset.y);
                    mantleEndPosition = new Vector2(ledgeSide - endOffset.x, ledgeTop + endOffset.y);
                }

                // Disable player move
                canFlip = false;
                rb.simulated = false;
                canMantle = true;
            }

            if (canMantle)
            {
                Vector2 offset = (Vector2)ledge.position - ledgeStartPosition;
                transform.position = mantleStartPosition + offset;
                //transform.position = mantleStartPosition;
            }
        }

        public void FinishMantle()
        {
            canMantle = false;
            Vector2 offset = (Vector2)ledge.position - ledgeStartPosition;
            transform.position = mantleEndPosition + offset;

            // Restore player move
            canFlip = true;
            rb.velocity = Vector2.zero;
            rb.simulated = true;
            Invoke("AllowLedgeCheck", 0.2f);
        }

        private void DamageBounce()
        {
            playerHearts.LoseHeart();
            float counterForce = rb.velocity.y;
            rb.AddForce(Vector2.up * (damageBounceForce + counterForce), ForceMode2D.Impulse);
            isInDamageBounce = true;
        }

        public void FinishDamageBounce()
        {
            isInDamageBounce = false;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.layer == 8)
            {
                Debug.Log("Hit piano.");
                DamageBounce();
            }
        }

        // ---------------------------------
        // SPRITE AND ANIMATIONS
        // ---------------------------------
        private void Flip()
        {
            if (moveInput != 0 && Mathf.Sign(moveInput) != Mathf.Sign(sprite.localScale.x) && canFlip)
            {
                sprite.localScale = new Vector3(-sprite.localScale.x, sprite.localScale.y, sprite.localScale.z);
                //ledgeCheck.transform.localPosition = new Vector2(-ledgeCheck.transform.localPosition.x, ledgeCheck.transform.localPosition.y);
                ledgeCheck.transform.localScale = new Vector2(-ledgeCheck.transform.localScale.x, ledgeCheck.transform.localScale.y);
            }
        }

        private void UpdateAnimationParameters()
        {
            animator.SetBool("isGrounded", IsGrounded());
            animator.SetFloat("horizontalVelocity", Mathf.Abs(moveInput));
            animator.SetBool("isWalled", IsWalled());
            animator.SetBool("canMantle", canMantle);
            animator.SetBool("isInDamageBounce", isInDamageBounce);

            if (rb.velocity.y < -1f)
            {
                animator.SetFloat("verticalVelocity", -1f);
            }
            else if (rb.velocity.y > -1f && rb.velocity.y < 1f)
            {
                animator.SetFloat("verticalVelocity", 0f);
            }
            else if (rb.velocity.y > 1f)
            {
                animator.SetFloat("verticalVelocity", 1f);
            }
        }

        // ---------------------------------
        // INPUT CHECKS
        // ---------------------------------
        public void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<float>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (coyoteTimeCounter > 0f) // Regular Jump
                {
                    Jump();
                }
                else if (wallJumpCoyoteTimeCounter > 0f) // Wall jump
                {
                    WallJump();
                }
            }

            if (context.canceled && rb.velocity.y > 0f)
            {
                coyoteTimeCounter = 0f;
                wallJumpCoyoteTimeCounter = 0f;

                rb.AddForce(Vector2.down * rb.velocity.y * 0.5f, ForceMode2D.Impulse);
            }
        }

        private void OnDrawGizmos()
        {
            //Vector2 boxPos = new Vector2(transform.position.x + Mathf.Sign(ledgeCheck.localScale.x) * 1.08f, transform.position.y + 1.2f);
            //Vector2 boxSize = new Vector2(0.64f, 1.21f);
            //Gizmos.DrawCube(boxPos, boxSize);
        }
    }
}
