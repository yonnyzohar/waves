namespace Engine
{
    using System;
    public class Context
    {
        public Factory factory;
        public Model model;

        public Context()
        {
        }

        public Model getModel()
        {
            return model;
        }
    }

}
