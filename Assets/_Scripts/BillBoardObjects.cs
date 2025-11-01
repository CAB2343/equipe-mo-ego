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

    void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null && spriteRenderer.transform == transform)
        {
            GameObject visual = new GameObject("SpriteVisual");
            visual.transform.SetParent(transform, false);

            SpriteRenderer childSr = visual.AddComponent<SpriteRenderer>();

            // copia propriedades essenciais
            childSr.sprite = spriteRenderer.sprite;
            childSr.color = spriteRenderer.color;
            childSr.flipX = spriteRenderer.flipX;
            childSr.flipY = spriteRenderer.flipY;
            childSr.sortingLayerID = spriteRenderer.sortingLayerID;
            childSr.sortingOrder = spriteRenderer.sortingOrder;
            childSr.material = spriteRenderer.material;
            childSr.maskInteraction = spriteRenderer.maskInteraction;
            childSr.drawMode = spriteRenderer.drawMode;
            childSr.size = spriteRenderer.size;

            // desativa o renderer original e passa a referência para o filho
            spriteRenderer.enabled = false;
            spriteRenderer = childSr; 
        }
    }

    void Update()
    {
        if (Camera.main == null) return;

        // direção da câmera em espaço mundial (projeta no plano XZ para ignorar altura)
        Vector3 toCamera = Camera.main.transform.position - transform.position;
        Vector3 toCameraXZ = Vector3.ProjectOnPlane(toCamera, Vector3.up);
        if (toCameraXZ.sqrMagnitude < 0.0001f) return;
        toCameraXZ.Normalize();

        // calcular direção LOCAL relativo ao transform PAI (antes de rotacionar o filho)
        Vector3 localDirection = transform.InverseTransformDirection(toCameraXZ).normalized;

        if (_enableAnyDirections)
        {
            if (localDirection.z > 0.7f)
            {
                spriteRenderer.sprite = frontSprite;
            }
            else if (localDirection.z < -0.7f)
            {
                spriteRenderer.sprite = backSprite;
            }
            else if (localDirection.x > 0f)
            {
                spriteRenderer.sprite = rightSprite;
            }
            else
            {
                spriteRenderer.sprite = leftSprite;
            }
        }

        // rotaciona apenas o SpriteRenderer (filho visual) para olhar a câmera
        if (spriteRenderer != null)
        {
            Quaternion lookRot = Quaternion.LookRotation(Camera.main.transform.position - spriteRenderer.transform.position, Vector3.up);
            spriteRenderer.transform.rotation = lookRot;
        }
    }
}
