using System;
using System.Collections;
using Code.Scripts;
using UnityEngine;


namespace PlayerControls
{
    [RequireComponent(typeof(Collider2D))] 
    public class CardHoverEffect : MonoBehaviour
    {
        public float scaleMultiplier = 1.15f; 
        private Vector3 _originalScale; 
        private SpriteRenderer _spriteRenderer; 
        private int _originalSortingOrder; 
        private Vector3 _originalPosition; 
        private float _deltaY = 0.15f; 
        private bool _targetable = true;
        private Collider2D _collider; 

        private bool hasBeenDestroyed = false;
        public Card card;

        public bool Targetable
        {
            get { return _targetable; }
            set
            {
                _targetable = value;
                _collider.enabled = _targetable;
            }
        }

        public Vector3 targetSourcePosition;
        
        private void Awake()
        {
            
            _spriteRenderer = GetComponent<SpriteRenderer>(); 
            _collider = GetComponent<Collider2D>(); 
        }

        private void Start()
        {
            _originalScale = transform.localScale; 
            _originalSortingOrder = _spriteRenderer.sortingOrder; 
            _originalPosition = transform.position; 
        }

        private void OnMouseEnter()
        {
            // Scale up when the mouse hovers over the card
            StartCoroutine(HoverEffect());
            _spriteRenderer.sortingOrder = 1000; // Bring the card to the front
        }

        private void OnMouseExit()
        {
            // Return to original scale when the mouse stops hovering over the card
            StartCoroutine(UnhoverEffect());
            _spriteRenderer.sortingOrder = _originalSortingOrder; // Return to original sorting order
        }

        private void OnMouseDown()
        {
            GameManager.instance.currentCardInteraction?.Invoke(card);
            if(GameManager.instance.currentCardInteraction != null)
                this.gameObject.SetActive(false);
            
        }

        private IEnumerator HoverEffect()
        {
            Vector3 targetScale = _originalScale * scaleMultiplier;
            Vector3 originalScale = transform.localScale;
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 20; 
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                // move the card slightly up its transform y-axis
                transform.position = Vector3.Lerp(targetSourcePosition, targetSourcePosition + transform.up * _deltaY, t);

                yield return null;
            }
        }

        private IEnumerator UnhoverEffect()
        {
            Vector3 targetScale = _originalScale;
            Vector3 originalScale = transform.localScale;
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 20; 
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                // move the card slightly down its y-axis
                transform.position = Vector3.Lerp(targetSourcePosition + transform.up * _deltaY, targetSourcePosition, t);

                yield return null;
            }
        }
        
        public IEnumerator FlyToSourceTargetPos()
        {
            float currentTime = GameManager.instance.showAnimations ? 0f : 1;
            float duration = 1;
            float t = 0;
            
            while (currentTime <= duration)
            {   
                if (hasBeenDestroyed)
                {
                    yield break;
                }
                
                currentTime += Time.deltaTime;
                t = currentTime / duration;
                t = 1f - (1f - t) * (1f - t); // Quadratic ease out
                
                transform.position = Vector3.Lerp(Vector3.zero, targetSourcePosition, t);

                yield return null;
            }
        }


        private void OnDestroy()
        {
            hasBeenDestroyed = true;
        }
    }
}