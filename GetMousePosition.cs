using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetMousePosition : MonoBehaviour {

    public float X { get; private set; }
    public float Z { get; private set; }

    private void OnMouseOver()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane xy = new Plane(Vector3.up, new Vector3(0, 0, 0));
        float distance;
        xy.Raycast(ray, out distance);
        X = ray.GetPoint(distance).x;
        Z = ray.GetPoint(distance).z;
    }
}
