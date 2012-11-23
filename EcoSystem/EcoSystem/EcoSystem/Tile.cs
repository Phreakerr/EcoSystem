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
        public bool faction { get; set; }  //True for urban, false for forest
        public int health { get; set; }
        public Texture2D texture { get; set; }
        public bool isFactory { get; set; }

        const int chanceOfFireDamage = 40;
        const int damageByFire = 10;

        private int coordX;
        private int coordY;
        private bool isOnFire;
        private Random rndFireDamage;

        public Tile()
        {

        }

        public Tile(int X, int Y, bool faction, Texture2D texture, int randomSeed)
        {
            coordX = X;
            coordY = Y;
            health = 100;
            this.faction = faction;
            this.texture = texture;
            position = new Vector2(X, Y);
            rndFireDamage = new Random(randomSeed);
            isFactory = false;
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
            if (isOnFire && rndFireDamage.Next(0, chanceOfFireDamage) == 7) //Lucky number 7
            {
                health -= damageByFire;
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

        public void doDamage(int damage)
        {
            health -= damage;
        }

        public void upgrade()
        {
            isFactory = true;
            health = 250;
        }
    }
}
