using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plattko
{
    public class LevelCompleteStar : MonoBehaviour
    {
        private GameObject levelCompletePanel;
        private float fallSpeed = 0f;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                collision.GetComponent<Rigidbody2D>().simulated = false;
                fallSpeed = 0f;
                levelCompletePanel.SetActive(true);
            }
        }

        private void Update()
        {
            transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);
        }

        public void StarSetup(float fallSpeed, GameObject levelCompletePanel)
        {
            this.fallSpeed = fallSpeed;
            this.levelCompletePanel = levelCompletePanel;
        }
    }
}
