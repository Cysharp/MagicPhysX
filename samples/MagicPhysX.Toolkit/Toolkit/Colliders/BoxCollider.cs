using System.Numerics;
using System.Runtime.CompilerServices;

namespace MagicPhysX.Toolkit.Colliders
{
    /// <summary>
    ///   <para>A box-shaped primitive collider.</para>
    /// </summary>
    public unsafe class BoxCollider : Collider
    {
        ref PxBoxGeometry GetGeometry() => ref Unsafe.AsRef<PxBoxGeometry>(shape->GetGeometry());

        internal BoxCollider(PxShape* shape, ColliderType type) : base(shape, type)
        {
        }

        /// <summary>
        ///   <para>The center of the box, measured in the object's local space.</para>
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
        ///   <para>The size of the box, measured in the object's local space.</para>
        /// </summary>
        public Vector3 size
        {
            get => GetGeometry().halfExtents;
            set => GetGeometry().halfExtents = value;
        }
    }
}
