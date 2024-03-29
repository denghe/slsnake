﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using SLSnake.Elements;
using SLSnake.Views;

namespace SLSnake
{
    public partial class Game : Page
    {
        public Game()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(Game_Loaded);
        }

        void Game_Loaded(object sender, RoutedEventArgs e)
        {
            var cgame = new CGame();
            cgame.Show();
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }
    }

}