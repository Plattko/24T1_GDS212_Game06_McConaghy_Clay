using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plattko
{
    public class Note : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private BoxCollider2D[] sideColliders;
        private BoxCollider2D bottomCollider;

        [Header("Note Settings")]
        [SerializeField] private float destroyDelay;
        [HideInInspector] public float fallSpeed = 0f;

        public void NoteSetup(float width, float height, Color colour, float fallSpeed)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            sideColliders = transform.GetChild(1).GetComponents<BoxCollider2D>();
            bottomCollider = transform.GetChild(2).GetComponent<BoxCollider2D>();

            transform.localScale = new Vector2(width, height);

            foreach (BoxCollider2D collider in sideColliders)
            {
                collider.size = new Vector2(collider.size.x, collider.size.y - 0.1f);
            }
            bottomCollider.size = new Vector3(bottomCollider.size.x - 0.1f, bottomCollider.size.y);

            spriteRenderer.color = colour;
            //rb.velocity = new Vector2(0, -fallSpeed);
            this.fallSpeed = fallSpeed;

            Destroy(gameObject, destroyDelay);
        }

        private void Update()
        {
            transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Debug.Log("Collided with player");
                collision.transform.GetComponent<Rigidbody2D>().interpolation = RigidbodyInterpolation2D.None;
                collision.transform.SetParent(transform);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Debug.Log("Collision exited with player");
                collision.transform.GetComponent<Rigidbody2D>().interpolation = RigidbodyInterpolation2D.Interpolate;
                collision.transform.SetParent(null);
            }
        }
    }
}
