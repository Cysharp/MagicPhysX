using MagicPhysX;

namespace MagicPhysX.Toolkit.Internal;

internal static class RigidbodyConstraintsExtensions
{
    public static PxRigidDynamicLockFlags AsPxRigidDynamicLockFlags(this RigidbodyConstraints constraints)
    {
        PxRigidDynamicLockFlags result = 0;
        if ((constraints & RigidbodyConstraints.FreezePositionX) == RigidbodyConstraints.FreezePositionX)
        {
            result |= PxRigidDynamicLockFlags.LockLinearX;
        }

        if ((constraints & RigidbodyConstraints.FreezePositionY) == RigidbodyConstraints.FreezePositionY)
        {
            result |= PxRigidDynamicLockFlags.LockLinearY;
        }

        if ((constraints & RigidbodyConstraints.FreezePositionZ) == RigidbodyConstraints.FreezePositionZ)
        {
            result |= PxRigidDynamicLockFlags.LockLinearZ;
        }

        if ((constraints & RigidbodyConstraints.FreezeRotationX) == RigidbodyConstraints.FreezeRotationX)
        {
            result |= PxRigidDynamicLockFlags.LockAngularX;
        }

        if ((constraints & RigidbodyConstraints.FreezeRotationY) == RigidbodyConstraints.FreezeRotationY)
        {
            result |= PxRigidDynamicLockFlags.LockAngularY;
        }

        if ((constraints & RigidbodyConstraints.FreezeRotationZ) == RigidbodyConstraints.FreezeRotationZ)
        {
            result |= PxRigidDynamicLockFlags.LockAngularZ;
        }

        return result;
    }
}
