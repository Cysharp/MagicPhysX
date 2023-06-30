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
