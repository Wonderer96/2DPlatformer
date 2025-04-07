using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteToBorder : MonoBehaviour
{
    [SerializeField] private float lineWidth = 0.2f; // 调大线宽
    [SerializeField] private Color lineColor = Color.white;

    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false; // 隐藏 Sprite

        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        ConfigureLineRenderer(lineRenderer);
        SetBorderPositions(lineRenderer, spriteRenderer);
    }

    void ConfigureLineRenderer(LineRenderer renderer)
    {
        // 使用 Unity 内置的 Sprites/Default 着色器
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.startColor = lineColor;
        renderer.endColor = lineColor;
        renderer.startWidth = lineWidth;
        renderer.endWidth = lineWidth;
        renderer.loop = true; // 确保闭合
        renderer.useWorldSpace = false; // 使用本地坐标系
    }

    void SetBorderPositions(LineRenderer renderer, SpriteRenderer sprite)
    {
        Vector2 size = sprite.sprite.bounds.size;
        float halfWidth = size.x / 2f;
        float halfHeight = size.y / 2f;

        Vector3[] positions = new Vector3[4]
        {
            new Vector3(-halfWidth, halfHeight, 0),  // 左上
            new Vector3(halfWidth, halfHeight, 0),   // 右上
            new Vector3(halfWidth, -halfHeight, 0),   // 右下
            new Vector3(-halfWidth, -halfHeight, 0)   // 左下
        };

        renderer.positionCount = positions.Length;
        renderer.SetPositions(positions);
    }
}

