using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFalling : MonoBehaviour
{
    public Rigidbody rb;
    private bool isFalling;
    public PlayerStatus status;
    [SerializeField] GameObject intactSphere;
    [SerializeField] GameObject brokenSphere;
    BoxCollider bc;
    
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        GetComponent<Rigidbody>().isKinematic = true;
        isFalling = false;
        bc = GetComponent<BoxCollider>();
        intactSphere.SetActive(true);
        brokenSphere.SetActive(false);
    }
   

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Debug.Log("Was hit");
            
            isFalling = true;
            rb.isKinematic = false;
            
        }
        if (collision.gameObject.CompareTag("Ground"))
        {
            intactSphere.SetActive(false);
            brokenSphere.SetActive(true);
            Destroy(gameObject, 5f);
        }
        if (isFalling && collision.gameObject.CompareTag("Player"))
        {
            intactSphere.SetActive(false);
            brokenSphere.SetActive(true);
            Debug.Log("Stunned");
            status.Stun();
            Destroy(gameObject, 5f);
        }
    }
}

            

    

        
    
