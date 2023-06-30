using MagicPhysX;

namespace MagicPhysX.Toolkit.Internal;

internal static class PxRigidDynamicLockFlagsExtensions
{
    public static RigidbodyConstraints AsRigidbodyConstraints(this PxRigidDynamicLockFlags flags)
    {
        var result = RigidbodyConstraints.None;
        if ((flags & PxRigidDynamicLockFlags.LockLinearX) == PxRigidDynamicLockFlags.LockLinearX)
        {
            result |= RigidbodyConstraints.FreezePositionX;
        }

        if ((flags & PxRigidDynamicLockFlags.LockLinearY) == PxRigidDynamicLockFlags.LockLinearY)
        {
            result |= RigidbodyConstraints.FreezePositionY;
        }

        if ((flags & PxRigidDynamicLockFlags.LockLinearZ) == PxRigidDynamicLockFlags.LockLinearZ)
        {
            result |= RigidbodyConstraints.FreezePositionZ;
        }

        if ((flags & PxRigidDynamicLockFlags.LockAngularX) == PxRigidDynamicLockFlags.LockAngularX)
        {
            result |= RigidbodyConstraints.FreezeRotationX;
        }

        if ((flags & PxRigidDynamicLockFlags.LockAngularY) == PxRigidDynamicLockFlags.LockAngularY)
        {
            result |= RigidbodyConstraints.FreezeRotationY;
        }

        if ((flags & PxRigidDynamicLockFlags.LockAngularZ) == PxRigidDynamicLockFlags.LockAngularZ)
        {
            result |= RigidbodyConstraints.FreezeRotationZ;
        }

        return result;
    }
}
