using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plattko
{
    public class Note : MonoBehaviour
    {
        private SongManager noteGenerator;
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        
        private double timeInstantiated;
        
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            //timeInstantiated = noteGenerator.GetAudioSourceTime();
        }

        void Update()
        {
            //double timeSinceInstantiated = noteGenerator.GetAudioSourceTime() - timeInstantiated;
            //float t = (float)(timeSinceInstantiated / (noteGenerator.noteTime * 2f));

            //if (t > 1)
            //{
            //    Destroy(gameObject);
            //}
            //else
            //{
            //    transform.localPosition = Vector2.Lerp(Vector2.up * noteGenerator.noteSpawnY, Vector2.up * noteGenerator.noteDespawnY, t);
            //}
        }

        public void NoteSetup(float width, float height, Color colour, float fallSpeed)
        {
            transform.localScale = new Vector2(width, height);
            spriteRenderer.color = colour;
            rb.velocity = new Vector2(0, fallSpeed);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("NoteDestroy"))
            {
                Destroy(gameObject);
            }
        }
    }
}
