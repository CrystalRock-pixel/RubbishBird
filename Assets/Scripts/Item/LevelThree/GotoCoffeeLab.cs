using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GotoCoffeeLab : MonoBehaviour
{
    public Transform coffeeLabPoint;
    public GameObject trueCoffeeLab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!trueCoffeeLab.activeSelf)
            {
                trueCoffeeLab.SetActive(true);
            }
            Player.Instance.SetPosition(coffeeLabPoint.position);
            this.gameObject.SetActive(false);
        }
    }
}
