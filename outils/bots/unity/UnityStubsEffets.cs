// Stubs du système de particules et des courbes (vérification de compilation hors Unity)
namespace UnityEngine
{
    public enum ParticleSystemStopBehavior { StopEmittingAndClear, StopEmitting }
    public enum ParticleSystemSimulationSpace { Local, World, Custom }
    public enum ParticleSystemShapeType { Sphere, Hemisphere, Cone, Box, Mesh, Circle }
    public enum ParticleSystemCollisionType { Planes, World }
    public enum ParticleSystemCollisionMode { Collision3D, Collision2D }
    public enum ParticleSystemCollisionQuality { High, Medium, Low }
    public enum ParticleSystemRenderMode { Billboard, Stretch, HorizontalBillboard, VerticalBillboard, Mesh, None }
    public enum GradientMode { Blend, Fixed }
    public enum ParticleSystemAnimationType { WholeSheet, SingleRow }
    public enum ParticleSystemAnimationTimeMode { Lifetime, Speed, FPS }
    public struct Keyframe { public Keyframe(float t, float v) { } }
    public class AnimationCurve
    {
        public AnimationCurve(params Keyframe[] k) { }
        public static AnimationCurve Linear(float t0, float v0, float t1, float v1) => null;
        public static AnimationCurve Constant(float t0, float t1, float v) => null;
        public static AnimationCurve EaseInOut(float t0, float v0, float t1, float v1) => null;
        public float Evaluate(float t) => 0;
    }
    public struct GradientColorKey { public GradientColorKey(Color c, float t) { } }
    public struct GradientAlphaKey { public GradientAlphaKey(float a, float t) { } }
    public class Gradient { public void SetKeys(GradientColorKey[] c, GradientAlphaKey[] a) { } public Color Evaluate(float t) => default; }
    public class ParticleSystem : Component
    {
        public struct MinMaxCurve
        {
            public MinMaxCurve(float c) { } public MinMaxCurve(float min, float max) { } public MinMaxCurve(float m, AnimationCurve c) { }
            public static implicit operator MinMaxCurve(float c) => default;
        }
        public struct MinMaxGradient
        {
            public MinMaxGradient(Color c) { } public MinMaxGradient(Gradient g) { } public MinMaxGradient(Color a, Color b) { }
            public static implicit operator MinMaxGradient(Color c) => default;
            public static implicit operator MinMaxGradient(Gradient g) => default;
        }
        public struct MainModule
        {
            public bool loop { get; set; } public bool playOnAwake { get; set; } public float duration { get; set; }
            public ParticleSystemSimulationSpace simulationSpace { get; set; } public int maxParticles { get; set; }
            public MinMaxCurve gravityModifier { get; set; } public MinMaxCurve startLifetime { get; set; } public MinMaxCurve startSpeed { get; set; }
            public MinMaxCurve startSize { get; set; } public MinMaxCurve startRotation { get; set; } public MinMaxGradient startColor { get; set; }
        }
        public struct EmissionModule { public bool enabled { get; set; } public MinMaxCurve rateOverTime { get; set; } }
        public struct ShapeModule { public bool enabled { get; set; } public ParticleSystemShapeType shapeType { get; set; } public float angle { get; set; } public float radius { get; set; } }
        public struct ColorOverLifetimeModule { public bool enabled { get; set; } public MinMaxGradient color { get; set; } }
        public struct SizeOverLifetimeModule { public bool enabled { get; set; } public MinMaxCurve size { get; set; } }
        public struct RotationOverLifetimeModule { public bool enabled { get; set; } public MinMaxCurve z { get; set; } }
        public struct LimitVelocityOverLifetimeModule { public bool enabled { get; set; } public MinMaxCurve drag { get; set; } }
        public struct CollisionModule
        {
            public bool enabled { get; set; } public ParticleSystemCollisionType type { get; set; } public ParticleSystemCollisionMode mode { get; set; }
            public MinMaxCurve bounce { get; set; } public MinMaxCurve dampen { get; set; } public ParticleSystemCollisionQuality quality { get; set; }
        }
        public struct EmitParams
        {
            public Vector3 position { get; set; } public Vector3 velocity { get; set; } public float startLifetime { get; set; } public float startSize { get; set; }
            public Color32 startColor { get; set; } public float rotation { get; set; } public bool applyShapeToPosition { get; set; }
        }
        public MainModule main => default; public EmissionModule emission => default; public ShapeModule shape => default;
        public ColorOverLifetimeModule colorOverLifetime => default; public SizeOverLifetimeModule sizeOverLifetime => default;
        public RotationOverLifetimeModule rotationOverLifetime => default; public LimitVelocityOverLifetimeModule limitVelocityOverLifetime => default;
        public CollisionModule collision => default;
        public struct TextureSheetAnimationModule
        {
            public bool enabled { get; set; } public int numTilesX { get; set; } public int numTilesY { get; set; }
            public ParticleSystemAnimationType animation { get; set; } public ParticleSystemAnimationTimeMode timeMode { get; set; }
            public float fps { get; set; } public MinMaxCurve frameOverTime { get; set; } public int cycleCount { get; set; }
        }
        public TextureSheetAnimationModule textureSheetAnimation => default;
        public void Play() { } public void Stop(bool withChildren, ParticleSystemStopBehavior b) { } public void Emit(EmitParams p, int n) { } public void Emit(int n) { }
    }
}
namespace UnityEngine
{
    public struct LayerMask
    {
        public int value { get; set; }
        public static implicit operator int(LayerMask m) => 0;
        public static implicit operator LayerMask(int i) => default;
    }
    [System.AttributeUsage(System.AttributeTargets.Method)] public class ContextMenu : System.Attribute { public ContextMenu(string n) { } }
    public class CharacterController : Collider
    {
        public float height { get; set; } public Vector3 center { get; set; } public float radius { get; set; } public bool isGrounded => false;
        public CollisionFlags Move(Vector3 m) => default;
    }
    [System.Flags] public enum CollisionFlags { None = 0, Sides = 1, Above = 2, Below = 4 }
    public class AnimationClip : Object { public float length => 0; public void SampleAnimation(GameObject g, float t) { } }
}
namespace UnityEngine.Playables
{
    public enum DirectorUpdateMode { DSPClock, GameTime, UnscaledGameTime, Manual }
    public interface IPlayable { }
    public interface IPlayableOutput { }
    public struct Playable : IPlayable { }
    public struct PlayableGraph
    {
        public static PlayableGraph Create(string n) => default;
        public void SetTimeUpdateMode(DirectorUpdateMode m) { }
        public bool Connect<U, V>(U source, int sourceOutputPort, V destination, int destinationInputPort) where U : struct, IPlayable where V : struct, IPlayable => true;
        public bool IsValid() => true; public void Destroy() { } public void Play() { } public void Evaluate(float dt) { }
    }
    public static class PlayableExtensions
    {
        public static void SetInputWeight<U>(this U p, int i, float w) where U : struct, IPlayable { }
        public static float GetInputWeight<U>(this U p, int i) where U : struct, IPlayable => 0;
        public static Playable GetInput<U>(this U p, int i) where U : struct, IPlayable => default;
        public static void SetSpeed<U>(this U p, double v) where U : struct, IPlayable { }
        public static void SetTime<U>(this U p, double v) where U : struct, IPlayable { }
    }
    public static class PlayableOutputExtensions
    {
        public static void SetSourcePlayable<U, V>(this U o, V p) where U : struct, IPlayableOutput where V : struct, IPlayable { }
    }
}
namespace UnityEngine.Animations
{
    using UnityEngine.Playables;
    public struct AnimationMixerPlayable : IPlayable { public static AnimationMixerPlayable Create(PlayableGraph g, int n = 0) => default; }
    public struct AnimationClipPlayable : IPlayable { public static AnimationClipPlayable Create(PlayableGraph g, AnimationClip c) => default; public void SetApplyFootIK(bool v) { } }
    public struct AnimationPlayableOutput : IPlayableOutput { public static AnimationPlayableOutput Create(PlayableGraph g, string n, Animator a) => default; }
}
