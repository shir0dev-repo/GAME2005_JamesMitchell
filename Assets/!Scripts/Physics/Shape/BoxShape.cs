using UnityEngine;

[System.Serializable]
public class BoxShape : Shape
{
    [SerializeField] private Vector3 m_halfExtents;
    public Vector3 HalfExtents => m_halfExtents;

    public BoxShape() : this(Vector3.one * 0.5f) { }

    public BoxShape(float halfExtentX, float halfExtentY, float halfExtentZ) : this(new Vector3(halfExtentX, halfExtentY, halfExtentZ)) { }

    public BoxShape(Vector3 halfExtents)
    {
        m_halfExtents = halfExtents;
    }

    public override bool CheckCollisions()
    {
        return false;
    }
}
