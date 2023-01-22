namespace RTS
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class Main : MonoBehaviour
    {
        // Start is called before the first frame update

        RTSContext context;

        
        void Start()
        {
            context = new RTSContext();

            
            

        }

        


        // Update is called once per frame
        void Update()
        {
            context.Update();
            

        }
    }

}
