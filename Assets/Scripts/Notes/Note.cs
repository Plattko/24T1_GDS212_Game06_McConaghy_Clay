using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plattko
{
    public class Note : MonoBehaviour
    {
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;

        [Header("Note Settings")]
        [SerializeField] private float destroyDelay;
        [HideInInspector] public float fallSpeed = 0f;

        public void NoteSetup(float width, float height, Color colour, float fallSpeed)
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            transform.localScale = new Vector2(width, height);
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
