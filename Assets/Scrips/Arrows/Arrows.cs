using UnityEngine;

public class Arrow : MonoBehaviour
{

    [SerializeField]
    public ArrowPool Pool; 
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;
    private Rigidbody rb;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        
    }
public void Launch() {
    rb.linearVelocity = Vector3.zero; 
    rb.AddForce(transform.forward * speed, ForceMode.Impulse);
    Invoke(nameof(ReturnToPool), lifeTime);
}
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision enter" + other.tag);
        if (other.CompareTag("Enemy") || other.CompareTag("Wall"))
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        if (Pool != null)
            Pool.ReturnToPool(this);
        else
            gameObject.SetActive(false);
    }
}