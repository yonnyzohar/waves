namespace Engine
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    
    public class Pool
    {
        private Context context;

        public delegate void PoolCreateFnctn(object o);
        public delegate void PoolRetrieveFnctn(object o);

        PoolCreateFnctn cfunc;
        PoolRetrieveFnctn rfunc;
        private int active = 0;

        private object[] arr;

        public Pool(Context _context, Type CLS, GameObject prefab, int num, PoolCreateFnctn _cfunc = null, PoolRetrieveFnctn _rfunc = null)
        {
            arr = new object[num];
            cfunc = _cfunc;
            rfunc = _rfunc;
            context = _context;
            active = 0;
            for (int i = 0; i < num; i++)
            {
                object o = Activator.CreateInstance(CLS, context, 0, 0, MonoBehaviour.Instantiate(prefab));
                cfunc(o);
                arr[i] = o;
            }
        }

        public object get()
        {
            object o = arr[active];
            rfunc(o);
            active++;
            return o;
        }

        public void putBack(object o)
        {
            active--;
            arr[active] = o;
            cfunc(o);
        }

        public int getNumActive()
        {
            return active;
        }
    }

}
