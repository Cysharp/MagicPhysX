using MagicPhysX;
using System.Runtime.CompilerServices;
using System.Text;
using static MagicPhysX.NativeMethods;

namespace ConsoleSandbox;

public static class FixedJoint
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

        var height = 10.0f;

        // box1
        var box1Position = new PxVec3
        {
            x = 0.0f,
            y = 1.1f + height,
            z = 0.0f
        };

        var box1Transform = PxTransform_new_1(&box1Position);
        var box1ShapeOffset = PxTransform_new_2(PxIDENTITY.PxIdentity);
        var box1Geometry = PxBoxGeometry_new(1.0f, 0.1f, 0.2f);

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

        // box2
        var box2Position = new PxVec3
        {
            x = 0.9f,
            y = 0.6f + height,
            z = 0.0f
        };

        var box2Transform = PxTransform_new_1(&box2Position);
        var box2ShapeOffset = PxTransform_new_2(PxIDENTITY.PxIdentity);
        var box2Geometry = PxBoxGeometry_new(0.1f, 0.4f, 0.2f);

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
            x = 1.05f,
            y = 0.1f,
            z = 0.0f
        };

        var pos1 = new PxVec3
        {
            x = anchor1.x - box1Position.x,
            y = anchor1.y - box1Position.y,
            z = anchor1.z - box1Position.z
        };

        var transform1 = PxTransform_new_1((PxVec3*)Unsafe.AsPointer(ref pos1));

        var pos2 = new PxVec3
        {
            x = anchor1.x - box2Position.x,
            y = anchor1.y - box2Position.y,
            z = anchor1.z - box2Position.z
        };

        var transform2 = PxTransform_new_1((PxVec3*)Unsafe.AsPointer(ref pos2));

        var joint1 = phys_PxFixedJointCreate(
            physics,
            (PxRigidActor*)box1,
            &transform1,
            (PxRigidActor*)box2,
            &transform2);

        PxJoint_setBreakForce_mut((PxJoint*)joint1, 0.1f, 0.1f);

        // box3
        var box3Position = new PxVec3
        {
            x = 0.0f + 5.0f,
            y = 1.1f + height,
            z = 0.0f
        };

        var box3Transform = PxTransform_new_1(&box3Position);
        var box3ShapeOffset = PxTransform_new_2(PxIDENTITY.PxIdentity);
        var box3Geometry = PxBoxGeometry_new(1.0f, 0.1f, 0.2f);

        physics->CreateShapeMut(
            (PxGeometry*)&box3Geometry,
            material,
            false,
            PxShapeFlags.Visualization | PxShapeFlags.SceneQueryShape | PxShapeFlags.SimulationShape);

        var box3 = phys_PxCreateDynamic(
            physics,
            &box3Transform,
            (PxGeometry*)&box3Geometry,
            material,
            1.0f,
            &box3ShapeOffset);

        PxScene_addActor_mut(scene, (PxActor*)box3, null);

        // box4
        var box4Position = new PxVec3
        {
            x = 0.9f + 5.0f,
            y = 0.6f + height,
            z = 0.0f
        };

        var box4Transform = PxTransform_new_1(&box4Position);
        var box4ShapeOffset = PxTransform_new_2(PxIDENTITY.PxIdentity);
        var box4Geometry = PxBoxGeometry_new(0.1f, 0.4f, 0.2f);

        physics->CreateShapeMut(
            (PxGeometry*)&box4Geometry,
            material,
            false,
            PxShapeFlags.Visualization | PxShapeFlags.SceneQueryShape | PxShapeFlags.SimulationShape);

        var box4 = phys_PxCreateDynamic(
            physics,
            &box4Transform,
            (PxGeometry*)&box4Geometry,
            material,
            1.0f,
            &box4ShapeOffset);

        PxScene_addActor_mut(scene, (PxActor*)box4, null);

        // joint2
        var anchor2 = new PxVec3
        {
            x = 1.05f,
            y = 0.1f,
            z = 0.0f
        };

        var pos3 = new PxVec3
        {
            x = anchor2.x - box3Position.x,
            y = anchor2.y - box3Position.y,
            z = anchor2.z - box3Position.z
        };

        var transform3 = PxTransform_new_1((PxVec3*)Unsafe.AsPointer(ref pos3));

        var pos4 = new PxVec3
        {
            x = anchor2.x - box4Position.x,
            y = anchor2.y - box4Position.y,
            z = anchor2.z - box4Position.z
        };

        var transform4 = PxTransform_new_1((PxVec3*)Unsafe.AsPointer(ref pos4));

        var joint2 = phys_PxFixedJointCreate(
            physics,
            (PxRigidActor*)box3,
            &transform3,
            (PxRigidActor*)box4,
            &transform4);

        // simulate
        Console.WriteLine("Start simulate");

        for (var i = 0; i < 200; i++)
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
