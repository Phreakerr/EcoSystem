using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcoSystem
{
    class Player
    {
        public bool faction;
        public int resources;
        public int numberOfFactories;
        public bool citadelStanding;

        public Player(bool faction)
        {
            this.faction = faction;
            resources = 500;
            numberOfFactories = 0;
            citadelStanding = true;
        }
    }
}
