// Imitation minimale de l'API Unity 6 pour vérifier que les scripts compilent (signatures reprises de UnityCsReference).
#pragma warning disable CS0067, CS0626, CS0660, CS0661, CS0169, CS0414, CS0649
using System;
using System.Collections.Generic;

namespace UnityEngine
{
    public class Object
    {
        public string name { get; set; }
        public int GetInstanceID() => 0;
        public static void Destroy(Object o) { }
        public static void Destroy(Object o, float t) { }
        public static void DestroyImmediate(Object o) { }
        public static T Instantiate<T>(T o) where T : Object => o;
        public static T Instantiate<T>(T o, Transform parent) where T : Object => o;
        public static T FindFirstObjectByType<T>() where T : Object => null;
        public static T[] FindObjectsByType<T>(FindObjectsSortMode m) where T : Object => null;
        public static T FindFirstObjectByType<T>(FindObjectsInactive i) where T : Object => null;
        public static T[] FindObjectsByType<T>(FindObjectsInactive i, FindObjectsSortMode m) where T : Object => null;
        public static bool operator ==(Object a, Object b) => ReferenceEquals(a, b);
        public static bool operator !=(Object a, Object b) => !ReferenceEquals(a, b);
        public static implicit operator bool(Object o) => o != null;
    }
    public enum FindObjectsSortMode { None, InstanceID }
    public enum FindObjectsInactive { Exclude, Include }
    public class Component : Object
    {
        public bool CompareTag(string t) => false;
        public GameObject gameObject => null;
        public Transform transform => null;
        public T GetComponent<T>() => default;
        public T GetComponentInParent<T>() => default;
        public T GetComponentInChildren<T>() => default;
        public T GetComponentInChildren<T>(bool b) => default;
        public T[] GetComponentsInChildren<T>() => null;
        public T[] GetComponentsInChildren<T>(bool b) => null;
        public string tag { get; set; }
    }
    public class Behaviour : Component { public bool enabled { get; set; } public bool isActiveAndEnabled => true; }
    public class MonoBehaviour : Behaviour
    {
        public void Invoke(string m, float t) { }
        public void CancelInvoke() { }
        public Coroutine StartCoroutine(System.Collections.IEnumerator e) => null;
    }
    public enum WrapMode { Default = 0, Once = 1, Loop = 2, PingPong = 4, ClampForever = 8 }
    public class AnimationState { public string name { get; set; } public float speed { get; set; } public WrapMode wrapMode { get; set; } }
    public class Animation : Behaviour, System.Collections.IEnumerable
    {
        public bool playAutomatically { get; set; } public AnimationClip GetClip(string n) => null;
        public bool Play(string n) => true; public void CrossFade(string n, float t) { } public bool IsPlaying(string n) => false;
        public AnimationState this[string n] => null;
        public System.Collections.IEnumerator GetEnumerator() => null;
    }
    public enum HumanBodyBones { Hips, LeftUpperLeg, RightUpperLeg, LeftLowerLeg, RightLowerLeg, LeftFoot, RightFoot, Spine, Chest, Neck, Head, LeftShoulder, RightShoulder, LeftUpperArm, RightUpperArm, LeftLowerArm, RightLowerArm, LeftHand, RightHand, RightMiddleProximal, RightIndexProximal, RightLittleProximal, LeftMiddleProximal, LeftIndexProximal, LeftLittleProximal }
    public enum AnimatorControllerParameterType { Float = 1, Int = 3, Bool = 4, Trigger = 9 }
    public class AnimatorControllerParameter { public string name => ""; public int nameHash => 0; public AnimatorControllerParameterType type => default; }
    public class RuntimeAnimatorController : Object { }
    public class Avatar : Object { }
    public class Animator : Behaviour
    {
        public RuntimeAnimatorController runtimeAnimatorController { get; set; } public bool applyRootMotion { get; set; } public bool isHuman => false; public Avatar avatar { get; set; }
        public AnimatorControllerParameter[] parameters => null;
        public Transform GetBoneTransform(HumanBodyBones b) => null;
        public void SetFloat(int id, float v) { } public void SetBool(int id, bool v) { }
    }
    public enum LogType { Error, Assert, Warning, Log, Exception }
    public enum RuntimeInitializeLoadType { AfterSceneLoad, BeforeSceneLoad, AfterAssembliesLoaded, BeforeSplashScreen, SubsystemRegistration }
    public class RuntimeInitializeOnLoadMethodAttribute : Attribute { public RuntimeInitializeOnLoadMethodAttribute() { } public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType t) { } }
    public class CustomYieldInstruction : System.Collections.IEnumerator { public object Current => null; public bool MoveNext() => false; public void Reset() { } }
    public class WaitForSecondsRealtime : CustomYieldInstruction { public WaitForSecondsRealtime(float s) { } }
    public class Coroutine { }
    public static class ScreenCapture { public static void CaptureScreenshot(string f) { } }
    public static class GUIUtility { public static string systemCopyBuffer { get; set; } }
    public class ScriptableObject : Object { public static T CreateInstance<T>() where T : ScriptableObject => null; }
    public class GameObject : Object
    {
        public GameObject() { }
        public GameObject(string n) { }
        public GameObject(string n, params Type[] c) { }
        public UnityEngine.SceneManagement.Scene scene => default;
        public Transform transform => null;
        public bool activeInHierarchy => true;
        public bool activeSelf => true;
        public void SetActive(bool v) { }
        public T AddComponent<T>() where T : Component => null;
        public T GetComponent<T>() => default;
        public T GetComponentInChildren<T>() => default;
        public T GetComponentInChildren<T>(bool b) => default;
        public T[] GetComponentsInChildren<T>() => null;
        public T[] GetComponentsInChildren<T>(bool b) => null;
        public static GameObject CreatePrimitive(PrimitiveType t) => null;
        public static GameObject Find(string n) => null;
    }
    public enum PrimitiveType { Sphere, Capsule, Cylinder, Cube, Plane, Quad }
    public class Transform : Component, System.Collections.IEnumerable
    {
        public Vector3 position { get; set; }
        public Vector3 localPosition { get; set; }
        public Vector3 localEulerAngles { get; set; }
        public Quaternion rotation { get; set; }
        public Quaternion localRotation { get; set; }
        public Vector3 localScale { get; set; }
        public Vector3 lossyScale => default;
        public Vector3 forward { get; set; }
        public Vector3 right { get; set; }
        public Vector3 up { get; set; }
        public Transform parent { get; set; }
        public void SetParent(Transform p, bool w) { }
        public void SetParent(Transform p) { }
        public void SetPositionAndRotation(Vector3 p, Quaternion r) { }
        public Transform Find(string n) => null;
        public bool IsChildOf(Transform t) => false;
        public Vector3 InverseTransformPoint(Vector3 p) => p;
        public Vector3 TransformPoint(Vector3 p) => p;
        public Vector3 TransformDirection(Vector3 p) => p;
        public Vector3 InverseTransformDirection(Vector3 p) => p;
        public Vector3 eulerAngles { get; set; }
        public int childCount => 0;
        public void Rotate(float x, float y, float z, Space s) { }
        public void Rotate(Vector3 e) { }
        public System.Collections.IEnumerator GetEnumerator() => null;
    }
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero => default;
        public float magnitude => 0; public float sqrMagnitude => 0;
        public static Vector2 ClampMagnitude(Vector2 v, float m) => v;
        public static Vector2 operator *(Vector2 a, float b) => a;
        public static Vector2 operator +(Vector2 a, Vector2 b) => a;
        public static implicit operator Vector3(Vector2 v) => default;
        public static implicit operator Vector2(Vector3 v) => default;
    }
    public struct Vector2Int { public int x, y; public Vector2Int(int x, int y) { this.x = x; this.y = y; } }
    public struct Vector3
    {
        public static Vector3 Scale(Vector3 a, Vector3 b) => a;
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero => default; public static Vector3 one => default; public static Vector3 up => default; public static Vector3 down => default;
        public static Vector3 forward => default; public static Vector3 back => default; public static Vector3 right => default; public static Vector3 left => default;
        public Vector3 normalized => this; public float magnitude => 0; public float sqrMagnitude => 0;
        public static float Dot(Vector3 a, Vector3 b) => 0;
        public static Vector3 Cross(Vector3 a, Vector3 b) => a;
        public static float Distance(Vector3 a, Vector3 b) => 0;
        public static Vector3 ProjectOnPlane(Vector3 v, Vector3 n) => v;
        public static Vector3 Project(Vector3 v, Vector3 n) => v;
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => a;
        public static Vector3 Slerp(Vector3 a, Vector3 b, float t) => a;
        public static Vector3 operator +(Vector3 a, Vector3 b) => a;
        public static Vector3 operator -(Vector3 a, Vector3 b) => a;
        public static Vector3 operator -(Vector3 a) => a;
        public static Vector3 operator *(Vector3 a, float b) => a;
        public static Vector3 operator *(float b, Vector3 a) => a;
        public static Vector3 operator /(Vector3 a, float b) => a;
        public static bool operator ==(Vector3 a, Vector3 b) => true;
        public static bool operator !=(Vector3 a, Vector3 b) => false;
        public float[] ToArray() => null;
        public void Normalize() { }
        public static float Angle(Vector3 a, Vector3 b) => 0;
        public static float SignedAngle(Vector3 a, Vector3 b, Vector3 axe) => 0;
        public static Vector3 MoveTowards(Vector3 a, Vector3 b, float d) => a;
        public static Vector3 ClampMagnitude(Vector3 v, float m) => v;
        public static Vector3 Reflect(Vector3 v, Vector3 n) => v;
    }
    public struct Ray { public Ray(Vector3 origin, Vector3 direction) { this.origin = origin; this.direction = direction; } public Vector3 origin, direction; }
    public struct Vector4 { public Vector4(float x, float y, float z, float w) { } public static implicit operator Vector4(Vector3 v) => default; }
    public struct Quaternion
    {
        public static Quaternion identity => default;
        public Vector3 eulerAngles { get; set; }
        public static Quaternion AngleAxis(float a, Vector3 axe) => default;
        public static Quaternion Euler(float x, float y, float z) => default;
        public static Quaternion Euler(Vector3 e) => default;
        public static Quaternion LookRotation(Vector3 f) => default;
        public static Quaternion LookRotation(Vector3 f, Vector3 u) => default;
        public static Quaternion FromToRotation(Vector3 a, Vector3 b) => default;
        public static Quaternion Inverse(Quaternion q) => q;
        public static Quaternion Slerp(Quaternion a, Quaternion b, float t) => a;
        public static Quaternion RotateTowards(Quaternion a, Quaternion b, float d) => a;
        public static Quaternion operator *(Quaternion a, Quaternion b) => a;
        public static Vector3 operator *(Quaternion a, Vector3 b) => b;
    }
    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b) { this.r = r; this.g = g; this.b = b; a = 1; }
        public Color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static Color white => default; public static Color black => default; public static Color cyan => default; public static Color clear => default;
        public static Color operator *(Color c, float f) => c;
        public static Color operator *(Color c, Color d) => c;
        public static Color operator +(Color c, Color d) => c;
        public float maxColorComponent => 0;
        public static Color Lerp(Color a, Color b, float t) => a;
    }
    public struct Color32 { public Color32(byte r, byte g, byte b, byte a) { } public static implicit operator Color32(Color c) => default; }
    public static class ColorUtility { public static bool TryParseHtmlString(string s, out Color c) { c = default; return true; } }
    public struct Rect { public Rect(float x, float y, float w, float h) { } public float width => 0; public float height => 0; }
    public struct Bounds
    {
        public Bounds(Vector3 c, Vector3 s) { }
        public Vector3 center => default; public Vector3 size => default; public Vector3 extents => default; public Vector3 min => default; public Vector3 max => default;
        public void Encapsulate(Bounds b) { } public void Encapsulate(Vector3 p) { }
        public bool Contains(Vector3 p) => false;
    }
    public enum Space { World, Self }
    public static class Mathf
    {
        public static float Floor(float a) => a;
        public const float PI = 3.14159f, Rad2Deg = 57.29f, Deg2Rad = 0.0174f;
        public static float Clamp(float v, float a, float b) => v; public static int Clamp(int v, int a, int b) => v;
        public static float Clamp01(float v) => v;
        public static float Lerp(float a, float b, float t) => a;
        public static float InverseLerp(float a, float b, float t) => a;
        public static float SmoothStep(float a, float b, float t) => a;
        public static float Max(float a, float b) => a; public static float Max(params float[] v) => 0; public static int Max(int a, int b) => a;
        public static float Min(float a, float b) => a; public static int Min(int a, int b) => a;
        public static float Abs(float a) => a; public static int Abs(int a) => a;
        public static float Sin(float a) => a; public static float Cos(float a) => a; public static float Asin(float a) => a; public static float Atan2(float y, float x) => y;
        public static float Exp(float a) => a; public static float Sqrt(float a) => a; public static float Pow(float a, float b) => a;
        public static float MoveTowards(float a, float b, float d) => a; public static float MoveTowardsAngle(float a, float b, float d) => a;
        public static float Repeat(float t, float l) => t; public static float Tan(float a) => a; public static float Acos(float a) => a; public static float Sign(float a) => a;
        public static int RoundToInt(float a) => 0; public static int CeilToInt(float a) => 0; public static int FloorToInt(float a) => 0;
    }
    public static class Random
    {
        public static float value => 0;
        public static int Range(int a, int b) => a;
        public static float Range(float a, float b) => a;
        public static Vector3 onUnitSphere => default;
        public static Vector3 insideUnitSphere => default;
        public static Vector2 insideUnitCircle => default;
        public static Quaternion rotation => default;
    }
    public static class Time
    {
        public static float deltaTime => 0; public static float time => 0; public static float unscaledTime => 0; public static float unscaledDeltaTime => 0; public static float fixedDeltaTime => 0;
        public static float timeScale { get; set; }
        public static float realtimeSinceStartup => 0;
        public static int frameCount => 0;
    }
    public static class Debug
    {
        public static void Log(object o) { }
        public static void LogWarning(object o) { }
        public static void LogError(object o) { }
    }
    public static class Application
    {
        public static void Quit() { } public static string unityVersion => ""; public static string dataPath => "";
        public delegate void LogCallback(string condition, string stackTrace, LogType type);
        public static event LogCallback logMessageReceived;
    }
    public enum ForceMode { Force, Acceleration, Impulse, VelocityChange }
    [Flags] public enum RigidbodyConstraints { None = 0, FreezeRotation = 112 }
    public enum RigidbodyInterpolation { None, Interpolate, Extrapolate }
    public enum CollisionDetectionMode { Discrete, Continuous, ContinuousDynamic, ContinuousSpeculative }
    public class Rigidbody : Component
    {
        public Vector3 linearVelocity { get; set; }
        public Vector3 position { get; set; }
        public Quaternion rotation { get; set; }
        public bool useGravity { get; set; }
        public bool isKinematic { get; set; }
        public float mass { get; set; }
        public RigidbodyConstraints constraints { get; set; }
        public RigidbodyInterpolation interpolation { get; set; }
        public CollisionDetectionMode collisionDetectionMode { get; set; }
        public void AddForce(Vector3 f, ForceMode m) { }
        public void MoveRotation(Quaternion q) { }
    }
    public class Collider : Component { public Rigidbody attachedRigidbody => null; public bool enabled { get; set; } public Bounds bounds => default; }
    public class SphereCollider : Collider { public float radius { get; set; } public Vector3 center { get; set; } }
    public class BoxCollider : Collider { public bool isTrigger { get; set; } public Vector3 center { get; set; } public Vector3 size { get; set; } }
    public enum QueryTriggerInteraction { UseGlobal, Ignore, Collide }
    public struct RaycastHit
    {
        public Vector3 point => default; public Vector3 normal => default; public float distance => 0;
        public Collider collider => null; public Transform transform => null; public Rigidbody rigidbody => null;
    }
    public static class Physics
    {
        public static bool Raycast(Vector3 o, Vector3 d, float l, int m, QueryTriggerInteraction q) => false;
        public static bool Raycast(Vector3 o, Vector3 d, out RaycastHit h, float l) { h = default; return false; }
        public static RaycastHit[] RaycastAll(Vector3 o, Vector3 d, float l, int m, QueryTriggerInteraction q) => null;
        public static int RaycastNonAlloc(Vector3 o, Vector3 d, RaycastHit[] r, float l, int m, QueryTriggerInteraction q) => 0;
        public static void SyncTransforms() { }
        public static int SphereCastNonAlloc(Vector3 o, float r, Vector3 d, RaycastHit[] h, float l, int m, QueryTriggerInteraction q) => 0;
    }
    public class Mesh : Object
    {
        public Bounds bounds { get; set; }
        public void SetVertices(List<Vector3> v) { }
        public void GetVertices(List<Vector3> v) { }
        public void SetTriangles(List<int> t, int s) { }
        public void RecalculateNormals() { }
        public string name { get; set; }
        public void SetNormals(List<Vector3> n) { }
        public void SetUVs(int c, List<Vector2> u) { }
        public void RecalculateBounds() { }
    }
    public class Shader : Object
    {
        public static Shader Find(string n) => null;
        public static int PropertyToID(string n) => 0;
        public static void SetGlobalVector(int id, Vector4 v) { }
        public static void SetGlobalColor(int id, Color c) { }
    }
    public class Texture : Object { }
    public enum TextureFormat { RGBA32 }
    public enum TextureWrapMode { Repeat, Clamp }
    public class Texture2D : Texture
    {
        public Texture2D(int w, int h, TextureFormat f, bool mip) { }
        public void SetPixels32(Color32[] p) { }
        public void SetPixels(Color[] p) { }
        public void Apply(bool mip, bool nonReadable) { }
        public TextureWrapMode wrapMode { get; set; }
        public string name { get; set; }
        public void Apply(bool mip) { }
    }
    public class Material : Object
    {
        public Material(Shader s) { }
        public Material(Material m) { }
        public void SetColor(string n, Color c) { } public void SetColor(int n, Color c) { }
        public Color GetColor(int n) => default; public Color GetColor(string n) => default; public Texture GetTexture(string n) => null;
        public float GetFloat(string n) => 0;
        public void SetFloat(int n, float f) { }
        public void SetFloat(string n, float f) { }
        public void SetTexture(string n, Texture t) { }
        public bool HasProperty(string n) => true; public bool HasProperty(int n) => true;
        public void EnableKeyword(string k) { }
        public int renderQueue { get; set; }
        public string name { get; set; }
        public void SetVector(string n, Vector4 v) { }
        public void SetVectorArray(string n, Vector4[] v) { }
    }
    public class Renderer : Component
    {
        public Material material { get; set; }
        public Material sharedMaterial { get; set; }
        public Material[] materials { get; set; }
        public Bounds bounds => default;
        public bool enabled { get; set; }
        public UnityEngine.Rendering.ShadowCastingMode shadowCastingMode { get; set; }
        public bool receiveShadows { get; set; }
        public void SetPropertyBlock(MaterialPropertyBlock b) { }
    }
    public class MaterialPropertyBlock { public void SetColor(int n, Color c) { } public void SetFloat(int n, float f) { } }
    public class SkinnedMeshRenderer : Renderer { public Mesh sharedMesh { get; set; } public Transform[] bones { get; set; } public void BakeMesh(Mesh m, bool useScale) { } }
    public class MeshRenderer : Renderer { }
    public class ParticleSystemRenderer : Renderer { public ParticleSystemRenderMode renderMode { get; set; } public float velocityScale { get; set; } public float lengthScale { get; set; } }
    public class TrailRenderer : Renderer { }
    public class LineRenderer : Renderer
    {
        public bool useWorldSpace { get; set; } public bool loop { get; set; } public float widthMultiplier { get; set; } public int positionCount { get; set; }
        public void SetPosition(int i, Vector3 p) { }
        public float startWidth { get; set; } public float endWidth { get; set; } public int numCapVertices { get; set; } public bool generateLightingData { get; set; }
    }
    public class MeshFilter : Component { public Mesh sharedMesh { get; set; } }
    public enum LightType { Spot, Directional, Point }
    public enum LightShadows { None, Hard, Soft }
    public class Light : Behaviour
    {
        public LightType type { get; set; } public Color color { get; set; } public float range { get; set; } public float intensity { get; set; } public LightShadows shadows { get; set; } public float spotAngle { get; set; }
    }
    public class Camera : Behaviour
    {
        public static Camera main => null;
        public float aspect => 1;
        public float fieldOfView { get; set; }
        public float nearClipPlane { get; set; } public float farClipPlane { get; set; }
        public Vector3 WorldToViewportPoint(Vector3 p) => p;
    }
    public enum CursorLockMode { None, Locked, Confined }
    public static class Cursor { public static CursorLockMode lockState { get; set; } public static bool visible { get; set; } }
    public enum KeyCode { F8 = 289, X,  None, Space, Escape, Return, LeftShift, LeftControl, E, F, C, N, J, M, Q, R, G, V, K, A, Alpha1, Alpha2, Alpha3, Alpha4, Alpha5, Alpha6 }
    public static class Input
    {
        public static bool GetKey(KeyCode k) => false; public static bool GetKeyDown(KeyCode k) => false;
        public static bool GetMouseButtonDown(int b) => false;
        public static bool GetMouseButton(int b) => false;
        public static Vector2 mouseScrollDelta => default;
        public static float GetAxis(string a) => 0; public static float GetAxisRaw(string a) => 0;
    }
    public static class PlayerPrefs
    {
        public static string GetString(string k, string d) => d; public static void SetString(string k, string v) { }
        public static float GetFloat(string k, float d) => d; public static void SetFloat(string k, float v) { }
        public static int GetInt(string k, int d) => d; public static void SetInt(string k, int v) { }
        public static void DeleteKey(string k) { } public static void Save() { }
    }
    public static class JsonUtility { public static string ToJson(object o) => ""; public static T FromJson<T>(string s) => default; }
    public static class QualitySettings
    {
        public static string[] names => null;
        public static int GetQualityLevel() => 0;
        public static void SetQualityLevel(int i, bool b) { }
    }
    public class AudioClip : Object
    {
        public delegate void PCMReaderCallback(float[] data);
        public static AudioClip Create(string name, int lengthSamples, int channels, int frequency, bool stream) => null;
        public static AudioClip Create(string name, int lengthSamples, int channels, int frequency, bool stream, PCMReaderCallback pcmreadercallback) => null;
    }
    public sealed class AudioSettings { public static int outputSampleRate => 48000; }
    public class AudioListener : Behaviour { public static float volume { get; set; } }
    public class AudioSource : Behaviour
    {
        public bool playOnAwake { get; set; } public bool loop { get; set; } public float spatialBlend { get; set; } public AudioClip clip { get; set; }
        public float volume { get; set; } public float pitch { get; set; } public bool isPlaying => false;
        public void PlayOneShot(AudioClip c, float v) { } public void Play() { } public void Stop() { }
        public static void PlayClipAtPoint(AudioClip c, Vector3 p, float v) { }
    }
    public class GUIStyleState { public Color textColor { get; set; } }
    public class GUIStyle { public GUIStyle(GUIStyle o) { } public int fontSize { get; set; } public GUIStyleState normal { get; } = new GUIStyleState(); }
    public class GUISkin { public GUIStyle label => null; }
    public static class GUI { public static GUISkin skin => null; public static void Label(Rect r, string t, GUIStyle s) { } }
    public static class Resources { public static T Load<T>(string p) where T : Object => null; }
    public static class Gizmos
    {
        public static Color color { get; set; }
        public static void DrawWireSphere(Vector3 c, float r) { } public static void DrawSphere(Vector3 c, float r) { }
    }

    public class TooltipAttribute : Attribute { public TooltipAttribute(string t) { } }
    public class HeaderAttribute : Attribute { public HeaderAttribute(string t) { } }
    public class RangeAttribute : Attribute { public RangeAttribute(float a, float b) { } }
    public class TextAreaAttribute : Attribute { public TextAreaAttribute() { } public TextAreaAttribute(int a, int b) { } }
    public class ColorUsageAttribute : Attribute { public ColorUsageAttribute(bool a, bool b) { } }
    public class RequireComponent : Attribute { public RequireComponent(Type t) { } }
    public class DefaultExecutionOrder : Attribute { public DefaultExecutionOrder(int o) { } }
    public class ExecuteAlways : Attribute { }
}

