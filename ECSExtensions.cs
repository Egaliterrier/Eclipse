using Il2CppInterop.Runtime;
using ProjectM;
using Stunlock.Core;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;

namespace Eclipse;
internal static class ECSExtensions
{
    static EntityManager EntityManager => Core.EntityManager;
    static PrefabCollectionSystem PrefabCollectionSystem => Core.PrefabCollectionSystem;

    public static unsafe void Write<T>(this Entity entity, T componentData) where T : struct
    {
        // Get the ComponentType for T
        var ct = new ComponentType(Il2CppType.Of<T>());

        // Marshal the component data to a byte array
        byte[] byteArray = StructureToByteArray(componentData);

        // Get the size of T
        int size = Marshal.SizeOf<T>();

        // Create a pointer to the byte array
        fixed (byte* p = byteArray)
        {
            // Set the component data
            EntityManager.SetComponentDataRaw(entity, ct.TypeIndex, p, size);
        }
    }
    public static byte[] StructureToByteArray<T>(T structure) where T : struct
    {
        int size = Marshal.SizeOf(structure);
        byte[] byteArray = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(structure, ptr, true);
        Marshal.Copy(ptr, byteArray, 0, size);
        Marshal.FreeHGlobal(ptr);

        return byteArray;
    }
    public static unsafe T Read<T>(this Entity entity) where T : struct
    {
        // Get the ComponentType for T
        var ct = new ComponentType(Il2CppType.Of<T>());

        // Get a pointer to the raw component data
        void* rawPointer = EntityManager.GetComponentDataRawRO(entity, ct.TypeIndex);

        // Marshal the raw data to a T struct
        T componentData = Marshal.PtrToStructure<T>(new IntPtr(rawPointer));

        return componentData;
    }
    public static string LookupName(this PrefabGUID prefabGUID)
    {
        return (PrefabCollectionSystem.PrefabGuidToNameDictionary.ContainsKey(prefabGUID)
            ? PrefabCollectionSystem.PrefabGuidToNameDictionary[prefabGUID] + " " + prefabGUID : "Guid Not Found").ToString();
    }
    public static DynamicBuffer<T> ReadBuffer<T>(this Entity entity) where T : struct
    {
        return EntityManager.GetBuffer<T>(entity);
    }
    public static bool TryGetComponent<T>(this Entity entity, out T componentData) where T : struct
    {
        componentData = default;
        if (entity.Has<T>())
        {
            componentData = entity.Read<T>();
            return true;
        }
        return false;
    }
    public static bool Has<T>(this Entity entity)
    {
        var ct = new ComponentType(Il2CppType.Of<T>());
        return EntityManager.HasComponent(entity, ct);
    }
    public static void Add<T>(this Entity entity)
    {
        var ct = new ComponentType(Il2CppType.Of<T>());
        EntityManager.AddComponent(entity, ct);
    }
    public static void Remove<T>(this Entity entity)
    {
        var ct = new ComponentType(Il2CppType.Of<T>());
        EntityManager.RemoveComponent(entity, ct);
    }
    public static void LogComponentTypes(this Entity entity)
    {
        NativeArray<ComponentType>.Enumerator enumerator = EntityManager.GetComponentTypes(entity).GetEnumerator();
        Core.Log.LogInfo("===");
        while (enumerator.MoveNext())
        {
            ComponentType current = enumerator.Current;
            Core.Log.LogInfo($"{current}");
        }
        Core.Log.LogInfo("===");
    }
    public static bool Exists(this Entity entity)
    {
        return EntityManager.Exists(entity);
    }
}