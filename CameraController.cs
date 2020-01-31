using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    private PlayerController playerControl;
    private Vector3 targetPosition = Vector3.zero;
    private float originalZDistance = 10;
    private float originalXdistance = 0;

    void Update () {

        if (playerControl == null)
        {
            playerControl = FindObjectOfType<PlayerController>();
        }
        else
        {
            if (!playerControl.isDead)
            {
                float currentZDistance = playerControl.transform.position.z - transform.position.z;
                float currentXDistance = playerControl.transform.position.x - transform.position.x;
                float z = originalZDistance - currentZDistance;
                float x = originalXdistance - currentXDistance;

                targetPosition = new Vector3(transform.position.x - x, transform.position.y, transform.position.z - z);
                transform.position = targetPosition;
            }
        }
	}
}
