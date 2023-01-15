namespace RTS
{
    using UnityEngine;
    public class PoolItem
    {
        public GameObject o = null;
        public PoolItem next = null;
    }

    
    public class Pool
    {
        private PoolItem next;

        public Pool(GameObject CLS, int num)
        {
            PoolItem item = null;
            for (int i = 0; i < num; i++)
            {
                if (i == 0)
                {
                    item = new PoolItem();
                    next = item;
                }
                else
                {
                    item.next = new PoolItem();
                    item = item.next;
                }

                item.o = MonoBehaviour.Instantiate(CLS);
                item.o.SetActive(false);

            }
        }

        public PoolItem get()
        {
            PoolItem item = next;
            item.o.SetActive(true);
            next = item.next;
            return item;
        }

        public void putBack(PoolItem item)
        {
            item.next = next;
            item.o.SetActive(false);
            next = item;
            
        }
    }

}
