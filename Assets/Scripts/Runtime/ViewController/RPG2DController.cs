using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class RPG2DController : MonoBehaviour, IController
    {
        public IArchitecture GetArchitecture()
        {
            return RPG2D.Interface;
        }
    }
}

