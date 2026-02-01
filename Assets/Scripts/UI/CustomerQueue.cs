using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CustomerQueue : MonoBehaviour
{
    public GameObject customerPrefab;
    private List<GameObject> customerPool = new();
    private Queue<GameObject> customerQueue = new();
    public Dictionary<string, Sprite> specialCustomerSprites = new();
    public List<Sprite> commonCustomerSprites = new();
    public Dictionary<GameObject, Coroutine> customerMovingCoroutines = new();
    public int stepsToBooth = 7, stepsToLeave = 10;
    public float specialCustomerChance = 0.3f;
    public int maxCustomer = 5;
    private bool isQueueMoving = false;

    public void Init()
    {
        ClearQueue();
        LoadCustomerSprites();
        StartCoroutine(InitCustomerQueue());
    }

    void LoadCustomerSprites()
    {
        Sprite[] loadedSpecialSprites = Resources.LoadAll<Sprite>("Sprites/Customers/Special");
        foreach (Sprite sprite in loadedSpecialSprites)
        {
            specialCustomerSprites[sprite.name] = sprite;
        }

        Sprite[] loadedCommonSprites = Resources.LoadAll<Sprite>("Sprites/Customers/Common");
        commonCustomerSprites.AddRange(loadedCommonSprites);
    }

    public void ClearQueue()
    {
        StopAllCoroutines();
        foreach (GameObject customer in customerQueue)
        {
            customer.SetActive(false);
        }
        customerQueue.Clear();
    }

    IEnumerator InitCustomerQueue()
    {
        customerQueue.Clear();
        for (int i = 0; i < maxCustomer; i++)
        {
            GameObject customer = CreateCustomer();
            StartCustomerJumpRoutine(customer, 1, stepsToBooth - i);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        }
        GameManager.Instance.StartFirstRound();
        StartCoroutine(AddCustomerListener());
    }

    GameObject CreateCustomer()
    {
        GameObject customer = customerPool.Find(c => !c.activeSelf);
        if (customer == null)
        {
            customer = Instantiate(customerPrefab, transform);
            Customer customerScript = customer.GetComponent<Customer>();
            customerPool.Add(customer);
        }
        Customer custScript = customer.GetComponent<Customer>();
        if(Random.value < specialCustomerChance && specialCustomerSprites.Count > 0)
        {
            List<string> keys = new List<string>(specialCustomerSprites.Keys);
            string specialName = keys[Random.Range(0, keys.Count)];
            custScript.SetCustomerSprite(specialCustomerSprites[specialName]);
            custScript.isSpecial = true;
        }
        else
        {
            int index = Random.Range(0, commonCustomerSprites.Count);
            custScript.SetCustomerSprite(commonCustomerSprites[index]);
            custScript.isSpecial = false;
        }
        customerQueue.Enqueue(customer);
        customer.GetComponent<RectTransform>().anchoredPosition = new Vector3(-100, 0, 0);
        customer.SetActive(true);
        return customer;
    }

    public string GetNameOfFirstCustomer()
    {
        if (customerQueue.Count == 0) return "";
        GameObject customer = customerQueue.Peek();
        Customer customerScript = customer.GetComponent<Customer>();
        return customerScript.GetCustomerName();
    }

    public bool IsFirstCustomerSpecial()
    {
        if (customerQueue.Count == 0) return false;
        GameObject customer = customerQueue.Peek();
        Customer customerScript = customer.GetComponent<Customer>();
        return customerScript.isSpecial;
    }

    public void CustomerLeave(int direction = 1)
    {
        if (customerQueue.Count == 0) return;
        GameObject customer = customerQueue.Dequeue();
        AnimationManager.Instance.ItemSlideOutAnimation();
        StartCoroutine(CustomerLeaveCoroutine(customer, direction));
        StartCoroutine(QueueCustomerCoroutine());
        isQueueMoving = true;
    }

    IEnumerator AddCustomerListener()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(0f, 0.8f));
            if (!isQueueMoving && customerQueue.Count < maxCustomer)
            {
                GameObject customer = CreateCustomer();
                StartCustomerJumpRoutine(customer, 1, stepsToBooth - customerQueue.Count + 1);
            }
        }
    }

    IEnumerator CustomerLeaveCoroutine(GameObject customer, int direction = 1)
    {
        Customer customerScript = customer.GetComponent<Customer>();
        StartCustomerJumpRoutine(customer, direction, stepsToLeave);
        yield return new WaitUntil(() => !customerScript.isJumping);
        customer.SetActive(false);
    }

    IEnumerator QueueCustomerCoroutine()
    {
        customerMovingCoroutines.Clear();
        foreach (GameObject cust in customerQueue)
        {
            yield return new WaitForSeconds(0.2f);
            Coroutine movingCoroutine = StartCustomerJumpRoutine(cust, 1, 1);
            customerMovingCoroutines[cust] = movingCoroutine;
        }
        yield return new WaitUntil(() => customerMovingCoroutines.Values.All(v => v == null));
        isQueueMoving = false;
        AnimationManager.Instance.ItemSlideInAnimation();
    }

    public Coroutine StartCustomerJumpRoutine(GameObject customer, int direction = 1, int steps = 1)
    {
        if (steps <= 0) return null;
        customer.SetActive(true);
        Customer customerScript = customer.GetComponent<Customer>();
        Coroutine movingCoroutine = StartCoroutine(CustomerJumpCoroutine(customerScript, direction, steps));
        return movingCoroutine;
    }

    IEnumerator CustomerJumpCoroutine(Customer customerScript, int direction, int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            customerScript.Jump(direction > 0);
            yield return new WaitUntil(() => !customerScript.isJumping);
        }
        if (!customerMovingCoroutines.ContainsKey(customerScript.gameObject)) yield break;
        customerMovingCoroutines[customerScript.gameObject] = null;
    }
}