﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace Xtv.Windows
{
    using Common.Parsers;
    using Xtv.Common;
    using Xtv.Common.Filters;

    public partial class MainWindow : Window
    {
        private TraceBox traceBox;
        private FlexibleTraceNode rootNode;

        public MainWindow()
        {
            InitializeComponent();
            var textChanged = ((EventHandler<TextChangedEventArgs>)SearchBox_TextChanged).Debounce(333);
            SearchBox.TextChanged += (s, e) => textChanged(s, e);

#if DEBUG
            try
            {
                openFile(@"C:\tmp\traces\trace.sample.xt");
            }
            catch { }
#endif
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text;
            if (!SearchBox.Dispatcher.CheckAccess())
            {
                text = SearchBox.Dispatcher.Invoke(() => SearchBox.Text);
            }
            else
            {
                text = SearchBox.Text;
            }
            rootNode.ApplyFilter(new CallNameFilter(text));
        }

        private void openFile(string filename)
        {
            var parser = Parser.ParseFile(filename);
            TraceViewer.Content = traceBox = new TraceBox(parser.RootTrace) { ProfileInfoVisible = ProfileButton.IsChecked ?? false };
            rootNode = new FlexibleTraceNode(parser.RootTrace) { UiNode = traceBox };
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var fd = new OpenFileDialog()
            {
                CheckFileExists = true,
                DefaultExt = "xt",
                Multiselect = false,
                Filter = "Xdebug Trace files (*.xt)|*.xt|All Files (*.*)|*.*"
            };
            if (fd.ShowDialog(this) ?? false)
            {
                try
                {
                    openFile(fd.FileName);
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load/parse the selected file! \r\nSome indication: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            var about = new AboutWindow();
            about.Owner = this;
            about.ShowDialog();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var rootTrace = TraceViewer.Content as TraceBox;
            if (rootTrace != null)
            {
                rootTrace.ProfileInfoVisible = !rootTrace.ProfileInfoVisible;
            }
        }
    }
}
