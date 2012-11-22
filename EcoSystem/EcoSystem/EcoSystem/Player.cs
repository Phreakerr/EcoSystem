using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcoSystem
{
    class Player
    {
        public int resources;
        public int numberOfFactories;
        public bool citadelStanding;

        public Player()
        {
            resources = 0;
            numberOfFactories = 0;
            citadelStanding = true;
        }
    }
}