namespace UnityEngine.Rendering
{
    public enum ShadowCastingMode { Off, On, TwoSided, ShadowsOnly }
    public enum BlendMode { Zero = 0, One = 1, DstColor = 2, SrcColor = 3, OneMinusDstColor = 4, SrcAlpha = 5, OneMinusSrcColor = 6, DstAlpha = 7, OneMinusDstAlpha = 8, SrcAlphaSaturate = 9, OneMinusSrcAlpha = 10 }
    public abstract class RenderPipelineAsset : UnityEngine.ScriptableObject { }
    public static class GraphicsSettings { public static RenderPipelineAsset currentRenderPipeline => null; }
}
namespace UnityEngine.SceneManagement
{
    public struct Scene { public int buildIndex => 0; public string path => ""; public string name => ""; }
    public enum LoadSceneMode { Single, Additive }
    public struct LoadSceneParameters { public LoadSceneParameters(LoadSceneMode m) { } }
    public static class SceneManager
    {
        public static Scene GetActiveScene() => default;
        public static void LoadScene(int i) { }
        public static void LoadScene(string n) { }
    }
}

namespace UnityEngine.Rendering.HighDefinition
{
    public class HDAdditionalLightData : MonoBehaviour { }
    public enum CustomPassInjectionPoint { BeforePostProcess }
    public class CustomPass { public enum TargetBuffer { Camera, Custom, None } public string name { get; set; } public TargetBuffer targetColorBuffer { get; set; } }
    public class FullScreenCustomPass : CustomPass { public Material fullscreenPassMaterial; public string materialPassName; public bool fetchColorBuffer; }
    public class CustomPassVolume : MonoBehaviour { public CustomPassInjectionPoint injectionPoint { get; set; } public List<CustomPass> customPasses { get; } = new List<CustomPass>(); }
}

