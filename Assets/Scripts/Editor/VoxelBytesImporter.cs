using LedenevTV.Voxel.Serialization;
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace LedenevTV.Editor
{
    [ScriptedImporter(1, new[] { "vox", "ply" })]
    public sealed class VoxelBytesImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            byte[] data = File.ReadAllBytes(ctx.assetPath);

            VoxelBytesAsset asset = ScriptableObject.CreateInstance<VoxelBytesAsset>();
            asset.name = Path.GetFileNameWithoutExtension(ctx.assetPath);
            asset.SetData(data);

            ctx.AddObjectToAsset("VoxelBytes", asset);
            ctx.SetMainObject(asset);
        }
    }
}