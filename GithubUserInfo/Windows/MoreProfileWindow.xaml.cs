﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using MahApps.Metro.Controls;

namespace GithubUserInfo.Windows
{
    /// <summary>
    /// MoreProfileWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MoreProfileWindow : MetroWindow
    {
        public MoreProfileWindow()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(e.Uri.AbsoluteUri);
            }catch(Exception err)
            {
                MessageBox.Show(err.ToString());
            }
            e.Handled = true;
        }
    }
}
