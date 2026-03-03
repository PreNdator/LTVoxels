

using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.SceneManagement;

namespace LedenevTV.Runtime.SceneLoading
{
    public class SceneLoader : ISceneLoader
    {
        private bool _isLoading = false;

        public async UniTask Load(string sceneName, CancellationToken ct = default)
        {
            if (_isLoading) return;

            try
            {
                _isLoading = true;

                await SceneManager.LoadSceneAsync(sceneName)
                    .ToUniTask(progress: null, timing: PlayerLoopTiming.Update, cancellationToken: ct);
            }
            finally
            {
                _isLoading = false;
            }
        }
    }
}
