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
        private Animator animator;

        // Collision check variables
        private int groundLayer = 6;
        private int wallLayer = 7;
        private LayerMask groundLayerMask;
        private LayerMask wallLayerMask;
        private Collider2D wallCollider;

        [Header("Movement Variables")]
        private float moveInput;
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float acceleration = 7f;
        [SerializeField] private float deceleration = 7f;
        [SerializeField] private float velPower = 0.9f;
        [SerializeField] private float friction = 0.25f;

        [Header("Jump Variables")]
        [SerializeField] private float jumpPower = 8f;
        [SerializeField] private float coyoteTime = 0.15f;
        private float coyoteTimeCounter;

        [SerializeField] private float gravityScale = 1f;
        [SerializeField] private float gravityFallMultiplier = 1.9f;

        [Header("Wall-Slide Variables")]
        [SerializeField] private float wallSlideSpeed = 4f;
        private bool isWallSliding = false;

        [Header("Wall-Jump Variables")]
        [SerializeField] private Vector2 wallJumpPower;
        [SerializeField] private float wallJumpCoyoteTime = 0.15f;
        private float wallJumpCoyoteTimeCounter;

        [SerializeField] private float wallJumpTime = 0.75f;
        private float wallJumpEndTime;
        [SerializeField] private float wallJumpMoveLerp = 0.2f;
        private int wallJumpDirection;
        private bool isWallJumping = false;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            sprite = transform.GetChild(0).gameObject.GetComponent<Transform>();
            groundCheck = transform.GetChild(1).gameObject.GetComponent<Transform>();
            animator = GetComponent<Animator>();

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
                if (wallCollider.transform.position.x < transform.position.x)
                {
                    wallJumpDirection = 1;
                }
                else if (wallCollider.transform.position.x > transform.position.x)
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
                Debug.Log("Grounded = true");
                return true;
            }
            Debug.Log("Grounded = false");
            return false;
        }

        private bool IsWalled()
        {
            wallCollider = Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y), new Vector2(1.0f, 0.8f), 0f, wallLayerMask);
            if (wallCollider != null)
            {
                return wallCollider;
            }
            return false;
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
            if (IsWalled() && !IsGrounded())
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

        // ---------------------------------
        // SPRITE AND ANIMATIONS
        // ---------------------------------
        private void Flip()
        {
            if (moveInput != 0 && Mathf.Sign(moveInput) != Mathf.Sign(sprite.localScale.x))
            {
                sprite.localScale = new Vector3(-sprite.localScale.x, sprite.localScale.y, sprite.localScale.z);
            }
        }

        private void UpdateAnimationParameters()
        {
            animator.SetBool("isGrounded", IsGrounded());
            animator.SetFloat("horizontalVelocity", Mathf.Abs(moveInput));

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
    }
}
