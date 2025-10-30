using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LevelManager;

public class CoffeeDoor : MonoBehaviour
{
    Player player=Player.Instance;
    public GameObject trueCoffeeLab;
    public GameObject fakeCoffeeLab;
    public Transform level2Point;

    private void Start()
    {
        player = Player.Instance;
    }
    private void OnTriggerEnter(Collider other)
    {
         if(other.CompareTag("Player") && LevelManager.instance.type == LevelType.Three)
        {
            player.SetPosition(level2Point.position);
            MainCamara.Instance.SetPosition(level2Point.position);
            LevelManager.instance.LightAdjust(false);
        }
    }
}
