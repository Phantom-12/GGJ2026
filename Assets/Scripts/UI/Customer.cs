using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Customer : MonoBehaviour
{
    [Header("Data")]
    public string customerName;

	[Header("Jump Settings")]
	[SerializeField] private float jumpDistance = 200f;
	[SerializeField] private float jumpHeight = 60f;
	[SerializeField] private float jumpDuration = 0.35f;

	private RectTransform rectTransform;
	private Coroutine jumpRoutine;
    public bool isJumping => jumpRoutine != null;
    private Image customerImage;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
        customerImage = GetComponent<Image>();
	}

    public void SetCustomerName(string name)
    {
        customerName = name;
        Sprite customerSprite = Resources.Load<Sprite>($"Sprites/Customers/{name}");
        if (customerSprite == null){
            customerSprite = Resources.Load<Sprite>($"Sprites/Customers/default");
        }
        if (customerImage != null)
        {
            customerImage.sprite = customerSprite;
        }
    }

	public void Jump(bool toRight = true)
	{
        if (isJumping) return;
		if (rectTransform == null)
		{
			rectTransform = GetComponent<RectTransform>();
		}

		if (jumpRoutine != null)
		{
			StopCoroutine(jumpRoutine);
		}

		jumpRoutine = StartCoroutine(JumpRoutine(toRight));
	}

	private IEnumerator JumpRoutine(bool toRight)
	{
		Vector2 start = rectTransform.anchoredPosition;
		Vector2 end = start + new Vector2(toRight ? jumpDistance : -jumpDistance, 0f);
		float elapsed = 0f;

		while (elapsed < jumpDuration)
		{
			elapsed += Time.unscaledDeltaTime;
			float t = Mathf.Clamp01(elapsed / jumpDuration);
			float height = Mathf.Sin(t * Mathf.PI) * jumpHeight;
			Vector2 pos = Vector2.LerpUnclamped(start, end, t);
			pos.y += height;
			rectTransform.anchoredPosition = pos;
			yield return null;
		}

		rectTransform.anchoredPosition = end;
		jumpRoutine = null;
	}
}