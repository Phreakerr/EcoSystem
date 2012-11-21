using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EcoSystem
{
    class Tile
    {
        public Vector2 position { get; set; }
        public bool faction { get; set; }  //True for urban, false for factory
        public int health { get; set; }
        public Texture2D texture { get; set; }

        private int coordX;
        private int coordY;
        private bool isOnFire;

        public Tile()
        {

        }

        public Tile(int X, int Y, bool faction, Texture2D texture)
        {
            coordX = X;
            coordY = Y;
            this.faction = faction;
            this.texture = texture;
            position = new Vector2(X, Y);
        }

        public void fireStart()
        {
            isOnFire = true;
        }

        public void fireExtinguish()
        {
            isOnFire = false;
        }

        public bool checkFire()
        {
            return isOnFire;
        }

        public Texture2D getTexture()
        {
            return texture;
        }
    }
}
