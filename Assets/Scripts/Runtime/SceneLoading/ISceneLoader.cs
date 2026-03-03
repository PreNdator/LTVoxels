using Cysharp.Threading.Tasks;
using System.Threading;

namespace LedenevTV.Runtime.SceneLoading
{
    public interface ISceneLoader
    {
        public UniTask Load(string sceneName, CancellationToken ct = default);
    }
}
