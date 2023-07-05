using MagicPhysX;
using System.Runtime.CompilerServices;
using System.Text;
using static MagicPhysX.NativeMethods;

namespace ConsoleSandbox;

public static class RaycastSingle
{
    public static unsafe void Run()
    {
        var foundation = physx_create_foundation();

        // create pvd
        var pvd = phys_PxCreatePvd(foundation);

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

        phys_PxInitExtensions(physics, pvd);

        var sceneDesc = PxSceneDesc_new(PxPhysics_getTolerancesScale(physics));
        sceneDesc.gravity = new PxVec3 { x = 0.0f, y = -9.81f, z = 0.0f };

        var dispatcher = phys_PxDefaultCpuDispatcherCreate(1, null, PxDefaultCpuDispatcherWaitForWorkMode.WaitForWork, 0);
        sceneDesc.cpuDispatcher = (PxCpuDispatcher*)dispatcher;
        sceneDesc.filterShader = get_default_simulation_filter_shader();

        var scene = PxPhysics_createScene_mut(physics, &sceneDesc);

        // pvd client
        var pvdClient = scene->GetScenePvdClientMut();
        if (pvdClient != null)
        {
            pvdClient->SetScenePvdFlagMut(PxPvdSceneFlag.TransmitConstraints, true);
            pvdClient->SetScenePvdFlagMut(PxPvdSceneFlag.TransmitContacts, true);
            pvdClient->SetScenePvdFlagMut(PxPvdSceneFlag.TransmitScenequeries, true);
        }

        var material = PxPhysics_createMaterial_mut(physics, 0.5f, 0.5f, 0.6f);

        // plane
        var plane = PxPlane_new_1(0.0f, 1.0f, 0.0f, 0.0f);
        var groundPlane = phys_PxCreatePlane(physics, &plane, material);

        PxScene_addActor_mut(scene, (PxActor*)groundPlane, null);

        // box1
        var box1Position = new PxVec3
        {
            x = 0f,
            y = 0f,
            z = 0f
        };

        var box1Transform = PxTransform_new_1(&box1Position);
        var box1ShapeOffset = PxTransform_new_2(PxIDENTITY.PxIdentity);
        var box1Geometry = PxBoxGeometry_new(1.0f, 1.0f, 1.0f);

        physics->CreateShapeMut(
            (PxGeometry*)&box1Geometry,
            material,
            false,
            PxShapeFlags.Visualization | PxShapeFlags.SceneQueryShape | PxShapeFlags.SimulationShape);

        var box1 = phys_PxCreateDynamic(
            physics,
            &box1Transform,
            (PxGeometry*)&box1Geometry,
            material,
            1.0f,
            &box1ShapeOffset);

        PxScene_addActor_mut(scene, (PxActor*)box1, null);

        var origin = new PxVec3
        {
            x = 3.0f,
            y = 3.0f,
            z = 3.0f
        };

        var direction1 = new PxVec3
        {
            x = -1.0f,
            y = -1.0f,
            z = -1.0f
        };

        var normalizedDirection1 = direction1.GetNormalized();

        var outputFlags1 = PxHitFlags.Default;
        var hit1 = new PxRaycastHit();
        var filterData1 = PxQueryFilterData_new();

        var result1 = scene->QueryExtRaycastSingle(
            (PxVec3*)Unsafe.AsPointer(ref origin),
            (PxVec3*)Unsafe.AsPointer(ref normalizedDirection1),
            10.0f,
            outputFlags1,
            &hit1,
            &filterData1,
            null,
            null);

        Console.WriteLine($"Raycast result1: {result1}");
        Console.WriteLine($"    hit.position=x:{hit1.position.x},y:{hit1.position.y},z:{hit1.position.z}");

        var direction2 = new PxVec3
        {
            x = 1.0f,
            y = 1.0f,
            z = 1.0f
        };

        var normalizedDirection2 = direction2.GetNormalized();

        var outputFlags2 = PxHitFlags.Default;
        var hit2 = new PxRaycastHit();
        var filterData2 = PxQueryFilterData_new();

        var result2 = scene->QueryExtRaycastSingle(
            (PxVec3*)Unsafe.AsPointer(ref origin),
            (PxVec3*)Unsafe.AsPointer(ref normalizedDirection2),
            10.0f,
            outputFlags2,
            &hit2,
            &filterData2,
            null,
            null);

        Console.WriteLine($"Raycast result2: {result2}");

    }
}
