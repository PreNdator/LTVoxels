using LedenevTV.Runtime.SceneLoading;
using Zenject;

namespace LedenevTV.Installers
{
    public class LoadingSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ISceneLoader>().To<SceneLoader>().AsSingle();
        }
    }
}