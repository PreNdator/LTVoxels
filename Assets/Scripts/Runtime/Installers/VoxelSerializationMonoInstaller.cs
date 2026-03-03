using LedenevTV.Voxel.Serialization;
using System;
using Zenject;

namespace LedenevTV.Installers
{
    public sealed class VoxelSerializationMonoInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            MagicaVoxelVoxImporter vox = new MagicaVoxelVoxImporter();
            PlyPointCloudImporter ply = new PlyPointCloudImporter();
            VoxchImportExport voxch = new VoxchImportExport();

            Container.Bind<MagicaVoxelVoxImporter>().FromInstance(vox);
            Container.Bind<PlyPointCloudImporter>().FromInstance(ply);
            Container.Bind<VoxchImportExport>().FromInstance(voxch);

            IChunkImporterResolver chunkImporterResolver = new ChunkImporterResolver(vox, ply, voxch);

            Container.Bind<IChunkImporterResolver>().FromInstance(chunkImporterResolver);

            Container.Bind<IAsyncChunkImportService>().To<AsyncChunkImportService>().AsSingle();
            Container.Bind<AsyncLazyChunkProvider>().AsSingle();
            Container.Bind<IAsyncChunkProvider>().To<AsyncLazyChunkProvider>().FromResolve();
            Container.Bind<IDisposable>().To<AsyncLazyChunkProvider>().FromResolve();

            Container.Bind<IChunkImportService>().To<ChunkImportService>().AsSingle();
            Container.Bind<LazyChunkProvider>().AsSingle();
            Container.Bind<IChunkProvider>().To<LazyChunkProvider>().FromResolve();
            Container.Bind<IDisposable>().To<LazyChunkProvider>().FromResolve();

        }
    }
}