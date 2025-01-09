using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    private bool isFalling;
    public PlayerMovementAdvanced pm;
    
    public Sliding slide;
    public Throwing FPPshooting;
    //public CameraAim aimTPP;
    //public TPPThrowing TPPthrowing;
    //public CameraView enabler;
    public LeapingTPP dash;
    public PlayerMovement aimFPP;
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
                pm.enabled = false;
                slide.enabled = false;
                FPPshooting.enabled = false;
                //aimTPP.enabled = false;
                //aimFPP.enabled = false;
                //TPPthrowing.enabled = false;
                //enabler.enabled = false;
                dash.enabled = false;
                stunParticle.SetActive (true);
                stunParticle1.SetActive (true); 
            }
            yield return new WaitForSeconds(stunDuration);
            if (pm != null)
            {
                Debug.Log("Not Stunned");
                pm.enabled = true;
                slide.enabled = true;
                FPPshooting.enabled = true;
                //aimTPP.enabled = true;
                //aimFPP.enabled = true;
                //TPPthrowing.enabled = true;
                //enabler.enabled = true;
                dash.enabled = true;
                stunParticle.SetActive (false);
                stunParticle1.SetActive (false);
            }
        }
    }
}
