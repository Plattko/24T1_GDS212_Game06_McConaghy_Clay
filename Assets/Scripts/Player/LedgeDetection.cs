using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plattko
{
    public class LedgeDetection : MonoBehaviour
    {
        private PlayerController playerController;
        
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
            Gizmos.DrawRay(transform.position, Mathf.Sign(transform.localScale.x) * transform.right * ledgeCheckDistance);
            Gizmos.DrawRay(transform.parent.transform.position, Mathf.Sign(transform.localScale.x) * transform.right * ledgeCheckDistance);
        }
    }
}
