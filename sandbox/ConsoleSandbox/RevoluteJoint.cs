using MagicPhysX;
using System.Runtime.CompilerServices;
using static MagicPhysX.NativeMethods;

namespace ConsoleSandbox;

public static class RevoluteJoint
{
    public static unsafe void Run()
    {
        var foundation = physx_create_foundation();

        // create pvd
        var pvd = phys_PxCreatePvd(foundation);

        fixed (byte* bytePointer = "127.0.0.1"u8.ToArray())
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
            x = 0.0f,
            y = 10.0f,
            z = 0.0f
        };

        var box1Transform = PxTransform_new_1(&box1Position);
        var box1ShapeOffset = PxTransform_new_2(PxIDENTITY.PxIdentity);
        var box1Geometry = PxBoxGeometry_new(2.0f, 12.0f, 2.0f);

        physics->CreateShapeMut(
            (PxGeometry*)&box1Geometry,
            material,
            false,
            PxShapeFlags.Visualization | PxShapeFlags.SceneQueryShape | PxShapeFlags.SimulationShape);

        var box1 = phys_PxCreateStatic(
            physics,
            &box1Transform,
            (PxGeometry*)&box1Geometry,
            material,
            &box1ShapeOffset);

        PxScene_addActor_mut(scene, (PxActor*)box1, null);

        // box2
        var box2Position = new PxVec3
        {
            x = 12.0f,
            y = 20.0f,
            z = 4.0f
        };

        var box2Transform = PxTransform_new_1(&box2Position);
        var box2ShapeOffset = PxTransform_new_2(PxIDENTITY.PxIdentity);
        var box2Geometry = PxBoxGeometry_new(10.0f, 2.0f, 6.0f);

        physics->CreateShapeMut(
            (PxGeometry*)&box2Geometry,
            material,
            false,
            PxShapeFlags.Visualization | PxShapeFlags.SceneQueryShape | PxShapeFlags.SimulationShape);

        var box2 = phys_PxCreateDynamic(
            physics,
            &box2Transform,
            (PxGeometry*)&box2Geometry,
            material,
            1.0f,
            &box2ShapeOffset);

        PxScene_addActor_mut(scene, (PxActor*)box2, null);

        // joint1
        var anchor1 = new PxVec3
        {
            x = 2.0f,
            y = 10.0f,
            z = 0.0f
        };

        var anchor2 = new PxVec3
        {
            x = -10.0f,
            y = 0.0f,
            z = -4.0f
        };

        var transform1 = PxTransform_new_1((PxVec3*)Unsafe.AsPointer(ref anchor1));
        var transform2 = PxTransform_new_1((PxVec3*)Unsafe.AsPointer(ref anchor2));

        var joint1 = phys_PxRevoluteJointCreate(
            physics,
            (PxRigidActor*)box1,
            &transform1,
            (PxRigidActor*)box2,
            &transform2);

        // simulate
        Console.WriteLine("Start simulate");

        for (var i = 0; i < 300; i++)
        {
            PxScene_simulate_mut(scene, 1.0f / 30.0f, null, null, 0, true);
            uint error = 0;
            PxScene_fetchResults_mut(scene, true, &error);

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"Frame: {i}");
        }

        Console.WriteLine("\nDone");
    }
}
