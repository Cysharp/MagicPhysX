using MagicPhysX.Toolkit.Internal;
using MagicPhysX;
using System.Text;
using static MagicPhysX.NativeMethods;
using System.Numerics;

namespace MagicPhysX.Toolkit;

public sealed unsafe class PhysicsSystem : IDisposable
{
    static readonly uint VersionNumber = (5 << 24) + (1 << 16) + (3 << 8);
    PxDefaultCpuDispatcher* dispatcher;
    internal PxPhysics* physics;

    bool disposedValue;
    bool enablePvd;

    UnorderedKeyedCollection<PhysicsScene> scenes = new UnorderedKeyedCollection<PhysicsScene>();

    public PhysicsSystem()
        : this(false)
    {
    }

    public PhysicsSystem(bool enablePvd)
        : this(enablePvd, "127.0.0.1", 5425)
    {
    }

    public PhysicsSystem(string pvdIp, int pvdPort)
        : this(true, pvdIp, pvdPort)
    {
    }

    PhysicsSystem(bool enablePvd, string pvdIp, int pvdPort)
    {
        this.enablePvd = enablePvd;

        if (!enablePvd)
        {
            this.physics = physx_create_physics(PhysicsFoundation.GetFoundation());
            this.dispatcher = phys_PxDefaultCpuDispatcherCreate(1, null, PxDefaultCpuDispatcherWaitForWorkMode.WaitForWork, 0);
            return;
        }

        PxPvd* pvd = default;

        // create pvd
        pvd = phys_PxCreatePvd(PhysicsFoundation.GetFoundation());

        fixed (byte* bytePointer = Encoding.UTF8.GetBytes(pvdIp))
        {
            var transport = phys_PxDefaultPvdSocketTransportCreate(bytePointer, pvdPort, 10);
            pvd->ConnectMut(transport, PxPvdInstrumentationFlags.All);
        }

        var tolerancesScale = new PxTolerancesScale
        {
            length = 1,
            speed = 10
        };

        this.physics = phys_PxCreatePhysics(
            VersionNumber,
            PhysicsFoundation.GetFoundation(),
            &tolerancesScale,
            true,
            pvd,
            null);
        phys_PxInitExtensions(physics, pvd);

        this.dispatcher = phys_PxDefaultCpuDispatcherCreate(1, null, PxDefaultCpuDispatcherWaitForWorkMode.WaitForWork, 0);
    }

    public event Action<PhysicsScene>? SceneCreated;

    public PhysicsScene CreateScene()
    {
        var scene = CreateScene(new Vector3(0.0f, -9.81f, 0.0f));
        lock (scenes)
        {
            scenes.Add(scene);
        }

        SceneCreated?.Invoke(scene);
        return scene;
    }

    public PhysicsScene CreateScene(Vector3 gravity)
    {
        var sceneDesc = PxSceneDesc_new(PxPhysics_getTolerancesScale(physics));
        sceneDesc.gravity = gravity;
        sceneDesc.cpuDispatcher = (PxCpuDispatcher*)dispatcher;
        sceneDesc.filterShader = get_default_simulation_filter_shader();
        sceneDesc.solverType = PxSolverType.Pgs;

        var scene = physics->CreateSceneMut(&sceneDesc);

        if (enablePvd)
        {
            var pvdClient = scene->GetScenePvdClientMut();

            if (pvdClient != null)
            {
                pvdClient->SetScenePvdFlagMut(PxPvdSceneFlag.TransmitConstraints, true);
                pvdClient->SetScenePvdFlagMut(PxPvdSceneFlag.TransmitContacts, true);
                pvdClient->SetScenePvdFlagMut(PxPvdSceneFlag.TransmitScenequeries, true);
            }
        }

        return new PhysicsScene(this, scene);
    }

    internal void RemoveScene(PhysicsScene scene)
    {
        lock (scenes)
        {
            scenes.Remove(scene);
        }
    }

    public PhysicsScene[] GetPhysicsScenes()
    {
        lock (scenes)
        {
            return scenes.ToArray();
        }
    }

    public void ForEachPhysicsScenes(Action<PhysicsScene> action)
    {
        lock (scenes)
        {
            foreach (var item in scenes.AsSpan())
            {
                action(item);
            }
        }
    }

    public bool TryCopyPhysicsScenes(Span<PhysicsScene> dest)
    {
        lock (scenes)
        {
            var span = scenes.AsSpan();
            return span.TryCopyTo(dest);
        }
    }

    public PxMaterial* CreateMaterial(float staticFriction, float dynamicFriction, float restitution)
    {
        return physics->CreateMaterialMut(staticFriction, @dynamicFriction, restitution);
    }

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // cleanup managed code
                foreach (var item in scenes.AsSpan())
                {
                    item.Dispose();
                }
                scenes.Clear();
                scenes = null!;
            }

            // cleanup unmanaged resource
            PxDefaultCpuDispatcher_release_mut(dispatcher);
            PxPhysics_release_mut(physics);

            dispatcher = null;
            physics = null;

            disposedValue = true;
        }
    }

    ~PhysicsSystem()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
