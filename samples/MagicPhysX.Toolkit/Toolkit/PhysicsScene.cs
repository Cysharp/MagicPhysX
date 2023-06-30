using MagicPhysX.Toolkit.Internal;
using MagicPhysX;
using static MagicPhysX.NativeMethods;
using System.Numerics;
using MagicPhysX.Toolkit.Colliders;

namespace MagicPhysX.Toolkit;

public sealed unsafe partial class PhysicsScene : IDisposable
{
    PhysicsSystem physicsSystem;
    PxScene* scene;
    bool disposedValue;
    int frameCount;

    UnorderedKeyedCollection<RigidActor> activeActors = new UnorderedKeyedCollection<RigidActor>();

    public PhysicsScene(PhysicsSystem physicsSystem, PxScene* scene)
    {
        this.physicsSystem = physicsSystem;
        this.scene = scene;
        this.frameCount = 0;

        // NOTE: set contact manifold
        // https://docs.nvidia.com/gameworks/content/gameworkslibrary/physx/guide/Manual/AdvancedCollisionDetection.html#persistent-contact-manifold-pcm

        // NOTE: set broadphase type(see:Broad-phase Algorithms)
        // https://docs.nvidia.com/gameworks/content/gameworkslibrary/physx/guide/Manual/RigidBodyCollision.html
    }

    PxPhysics* physics => physicsSystem.physics;
    public int FrameCount => frameCount;

    public event Action<int>? Updating;
    public event Action<int>? Updated;
    public event Action? Disposing;

    public PhysicsSystem PhysicsSystem => physicsSystem;

    public RigidActor[] GetActiveActors()
    {
        lock (activeActors)
        {
            return activeActors.ToArray();
        }
    }

    public void ForEachActiveActors(Action<RigidActor> action)
    {
        lock (activeActors)
        {
            foreach (var item in activeActors.AsSpan())
            {
                action(item);
            }
        }
    }

    public bool TryCopyActiveActors(Span<RigidActor> dest)
    {
        lock (activeActors)
        {
            var span = activeActors.AsSpan();
            return span.TryCopyTo(dest);
        }
    }

    public int GetActiveActorCount()
    {
        lock (activeActors)
        {
            return activeActors.Count;
        }
    }

    public PxMaterial* CreateMaterial(float staticFriction, float dynamicFriction, float restitution)
    {
        return physicsSystem.CreateMaterial(staticFriction, @dynamicFriction, restitution);
    }

    public void Destroy(RigidActor actor)
    {
        scene->RemoveActorMut((PxActor*)actor.handler, wakeOnLostTouch: true);
        actor.handler = null;
        lock (activeActors)
        {
            activeActors.Remove(actor);
        }
    }

    public Rigidstatic AddStaticPlane(float x, float y, float z, float distance, Vector3 position, Quaternion rotation, PxMaterial* material = null)
    {
        var plane = PxPlane_new_1(x, y, z, distance);
        var transform = phys_PxTransformFromPlaneEquation(&plane);
        var geometry = PxPlaneGeometry_new();

        lock (activeActors)
        {
            var actor = AddStaticGeometry((PxGeometry*)&geometry, transform.p, transform.q, material, ColliderType.Plane);

            var shape = actor.GetComponent<PlaneCollider>().shape;
            var shapeOffset = new Transform { position = position, rotation = rotation };
            shape->SetLocalPoseMut(shapeOffset.AsPxPointer());

            return actor;
        }
    }

    Rigidstatic AddStaticGeometry(PxGeometry* geometry, Vector3 position, Quaternion rotation, PxMaterial* material, ColliderType type)
    {
        var shape = physics->CreateShapeMut(geometry, material, isExclusive: true, PxShapeFlags.Visualization | PxShapeFlags.SceneQueryShape | PxShapeFlags.SimulationShape);

        var transform = new Transform { position = position, rotation = rotation };

        var rigidStatic = physics->PhysPxCreateStatic1(transform.AsPxPointer(), shape);

        scene->AddActorMut((PxActor*)rigidStatic, bvh: null);

        var collider = Collider.Create(shape, type);
        var actor = new Rigidstatic(rigidStatic, collider);
        lock (activeActors)
        {
            activeActors.Add(actor);
        }
        return actor;
    }

    Rigidbody AddDynamicGeometry(PxGeometry* geometry, Vector3 position, Quaternion rotation, float density, PxMaterial* material, ColliderType type)
    {
        // use default option parameter in C++
        var shape = physics->CreateShapeMut(geometry, material, isExclusive: false, PxShapeFlags.Visualization | PxShapeFlags.SceneQueryShape | PxShapeFlags.SimulationShape);

        var transform = new Transform { position = position, rotation = rotation };
        var rigidDynamic = physics->PhysPxCreateDynamic1(transform.AsPxPointer(), shape, density);

        scene->AddActorMut((PxActor*)rigidDynamic, bvh: null);

        var collider = Collider.Create(shape, type);
        var actor = new Rigidbody(rigidDynamic, collider, scene);
        lock (activeActors)
        {
            activeActors.Add(actor);
        }
        return actor;
    }

    Rigidbody AddKinematicGeometry(PxGeometry* geometry, Vector3 position, Quaternion rotation, float density, PxMaterial* material, ColliderType type)
    {
        // use default option parameter in C++
        var shape = physics->CreateShapeMut(geometry, material, isExclusive: false, PxShapeFlags.Visualization | PxShapeFlags.SceneQueryShape | PxShapeFlags.SimulationShape);

        var transform = new Transform { position = position, rotation = rotation };
        var rigidDynamic = physics->PhysPxCreateKinematic1(transform.AsPxPointer(), shape, density);

        scene->AddActorMut((PxActor*)rigidDynamic, bvh: null);

        var collider = Collider.Create(shape, type);
        var actor = new Rigidbody(rigidDynamic, collider, scene);
        lock (activeActors)
        {
            activeActors.Add(actor);
        }
        return actor;
    }

    public void Update(float elapsedTime = 1.0f / 60.0f)
    {
        Updating?.Invoke(frameCount);

        scene->SimulateMut(elapsedTime, null, null, 0, controlSimulation: true);
        uint error = 0;
        PxScene_fetchResults_mut(scene, true, &error);
        this.frameCount++;

        Updated?.Invoke(frameCount);
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            Disposing?.Invoke();

            if (disposing)
            {
                // cleanup managed code
                physicsSystem.RemoveScene(this);
                physicsSystem = null!;
                activeActors.Clear();
                activeActors = null!;
            }

            // cleanup unmanaged resource
            PxScene_release_mut(scene);
            scene = null;
            disposedValue = true;
        }
    }

    ~PhysicsScene()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
