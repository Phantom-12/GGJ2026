using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    private OperableObject operableObject;
    public Vector3 startPosition;
    public float slideOutOffset = 10f;
    public SpriteRenderer spriteRenderer => GetComponent<SpriteRenderer>();
    private Vector3 targetPosition;

    public void Awake()
    {
        startPosition = transform.position;
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
        gameObject.SetActive(true);
        StartCoroutine(SlideInRoutine());
        operableObject.Appear();
    }

    public void SlideOut()
    {
        targetPosition = transform.position + new Vector3(slideOutOffset, 0, 0);
        gameObject.SetActive(true);
        StartCoroutine(SlideOutRoutine());
    }

    IEnumerator SlideInRoutine()
    {
        float elapsedTime = 0f;
        float duration = AnimationManager.Instance.itemAnimationDuration;
        Vector3 startPos = transform.position;
        while (elapsedTime < duration)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPosition, elapsedTime / duration);
        }
        AnimationManager.Instance.CatStartWorking();
        GameManager.Instance.canPutDown = true;
    }

    IEnumerator SlideOutRoutine()
    {
        float elapsedTime = 0f;
        float duration = AnimationManager.Instance.itemAnimationDuration;
        Vector3 startPos = transform.position;
        while (elapsedTime < duration)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPosition, elapsedTime / duration);
        }
        gameObject.SetActive(false);
    }
}
