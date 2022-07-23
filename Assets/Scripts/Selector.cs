using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
    public Player playerScript;

   

    public bool CheckOverlap()
    {
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(new Vector2(transform.position.x, transform.position.y), new Vector2(0.8f, 0.8f), 0f);
        
        if (hitColliders.Length > 0)
        {
            playerScript.currentOverlap = hitColliders[0].gameObject;
            return true;
        }
        else
            return false; 
    }
}
