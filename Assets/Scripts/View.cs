using UnityEngine;

namespace FriendsSystem
{
    //the controller contains the Logic of the view. 
    public abstract class Controller
    {
        public abstract void OnDestroy();
    }

    //The View contains all the reqired logic for the visuals of the UI .. ie Buttons/Text etc. 
    public abstract class View : MonoBehaviour
    {
        public abstract bool HasBack();
        public abstract void SetController(Controller controller = null);
        public virtual void OnBack() { }
    }

    public abstract class View<T> : View where T : Controller
    {
        protected T Controller;
        public override void SetController(Controller controller)
        {
            if(controller != null)
            {
                Controller = controller as T;
            }
        }

        protected virtual void OnDestroy()
        {
            Controller?.OnDestroy();
        }
    }
    
}