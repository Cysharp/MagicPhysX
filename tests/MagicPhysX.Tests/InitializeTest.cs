using System.Text;
using static MagicPhysX.NativeMethods;

namespace MagicPhysX.Tests;

public class BasicTest
{
    [Fact]
    public unsafe void InitializeTest()
    {
        // create foundation
        var foundation = physx_create_foundation();
        (foundation != (PxFoundation*)IntPtr.Zero).Should().BeTrue();

        // create pvd
        var pvd = phys_PxCreatePvd(foundation);
        (pvd != (PxPvd*)IntPtr.Zero).Should().BeTrue();

        fixed (byte* bytePointer = Encoding.UTF8.GetBytes("127.0.0.1"))
        {
            var transport = phys_PxDefaultPvdSocketTransportCreate(bytePointer, 5425, 10);
            pvd->ConnectMut(transport, PxPvdInstrumentationFlags.All);
        }

        // create physics
        uint PX_PHYSICS_VERSION_MAJOR = 5;
        uint PX_PHYSICS_VERSION_MINOR = 1;
        uint PX_PHYSICS_VERSION_BUGFIX = 3;
        uint versionNumber = (PX_PHYSICS_VERSION_MAJOR << 24) + (PX_PHYSICS_VERSION_MINOR << 16) + (PX_PHYSICS_VERSION_BUGFIX << 8);

        var tolerancesScale = new PxTolerancesScale { length = 1, speed = 10 };
        var physics = phys_PxCreatePhysics(versionNumber, foundation, &tolerancesScale, true, pvd, null);
        (physics != (PxPhysics*)IntPtr.Zero).Should().BeTrue();

        phys_PxInitExtensions(physics, pvd);

        var sceneDesc = PxSceneDesc_new(PxPhysics_getTolerancesScale(physics));
        sceneDesc.gravity = new PxVec3 { x = 0.0f, y = -9.81f, z = 0.0f };

        var dispatcher = phys_PxDefaultCpuDispatcherCreate(1, null, PxDefaultCpuDispatcherWaitForWorkMode.WaitForWork, 0);
        sceneDesc.cpuDispatcher = (PxCpuDispatcher*)dispatcher;
        sceneDesc.filterShader = get_default_simulation_filter_shader();

        var scene = PxPhysics_createScene_mut(physics, &sceneDesc);
        (scene != (PxScene*)IntPtr.Zero).Should().BeTrue();

        // pvd client
        var pvdClient = scene->GetScenePvdClientMut();
        (pvdClient != (PxPvdSceneClient*)IntPtr.Zero).Should().BeTrue();
    }
}
