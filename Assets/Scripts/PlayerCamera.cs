using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private Vector3 oldPosition;
    private int pixelsPerUnit = 16;

    void LateUpdate()
    {
        oldPosition = transform.localPosition;
        float snaped_x = Mathf.FloorToInt(oldPosition.x * pixelsPerUnit);
        float snaped_y = Mathf.FloorToInt(oldPosition.y * pixelsPerUnit);
        float snaped_z = Mathf.FloorToInt(oldPosition.z * pixelsPerUnit);
        Vector3 newPosition = new Vector3(snaped_x / pixelsPerUnit, snaped_y / pixelsPerUnit, snaped_z / pixelsPerUnit);
        transform.localPosition = newPosition;
    }
}
