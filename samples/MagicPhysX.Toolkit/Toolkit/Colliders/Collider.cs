using System;
using System.Runtime.CompilerServices;

namespace MagicPhysX.Toolkit.Colliders
{
    /// <summary>
    ///   <para>A base class of all colliders.</para>
    /// </summary>
    public unsafe abstract class Collider
    {
        internal PxShape* shape;
        readonly ColliderType type;

        internal Collider(PxShape* shape, ColliderType type)
        {
            this.shape = shape;
            this.type = type;
        }

        public PxShape* GetShapeHandler() => shape;

        public static Collider Create(PxShape* shape, ColliderType type)
        {
            switch (type)
            {
                case ColliderType.Box:
                    return new BoxCollider(shape, type);
                case ColliderType.Capsule:
                    return new CapsuleCollider(shape, type);
                case ColliderType.Sphere:
                    return new SphereCollider(shape, type);
                case ColliderType.Plane:
                    return new PlaneCollider(shape, type);
                default:
                    throw new ArgumentException();
            }
        }

        public ColliderType Type => type;
    }
}
