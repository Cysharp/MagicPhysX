using System.Numerics;
using System.Runtime.CompilerServices;

namespace MagicPhysX;

public partial struct PxVec2
{
    public static implicit operator PxVec2(Vector2 v) => Unsafe.As<Vector2, PxVec2>(ref v);
    public static implicit operator Vector2(PxVec2 v) => Unsafe.As<PxVec2, Vector2>(ref v);
}

public partial struct PxVec3
{
    public static implicit operator PxVec3(Vector3 v) => Unsafe.As<Vector3, PxVec3>(ref v);
    public static implicit operator Vector3(PxVec3 v) => Unsafe.As<PxVec3, Vector3>(ref v);
}

public partial struct PxVec4
{
    public static implicit operator PxVec4(Vector4 v) => Unsafe.As<Vector4, PxVec4>(ref v);
    public static implicit operator Vector4(PxVec4 v) => Unsafe.As<PxVec4, Vector4>(ref v);
}

public partial struct PxQuat
{
    public static implicit operator PxQuat(Quaternion v) => Unsafe.As<Quaternion, PxQuat>(ref v);
    public static implicit operator Quaternion(PxQuat v) => Unsafe.As<PxQuat, Quaternion>(ref v);
}

