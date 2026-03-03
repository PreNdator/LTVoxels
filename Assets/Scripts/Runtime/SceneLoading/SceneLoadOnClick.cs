using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace LedenevTV.Runtime.SceneLoading
{
    public class SceneLoadOnClick : MonoBehaviour
    {
        [SerializeField, Scene]
        private string _sceneToLoad;

        private ISceneLoader _sceneLoader;

        [Inject]
        private void Construct(ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        public void LoadScene()
        {
            _sceneLoader.Load(_sceneToLoad).Forget(Debug.LogError);
        }
    }
}
