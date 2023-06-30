using MagicPhysX.Toolkit.Colliders;
using MagicPhysX.Toolkit.Internal;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MagicPhysX.Toolkit;

// PxBase <- PxActor <- PxRigidActor <- PxRigidStatic
//                                   <- PxRidigBody <- PxArticulationLink
//                                                  <- PxRigidDynamic

public unsafe abstract class RigidActor
{
    internal void* handler;
    protected Collider collider;

    public ref PxRigidActor RigidActorHandler => ref Unsafe.AsRef<PxRigidActor>(handler);

    protected unsafe RigidActor(void* handler, Collider collider)
    {
        this.handler = handler;
        this.collider = collider;
    }

    public Collider GetComponent() => collider;

    public T GetComponent<T>()
        where T : Collider
    {
        return (T)collider;
    }

    public Transform transform
    {
        get => RigidActorHandler.GetGlobalPose();
    }
}

public unsafe class Rigidstatic : RigidActor
{
    public ref PxRigidStatic RigidStatic => ref Unsafe.AsRef<PxRigidStatic>(handler);

    internal Rigidstatic(PxRigidStatic* handler, Collider collider)
        : base(handler, collider)
    {
    }
}

/// <summary>
///   <para>Control of an object's position through physics simulation.</para>
/// </summary>
public unsafe class Rigidbody : RigidActor
{
    PxScene* scene;

    public ref PxRigidDynamic RigidDynamic => ref Unsafe.AsRef<PxRigidDynamic>(handler);
    public ref PxRigidBody RigidBody => ref Unsafe.AsRef<PxRigidBody>(handler);
    public ref PxRigidActor RigidActor => ref Unsafe.AsRef<PxRigidActor>(handler);
    public ref PxActor Actor => ref Unsafe.AsRef<PxActor>(handler);

    static readonly PxRigidDynamicLockFlags PxRigidDynamicLockFlagsLockAngular =
        PxRigidDynamicLockFlags.LockAngularX |
        PxRigidDynamicLockFlags.LockAngularY |
        PxRigidDynamicLockFlags.LockAngularZ;

    internal Rigidbody(PxRigidDynamic* handler, Collider collider, PxScene* scene)
        : base(handler, collider)
    {
        this.scene = scene;
    }

    /// <summary>
    ///   <para>The velocity vector of the rigidbody. It represents the rate of change of Rigidbody position.</para>
    /// </summary>
    public Vector3 velocity
    {
        get => RigidDynamic.GetLinearVelocity();
        set => RigidDynamic.SetLinearVelocityMut(value.AsPxPointer(), autowake: true);
    }

    /// <summary>
    ///   <para>The angular velocity vector of the rigidbody measured in radians per second.</para>
    /// </summary>
    public Vector3 angularVelocity
    {
        get => RigidDynamic.GetAngularVelocity();
        set => RigidDynamic.SetAngularVelocityMut(value.AsPxPointer(), autowake: true);
    }

    /// <summary>
    ///   <para>The drag of the object.</para>
    /// </summary>
    public float drag
    {
        get => RigidBody.GetLinearDamping();
        set => RigidBody.SetLinearDampingMut(value);
    }

    /// <summary>
    ///   <para>The angular drag of the object.</para>
    /// </summary>
    public float angularDrag
    {
        get => RigidBody.GetAngularDamping();
        set => RigidBody.SetAngularDampingMut(value);
    }

    /// <summary>
    ///   <para>The mass of the rigidbody.</para>
    /// </summary>
    public float mass
    {
        get => RigidBody.GetMass();
        set => RigidBody.SetMassMut(value);
    }

    /// <summary>
    ///   <para>Controls whether gravity affects this rigidbody.</para>
    /// </summary>
    public bool useGravity
    {
        get => (Actor.GetActorFlags() & PxActorFlags.DisableGravity) == 0;
        set => Actor.SetActorFlagMut(PxActorFlag.DisableGravity, !value);
    }

    /// <summary>
    ///   <para>Maximum velocity of a rigidbody when moving out of penetrating state.</para>
    /// </summary>
    public float maxDepenetrationVelocity
    {
        get => RigidBody.GetMaxDepenetrationVelocity();
        set => RigidBody.SetMaxDepenetrationVelocityMut(value);
    }

    /// <summary>
    ///   <para>Controls whether physics affects the rigidbody.</para>
    /// </summary>
    public bool isKinematic
    {
        get => (RigidBody.GetRigidBodyFlags() & PxRigidBodyFlags.Kinematic) == PxRigidBodyFlags.Kinematic;
        set => RigidBody.SetRigidBodyFlagMut(PxRigidBodyFlag.Kinematic, value);
    }

