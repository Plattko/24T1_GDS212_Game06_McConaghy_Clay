using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plattko
{
    public class StartingPlatform : MonoBehaviour
    {
        private Animator animator;

        [SerializeField] private float disappearFlashDelay = 8f;
        private float timePassed = 0f;
        
        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            if (timePassed < disappearFlashDelay)
            {
                timePassed += Time.deltaTime;
            }
            else
            {
                animator.SetTrigger("DisappearFlash");
            }
        }

        public void DestroyPlatform()
        {
            gameObject.SetActive(false);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                DestroyPlatform();
            }
        }
    }
}
