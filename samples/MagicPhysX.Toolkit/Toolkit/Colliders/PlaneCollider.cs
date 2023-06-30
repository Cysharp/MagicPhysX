using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MagicPhysX.Toolkit.Colliders
{
    // PlaneCollider is not exists in Unity

    public unsafe class PlaneCollider : Collider
    {
        ref PxPlaneGeometry GetGeometry() => ref Unsafe.AsRef<PxPlaneGeometry>(shape->GetGeometry());

        internal PlaneCollider(PxShape* shape, ColliderType type) : base(shape, type)
        {
        }

        /// <summary>
        ///   <para>The center of the plane in the object's local space.</para>
        /// </summary>
        public Vector3 center
        {
            get => shape->GetLocalPose().p;
            set
            {
                var pose = shape->GetLocalPose();
                pose.p = value;
                shape->SetLocalPoseMut(&pose);
            }
        }
    }
}
