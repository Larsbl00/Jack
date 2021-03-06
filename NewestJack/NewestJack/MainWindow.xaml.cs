﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.IO.Ports;
using EV3MessengerLib;
using WebEye.Controls.Wpf;
using System.Drawing;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System.Windows.Threading;

namespace NewestJack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        List<Player> Allplayers = new List<Player>();
        private EV3Messenger ev3Messenger = new EV3Messenger();



        DispatcherTimer refreshTimer = new DispatcherTimer();


        bool players = false;
        bool webcam = false;
        bool bluetooth = false;

        PlayField playField;

        public MainWindow()
        {
            InitializeComponent();

            FillComPorts();

            refreshTimer.Interval = new TimeSpan(0, 0, 1);
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();


        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            foreach (ListBox lb in playerCards.Children)
            {
                lb.Items.Refresh();
            }
        }

        #region Bluetooth Settings
        private void FillComPorts()
        {
            String[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);

            comboComport.Items.Clear();
            foreach (String port in ports)
            {
                comboComport.Items.Add(port);
            }
        }

        private void comboComport_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String port = comboComport.SelectedValue.ToString();
            if (ev3Messenger.Connect(port))
            {
                bluetooth = true;
                ReadyToStart();
            }
            else
            {
                butAdd.IsEnabled = false;
                this.ShowMessageAsync("Hmmm.....", "Kon niet connecten met Jack op '" + port + "', zit Jack niet op een andere port? ");
            }
        }


        #endregion
        
        //Spelers toevoegen
        private async void butAdd_Click(object sender, RoutedEventArgs e)
        {
            if (tbxName.Text != "")
            {
                Player P = new Player() { Name = tbxName.Text, Score = 0 };
                lboxPlayers.Items.Add(P);
                Allplayers.Add(P);

                tbxName.Text = "";

                players = true;
                if (lboxPlayers.Items.Count == 6)
                {
                    tbxName.IsEnabled = false;
                    await this.ShowMessageAsync("Hmmm.....", "Top, je zit met een vol team. Meer kunnen er niet.");
                }
            }
            else
            {
                //Als er geen naam is ingevuld
                await this.ShowMessageAsync("Hmmm.....", "Misschien is het wel handig om een naam in te vullen");

            }
        }

        //Check of de game klaar is om te starten
        private void ReadyToStart()
        {
            butStart.IsEnabled = true;
            if (players && bluetooth && webcam)
            {
                butStart.IsEnabled = true;

                ev3Messenger.SendMessage("Players", (lboxPlayers.Items.Count - 1).ToString());
            }
        }

        private void lboxPlayers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            lboxPlayers.Items.RemoveAt(lboxPlayers.SelectedIndex);
        }

        private void butStart_Click(object sender, RoutedEventArgs e)
        {
            //connection check
            if (ev3Messenger.IsConnected)
            {
                Allplayers.Add(new Player() { Name="Jack", Dealer = true });
                ev3Messenger.SendMessage("Players", (Allplayers.Count - 1).ToString());
                playField = new PlayField(Allplayers, ev3Messenger, this);
                playField.Show();

                foreach (Player P in Allplayers)
                {
                    ListBox lb1 = new ListBox() { ItemsSource = P.Cards };
                    lb1.FontSize = 20;
                    lb1.Tag = P;
                    playerCards.Children.Add(lb1);

                    lb1.MouseDoubleClick += Lb1_MouseDoubleClick;
                }
            }
            else
            {
                //Try to reconnect
                try
                {
                    ev3Messenger.SendMessage("Players", (Allplayers.Count -1).ToString());
                    playField = new PlayField(Allplayers, ev3Messenger, this);
                    playField.Show();
                }
                catch
                {

                    //this.ShowMessageAsync("Hmmm.....", "Er ging iets mis met de connectie, haal een van de medewerkers er bij om de bluetooth te controlleren");
                }
            }


        }

        private void Lb1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox list = (ListBox)sender;
            Player P = (Player)list.Tag;

            try
            {
                string temp = Microsoft.VisualBasic.Interaction.InputBox("Voer een nieuwe value in voor de nieuwe kaar", "Edit card value", "0");
                float newValue = float.Parse(temp);


                P.Cards[list.SelectedIndex] = 0;
                P.Cards.Add(newValue);

                foreach (ListBox lb in playerCards.Children)
                {
                    lb.Items.Refresh();
                }
            }
            catch
            {
                MessageBox.Show("Please try again");

            }
        }

        private void STOP_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                ev3Messenger.SendMessage("ForceStop", "True");

                System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
                System.Windows.Application.Current.Shutdown();
                //playField.Close();
                //playField.Dispose();
                //Allplayers.Clear();
                //lboxPlayers.Items.Clear();
            }
            catch
            {

            }
        }
    }
}