namespace UnityEngine.InputSystem.Controls
{
    public class ButtonControl { public bool isPressed => false; public bool wasPressedThisFrame => false; }
    public class KeyControl : ButtonControl { public string displayName => ""; }
    public class Vector2Control { public Vector2 ReadValue() => default; }
    public class StickControl : Vector2Control { }
    public class DpadControl : Vector2Control { public ButtonControl up => null; public ButtonControl down => null; public ButtonControl left => null; public ButtonControl right => null; }
}

namespace UnityEngine.InputSystem
{
    using UnityEngine.InputSystem.Controls;
    public class Keyboard
    {
        public static Keyboard current => null;
        public KeyControl f8Key, qKey, rKey, gKey, vKey, kKey, digit1Key, digit2Key, digit3Key, digit4Key, digit5Key, digit6Key;
        public KeyControl xKey, tKey, equalsKey, minusKey, numpadPlusKey, numpadMinusKey;
        public KeyControl aKey, cKey, dKey, eKey, fKey, jKey, nKey, sKey, wKey, spaceKey, escapeKey, enterKey, leftShiftKey, leftCtrlKey, upArrowKey, downArrowKey, leftArrowKey, rightArrowKey;
        public KeyControl FindKeyOnCurrentKeyboardLayout(string n) => null;
    }
    public class Gamepad
    {
        public static Gamepad current => null;
        public StickControl leftStick => null; public StickControl rightStick => null; public DpadControl dpad => null;
        public ButtonControl buttonSouth => null; public ButtonControl buttonEast => null; public ButtonControl buttonWest => null; public ButtonControl buttonNorth => null;
        public ButtonControl startButton => null; public ButtonControl selectButton => null; public ButtonControl leftShoulder => null; public ButtonControl rightShoulder => null;
        public ButtonControl leftTrigger => null; public ButtonControl rightTrigger => null; public ButtonControl leftStickButton => null; public ButtonControl rightStickButton => null;
    }
    public class Mouse { public static Mouse current => null; public Vector2Control delta => null; public Vector2Control scroll => null; public ButtonControl leftButton => null; public ButtonControl rightButton => null; }
}

