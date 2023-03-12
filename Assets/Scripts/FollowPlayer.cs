using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private Vector3 moveCenter = new Vector3(0,1,0);

    private void Update()
    {
        if (player == null)
        {
            GetPlayerGO();
        }
        transform.position = player.transform.position + moveCenter;
        
    }
    
    private void GetPlayerGO()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
}
