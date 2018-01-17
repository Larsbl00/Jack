using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EV3MessengerLib;
using System.Windows.Threading;
using System.Threading;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Forms;

namespace NewestJack
{
    /// <summary>
    /// Interaction logic for PlayField.xaml
    /// </summary>
    public partial class PlayField
    {
        EV3Messenger ev3Messenger;
        DispatcherTimer MessageTimer = new DispatcherTimer();
        MainWindow MW;

        float val1 = 0;
        float val2 = 0;

        int playerTurn = 0;

        int BustedValue = 21;

        bool DealerTurn = false;
        Player Jack = new Player() { Name = "Jack", Score = 0 };

        public PlayField(List<Player> allPlayers, EV3Messenger ev3, MainWindow MainW)
        {
            InitializeComponent();

            this.Tag = 1;
            MW = MainW;

            ev3Messenger = ev3;

            lboxPlayers.ItemsSource = allPlayers;
            lboxPlayers.SelectedIndex = 0;

            MessageTimer.Interval = new TimeSpan(0,0,1);
            MessageTimer.Tick += MessageTimer_Tick;
            MessageTimer.Start();

        }

        private void MessageTimer_Tick(object sender, EventArgs e)
        {
            lboxPlayers.Items.Refresh();
            labPlayerName.Content = ((Player)lboxPlayers.Items[lboxPlayers.SelectedIndex]).Name;
            labPlayerScore.Content = ((Player)lboxPlayers.Items[lboxPlayers.SelectedIndex]).Score.ToString();

            labJackScore.Content = ((Player)lboxPlayers.Items[lboxPlayers.Items.Count - 1]).Score.ToString();
            //Get the incoming messages
            // Get the messages with 1 sec interval
            if (ev3Messenger.IsConnected)
            {
                EV3Message message = ev3Messenger.ReadMessage();
                if (message != null)
                {
                    if (message.MailboxTitle == "Finished")
                    {
                        if (message.ValueAsLogic)
                        {
                            this.IsEnabled = true;
                            this.Tag = "0";
                        }
                    }


                    if (message.MailboxTitle == "CardValue1")
                    {
                        val1 = message.ValueAsNumber;
                    }

                    if (message.MailboxTitle == "CardValue2" && val1 != 0)
                    {
                        #region get card value
                        val2 = message.ValueAsNumber;
                        string[] cardColors = new string[2];

                        float ActualValue = 0;
                        //Color values
                        int valueBlack = 0;
                        int valueBlue = 6;
                        int valueGreen = 11;
                        int valueRed = 16;

                        if (val1 >= valueBlack  && val1 < valueBlue)
                        {
                            cardColors[0] = "Black";
                        } // Black
                        else if (val1 >= valueBlue && val1 <valueGreen)
                        {
                            cardColors[0] = "Blue";
                        } // Blue
                        else if (val1 >= valueGreen && val1 < valueRed)
                        {
                            cardColors[0] = "Green";
                        } // Green
                        else if (val1 >= valueRed && val1 <= 100)
                        {
                            cardColors[0] = "Red";
                        } // Red

                        if (val2 >= valueBlack && val2 < valueBlue)
                        {
                            cardColors[1] = "Black";
                        } // Black
                        else if (val2 >= valueBlue && val2 < valueGreen)
                        {
                            cardColors[1] = "Blue";
                        } // Blue
                        else if (val2 >= valueGreen && val2 < valueRed)
                        {
                            cardColors[1] = "Green";
                        } // Green
                        else if (val2 >= valueRed && val2 <= 100)
                        {
                            cardColors[1] = "Red";
                        } // Red
                        

                        // Zwart : 0 - 5
                        // Blauw : 6 - 12
                        // Groen : 13 - 25
                        // Rood : 26-100

                        if (cardColors[0] == "Blue" && cardColors[1] == "Blue")
                        {
                            ActualValue = 11;
                        }
                        else if (cardColors[0] == "Green" && cardColors[1] == "Green")
                        {
                            ActualValue = 5;
                        }
                        else if (cardColors[0] == "Black" && cardColors[1] == "Black")
                        {
                            ActualValue = 8;
                        }
                        else if (cardColors[0] == "Red" && cardColors[1] == "Red")
                        {
                            ActualValue = 10;
                        }
                        else if (cardColors[0] == "Blue" && cardColors[1] == "Green" || cardColors[1] == "Blue" && cardColors[0] == "Green")
                        {
                            ActualValue = 2;
                        }
                        else if (cardColors[0] == "Black" && cardColors[1] == "Blue" || cardColors[1] == "Black" && cardColors[0] == "Blue")
                        {
                            ActualValue = 3;
                        }
                        else if (cardColors[0] == "Blue" && cardColors[1] == "Red" || cardColors[1] == "Blue" && cardColors[0] == "Red")
                        {
                            ActualValue = 4;
                        }
                        else if (cardColors[0] == "Black" && cardColors[1] == "Green" || cardColors[1] == "Black" && cardColors[0] == "Green")
                        {
                            ActualValue = 6;
                        }
                        else if (cardColors[0] == "Green" && cardColors[1] == "Red" || cardColors[1] == "Green" && cardColors[0] == "Red")
                        {
                            ActualValue = 7;
                        }
                        else if (cardColors[0] == "Black" && cardColors[1] == "Red" || cardColors[1] == "Black" && cardColors[0] == "Red")
                        {
                            ActualValue = 9;
                        }

                        //MessageBox.Show(ActualValue.ToString() + "\n 1:" + val1.ToString() + "\n 2:" + val2.ToString());

                        ev3Messenger.SendMessage("Gelezen", "True");

                        val1 = 0;
                        val2 = 0;

                        #endregion

                        if (this.Tag.ToString() == "1")
                        {
                            ((Player)lboxPlayers.Items[playerTurn]).Cards.Add(ActualValue);
                            if (playerTurn == lboxPlayers.Items.Count - 1)
                            {
                                playerTurn = 0;
                            }
                            else
                            {
                                playerTurn++;
                            }
                            lboxPlayers.Items.Refresh();
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            ((Player)lboxPlayers.Items[lboxPlayers.SelectedIndex]).Cards.Add(ActualValue);
                            lboxPlayers.Items.Refresh();

                            if (((Player)lboxPlayers.Items[lboxPlayers.SelectedIndex]).Dealer == true)
                            {
                                JacksTurn(((Player)lboxPlayers.Items[lboxPlayers.SelectedIndex]));
                            }
                        }

                        //read card in C#
                        ev3Messenger.SendMessage("Gelezen", "True");

                        if (((Player)lboxPlayers.Items[playerTurn]).Score >= 21)
                        {
                            butHit.IsEnabled = false;
                        }
                    }

                    if (message.MailboxTitle == "CardReady")
                    {

                    }
                }
            }
        }

