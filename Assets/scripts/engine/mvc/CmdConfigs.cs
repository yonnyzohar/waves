namespace Engine
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    public class CmdConfigs
    {
        public class CmdDefinition
        {
            private Type CLS;
            private object p;

            public CmdDefinition(Type _CLS, object _p = null)
            {
                CLS = _CLS;
                p = _p;
            }

            public Type getCls()
            {
                return CLS;
            }

            public object getParams()
            {
                return p;
            }
        }

        public class CmdInstance
        {
            public string id;
            public object instanceParams;
            public CmdInstance(string _id, object _instanceParams = null)
            {
                id = _id;
                instanceParams = _instanceParams;
            }

            public object getInstanceParams()
            {
                return instanceParams;
            }
        }

        public Dictionary<string, CmdDefinition> definitons = new Dictionary<string, CmdDefinition>();

        public CmdConfigs()
        {

        }
    }

}
