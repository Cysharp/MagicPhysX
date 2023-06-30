using System;

namespace MagicPhysX.Toolkit
{
    /// <summary>
    ///   <para>The collision detection mode constants used for Rigidbody.collisionDetectionMode.</para>
    /// </summary>
    public enum CollisionDetectionMode : int
    {
        /// <summary>
        ///   <para>Continuous collision detection is off for this Rigidbody.</para>
        /// </summary>
        Discrete = 0,
        /// <summary>
        ///   <para>Continuous collision detection is on for colliding with static mesh geometry.</para>
        /// </summary>
        Continuous = 1,
        /// <summary>
        ///   <para>Continuous collision detection is on for colliding with static and dynamic geometry.</para>
        /// </summary>
        ContinuousDynamic = 2,
        /// <summary>
        ///   <para>Speculative continuous collision detection is on for static and dynamic geometries</para>
        /// </summary>
        ContinuousSpeculative = 3,
    }
}
