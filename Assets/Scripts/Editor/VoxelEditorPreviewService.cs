using LedenevTV.Voxel;
using LedenevTV.Voxel.Drawing;
using LedenevTV.Voxel.Serialization;
using R3;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LedenevTV.Editor
{
    [InitializeOnLoad]
    public static class VoxelEditorPreviewService
    {
        private const string PreviewObjectName = "[Voxel Editor Preview]";
        private const byte MaterialLimit = 8;

        private static readonly HideFlags PreviewHideFlags =
            HideFlags.HideInHierarchy |
            HideFlags.DontSaveInEditor |
            HideFlags.DontSaveInBuild;

        private static readonly Dictionary<MonoBehaviour, PreviewHandle> Handles =
            new Dictionary<MonoBehaviour, PreviewHandle>();

        private static readonly IChunkImporterResolver ImporterResolver =
            new ChunkImporterResolver(
                new MagicaVoxelVoxImporter(),
                new PlyPointCloudImporter(),
                new VoxchImportExport());

        private static readonly IChunkImportService ChunkImportService =
            new ChunkImportService(ImporterResolver);

        private static readonly IAsyncChunkImportService AsyncChunkImportService =
            new AsyncChunkImportService(ImporterResolver);

        private static readonly IVoxelMeshBuilder MeshBuilder =
            new VoxelMeshBuilder(new VoxelMeshSettings(MaterialLimit), new CenterChunkSpace());

        private static bool _refreshScheduled;
        private static bool _isRefreshing;
        private static bool _isChangingPreviewObjects;

        static VoxelEditorPreviewService()
        {
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            AssemblyReloadEvents.beforeAssemblyReload += CleanupAll;

            ScheduleRefresh();
        }

        private static void OnHierarchyChanged()
        {
            if (_isRefreshing || _isChangingPreviewObjects)
                return;

            ScheduleRefresh();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.EnteredPlayMode)
            {
                CleanupAll();
                return;
            }

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                ScheduleRefresh();
            }
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            ScheduleRefresh();
        }

        private static void OnSceneClosing(Scene scene, bool removingScene)
        {
            CleanupAll();
        }

        private static void ScheduleRefresh()
        {
            if (_refreshScheduled)
                return;

            _refreshScheduled = true;
            EditorApplication.delayCall += Refresh;
        }

        private static void Refresh()
        {
            EditorApplication.delayCall -= Refresh;
            _refreshScheduled = false;

            if (_isRefreshing)
                return;

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                CleanupAll();
                return;
            }

            _isRefreshing = true;

            try
            {
                HashSet<MonoBehaviour> foundSources = new HashSet<MonoBehaviour>();
                MonoBehaviour[] behaviours = Resources.FindObjectsOfTypeAll<MonoBehaviour>();

                for (int i = 0; i < behaviours.Length; i++)
                {
                    MonoBehaviour behaviour = behaviours[i];
                    if (!IsSceneBehaviour(behaviour))
                        continue;

                    if (behaviour is IEditorVoxelPreviewSource syncSource)
                    {
                        foundSources.Add(behaviour);
                        RegisterSyncSource(behaviour, syncSource);
                        continue;
                    }

                    if (behaviour is IEditorAsyncVoxelPreviewSource asyncSource)
                    {
                        foundSources.Add(behaviour);
                        RegisterAsyncSource(behaviour, asyncSource);
                    }
                }

                RemoveStaleSources(foundSources);
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private static bool IsSceneBehaviour(MonoBehaviour behaviour)
        {
            if (behaviour == null)
                return false;

            GameObject gameObject = behaviour.gameObject;
            if (gameObject == null)
                return false;

            if (EditorUtility.IsPersistent(behaviour))
                return false;

            if ((behaviour.hideFlags & HideFlags.DontSaveInEditor) != 0)
                return false;

            Scene scene = gameObject.scene;
            return scene.IsValid() && scene.isLoaded;
        }

        private static void RegisterSyncSource(MonoBehaviour owner, IEditorVoxelPreviewSource source)
        {
            if (Handles.TryGetValue(owner, out PreviewHandle existing) && existing.Mode == PreviewMode.Sync)
                return;

            Unregister(owner);

            PreviewHandle handle = new PreviewHandle(owner, PreviewMode.Sync);
            Handles.Add(owner, handle);

            ReadOnlyReactiveProperty<IBytesSource> byteSourceProperty = source.EditorByteSource;
            if (byteSourceProperty != null)
            {
                handle.Subscription = byteSourceProperty.Subscribe(byteSource => RenderSync(handle, byteSource));
            }
        }

        private static void RegisterAsyncSource(MonoBehaviour owner, IEditorAsyncVoxelPreviewSource source)
        {
            if (Handles.TryGetValue(owner, out PreviewHandle existing) && existing.Mode == PreviewMode.Async)
                return;

            Unregister(owner);

            PreviewHandle handle = new PreviewHandle(owner, PreviewMode.Async);
            Handles.Add(owner, handle);

            ReadOnlyReactiveProperty<IAsyncBytesSource> byteSourceProperty = source.EditorAsyncByteSource;
            if (byteSourceProperty != null)
            {
                handle.Subscription = byteSourceProperty.Subscribe(byteSource => RenderAsync(handle, byteSource));
            }
        }

        private static void RemoveStaleSources(HashSet<MonoBehaviour> foundSources)
        {
            List<MonoBehaviour> staleSources = new List<MonoBehaviour>();

            foreach (MonoBehaviour owner in Handles.Keys)
            {
                if (owner == null || !foundSources.Contains(owner))
                {
                    staleSources.Add(owner);
                }
            }

            for (int i = 0; i < staleSources.Count; i++)
            {
                Unregister(staleSources[i]);
            }
        }

        private static void Unregister(MonoBehaviour owner)
        {
            if (!Handles.TryGetValue(owner, out PreviewHandle handle))
                return;

            handle.Dispose();
            Handles.Remove(owner);
        }

        private static void CleanupAll()
        {
            foreach (PreviewHandle handle in Handles.Values)
            {
                handle.Dispose();
            }

            Handles.Clear();
        }

        private static void RenderSync(PreviewHandle handle, IBytesSource source)
        {
            handle.BeginRender(out int renderVersion);
            DestroyPreviewMesh(handle);

            if (source == null)
            {
                DestroyPreviewObject(handle);
                return;
            }

            VoxelChunk chunk = null;

            try
            {
                chunk = ChunkImportService.Load(source);

                if (!handle.IsCurrent(renderVersion) || EditorApplication.isPlayingOrWillChangePlaymode)
                    return;

                BuildPreview(handle, chunk);
            }
            catch (Exception exception)
            {
                if (handle.IsCurrent(renderVersion))
                {
                    DestroyPreviewObject(handle);
                    Debug.LogException(exception, handle.Owner);
                }
            }
            finally
            {
                if (chunk != null)
                    chunk.Dispose();
            }
        }

        private static async void RenderAsync(PreviewHandle handle, IAsyncBytesSource source)
        {
            CancellationToken cancellationToken = handle.BeginRender(out int renderVersion);
            DestroyPreviewMesh(handle);

            if (source == null)
            {
                DestroyPreviewObject(handle);
                return;
            }

            VoxelChunk chunk = null;

            try
            {
                chunk = await AsyncChunkImportService.LoadAsync(source, cancellationToken);

                if (!handle.IsCurrent(renderVersion) || EditorApplication.isPlayingOrWillChangePlaymode)
                    return;

                BuildPreview(handle, chunk);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                if (handle.IsCurrent(renderVersion))
                {
                    DestroyPreviewObject(handle);
                    Debug.LogException(exception, handle.Owner);
                }
            }
            finally
            {
                if (chunk != null)
                    chunk.Dispose();
            }
        }

        private static void BuildPreview(PreviewHandle handle, VoxelChunk chunk)
        {
            if (handle.Owner == null || chunk == null || !chunk.IsAllocated)
                return;

            GameObject previewObject = EnsurePreviewObject(handle);
            Mesh previewMesh = new Mesh
            {
                name = handle.Owner.name + " Voxel Editor Preview",
                hideFlags = PreviewHideFlags
            };

            try
            {
                MeshBuilder.RebuildMesh(previewMesh, chunk, drawFacesOnBounds: true);
            }
            catch
            {
                DestroyUnityObject(previewMesh);
                throw;
            }

            DestroyPreviewMesh(handle);

            MeshFilter meshFilter = previewObject.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = previewMesh;
            handle.PreviewMesh = previewMesh;

            CopyRendererSettings(handle.Owner, previewObject.GetComponent<MeshRenderer>());
        }

        private static GameObject EnsurePreviewObject(PreviewHandle handle)
        {
            if (handle.PreviewObject != null)
                return handle.PreviewObject;

            DestroyExistingPreviewChildren(handle.Owner.transform);

            _isChangingPreviewObjects = true;

            try
            {
                GameObject previewObject = new GameObject(PreviewObjectName, typeof(MeshFilter), typeof(MeshRenderer));
                previewObject.hideFlags = PreviewHideFlags;
                previewObject.layer = handle.Owner.gameObject.layer;

                Transform previewTransform = previewObject.transform;
                previewTransform.hideFlags = PreviewHideFlags;
                previewTransform.SetParent(handle.Owner.transform, worldPositionStays: false);
                previewTransform.localPosition = Vector3.zero;
                previewTransform.localRotation = Quaternion.identity;
                previewTransform.localScale = Vector3.one;

                MeshFilter meshFilter = previewObject.GetComponent<MeshFilter>();
                MeshRenderer meshRenderer = previewObject.GetComponent<MeshRenderer>();
                meshFilter.hideFlags = PreviewHideFlags;
                meshRenderer.hideFlags = PreviewHideFlags;

                handle.PreviewObject = previewObject;
                return previewObject;
            }
            finally
            {
                _isChangingPreviewObjects = false;
            }
        }

        private static void CopyRendererSettings(MonoBehaviour owner, MeshRenderer target)
        {
            MeshRenderer source = owner.GetComponent<MeshRenderer>();
            if (source == null || target == null)
                return;

            target.enabled = source.enabled;
            target.sharedMaterials = source.sharedMaterials;
            target.shadowCastingMode = source.shadowCastingMode;
            target.receiveShadows = source.receiveShadows;
            target.lightProbeUsage = source.lightProbeUsage;
            target.reflectionProbeUsage = source.reflectionProbeUsage;
        }

        private static void DestroyPreviewMesh(PreviewHandle handle)
        {
            if (handle.PreviewMesh == null)
                return;

            if (handle.PreviewObject != null)
            {
                MeshFilter meshFilter = handle.PreviewObject.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh == handle.PreviewMesh)
                {
                    meshFilter.sharedMesh = null;
                }
            }

            DestroyUnityObject(handle.PreviewMesh);
            handle.PreviewMesh = null;
        }

        private static void DestroyPreviewObject(PreviewHandle handle)
        {
            DestroyPreviewMesh(handle);

            if (handle.PreviewObject == null)
                return;

            _isChangingPreviewObjects = true;

            try
            {
                DestroyUnityObject(handle.PreviewObject);
                handle.PreviewObject = null;
            }
            finally
            {
                _isChangingPreviewObjects = false;
            }
        }

        private static void DestroyExistingPreviewChildren(Transform owner)
        {
            _isChangingPreviewObjects = true;

            try
            {
                for (int i = owner.childCount - 1; i >= 0; i--)
                {
                    Transform child = owner.GetChild(i);
                    if (child != null && child.name == PreviewObjectName)
                    {
                        MeshFilter meshFilter = child.GetComponent<MeshFilter>();
                        if (meshFilter != null && meshFilter.sharedMesh != null)
                        {
                            DestroyUnityObject(meshFilter.sharedMesh);
                            meshFilter.sharedMesh = null;
                        }

                        DestroyUnityObject(child.gameObject);
                    }
                }
            }
            finally
            {
                _isChangingPreviewObjects = false;
            }
        }

        private static void DestroyUnityObject(UnityEngine.Object obj)
        {
            if (obj != null)
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
        }

        private sealed class PreviewHandle : IDisposable
        {
            private CancellationTokenSource _renderCts;
            private int _renderVersion;
            private bool _disposed;

            public PreviewHandle(MonoBehaviour owner, PreviewMode mode)
            {
                Owner = owner;
                Mode = mode;
            }

            public MonoBehaviour Owner { get; }
            public PreviewMode Mode { get; }
            public IDisposable Subscription { get; set; }
            public GameObject PreviewObject { get; set; }
            public Mesh PreviewMesh { get; set; }

            public CancellationToken BeginRender(out int renderVersion)
            {
                CancelRender();

                _renderVersion++;
                _renderCts = new CancellationTokenSource();
                renderVersion = _renderVersion;

                return _renderCts.Token;
            }

            public bool IsCurrent(int renderVersion)
            {
                return !_disposed &&
                       Owner != null &&
                       _renderCts != null &&
                       !_renderCts.IsCancellationRequested &&
                       _renderVersion == renderVersion;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                CancelRender();

                if (Subscription != null)
                {
                    Subscription.Dispose();
                    Subscription = null;
                }

                VoxelEditorPreviewService.DestroyPreviewObject(this);
            }

            private void CancelRender()
            {
                if (_renderCts == null)
                    return;

                _renderCts.Cancel();
                _renderCts.Dispose();
                _renderCts = null;
            }
        }

        private enum PreviewMode
        {
            Sync,
            Async
        }
    }
}
