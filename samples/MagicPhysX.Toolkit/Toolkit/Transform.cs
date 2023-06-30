// compatible for PxTransform
using MagicPhysX;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MagicPhysX.Toolkit;

[StructLayout(LayoutKind.Sequential)]
public partial struct Transform
{
    public Quaternion rotation;
    public Vector3 position;

    public static implicit operator PxTransform(Transform v) => Unsafe.As<Transform, PxTransform>(ref v);
    public static implicit operator Transform(PxTransform v) => Unsafe.As<PxTransform, Transform>(ref v);

    internal unsafe PxTransform* AsPxPointer() => (PxTransform*)Unsafe.AsPointer(ref Unsafe.AsRef(this));
}
