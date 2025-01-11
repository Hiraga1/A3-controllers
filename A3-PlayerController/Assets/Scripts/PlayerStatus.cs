using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    private bool isFalling;
    public PlayerMovementAdvanced pm;   
    public Sliding slide;
    public Throwing FPPshooting;
    public LeapingTPP dash;  
    public float stunDuration = 5f;
    public GameObject stunParticle;
    public GameObject stunParticle1;

    private void Start()
    {
        stunParticle.SetActive(false);
        stunParticle1.SetActive(false);
    }
    public void Stun()
    {
        StartCoroutine("Stunned");
    }
    
    public IEnumerator Stunned()
    {
        {
            if (pm != null)
            {
                stunParticle.SetActive (true);
                stunParticle1.SetActive (true); 
                pm.enabled = false;
                slide.enabled = false;
                FPPshooting.enabled = false;
                dash.enabled = false;
            }
            yield return new WaitForSeconds(stunDuration);
            if (pm != null)
            {
                dash.enabled = true;
                stunParticle.SetActive (false);
                stunParticle1.SetActive (false);
                Debug.Log("Not Stunned");
                pm.enabled = true;
                slide.enabled = true;
                FPPshooting.enabled = true;
                
            }
        }
    }
}
                