    /// <summary>
    ///   <para>Controls whether physics will change the rotation of the object.</para>
    /// </summary>
    public bool freezeRotation
    {
        get => (RigidDynamic.GetRigidDynamicLockFlags() & PxRigidDynamicLockFlagsLockAngular) ==
               PxRigidDynamicLockFlagsLockAngular;
        set => RigidDynamic.SetRigidDynamicLockFlagsMut(value ?
            RigidDynamic.GetRigidDynamicLockFlags() | PxRigidDynamicLockFlagsLockAngular :
            RigidDynamic.GetRigidDynamicLockFlags() & ~PxRigidDynamicLockFlagsLockAngular);
    }

    /// <summary>
    ///   <para>Controls which degrees of freedom are allowed for the simulation of this Rigidbody.</para>
    /// </summary>
    public RigidbodyConstraints constraints
    {
        get => RigidDynamic.GetRigidDynamicLockFlags().AsRigidbodyConstraints();
        set => RigidDynamic.SetRigidDynamicLockFlagsMut(constraints.AsPxRigidDynamicLockFlags());
    }

    /// <summary>
    ///   <para>The Rigidbody's collision detection mode.</para>
    /// </summary>
    public CollisionDetectionMode collisionDetectionMode
    {
        get
        {
            var flags = RigidBody.GetRigidBodyFlags();
            if ((flags & PxRigidBodyFlags.EnableSpeculativeCcd) == PxRigidBodyFlags.EnableSpeculativeCcd)
            {
                return CollisionDetectionMode.ContinuousSpeculative;
            }

            if ((flags & PxRigidBodyFlags.EnableCcd) == PxRigidBodyFlags.EnableCcd)
            {
                return CollisionDetectionMode.ContinuousDynamic;
            }

            return CollisionDetectionMode.Discrete;
        }
        set
        {
            switch (value)
            {
                case CollisionDetectionMode.ContinuousDynamic:
                    RigidBody.SetRigidBodyFlagMut(PxRigidBodyFlag.EnableCcd, true);
                    RigidBody.SetRigidBodyFlagMut(PxRigidBodyFlag.EnableSpeculativeCcd, false);
                    break;
                case CollisionDetectionMode.ContinuousSpeculative:
                    RigidBody.SetRigidBodyFlagMut(PxRigidBodyFlag.EnableCcd, false);
                    RigidBody.SetRigidBodyFlagMut(PxRigidBodyFlag.EnableSpeculativeCcd, true);
                    break;
                case CollisionDetectionMode.Continuous:
                    // ???
                    break;
                case CollisionDetectionMode.Discrete:
                    RigidBody.SetRigidBodyFlagMut(PxRigidBodyFlag.EnableCcd, false);
                    RigidBody.SetRigidBodyFlagMut(PxRigidBodyFlag.EnableSpeculativeCcd, false);
                    break;
            }
        }
    }

    /// <summary>
    ///   <para>The center of mass relative to the transform's origin.</para>
    /// </summary>
    public Vector3 centerOfMass
    {
        get => RigidBody.GetCMassLocalPose().p;
        set
        {
            var pose = RigidBody.GetCMassLocalPose();
            pose.p = value;
            RigidBody.SetCMassLocalPoseMut(&pose);
        }
    }

    /// <summary>
    ///   <para>The center of mass of the rigidbody in world space (Read Only).</para>
    /// </summary>
    public Vector3 worldCenterOfMass
    {
        get
        {
            var globalPose = RigidActor.GetGlobalPose();
            return globalPose.Transform(centerOfMass.AsPxPointer());
        }
    }

    /// <summary>
    ///   <para>The inertia tensor of this body, defined as a diagonal matrix in a reference frame positioned at this body's center of mass and rotated by Rigidbody.inertiaTensorRotation.</para>
    /// </summary>
    public Vector3 inertiaTensor
    {
        get => RigidBody.GetMassSpaceInertiaTensor();
        set => RigidBody.SetMassSpaceInertiaTensorMut(value.AsPxPointer());
    }

    /// <summary>
    ///   <para>Should collision detection be enabled? (By default always enabled).</para>
    /// </summary>
    public bool detectCollisions
    {
        get
        {
            var num = RigidActor.GetNbShapes();
            if (num > 0)
            {
                PxShape*[] shapes = new PxShape*[num];
                fixed (PxShape** p = shapes)
                {
                    RigidActor.GetShapes(p, num, 0);
                    ref PxShape shape = ref Unsafe.AsRef<PxShape>(shapes[0]);
                    return (shape.GetFlags() & PxShapeFlags.SimulationShape) == PxShapeFlags.SimulationShape;
                }
            }

            return false;
        }

        set
        {
            var num = RigidActor.GetNbShapes();
            if (num > 0)
            {
                PxShape*[] shapes = new PxShape*[num];
                fixed (PxShape** p = shapes)
                {
                    RigidActor.GetShapes(p, num, 0);
                    for (var i = 0; i < num; i++)
                    {
                        ref PxShape shape = ref Unsafe.AsRef<PxShape>(shapes[i]);
                        shape.SetFlagMut(PxShapeFlag.SimulationShape, value);
                    }
                }
            }
        }
    }