namespace UnityEngine.UIElements
{
    public struct TimerState { }
    public interface IVisualElementScheduledItem { IVisualElementScheduledItem StartingIn(long delayMs); IVisualElementScheduledItem Every(long intervalMs); void Pause(); }
    public interface IVisualElementScheduler { IVisualElementScheduledItem Execute(Action<TimerState> e); IVisualElementScheduledItem Execute(Action e); }
    public enum PickingMode { Position, Ignore }
    public enum DisplayStyle { Flex, None }
    public enum Justify { FlexStart, Center, FlexEnd, SpaceBetween }
    public enum ScrollViewMode { Vertical, Horizontal }
    public enum PanelScaleMode { ConstantPixelSize, ConstantPhysicalSize, ScaleWithScreenSize }
    public enum PanelScreenMatchMode { MatchWidthOrHeight, Shrink, Expand }
    public struct Length { public static Length Percent(float v) => default; public static implicit operator Length(float v) => default; }
    public struct StyleLength { public static implicit operator StyleLength(float v) => default; public static implicit operator StyleLength(Length v) => default; }
    public enum AngleUnit { Degree, Gradian, Radian, Turn }
    public struct Angle { public Angle(float v, AngleUnit u) { } }
    public struct Rotate { public Rotate(Angle a) { } }
    public struct StyleRotate { public static implicit operator StyleRotate(Rotate r) => default; }
    public struct StyleFloat { public static implicit operator StyleFloat(float v) => default; }
    public struct StyleColor { public static implicit operator StyleColor(Color v) => default; }
    public struct StyleEnum<T> where T : struct { public T value => default; public static implicit operator StyleEnum<T>(T v) => default; }
    public interface IStyle
    {
        StyleEnum<DisplayStyle> display { get; set; }
        StyleLength width { get; set; } StyleLength left { get; set; } StyleLength top { get; set; }
        StyleFloat opacity { get; set; } StyleFloat flexGrow { get; set; } StyleRotate rotate { get; set; }
        StyleColor backgroundColor { get; set; } StyleColor color { get; set; }
        StyleColor borderTopColor { get; set; } StyleColor borderBottomColor { get; set; } StyleColor borderLeftColor { get; set; } StyleColor borderRightColor { get; set; }
        StyleEnum<Justify> justifyContent { get; set; }
    }
    public interface IResolvedStyle { DisplayStyle display { get; } }
    public interface IPanel { }
    public interface IEventHandler { }
    public delegate void EventCallback<in T>(T evt);
    public class EventBase { public IEventHandler target => null; }
    public class PointerEventBase<T> : EventBase { public int pointerId => 0; public Vector3 deltaPosition => default; }
    public class PointerDownEvent : PointerEventBase<PointerDownEvent> { }
    public class PointerMoveEvent : PointerEventBase<PointerMoveEvent> { }
    public class PointerUpEvent : PointerEventBase<PointerUpEvent> { }
    public class ChangeEvent<T> : EventBase { public T newValue => default; public T previousValue => default; }
    public struct Rect2 { }
    public class VisualElement : IEventHandler
    {
        public IVisualElementScheduler schedule => null;
        public IStyle style => null;
        public IResolvedStyle resolvedStyle => null;
        public PickingMode pickingMode { get; set; }
        public VisualElement parent => null;
        public virtual VisualElement contentContainer => this;
        public IPanel panel => null;
        public Rect layout => default;
        public List<StyleSheet> styleSheetsList = null;
        public VisualElementStyleSheetSet styleSheets => default;
        public void Add(VisualElement e) { }
        public void Clear() { }
        public void RemoveFromHierarchy() { }
        public void AddToClassList(string c) { }
        public void RemoveFromClassList(string c) { }
        public bool ClassListContains(string c) => false;
        public void EnableInClassList(string c, bool b) { }
        public void Focus() { }
        public void RegisterCallback<T>(EventCallback<T> cb) where T : EventBase { }
    }
    public struct VisualElementStyleSheetSet { public void Add(StyleSheet s) { } }
    public static class UQueryExtensions
    {
        public static UQueryBuilder<T> Query<T>(this VisualElement e) where T : VisualElement => default;
        public static T Q<T>(this VisualElement e, string name = null, string className = null) where T : VisualElement => null;
    }
    public struct UQueryBuilder<T> where T : VisualElement { public void ForEach(Action<T> a) { } }
    public static class PointerCaptureHelper
    {
        public static void CapturePointer(this IEventHandler h, int id) { }
        public static void ReleasePointer(this IEventHandler h, int id) { }
    }
    public interface INotifyValueChanged<T> { T value { get; set; } void SetValueWithoutNotify(T v); }
    public static class INotifyValueChangedExtensions
    {
        public static bool RegisterValueChangedCallback<T>(this INotifyValueChanged<T> c, EventCallback<ChangeEvent<T>> cb) => true;
    }
    public class TextElement : VisualElement { public string text { get; set; } public bool enableRichText { get; set; } }
    public class Label : TextElement { public Label() { } public Label(string t) { } }
    public class Button : TextElement { public Button() { } public Button(Action a) { } public event Action clicked; }
    public class BaseField<T> : VisualElement, INotifyValueChanged<T> { public T value { get; set; } public void SetValueWithoutNotify(T v) { } public string label { get; set; } }
    public class TextField : BaseField<string> { public TextField() { } public TextField(string l) { } public int maxLength { get; set; } }
    public class Slider : BaseField<float> { public Slider(string l, float a = 0, float b = 10) { } }
    public class Toggle : BaseField<bool> { public Toggle() { } public Toggle(string l) { } }
    public class DropdownField : BaseField<string> { public DropdownField(string l, List<string> c, int i, Func<string, string> a = null, Func<string, string> b = null) { } public int index { get; set; } }
    public class Foldout : BindableElement { public string text { get; set; } public bool value { get; set; } }
    public class BindableElement : VisualElement { }
    public class ScrollView : VisualElement { public ScrollView() { } public ScrollView(ScrollViewMode m) { } }
    public class StyleSheet : Object { }
    public class ThemeStyleSheet : StyleSheet { }
    public class PanelSettings : ScriptableObject
    {
        public ThemeStyleSheet themeStyleSheet { get; set; }
        public PanelScaleMode scaleMode { get; set; }
        public Vector2Int referenceResolution { get; set; }
        public PanelScreenMatchMode screenMatchMode { get; set; }
        public float match { get; set; }
        public float sortingOrder { get; set; }
    }
    public class UIDocument : MonoBehaviour { public PanelSettings panelSettings { get; set; } public VisualElement rootVisualElement => null; }
}

