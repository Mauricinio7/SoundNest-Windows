﻿using Microsoft.Extensions.DependencyInjection;
using SoundNest_Windows_Client.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SoundNest_Windows_Client.Views
{
    /// <summary>
    /// Lógica de interacción para SideBarView.xaml
    /// </summary>
    public partial class SideBarView : UserControl
    {
        public SideBarView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<SideBarViewModel>();
        }
    }
}