    /// <summary>
    ///   <para>The position of the rigidbody.</para>
    /// </summary>
    public Vector3 position
    {
        get => RigidActor.GetGlobalPose().p;
        set
        {
            var pose = RigidActor.GetGlobalPose();
            pose.p = value;
            RigidActor.SetGlobalPoseMut(&pose, autowake: true);
        }
    }

    /// <summary>
    ///   <para>The rotation of the Rigidbody.</para>
    /// </summary>
    public Quaternion rotation
    {
        get => RigidActor.GetGlobalPose().q;
        set
        {
            var pose = RigidActor.GetGlobalPose();
            pose.q = value;
            RigidActor.SetGlobalPoseMut(&pose, autowake: true);
        }
    }

    /// <summary>
    ///   <para>The solverIterations determines how accurately Rigidbody joints and collision contacts are resolved. Overrides Physics.defaultSolverIterations. Must be positive.</para>
    /// </summary>
    public int solverIterations
    {
        get
        {
            uint minPositionIters;
            uint* p = &minPositionIters;

            uint minVelocityIters;
            uint* _ = &minVelocityIters;

            RigidDynamic.GetSolverIterationCounts(p, _);

            return (int)*p;
        }
        set => RigidDynamic.SetSolverIterationCountsMut((uint)value, (uint)solverVelocityIterations);
    }

    /// <summary>
    ///   <para>The mass-normalized energy threshold, below which objects start going to sleep.</para>
    /// </summary>
    public float sleepThreshold
    {
        get => RigidDynamic.GetSleepThreshold();
        set => RigidDynamic.SetSleepThresholdMut(value);
    }

    /// <summary>
    ///   <para>The maximimum angular velocity of the rigidbody measured in radians per second. (Default 7) range { 0, infinity }.</para>
    /// </summary>
    public float maxAngularVelocity
    {
        get => RigidBody.GetMaxAngularVelocity();
        set => RigidBody.SetMaxAngularVelocityMut(value);
    }

    /// <summary>
    ///   <para>The solverVelocityIterations affects how how accurately Rigidbody joints and collision contacts are resolved. Overrides Physics.defaultSolverVelocityIterations. Must be positive.</para>
    /// </summary>
    public int solverVelocityIterations
    {
        get
        {
            uint minPositionIters;
            uint* _ = &minPositionIters;

            uint minVelocityIters;
            uint* v = &minVelocityIters;

            RigidDynamic.GetSolverIterationCounts(_, v);

            return (int)*v;
        }
        set => RigidDynamic.SetSolverIterationCountsMut((uint)solverIterations, (uint)value);
    }

    /// <summary>
    ///   <para>Sets the mass based on the attached colliders assuming a constant density.</para>
    /// </summary>
    /// <param name="density"></param>
    public void SetDensity(float density)
    {
        RigidBody.ExtUpdateMassAndInertia(&density, 1, null, false);
    }

    /// <summary>
    ///   <para>Forces a rigidbody to sleep at least one frame.</para>
    /// </summary>
    public void Sleep()
    {
        RigidDynamic.PutToSleepMut();
    }

    /// <summary>
    ///   <para>Is the rigidbody sleeping?</para>
    /// </summary>
    public bool IsSleeping()
    {
        return RigidDynamic.IsSleeping();
    }

    /// <summary>
    ///   <para>Forces a rigidbody to wake up.</para>
    /// </summary>
    public void WakeUp()
    {
        RigidDynamic.WakeUpMut();
    }

    /// <summary>
    ///   <para>Reset the center of mass of the rigidbody.</para>
    /// </summary>
    public void ResetCenterOfMass()
    {
        centerOfMass = Vector3.Zero;
    }

    /// <summary>
    ///   <para>Reset the inertia tensor value and rotation.</para>
    /// </summary>
    public void ResetInertiaTensor()
    {
        inertiaTensor = Vector3.Zero;
    }

    /// <summary>
    ///   <para>Adds a force to the Rigidbody.</para>
    /// </summary>
    /// <param name="force">Force vector in world coordinates.</param>
    /// <param name="mode">Type of force to apply.</param>
    public void AddForce(Vector3 force, ForceMode mode)
    {
        RigidBody.AddForceMut(force.AsPxPointer(), (PxForceMode)mode, autowake: true);
    }

