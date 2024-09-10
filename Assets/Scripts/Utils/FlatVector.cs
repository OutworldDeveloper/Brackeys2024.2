using UnityEngine;

[System.Serializable]
public struct FlatVector 
{

    public static readonly FlatVector zero = new FlatVector(0f, 0f);
    public static readonly FlatVector forward = new FlatVector(0f, 1f);
    public static readonly FlatVector back = new FlatVector(0f, -1f);
    public static readonly FlatVector right = new FlatVector(1f, 0f);
    public static readonly FlatVector left = new FlatVector(-1f, -0f);

    // Static Methods

    public static float Distance(FlatVector a, FlatVector b)
    {
        return Vector3.Distance(a.Vector3, b.Vector3);
    }

    public static float Angle(FlatVector from, FlatVector to)
    {
        return Vector3.Angle(from.Vector3, to.Vector3);
    }

    // Operators

    // -

    public static FlatVector operator -(FlatVector a, FlatVector b)
    {
        return new FlatVector(a.x - b.x, a.z - b.z);
    }

    public static FlatVector operator -(Vector3 a, FlatVector b)
    {
        return new FlatVector(a.x - b.x, a.z - b.z);
    }

    public static FlatVector operator -(FlatVector a, Vector3 b)
    {
        return new FlatVector(a.x - b.x, a.z - b.z);
    }

    public static FlatVector operator -(FlatVector a)
    {
        return new FlatVector(-a.x, -a.z);
    }

    // !=

    public static bool operator !=(FlatVector lhs, FlatVector rhs)
    {
        return lhs.Vector3 != rhs.Vector3;
    }

    // *

    public static FlatVector operator *(FlatVector a, float d)
    {
        return new FlatVector(a.x * d, a.z * d);
    }

    public static FlatVector operator *(float d, FlatVector a)
    {
        return a * d;
    }

    // /

    public static FlatVector operator /(FlatVector a, float d)
    {
        return new FlatVector(a.x / d, a.z / d);
    }

    // +

    public static FlatVector operator +(FlatVector a, FlatVector b)
    {
        return new FlatVector(a.x + b.x, a.z + b.z);
    }

    public static FlatVector operator +(Vector3 a, FlatVector b)
    {
        return new FlatVector(a.x + b.x, a.z + b.z);
    }

    public static FlatVector operator +(FlatVector a, Vector3 b)
    {
        return new FlatVector(a.x + b.x, a.z + b.z);
    }

    // ==

    public static bool operator ==(FlatVector lhs, FlatVector rhs)
    {
        return lhs.Vector3 == rhs.Vector3;
    }

    // Convertors

    public static implicit operator Vector3(FlatVector target)
    {
        return new Vector3(target.x, 0f, target.z);
    }

    public static explicit operator FlatVector(Vector3 target)
    {
        return new FlatVector(target.x, target.z);
    }

    public float x;
    public float z;

    public Vector3 Vector3 => new Vector3(x, 0f, z);
    public FlatVector normalized => (FlatVector)Vector3.Normalize(this);
    public float magnitude => new Vector2(x, z).magnitude;

    public FlatVector(float x, float z)
    {
        this.x = x;
        this.z = z;
    }

    public FlatVector(Vector3 vector3)
    {
        x = vector3.x;
        z = vector3.z;
    }

    public void Normalize()
    {
        FlatVector normalized = this.normalized;
        x = normalized.x;
        z = normalized.z;
    }

    public Vector3 WithY(float y)
    {
        return new Vector3(x, y, z);
    }

    public override bool Equals(object obj) => obj is FlatVector other && x == other.x && z == other.z;

    public override int GetHashCode() => Vector3.GetHashCode();

    public override string ToString() => Vector3.ToString();

}

public static class Vector3Extensions
{
    public static FlatVector Flat(this Vector3 target)
    {
        return new FlatVector(target);
    }

}