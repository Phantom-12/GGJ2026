using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public Vector3 originalPosition;
    public float offsetY = 10f;
    private Vector3 targetPosition;
    
    public void SlideIn()
    {
        targetPosition = AnimationManager.Instance.originalPosition;
        StartCoroutine(Slide());
    }

    public void SlideOut()
    {
        targetPosition = new Vector3(originalPosition.x, originalPosition.y + offsetY, originalPosition.z);
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
            transform.position = Vector3.Lerp(originalPosition, targetPosition, elapsedTime / duration);
        }
    }
}
