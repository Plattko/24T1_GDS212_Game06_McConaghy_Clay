using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Plattko
{
    public class PlayerHearts : MonoBehaviour
    {
        [SerializeField] private SceneController sceneController;
        [SerializeField] private List<Image> heartIcons = new List<Image>();
        
        private int hearts = 3;
        private int currentHearts;

        [Header("Heart Regeneration Time")]
        [SerializeField] private float heartRegenTime = 20f;
        private float timePassed = 0;
        
        private void Start()
        {
            currentHearts = hearts;
        }

        private void Update()
        {
            if (currentHearts < 3)
            {
                if (timePassed < heartRegenTime)
                {
                    Vector3 scale = Vector3.Lerp(Vector3.zero, Vector3.one, timePassed / heartRegenTime);
                    
                    if (currentHearts > 0)
                    {
                        heartIcons[currentHearts].rectTransform.localScale = scale;
                    }
                    timePassed += Time.deltaTime;
                }
                else
                {
                    heartIcons[currentHearts].rectTransform.localScale = Vector3.one;
                    GainHeart();
                }
            }
        }

        public void LoseHeart()
        {
            if (currentHearts == hearts && heartIcons[currentHearts - 1].rectTransform.localScale == Vector3.one)
            {
                Debug.Log("Hearts full");
                heartIcons[currentHearts - 1].rectTransform.localScale = Vector3.zero;
            }
            else
            {
                Debug.Log("Hearts not full");
                Debug.Log("Targeted heart: " + heartIcons[currentHearts - 1]);
                heartIcons[currentHearts].rectTransform.localScale = Vector3.zero;
                heartIcons[currentHearts - 1].rectTransform.localScale = Vector3.zero;

                //if (currentHearts >= 2)
                //{
                //    heartIcons[currentHearts - 2].rectTransform.localScale = Vector3.zero;
                //}
            }

            timePassed = 0f;
            currentHearts--;
            Debug.Log("Heart lost.");
            Debug.Log("Current hearts: " + currentHearts);

            if (currentHearts <= 0)
            {
                sceneController.ReloadScene();
            }
        }

        private void GainHeart()
        {
            timePassed = 0f;
            currentHearts++;

            Debug.Log("Heart regenerated.");
            Debug.Log("Current hearts: " + currentHearts);
        }
    }
}