    /// <summary>
    ///   <para>Adds a force to the Rigidbody.</para>
    /// </summary>
    /// <param name="force">Force vector in world coordinates.</param>
    public void AddForce(Vector3 force)
    {
        RigidBody.AddForceMut(force.AsPxPointer(), PxForceMode.Force, autowake: true);
    }


    public void AddForce(float x, float y, float z, ForceMode mode)
    {
        AddForce(new Vector3(x, y, z), mode);
    }


    public void AddForce(float x, float y, float z)
    {
        AddForce(new Vector3(x, y, z));
    }

    /// <summary>
    ///   <para>Adds a force to the rigidbody relative to its coordinate system.</para>
    /// </summary>
    /// <param name="force">Force vector in local coordinates.</param>
    /// <param name="mode">Type of force to apply.</param>
    public void AddRelativeForce(Vector3 force, ForceMode mode)
    {
        var globalPose = RigidActor.GetGlobalPose();
        var globalForce = globalPose.Transform(force.AsPxPointer());

        RigidBody.AddForceMut(&globalForce, (PxForceMode)mode, autowake: true);
    }

    /// <summary>
    ///   <para>Adds a force to the rigidbody relative to its coordinate system.</para>
    /// </summary>
    /// <param name="force">Force vector in local coordinates.</param>
    public void AddRelativeForce(Vector3 force)
    {
        AddRelativeForce(force, ForceMode.Force);
    }


    public void AddRelativeForce(float x, float y, float z, ForceMode mode)
    {
        AddRelativeForce(new Vector3(x, y, z), mode);
    }


    public void AddRelativeForce(float x, float y, float z)
    {
        AddRelativeForce(new Vector3(x, y, z));
    }

    /// <summary>
    ///   <para>Adds a torque to the rigidbody.</para>
    /// </summary>
    /// <param name="torque">Torque vector in world coordinates.</param>
    /// <param name="mode">The type of torque to apply.</param>
    public void AddTorque(Vector3 torque, ForceMode mode)
    {
        RigidBody.AddTorqueMut(torque.AsPxPointer(), (PxForceMode)mode, autowake: true);
    }

    /// <summary>
    ///   <para>Adds a torque to the rigidbody.</para>
    /// </summary>
    /// <param name="torque">Torque vector in world coordinates.</param>
    public void AddTorque(Vector3 torque)
    {
        RigidBody.AddTorqueMut(torque.AsPxPointer(), PxForceMode.Force, autowake: true);
    }


    public void AddTorque(float x, float y, float z, ForceMode mode)
    {
        AddTorque(new Vector3(x, y, z), mode);
    }


    public void AddTorque(float x, float y, float z)
    {
        AddTorque(new Vector3(x, y, z));
    }

    /// <summary>
    ///   <para>Adds a torque to the rigidbody relative to its coordinate system.</para>
    /// </summary>
    /// <param name="torque">Torque vector in local coordinates.</param>
    /// <param name="mode">Type of force to apply.</param>
    public void AddRelativeTorque(Vector3 torque, ForceMode mode)
    {
        var globalPose = RigidActor.GetGlobalPose();
        var globalToque = globalPose.Transform(torque.AsPxPointer());

        RigidBody.AddTorqueMut(&globalToque, (PxForceMode)mode, autowake: true);
    }

    /// <summary>
    ///   <para>Adds a torque to the rigidbody relative to its coordinate system.</para>
    /// </summary>
    /// <param name="torque">Torque vector in local coordinates.</param>
    public void AddRelativeTorque(Vector3 torque)
    {
        AddRelativeTorque(torque, ForceMode.Force);
    }


    public void AddRelativeTorque(float x, float y, float z, ForceMode mode)
    {
        AddRelativeTorque(new Vector3(x, y, z), mode);
    }


    public void AddRelativeTorque(float x, float y, float z)
    {
        AddRelativeTorque(new Vector3(x, y, z));
    }

    /// <summary>
    ///   <para>Applies force at position. As a result this will apply a torque and force on the object.</para>
    /// </summary>
    /// <param name="force">Force vector in world coordinates.</param>
    /// <param name="position">Position in world coordinates.</param>
    /// <param name="mode">Type of force to apply.</param>
    public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode)
    {
        RigidBody.ExtAddForceAtPos(force.AsPxPointer(), position.AsPxPointer(), (PxForceMode)mode, wakeup: true);
    }

    /// <summary>
    ///   <para>Applies force at position. As a result this will apply a torque and force on the object.</para>
    /// </summary>
    /// <param name="force">Force vector in world coordinates.</param>
    /// <param name="position">Position in world coordinates.</param>
    public void AddForceAtPosition(Vector3 force, Vector3 position)
    {
        RigidBody.ExtAddForceAtPos(force.AsPxPointer(), position.AsPxPointer(), PxForceMode.Force, wakeup: true);
    }
}
