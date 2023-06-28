using MagicPhysX; // for enable Extension Methods.
using static MagicPhysX.NativeMethods; // recommend to use C API.

public static class ReadMeSample
{
    public static unsafe void Run()
    {
        // create foundation(allocator, logging, etc...)
        var foundation = physx_create_foundation();

        // create physics system
        var physics = physx_create_physics(foundation);

        // create physics scene settings
        var sceneDesc = PxSceneDesc_new(PxPhysics_getTolerancesScale(physics));

        // you can create PhysX primitive(PxVec3, etc...) by C# struct
        sceneDesc.gravity = new PxVec3 { x = 0.0f, y = -9.81f, z = 0.0f };

        var dispatcher = phys_PxDefaultCpuDispatcherCreate(1, null, PxDefaultCpuDispatcherWaitForWorkMode.WaitForWork, 0);
        sceneDesc.cpuDispatcher = (PxCpuDispatcher*)dispatcher;
        sceneDesc.filterShader = get_default_simulation_filter_shader();

        // create physics scene
        var scene = physics->CreateSceneMut(&sceneDesc);

        var material = physics->CreateMaterialMut(0.5f, 0.5f, 0.6f);

        // create plane and add to scene
        var plane = PxPlane_new_1(0.0f, 1.0f, 0.0f, 0.0f);
        var groundPlane = physics->PhysPxCreatePlane(&plane, material);
        scene->AddActorMut((PxActor*)groundPlane, null);

        // create sphere and add to scene
        var sphereGeo = PxSphereGeometry_new(10.0f);
        var vec3 = new PxVec3 { x = 0.0f, y = 40.0f, z = 100.0f };
        var transform = PxTransform_new_1(&vec3);
        var identity = PxTransform_new_2(PxIDENTITY.PxIdentity);
        var sphere = physics->PhysPxCreateDynamic(&transform, (PxGeometry*)&sphereGeo, material, 10.0f, &identity);
        PxRigidBody_setAngularDamping_mut((PxRigidBody*)sphere, 0.5f);
        scene->AddActorMut((PxActor*)sphere, null);

        // simulate scene
        for (int i = 0; i < 300; i++)
        {
            // 30fps update
            scene->SimulateMut(1.0f / 30.0f, null, null, 0, true);
            uint error = 0;
            scene->FetchResultsMut(true, &error);

            // output to console(frame-count: position-y)
            var pose = PxRigidActor_getGlobalPose((PxRigidActor*)sphere);
            Console.WriteLine($"{i:000}: {pose.p.y}");
        }

        // release resources
        PxScene_release_mut(scene);
        PxDefaultCpuDispatcher_release_mut(dispatcher);
        PxPhysics_release_mut(physics);
    }
}
