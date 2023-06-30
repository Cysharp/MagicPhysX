# MagicPhysX

[![NuGet](https://img.shields.io/nuget/v/MagicPhysX.svg)](https://www.nuget.org/packages/MagicPhysX)
[![GitHub Actions](https://github.com/Cysharp/MagicPhysX/workflows/Build-Debug/badge.svg)](https://github.com/Cysharp/MagicPhysX/actions)
[![Releases](https://img.shields.io/github/release/Cysharp/MagicPhysX.svg)](https://github.com/Cysharp/MagicPhysX/releases)

.NET PhysX 5 binding to all platforms(win-x64, osx-x64, osx-arm64, linux-x64, linux-arm64) for 3D engine, deep learning, dedicated server of gaming. This library is built on top of [NVIDIA PhysX 5](https://github.com/NVIDIA-Omniverse/PhysX) and [physx-rs](https://github.com/EmbarkStudios/physx-rs).

Use case:
* 3D View for MAUI, WPF, Avalonia
* Physics for Your Own Game Engine
* Simulate robotics for deep learning
* Server side physics for dedicated server of gaming

Getting Started
---
PhysX Binding provides all of PhysX feature through C API. This library is distributed via NuGet.

> PM> Install-Package [MagicPhysX](https://www.nuget.org/packages/MagicPhysX)

C API is provided in `NativeMethods` in `MagicPhysX` namespace. Methods are almostly prefixed `phys_Px` or `Px`. In addition, extension methods are defined for contexts, just like object-oriented methods. For example, for `PxPhysics*`:

```csharp
PxPhysics* physics = physx_create_physics(foundation);

// C API
PxScene* scene1 = PxPhysics_createScene_mut(physics, &sceneDesc);

// Extension methods
PxScene* scene2 = physics->CreateSceneMut(&sceneDesc);
```

Extension methods API is simpler and similar as original C++ PhysX API.

Here is the simple bound sphere on plane sample.

```csharp
using MagicPhysX; // for enable Extension Methods.
using static MagicPhysX.NativeMethods; // recommend to use C API.

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
for (int i = 0; i < 200; i++)
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
```

Other samples(`FixedJoint`, `DistanceJoint`, `SphericalJoint`, `RevoluteJoint`,`RaycastSingle`, `BricksDoubleDomino`, `Geometries`) are exist in [ConsoleSandbox](https://github.com/Cysharp/MagicPhysX/tree/main/sandbox/ConsoleSandbox).

### Document

MagicPhysX uses [physx-rs](https://github.com/EmbarkStudios/physx-rs) C binding([physx-sys](https://github.com/EmbarkStudios/physx-rs/tree/main/physx-sys)). You can refer these document.

* [physx-sys Changelog](https://github.com/EmbarkStudios/physx-rs/blob/main/physx-sys/CHANGELOG.md)
* [NVIDIA PhysX](https://github.com/NVIDIA-Omniverse/PhysX)
* [PhysX 5 Documantation](https://nvidia-omniverse.github.io/PhysX/physx/5.1.3/)

### PhysX Visual Debugger

MagicPhysX can enable [PhysX Visual Debugger](https://developer.nvidia.com/physx-visual-debugger) to debug physcs scene.

![image](https://github.com/Cysharp/MagicPhysX/assets/46207/83b6b7c7-c8d6-4ab3-bd5e-f85905831d43)

To use pvd, add this instruction on scene init.

```csharp
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
```

Toolkit Sample
---
C API is slightly complex in C# usage. Here is the sample of high level framework, PhysicsSystem.

```csharp
using MagicPhysX.Toolkit;
using System.Numerics;

unsafe
{
    using var physics = new PhysicsSystem(enablePvd: false);
    using var scene = physics.CreateScene();

    var material = physics.CreateMaterial(0.5f, 0.5f, 0.6f);

    var plane = scene.AddStaticPlane(0.0f, 1.0f, 0.0f, 0.0f, new Vector3(0, 0, 0), Quaternion.Identity, material);
    var sphere = scene.AddDynamicSphere(1.0f, new Vector3(0.0f, 10.0f, 0.0f), Quaternion.Identity, 10.0f, material);

    for (var i = 0; i < 200; i++)
    {
        scene.Update(1.0f / 30.0f);

        var position = sphere.transform.position;
        Console.WriteLine($"{i:D2} : x={position.X:F6}, y={position.Y:F6}, z={position.Z:F6}");
    }
}
```

Code sample is available in [MagicPhysX.Toolkit](https://github.com/Cysharp/MagicPhysX/tree/main/samples/MagicPhysX.Toolkit).

Native Build Instruction
---
require [Rust](https://www.rust-lang.org/).

Open directory `src\libmagicphysx`.
Run `cargo build`.

Native binaries in package is built on GitHub Actions [build-physx.yml](https://github.com/Cysharp/MagicPhysX/blob/main/.github/workflows/build-physx.yml). If we need to update physx-sys(PhysX) version, run this GitHub Actions to input physxversion.

License
---
This library is licensed under the MIT License.