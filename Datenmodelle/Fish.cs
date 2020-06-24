using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Datenmodelle
{
    //Klasse zur Abschätzung wieviele Peers im Netzwerk sind.
    public class Fish
    {
        private int size;           //size of the fish
        private double portion;     //how much of this fish do you really have?


        public Fish(int size, double portion)
        {
            this.size = size;
            if (portion > 1 || portion < 0)
            {
                throw new System.ArgumentException("Proportion cannot be below zero or above one. ");
            } else {
                this.portion = portion;
            }
        }

        public int GetSize()
        {
            return size;
        }
        public double GetPortion()
        {
            return portion;
        }
        public void SetPortion(double newPortion)
        {
            if (newPortion > 1 || newPortion < 0)
            {
                throw new System.ArgumentException("Proportion cannot be below zero or above one. ");
            }
            else
            {
                this.portion = newPortion;
            }
        }

    }
}
