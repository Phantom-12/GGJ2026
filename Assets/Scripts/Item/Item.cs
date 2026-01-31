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

    public void Init(string spriteName = "")
    {
        Sprite newSprite = ItemManager.Instance.GetSpecialItemSprite(spriteName);
        if (newSprite == null)
        {
            newSprite = ItemManager.Instance.GetRandomCommonItemSprite();
        }
        spriteRenderer.sprite = newSprite;
        transform.position = new Vector3(startPosition.x, startPosition.y, 0);
    }
    
    public void SlideIn()
    {
        targetPosition = AnimationManager.Instance.originalPosition;
        StartCoroutine(Slide());
    }

    public void SlideOut()
    {
        targetPosition = transform.position + new Vector3(slideOutOffset, 0, 0);
        StartCoroutine(Slide());
    }

    IEnumerator Slide()
    {
        float elapsedTime = 0f;
        float duration = AnimationManager.Instance.itemAnimationDuration;
        while (elapsedTime < duration)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
        }
    }
}
