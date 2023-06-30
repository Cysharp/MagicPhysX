using System.Numerics;
using System.Runtime.CompilerServices;

namespace MagicPhysX.Toolkit.Colliders
{
    /// <summary>
    ///   <para>A capsule-shaped primitive collider.</para>
    /// </summary>
    public unsafe class CapsuleCollider : Collider
    {
        ref PxCapsuleGeometry GetGeometry() => ref Unsafe.AsRef<PxCapsuleGeometry>(shape->GetGeometry());

        internal CapsuleCollider(PxShape* shape, ColliderType type) : base(shape, type)
        {
        }

        /// <summary>
        ///   <para>The center of the capsule, measured in the object's local space.</para>
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
        ///   <para>The radius of the sphere, measured in the object's local space.</para>
        /// </summary>
        public float radius
        {
            get => GetGeometry().radius;
            set => GetGeometry().radius = value;
        }

        /// <summary>
        ///   <para>The height of the capsule measured in the object's local space.</para>
        /// </summary>
        public float height
        {
            get => GetGeometry().halfHeight * 2f;
            set => GetGeometry().halfHeight = value / 2f;
        }

        ///// <summary>
        /////   <para>The direction of the capsule.</para>
        ///// </summary>
        //public int direction
        //{
        //    // TODO:
        //    get => throw new NotImplementedException();
        //    // TODO:
        //    set => throw new NotImplementedException();
        //}
    }
}
