using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    private Vector3 mOffset;
    private float mZCoord;
    private Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void OnMouseDown() {
        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        mOffset = gameObject.transform.position - GetMouseWorldPos();
        
        // Biar benda gak kaku pas ditarik
        if(rb != null) {
            rb.isKinematic = true; 
        }
    }

    private Vector3 GetMouseWorldPos() {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mZCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    void OnMouseDrag() {
        transform.position = GetMouseWorldPos() + mOffset;
    }

    void OnMouseUp() {
        // Balikin biar benda bisa jatuh lagi
        if(rb != null) {
            rb.isKinematic = false;
        }
    }
}