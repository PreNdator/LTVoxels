using LedenevTV.Voxel.Collisions;
using LedenevTV.Voxel.Drawing;
using UnityEngine;
using Zenject;

namespace LedenevTV.Installers
{
    public sealed class VoxelDrawingInstaller : MonoInstaller
    {
        public enum ChunkSpaceKind
        {
            Center,
            MinCorner
        }

        [SerializeField] private byte _materialLimit = 8;

        [SerializeField] private ChunkSpaceKind _chunkSpace = ChunkSpaceKind.Center;
        [SerializeField] private bool _collideWithTransparentVoxels = true;

        public override void InstallBindings()
        {
            Container.Bind<VoxelMeshSettings>().AsSingle().WithArguments(_materialLimit);
            Container.Bind<VoxelColliderSettings>().FromInstance(new VoxelColliderSettings(_collideWithTransparentVoxels)).AsSingle();

            switch (_chunkSpace)
            {
                case ChunkSpaceKind.Center:
                    Container.Bind<IChunkSpace>().To<CenterChunkSpace>().AsSingle();
                    break;
                case ChunkSpaceKind.MinCorner:
                    Container.Bind<IChunkSpace>().To<MinCornerChunkSpace>().AsSingle();
                    break;
            }

            Container.Bind<IVoxelMeshBuilder>().To<VoxelMeshBuilder>().AsSingle();
            Container.Bind<IVoxelBoxColliderBuilder>().To<GreedyVoxelBoxColliderBuilder>().AsSingle();

        }
    }
}
