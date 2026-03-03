using LedenevTV.Voxel.Editing;
using Zenject;

namespace LedenevTV.Installers
{
    public class VoxelEditingInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IColorMaskApplier>().To<ColorMaskApplier>().AsSingle();
            Container.Bind<IVoxelTypeMaskApplier>().To<VoxelTypeMaskApplier>().AsSingle();
            Container.Bind<IMaterialMaskApplier>().To<MaterialMaskApplier>().AsSingle();
            Container.Bind<IChunkMaskApplier>().To<ChunkMaskApplier>().AsSingle();
        }
    }
}