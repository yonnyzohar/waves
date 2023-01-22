namespace Engine
{
    using System;
    using UnityEngine;
    public class PoolItem
    {
        public object o = null;//this is the calss itsef, e.g Tank, Player
        public PoolItem next = null;
    }

    
    public class Pool
    {
        private PoolItem next;
        private Context context;

        public delegate void PoolCreateFnctn(object o);
        public delegate void PoolRetrieveFnctn(object o);

        PoolCreateFnctn cfunc;
        PoolRetrieveFnctn rfunc;
        private int active = 0;

        public Pool(Context _context, Type CLS, GameObject prefab, int num, PoolCreateFnctn _cfunc = null, PoolRetrieveFnctn _rfunc = null)
        {
            cfunc = _cfunc;
            rfunc = _rfunc;
            context = _context;
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

                item.o = Activator.CreateInstance(CLS, context, 0, 0, MonoBehaviour.Instantiate(prefab));
                cfunc(item.o);
                

            }
        }

        public PoolItem get()
        {
            active++;
            PoolItem item = next;
            rfunc(item.o);
            next = item.next;
            return item;
        }

        public void putBack(PoolItem item)
        {
            active--;
            item.next = next;
            cfunc(item.o);
            next = item;
            
        }

        public int getNumActive()
        {
            return active;
        }
    }

}
