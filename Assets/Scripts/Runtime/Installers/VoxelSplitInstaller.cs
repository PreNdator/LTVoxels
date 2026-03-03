using LedenevTV.Runtime.Examples;
using LedenevTV.Voxel;
using LedenevTV.Voxel.Splitting;
using UnityEngine;
using Zenject;

namespace LedenevTV.Installers
{
    public sealed class VoxelSplitInstaller : MonoInstaller
    {
        [SerializeField] private NeighborMode _mode = NeighborMode.NeighborVoxels6;
        [SerializeField] private ChunkPieceView _chunkPiecePrefab;

        public enum NeighborMode
        {
            NeighborVoxels6,
            NeighborVoxels18,
            NeighborVoxels26
        }

        public override void InstallBindings()
        {
            switch (_mode)
            {
                case NeighborMode.NeighborVoxels6:
                    Container.Bind<INeighborVoxels>().To<NeighborVoxels6>().AsSingle();
                    break;
                case NeighborMode.NeighborVoxels18:
                    Container.Bind<INeighborVoxels>().To<NeighborVoxels18>().AsSingle();
                    break;
                case NeighborMode.NeighborVoxels26:
                    Container.Bind<INeighborVoxels>().To<NeighborVoxels26>().AsSingle();
                    break;
            }

            Container.BindFactory<ChunkPieceViewWithMesh, ChunkPieceViewFactory>().FromComponentInNewPrefab(_chunkPiecePrefab);

            Container.Bind<IChunkSplitter>().To<ConnectedComponentsChunkSplitter>().AsSingle();
        }
    }
}