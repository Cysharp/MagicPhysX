using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicPhysX.Toolkit;

// https://docs.unity3d.com/Manual/class-PhysicsManager.html

public enum ContactsGeneration
{
    LegacyContactsGeneration,
    PersistentContactManifold, // default
}

public enum ContactPairsMode
{
    /// <summary>
    /// Receive collision and trigger events from all contact pairs except kinematic-kinematic and kinematic-static pairs.
    /// </summary>
    DefaultContactPairs,
    /// <summary>
    /// Receive collision and trigger events from kinematic-kinematic contact pairs.
    /// </summary>
    EnableKinematicKinematicPairs,
    /// <summary>
    /// Receive collision and trigger events from kinematic-static contact pairs.
    /// </summary>
    EnableKinematicStaticPairs,
    /// <summary>
    /// Receive collision and trigger events from all contact pairs.
    /// </summary>
    EnableAllContactPairs,
}

// map for PxBroadPhaseType::Enum https://docs.nvidia.com/gameworks/content/gameworkslibrary/physx/apireference/files/structPxBroadPhaseType.html


public enum BroadphaseType
{
    SweepAndPruneBroadphase, // eSAP
    MultiboxPruningBroadphase, // MBP
    AutomaticBoxPruning, // ???
}


// map for PxFrictionType
public enum FrictionType : int
{
    /// <summary>
    /// 	A basic strong friction algorithm which typically leads to the most stable results at low solver iteration counts. This method uses only up to four scalar solver constraints per pair of touching objects.
    /// </summary>
    PatchFrictionType = 0,

    /// <summary>
    /// A simplification of the Coulomb friction model, in which the friction for a given point of contact is applied in the alternating tangent directions of the contact’s normal. This requires more solver iterations than patch friction but is not as accurate as the two-directional model. For Articulation bodies to work with this friction type, set the Solver Type to Temporal Gauss Seidel.
    /// </summary>
    OneDirectionalFrictionType = 1,

    /// <summary>
    /// Like the one-directional model, but applies friction in both tangent directions simultaneously. This requires more solver iterations but is more accurate. More expensive than patch friction for scenarios with many contact points because it is applied at every contact point. For Articulation bodies to work with this friction type, set the Solver Type to Temporal Gauss Seidel.
    /// </summary>
    TwoDirectionalFrictionType = 2,
}

public enum SolverType
{
    /// <summary>
    /// The default PhysX solver.
    /// </summary>
    ProjectedGaussSeidel,

    /// <summary>
    /// This solver offers a better convergence and a better handling of high-mass ratios, minimizes energy introduced when correcting penetrations and improves the resistance of joints
    /// to overstretch. It usually helps when you experience some erratic behavior during simulation with the default solver.
    /// </summary>
    TemporalGaussSeidel
}
