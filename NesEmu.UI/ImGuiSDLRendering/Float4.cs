using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace NesEmu.UI.ImGuiSDLRendering;

readonly internal struct Float4
{
    public Float4(float x, float y, float z, float w) : this(Vector128.Create(x, y, z, w))
    {
    }

    public Float4(in Vector128<float> v) => this.v = v;
    public readonly Vector128<float> v;


    public float X
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => v.GetElement(0);
    }

    public float Y
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => v.GetElement(1);
    }

    public float Z
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => v.GetElement(2);
    }

    public float W
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => v.GetElement(3);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Float4 Min(Float4 other) => new(Sse.Min(v, other.v));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Float4 Max(Float4 other) => new(Sse.Max(v, other.v));

    public static Float4 operator -(Float4 first, Float4 second) => new(Sse.Subtract(first.v, second.v));

    public Float4 XYXY => new(X, Y, X, Y);
    public Float4 __ZW => new(0, 0, Z, W);
    public static Float4 Zero => new(0, 0, 0, 0);

    public Float4 Absoluted
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            Vector128<uint> mask = Vector128.Create(~0u >> 1);
            return new Float4(Sse.And(v, mask.AsSingle()));
        }
    }
}
