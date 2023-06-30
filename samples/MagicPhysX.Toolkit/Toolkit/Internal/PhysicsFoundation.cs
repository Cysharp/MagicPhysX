using static MagicPhysX.NativeMethods;

namespace MagicPhysX.Toolkit.Internal;

public static unsafe class PhysicsFoundation
{
    static readonly object gate = new object();
    static PxFoundation* staticFoundation;

    public static PxFoundation* GetFoundation()
    {
        lock (gate)
        {
            if (staticFoundation == null)
            {
                staticFoundation = physx_create_foundation();
            }
            return staticFoundation;
        }
    }

    // foundation always be single instance per application
    // sometimes doesn't match app-lifetime and native-lifetime(for example, Unity Editor)
    // you can release foundation manually but be careful to use
    public static void ReleaseFoundtaion()
    {
        lock (gate)
        {
            if (staticFoundation != null)
            {
                PxFoundation_release_mut(staticFoundation);
            }
        }
    }
}
