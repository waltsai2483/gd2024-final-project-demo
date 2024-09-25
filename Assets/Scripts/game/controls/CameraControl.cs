using System;
using UnityEngine;

public class CameraControl: MonoSingleton<CameraControl> 
{
    public Entity follower;
    public Camera viewCamera;
    public Camera mapCamera;

    public float viewCameraRotX = 70.0f;
    
    void LateUpdate()
    {
        viewCamera.transform.eulerAngles = new Vector3(viewCameraRotX, 0, 0);
        viewCamera.transform.localPosition = new Vector3(0, follower.stats.visionRange, -follower.stats.visionRange * Mathf.Cos(viewCameraRotX / 180f * Mathf.PI));

        Vector2 clampedPos = GameManager.instance.ClampBorder(follower.transform.position.x, follower.transform.position.z);
        transform.position = new Vector3(clampedPos.x, follower.transform.position.y, clampedPos.y);
        mapCamera.orthographicSize = follower.stats.visionRange;
    }

    public Vector3 GetMousePos()
    {
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out var hit, float.MaxValue, LayerMask.GetMask("Ground")))
        {
            return hit.point;
        }
        return Vector3.zero;
    }
}