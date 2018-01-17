using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NewestJack
{
    public class Player
    {
        public string Name { get; set; }
        public int Money;
        public float Score { get; set; }
        public bool Dealer = false;
        bool JackTurn = false;
        public CardList<float> Cards = new CardList<float>();

        
        public Player()
        {
            Cards.OnAdd += new EventHandler(Cards_OnAdd);
        }

        public void Cards_OnAdd(object sender, EventArgs e)
        {
            Score = 0;
            foreach (float f in Cards)
            {
                Score += f;
            }

            if (Score > 21)
            {
                for (int i = 0; i < Cards.Count; i++)
                {
                    if (Cards[i] == 11)
                    {
                        Cards[i] = 1;
                    }
                }
            }

            Score = 0;
            foreach (float f in Cards)
            {
                Score += f;
            }

            if (Dealer == true && Cards.Count < 3)
            {
                try
                {
                    Score -= Cards[1];
                }
                catch
                {
                    
                }
            }
        }
    }

    public class CardList<Float> : List<Float>
    {

        public event EventHandler OnAdd;

        public new void Add(Float item)
        {
            base.Add(item);
            if (null != OnAdd)
            {
                OnAdd(this, null);
            }
        }

    }
}
