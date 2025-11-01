using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoardObjects : MonoBehaviour
{
    public bool _enableAnyDirections = true;
    public SpriteRenderer spriteRenderer;
    public Sprite frontSprite;
    public Sprite backSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;

    void Update()
    {

        transform.LookAt(Camera.main.transform);

        if(_enableAnyDirections == true)
        {
            Vector3 toCamera = Camera.main.transform.position - transform.position;
            Vector3 localDirection = transform.InverseTransformDirection(toCamera.normalized);

            if (localDirection.z > 0.5f)
            {
                spriteRenderer.sprite = frontSprite;
            }
            else if (localDirection.z < -0.5f)
            {
                spriteRenderer.sprite = backSprite;
            }
            else if (localDirection.x > 0.5f)
            {
                spriteRenderer.sprite = rightSprite;
            }
            else if (localDirection.x < -0.5f)
            {
                spriteRenderer.sprite = leftSprite;
            }
        }
    }
}
