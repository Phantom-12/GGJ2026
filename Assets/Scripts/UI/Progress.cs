using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Progress : MonoBehaviour
{
    public GameObject customerPrefab;
    private List<GameObject> customerPool = new();
    private Queue<GameObject> customerQueue = new();
    public List<string> customerNames = new();
    public int stepsToBooth = 7, stepsToLeave = 10;
    private int totalProgress = 5;
    public void InitProgress(int progress)
    {
        totalProgress = Mathf.Clamp(progress, 0, totalProgress);
        for (int i = customerPool.Count; i < totalProgress; i++)
        {
            GameObject customer = Instantiate(customerPrefab, transform);
            Customer customerScript = customer.GetComponent<Customer>();
            customer.SetActive(false);
            customer.transform.position = new Vector3(-100, 0, 0);
            customerPool.Add(customer);
        }
        StartCoroutine(InitCustomerQueue());
    }
    IEnumerator InitCustomerQueue()
    {
        customerQueue.Clear();
        for (int i = 0; i < totalProgress; i++)
        {
            GameObject customer = customerPool.Find(c => !c.activeSelf);
            if (customer != null)
            {
                Customer customerScript = customer.GetComponent<Customer>();
                customerScript.customerName = customerNames[Random.Range(0, customerNames.Count)];
                customer.SetActive(true);
                customer.transform.position = new Vector3(-100, 0, 0);
                customerQueue.Enqueue(customer);
            }
            yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
            StartCustomerJumpRoutine(customer, 1, stepsToBooth - i);
        }
        GameManager.Instance.StartFirstRound();
    }

    public string GetNameOfFirstCustomer()
    {
        if (customerQueue.Count == 0) return "";
        GameObject customer = customerQueue.Peek();
        Customer customerScript = customer.GetComponent<Customer>();
        return customerScript.customerName;
    }

    public void CustomerLeave()
    {
        if (customerQueue.Count == 0) return;
        AnimationManager.Instance.ItemSlideOutAnimation();
        GameObject customer = customerQueue.Dequeue();
        StartCoroutine(CustomerLeaveCoroutine(customer));
        StartCoroutine(QueueCustomerCoroutine());
    }

    IEnumerator CustomerLeaveCoroutine(GameObject customer)
    {
        Customer customerScript = customer.GetComponent<Customer>();
        StartCustomerJumpRoutine(customer, 1, stepsToLeave);
        yield return new WaitUntil(() => !customerScript.isJumping);
        customer.SetActive(false);
    }

    IEnumerator QueueCustomerCoroutine()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 1f));

        foreach (GameObject cust in customerQueue)
        {
            yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
            StartCustomerJumpRoutine(cust, 1, 1);
        }
    }

    public void StartCustomerJumpRoutine(GameObject customer, int direction = 1, int steps = 1)
    {
        if (steps <= 0) return;
        customer.SetActive(true);
        Customer customerScript = customer.GetComponent<Customer>();
        StartCoroutine(CustomerJumpCoroutine(customerScript, direction, steps));
    }

    IEnumerator CustomerJumpCoroutine(Customer customerScript, int direction, int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            customerScript.Jump(direction > 0);
            yield return new WaitUntil(() => !customerScript.isJumping);
        }
    }
}