using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIExt.Elements
{

    public class TetherList<E> : MonoBehaviour
    {
        public System.Action onPopulateList;

        [SerializeField] GameObject tetherBlockPrefab;

        protected List<GameObject> blocks = new List<GameObject>();
        protected List<ITether<E>> tethers = new List<ITether<E>>();

        protected virtual void OnEnable()
        {

        }

        protected virtual void OnDisable()
        {

        }

        public void PopulateList(E[] elements, Transform root = null, bool events = true)
        {
            if (root == null)
                root = transform;

            if (elements != null)
            {
                foreach (E el in elements)
                {
                    if (validateItem(el))
                    {
                        var obj = Instantiate(tetherBlockPrefab, root);
                        ITether<E> tether = obj.GetComponent<ITether<E>>();
                        tether.tether = el;

                        this.tethers.Add(tether);
                        onPopulateItem(tether);
                        this.blocks.Add(obj);
                    }
                }
            }

            if (events && onPopulateList != null)
                onPopulateList();
        }

        protected virtual bool validateItem(E tether) { return true; }
        protected virtual void onPopulateItem(ITether<E> tether) { }

        public void ClearList()
        {
            GameObject[] blocks = this.blocks.ToArray();

            foreach (GameObject block in blocks)
                GameObject.Destroy(block);

            this.blocks = new List<GameObject>();
            this.tethers = new List<ITether<E>>();
        }

    }

}
