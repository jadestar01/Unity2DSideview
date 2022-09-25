using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashVis : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    public GameObject CharacterController;
    public Color Up;
    public Color Down;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Up;
    }

    // Update is called once per frame
    void Update()
    {
        if (CharacterController.GetComponent<CharacterController>().canDash == true)
        {
            spriteRenderer.color = Up;
        }
        else
            spriteRenderer.color = Down;
    }
}