        private void lboxPlayers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Player P = ((Player)lboxPlayers.Items[lboxPlayers.SelectedIndex]);
            labPlayerName.Content = P.Name;
            labPlayerScore.Content = P.Score;

            playerTurn = lboxPlayers.SelectedIndex;

            if (P.Dealer)
            {
                DealerTurn = true;

                Thread.Sleep(1000);
                //butHit_Click(butHit, new RoutedEventArgs());
                JacksTurn(P);
            }
            else
            {
                butHit.IsEnabled = true;
                butPass.IsEnabled = true;
            }
        }

        private void butHit_Click(object sender, RoutedEventArgs e)
        {
            ev3Messenger.SendMessage("Card", "False");
        }

        private void butPass_Click(object sender, RoutedEventArgs e)
        {
            if (((Player)lboxPlayers.Items[lboxPlayers.SelectedIndex]).Dealer == false)
            {
                butHit.IsEnabled = true;
            }
            ev3Messenger.SendMessage("Card", "True");
            lboxPlayers.SelectedIndex = lboxPlayers.SelectedIndex + 1;

        }

        private async void JacksTurn(Player P)
        {
            Player J = new Player() { Name = "jack" };

            foreach (Player p in lboxPlayers.Items)
            {
                if (p.Dealer == true)
                {
                    J = p;
                }
            }

            J.Cards.Add(0);

                butHit.IsEnabled = false;
            butPass.IsEnabled = false;

            if (P.Score < 17)
            {
                butHit_Click(butHit, new RoutedEventArgs());
                playerTurn = lboxPlayers.Items.IndexOf(P);
                //JacksTurn(P);
            }
            else
            {
                butPass_Click(butPass, new RoutedEventArgs());

                foreach (Player p in lboxPlayers.Items)
                {
                    if (p.Dealer == false)
                    {
                        if (p.Score > J.Score && p.Score <= BustedValue || J.Score > BustedValue && p.Score <= BustedValue)
                        {
                            ev3Messenger.SendMessage("Win", "True");
                            await this.ShowMessageAsync("Gefeliciteerd " + p.Name + "!", "Gefeliciteerd " + p.Name + ", Je hebt gewonnen van Jack! Zijn score was " + J.Score + ". En jouw score was " + p.Score);
                        }
                        else if (p.Score == J.Score)
                        {
                            ev3Messenger.SendMessage("Win", "False");

                            await this.ShowMessageAsync("Gelijkspel " + p.Name + "!", p.Name + ", Je hebt Gelijk gespeeld met Jack! Zijn score was " + J.Score + ". En jouw score was " + p.Score);
                        }
                        else if (p.Score < J.Score)
                        {
                            ev3Messenger.SendMessage("Win", "False");

                            await this.ShowMessageAsync("Awww " + p.Name + "...", p.Name + ", Je verloren van Jack! Zijn score was " + J.Score + ". En jouw score was " + p.Score);
                        }
                        else
                        {
                            ev3Messenger.SendMessage("Win", "False");

                            await this.ShowMessageAsync(p.Name + " you got busted!", p.Name + ", Je verloren van Jack want je bent over 21 gegaan! Zijn score was " + J.Score + ". En jouw score was " + p.Score);

                        }
                    }
                }
                System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
                System.Windows.Application.Current.Shutdown();
            }
        }

        public void Dispose()
        {
            this.Dispose();
        }
    }
}
