using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    private bool isFalling;
    public InputHandler inputs;
    public float stunDuration = 5f;
    public GameObject stunParticle;
    public GameObject stunParticle1;

    private void Start()
    {
        //stunParticle.SetActive(false);
        //stunParticle1.SetActive(false);
    }
    public void Stun()
    {
        StartCoroutine("Stunned");
    }
    
    public IEnumerator Stunned()
    {
        {
            inputs.SetEnable(false);
            yield return new WaitForSeconds(stunDuration);
            inputs.SetEnable(true);
            
        }
    }
}
                
