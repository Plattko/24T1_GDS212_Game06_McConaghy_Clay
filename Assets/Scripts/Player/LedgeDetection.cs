using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plattko
{
    public class LedgeDetection : MonoBehaviour
    {
        private PlayerController playerController;
        
        [SerializeField] private float radius = 0.1f;
        [SerializeField] private float ledgeCheckDistance = 0.6f;
        private bool isTouchingLedge = false;

        private void Start()
        {
            playerController = transform.parent.GetComponent<PlayerController>();
        }

        private void Update()
        {
            IsLedgeDetected();
        }

        private void IsLedgeDetected()
        {
            //isTouchingLedge = Physics2D.OverlapCircle(transform.position, radius, playerController.wallLayerMask);
            isTouchingLedge = Physics2D.Raycast(transform.position, Mathf.Sign(transform.localScale.x) * transform.right, ledgeCheckDistance, playerController.wallLayerMask);

            if (playerController.IsWalled() && !isTouchingLedge)
            {
                playerController.isLedgeDetected = true;
            }
            else
            {
                playerController.isLedgeDetected = false;
            }
        }

        private void OnDrawGizmos()
        {
            //Gizmos.DrawWireSphere(transform.position, radius);
            Gizmos.DrawRay(transform.position, Mathf.Sign(transform.localScale.x) * transform.right * ledgeCheckDistance);
            //Gizmos.DrawWireCube(transform.parent.transform.position, new Vector2(1.0f, 0.4f));
            Gizmos.DrawRay(transform.parent.transform.position, Mathf.Sign(transform.localScale.x) * transform.right * ledgeCheckDistance);
        }
    }
}
