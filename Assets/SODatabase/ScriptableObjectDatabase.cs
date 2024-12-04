using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SODatabase
{
    [Serializable]
    public abstract partial class ScriptableObjectDatabase<T> : ScriptableObject where T : DatabaseObject
    {
        public string FilePath;
        [Space]
        public List<T> Database;

        /// <summary>
        /// Workaround for inability to use generic methods from Unity's Context Menu.
        /// </summary>
        /// <remarks>
        /// Unity does not allow calls to methods from generic classes from within the Context Menu.<br/>
        /// Simply call <see cref="ScriptableObjectDatabase{T}.FindAll()"/> within this method.
        /// </remarks>
        public abstract void Find();

        protected void FindAll()
        {
            List<T> resources = Resources.LoadAll<T>(FilePath).OrderBy(i => i.ID).ToList();
            Database = new();

            List<T> hasIDInRange = resources.Where(i => i.ID >= 0).OrderBy(i => i.ID).ToList();
            List<T> hasIDOutOfRange = resources.Where(i => i.ID != -1 && i.ID >= resources.Count).OrderBy(i => i.ID).ToList();
            List<T> noID = resources.Where(i => i.ID <= -1).ToList();

            int index = 0;

            for (int i = 0; i < resources.Count; i++)
            {
                T itemToAdd = hasIDInRange.Find(x => x.ID == i);

                if (itemToAdd != null)
                    Database.Add(itemToAdd);
                else if (index < noID.Count)
                {
                    noID[index].ID = i;
                    itemToAdd = noID[index++];
                    Database.Add(itemToAdd);
                }
            }

            foreach (T item in hasIDOutOfRange)
                Database.Add(item);
        }

        public T Get(int ID)
        {
            return Database.Find(i => i.ID == ID);
        }

        public T Get(string name)
        {
            return Database.Find(i => i.name.ToLower() == name.ToLower());
        }

        public virtual T Get(Func<T, bool> predicate) => Database.FirstOrDefault(predicate);
    }
}