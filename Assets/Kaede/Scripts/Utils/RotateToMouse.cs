using UnityEngine;

public class RotateToMouse : MonoBehaviour
{
    [SerializeField] private bool active = true;
    void Update()
    {
        if (!active) return;
            
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 lookDir = mousePos - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
        
    public void SetActive(bool isActive)
    {
        active = isActive;
    }
}
