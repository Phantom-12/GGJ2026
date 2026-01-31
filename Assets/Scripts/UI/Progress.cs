using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Progress : MonoBehaviour
{
    public GameObject customerPrefab;
    private List<GameObject> customerPool;
    public int stepsToBooth = 7;
    private int totalProgress = 5;
    private int currentProgress = 0;

    public void InitProgress(int progress)
    {
        totalProgress = Mathf.Clamp(progress, 0, totalProgress);
        currentProgress = 0;
        for (int i = customerPool.Count; i < totalProgress; i++)
        {
            GameObject customer = Instantiate(customerPrefab, transform);
            customer.SetActive(false);
            customer.transform.localPosition = new Vector3(-100, 0, 0);
            customerPool.Add(customer);
        }
        for (int i = 0; i < totalProgress; i++)
        {
            GameObject customer = customerPool.Find(c => !c.activeSelf);
            if (customer != null)
            {
                customer.SetActive(true);
                customer.transform.localPosition = new Vector3(-100, 0, 0);
            }

            StartCustomerJumpRoutine(customer, 1, stepsToBooth - i);
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
        yield return new WaitForSeconds(Random.Range(0f, 0.3f));
        for (int i = 0; i < steps; i++)
        {
            customerScript.Jump(direction > 0);
            yield return new WaitUntil(() => !customerScript.isJumping);
        }
    }
}