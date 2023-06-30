using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MagicPhysX.Toolkit.Colliders
{
    /// <summary>
    ///   <para>A sphere-shaped primitive collider.</para>
    /// </summary>
    public unsafe class SphereCollider : Collider
    {
        ref PxSphereGeometry GetGeometry() => ref Unsafe.AsRef<PxSphereGeometry>(shape->GetGeometry());

        internal SphereCollider(PxShape* shape, ColliderType type) : base(shape, type)
        {
        }

        /// <summary>
        ///   <para>The center of the sphere in the object's local space.</para>
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

        /// <summary>
        ///   <para>The radius of the sphere measured in the object's local space.</para>
        /// </summary>
        public float radius
        {
            get => GetGeometry().radius;
            set => GetGeometry().radius = value;
        }
    }
}
