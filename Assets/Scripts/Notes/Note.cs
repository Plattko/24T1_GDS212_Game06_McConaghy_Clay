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

        public void NoteSetup(float width, float height, Color colour, float fallSpeed)
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            transform.localScale = new Vector2(width, height);
            spriteRenderer.color = colour;
            rb.velocity = new Vector2(0, -fallSpeed);

            Destroy(gameObject, destroyDelay);
        }
    }
}
