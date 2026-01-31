using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public Vector3 startPosition;
    public float slideOutOffset = 10f;
    public SpriteRenderer spriteRenderer;
    private Vector3 targetPosition;

    public void Awake()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init()
    {
        Sprite newSprite = ItemManager.Instance.GetItemSprite();
        spriteRenderer.sprite = newSprite;
        transform.position = new Vector3(startPosition.x, startPosition.y, 0);
    }
    
    public void SlideIn()
    {
        targetPosition = AnimationManager.Instance.originalPosition;
        StartCoroutine(SlideInRoutine());
    }

    public void SlideOut()
    {
        targetPosition = transform.position + new Vector3(slideOutOffset, 0, 0);
        StartCoroutine(SlideOutRoutine());
    }

    IEnumerator SlideInRoutine()
    {
        gameObject.SetActive(true);
        float elapsedTime = 0f;
        float duration = AnimationManager.Instance.itemAnimationDuration;
        while (elapsedTime < duration)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
        }
        AnimationManager.Instance.CatStartWorking();
    }

    IEnumerator SlideOutRoutine()
    {
        float elapsedTime = 0f;
        float duration = AnimationManager.Instance.itemAnimationDuration;
        while (elapsedTime < duration)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
        }
        gameObject.SetActive(false);
    }
}
