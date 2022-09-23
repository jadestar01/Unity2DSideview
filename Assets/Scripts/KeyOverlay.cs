using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyOverlay : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    public KeyCode key;

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
        if (Input.GetKeyDown(key))
        {
            spriteRenderer.color = Down;
        }
        else if (Input.GetKeyUp(key))
        {
            spriteRenderer.color = Up;
        }
        else if (Input.GetKey(key))
        {
            spriteRenderer.color = Down;
        }
    }
}
