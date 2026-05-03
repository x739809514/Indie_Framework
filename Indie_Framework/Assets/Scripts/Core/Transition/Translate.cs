using UnityEngine.EventSystems;

namespace Core.Transition
{
    public class Translate: IPointerClickHandler
    {
        [SceneName]
        public string FromName;
        [SceneName]
        public string ToName;

        private void TranslateScene()
        {
            TransitionManager.Instance.Transition(FromName, ToName);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            TranslateScene();
        }
    }
}