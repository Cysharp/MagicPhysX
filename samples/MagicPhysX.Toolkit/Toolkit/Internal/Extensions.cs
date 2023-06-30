using System.Numerics;
using System.Runtime.CompilerServices;

namespace MagicPhysX.Toolkit.Internal;

internal static class Extensions
{
    internal static unsafe PxVec3* AsPxPointer(this Vector3 v) => (PxVec3*)Unsafe.AsPointer(ref Unsafe.AsRef(v));
}
