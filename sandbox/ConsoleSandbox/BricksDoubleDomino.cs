using MagicPhysX;
using static MagicPhysX.NativeMethods;
using System.Text;

namespace ConsoleSandbox;

public static class BricksDoubleDomino
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

        // sphere
        var sphereVec = new PxVec3
        {
            x = 0.0f,
            y = 10.0f,
            z = 0.0f
        };

        var transform1 = PxTransform_new_1(&sphereVec);
        var transform2 = PxTransform_new_2(PxIDENTITY.PxIdentity);
        var sphereGeo = PxSphereGeometry_new(1.0f);

        physics->CreateShapeMut(
            (PxGeometry*)&sphereGeo,
            material,
            false,
            PxShapeFlags.Visualization | PxShapeFlags.SceneQueryShape | PxShapeFlags.SimulationShape);

        var sphere = phys_PxCreateDynamic(
            physics,
            &transform1,
            (PxGeometry*)&sphereGeo,
            material,
            10.0f,
            &transform2);

        PxScene_addActor_mut(scene, (PxActor*)sphere, null);

        // boxes
        for (var i = 0; i < 30; i++)
        {
            var boxVec = new PxVec3
            {
                x = 1f + i * 3.05f,
                y = 1.5f,
                z = 0.0f
            };

            var boxTransform1 = PxTransform_new_1(&boxVec);
            var boxTransform2 = PxTransform_new_2(PxIDENTITY.PxIdentity);
            var boxGeo = PxBoxGeometry_new(0.5f, 1.5f, 1.0f);

            physics->CreateShapeMut(
                (PxGeometry*)&boxGeo,
                material,
                false,
                PxShapeFlags.Visualization | PxShapeFlags.SceneQueryShape | PxShapeFlags.SimulationShape);

            var box = phys_PxCreateDynamic(
                physics,
                &boxTransform1,
                (PxGeometry*)&boxGeo,
                material,
                10.0f,
                &boxTransform2);

            PxScene_addActor_mut(scene, (PxActor*)box, null);
        }

        // simulate
        Console.WriteLine("Start simulate");

        for (var i = 0; i < 1400; i++)
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
