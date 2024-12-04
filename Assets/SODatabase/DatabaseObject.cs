using UnityEngine;
using UnityEngine.UIElements;

namespace SODatabase
{
    [System.Serializable]
    public abstract class DatabaseObject : ScriptableObject
    {
        public int ID = -1;

        public string Name;
    }
}