namespace UnityEditor
{
    using UnityEngine;
    public class MenuItem : Attribute { public MenuItem(string p) { } public MenuItem(string p, bool v, int prio) { } }
    public static class AssetDatabase
    {
        public static string[] FindAssets(string f, string[] dossiers) => null;
        public static string[] FindAssets(string f) => null;
        public static Object[] LoadAllAssetsAtPath(string p) => null;
        public static string GUIDToAssetPath(string g) => null;
        public static string GUIDToAssetPath(GUID g) => null;
        public static T LoadAssetAtPath<T>(string p) where T : Object => null;
        public static string GetAssetPath(Object o) => null;
        public static bool IsValidFolder(string p) => true;
        public static string CreateFolder(string a, string b) => null;
        public static void CreateAsset(Object o, string p) { }
        public static void ImportAsset(string p, ImportAssetOptions o) { }
        public static void StartAssetEditing() { }
        public static void StopAssetEditing() { }
    }
    public struct GUID { }
    [Flags] public enum ImportAssetOptions { Default = 0, ForceUpdate = 1, ImportRecursive = 256 }
    public static class EditorUtility
    {
        public static bool DisplayDialog(string a, string b, string c) => true;
        public static bool DisplayDialog(string a, string b, string c, string d) => true;
        public static bool DisplayCancelableProgressBar(string a, string b, float c) => false;
        public static void ClearProgressBar() { }
        public static bool IsPersistent(Object o) => false;
        public static void SetDirty(Object o) { }
    }
    public static class Selection { public static GameObject activeGameObject { get; set; } public static Object activeObject { get; set; } }
    public static class PrefabUtility { public static Object InstantiatePrefab(Object o, Transform parent) => null; public static Object InstantiatePrefab(Object o) => null; }
    public class InitializeOnLoadAttribute : Attribute { }
    public static class SessionState { public static bool GetBool(string k, bool d) => d; public static void SetBool(string k, bool v) { } public static void EraseBool(string k) { } }
    public enum PlayModeStateChange { EnteredEditMode, ExitingEditMode, EnteredPlayMode, ExitingPlayMode }
    public static class Undo
    {
        public static void DestroyObjectImmediate(Object o) { } public static void RegisterCreatedObjectUndo(Object o, string n) { }
        public static void SetTransformParent(Transform t, Transform p, string n) { } public static void RecordObject(Object o, string n) { }
        public static T AddComponent<T>(GameObject g) where T : Component => null;
    }
    public class EditorBuildSettingsScene { public EditorBuildSettingsScene(string p, bool e) { } public string path => ""; public bool enabled { get; set; } }
    public static class ShaderUtil { public static bool ShaderHasError(UnityEngine.Shader s) => false; }
    public static class EditorGUIUtility { public static string systemCopyBuffer { get; set; } }
    public static class EditorBuildSettings { public static EditorBuildSettingsScene[] scenes { get; set; } }
    public static class EditorApplication { public static bool isPlaying { get; set; } public static event Action<PlayModeStateChange> playModeStateChanged; }
    public class AssetImporter : Object { public static AssetImporter GetAtPath(string p) => null; }
    public enum TextureImporterType { Default, NormalMap }
    public enum TextureImporterShape { Texture2D = 1, TextureCube = 2 }
    public enum TextureImporterGenerateCubemap { Spheremap = 1, Cylindrical = 2 }
    public enum TextureImporterCompression { Uncompressed, Compressed, CompressedHQ }
    public class TextureImporter : AssetImporter
    {
        public TextureImporterType textureType { get; set; } public TextureImporterShape textureShape { get; set; }
        public TextureImporterGenerateCubemap generateCubemap { get; set; } public int maxTextureSize { get; set; }
        public TextureImporterCompression textureCompression { get; set; } public bool sRGBTexture { get; set; }
    }
    public class AssetPostprocessor { public string assetPath { get; set; } public AssetImporter assetImporter => null; public virtual uint GetVersion() => 0; }
}

namespace UnityEditor.SceneManagement
{
    using UnityEngine.SceneManagement;
    public enum NewSceneSetup { EmptyScene, DefaultGameObjects }
    public enum NewSceneMode { Single, Additive }
    public static class EditorSceneManager
    {
        public static bool SaveCurrentModifiedScenesIfUserWantsTo() => true;
        public static Scene GetActiveScene() => default;
        public static Scene NewScene(NewSceneSetup s, NewSceneMode m) => default;
        public static bool SaveScene(Scene s, string p) => true;
        public static bool SaveScene(Scene s) => true;
        public static bool MarkSceneDirty(Scene s) => true;
        public static Scene LoadSceneInPlayMode(string p, LoadSceneParameters l) => default;
    }
}
