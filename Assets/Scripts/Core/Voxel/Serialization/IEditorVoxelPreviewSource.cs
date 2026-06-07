#if UNITY_EDITOR
using R3;

namespace LedenevTV.Voxel.Serialization
{
    public interface IEditorVoxelPreviewSource
    {
        ReadOnlyReactiveProperty<IBytesSource> EditorByteSource { get; }
    }

    public interface IEditorAsyncVoxelPreviewSource
    {
        ReadOnlyReactiveProperty<IAsyncBytesSource> EditorAsyncByteSource { get; }
    }
}
#endif
