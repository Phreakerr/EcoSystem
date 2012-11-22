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
        private Random rndFireDamage;

        public Tile()
        {

        }

        public Tile(int X, int Y, bool faction, Texture2D texture)
        {
            coordX = X;
            coordY = Y;
            health = 100;
            this.faction = faction;
            this.texture = texture;
            position = new Vector2(X, Y);
            rndFireDamage = new Random();
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

        public void fireDamage()
        {
            if (isOnFire && rndFireDamage.Next(0, 60) == 7) //Lucky number 7
            {
                health -= 5;
            }
        }

        public void spreadFire()
        {

        }

        public bool checkDestroyed()
        {
            if (health <= 0)
            {
                fireExtinguish();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
