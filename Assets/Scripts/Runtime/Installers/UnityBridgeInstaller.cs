using LedenevTV.UnityBridge;
using Zenject;

namespace LedenevTV.Installers
{
    public sealed class UnityBridgeInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IUnityObjectDestroyer>().To<UnityObjectDestroyer>().AsSingle();
        }
    }
}