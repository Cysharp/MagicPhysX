using MagicPhysX;
using System.Numerics;
using static MagicPhysX.NativeMethods;

namespace MagicPhysX.Toolkit;

public sealed unsafe partial class PhysicsScene
{
    public Rigidbody AddDynamicSphere(float radius, Vector3 position, Quaternion rotation, float density, PxMaterial* material = null)
    {
        var geometry = PxSphereGeometry_new(radius);
        return AddDynamicGeometry((PxGeometry*)&geometry, position, rotation, density, material, ColliderType.Sphere);
    }

    public Rigidbody AddKinematicSphere(float radius, Vector3 position, Quaternion rotation, float density, PxMaterial* material = null)
    {
        var geometry = PxSphereGeometry_new(radius);
        return AddKinematicGeometry((PxGeometry*)&geometry, position, rotation, density, material, ColliderType.Sphere);
    }

    public Rigidstatic AddStaticSphere(float radius, Vector3 position, Quaternion rotation, PxMaterial* material = null)
    {
        var geometry = PxSphereGeometry_new(radius);
        return AddStaticGeometry((PxGeometry*)&geometry, position, rotation, material, ColliderType.Sphere);
    }

    public Rigidbody AddDynamicBox(Vector3 halfExtent, Vector3 position, Quaternion rotation, float density, PxMaterial* material = null)
    {
        var geometry = PxBoxGeometry_new_1(halfExtent);
        return AddDynamicGeometry((PxGeometry*)&geometry, position, rotation, density, material, ColliderType.Box);
    }

    public Rigidbody AddKinematicBox(Vector3 halfExtent, Vector3 position, Quaternion rotation, float density, PxMaterial* material = null)
    {
        var geometry = PxBoxGeometry_new_1(halfExtent);
        return AddKinematicGeometry((PxGeometry*)&geometry, position, rotation, density, material, ColliderType.Box);
    }

    public Rigidstatic AddStaticBox(Vector3 halfExtent, Vector3 position, Quaternion rotation, PxMaterial* material = null)
    {
        var geometry = PxBoxGeometry_new_1(halfExtent);
        return AddStaticGeometry((PxGeometry*)&geometry, position, rotation, material, ColliderType.Box);
    }

    public Rigidbody AddDynamicCapsule(float radius, float halfHeight, Vector3 position, Quaternion rotation, float density, PxMaterial* material = null)
    {
        var geometry = PxCapsuleGeometry_new(radius, halfHeight);
        return AddDynamicGeometry((PxGeometry*)&geometry, position, rotation, density, material, ColliderType.Capsule);
    }

    public Rigidbody AddKinematicCapsule(float radius, float halfHeight, Vector3 position, Quaternion rotation, float density, PxMaterial* material = null)
    {
        var geometry = PxCapsuleGeometry_new(radius, halfHeight);
        return AddKinematicGeometry((PxGeometry*)&geometry, position, rotation, density, material, ColliderType.Capsule);
    }

    public Rigidstatic AddStaticCapsule(float radius, float halfHeight, Vector3 position, Quaternion rotation, PxMaterial* material = null)
    {
        var geometry = PxCapsuleGeometry_new(radius, halfHeight);
        return AddStaticGeometry((PxGeometry*)&geometry, position, rotation, material, ColliderType.Capsule);
    }

}
