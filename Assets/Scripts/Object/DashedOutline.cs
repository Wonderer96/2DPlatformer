using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteToBorder : MonoBehaviour
{
    [SerializeField] private float lineWidth = 0.2f; // �����߿�
    [SerializeField] private Color lineColor = Color.white;

    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false; // ���� Sprite

        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        ConfigureLineRenderer(lineRenderer);
        SetBorderPositions(lineRenderer, spriteRenderer);
    }

    void ConfigureLineRenderer(LineRenderer renderer)
    {
        // ʹ�� Unity ���õ� Sprites/Default ��ɫ��
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.startColor = lineColor;
        renderer.endColor = lineColor;
        renderer.startWidth = lineWidth;
        renderer.endWidth = lineWidth;
        renderer.loop = true; // ȷ���պ�
        renderer.useWorldSpace = false; // ʹ�ñ�������ϵ
    }

    void SetBorderPositions(LineRenderer renderer, SpriteRenderer sprite)
    {
        Vector2 size = sprite.sprite.bounds.size;
        float halfWidth = size.x / 2f;
        float halfHeight = size.y / 2f;

        Vector3[] positions = new Vector3[4]
        {
            new Vector3(-halfWidth, halfHeight, 0),  // ����
            new Vector3(halfWidth, halfHeight, 0),   // ����
            new Vector3(halfWidth, -halfHeight, 0),   // ����
            new Vector3(-halfWidth, -halfHeight, 0)   // ����
        };

        renderer.positionCount = positions.Length;
        renderer.SetPositions(positions);
    }
}

