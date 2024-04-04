using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Plattko
{
    public class TrackTitle : MonoBehaviour
    {
        private TextMeshProUGUI trackTitleText;

        [SerializeField] private float fadeDelay = 4f;
        [SerializeField] private float fadeTimer = 1f;
        private float timePassed = 0f;
        
        void Start()
        {
            trackTitleText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            StartCoroutine(FadeText());
        }

        void Update()
        {
            
        }

        private IEnumerator FadeText()
        {
            yield return new WaitForSeconds(fadeDelay);

            while (timePassed < fadeTimer)
            {
                float alpha = Mathf.Lerp(1f, 0f, timePassed / fadeTimer);
                trackTitleText.alpha = alpha;
                timePassed += Time.deltaTime;
                yield return null;
            }
            gameObject.SetActive(false);
        }
    }
}
