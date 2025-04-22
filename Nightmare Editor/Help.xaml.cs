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
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Diagnostics;
using MdXaml;

namespace Nightmare_Editor
{
    /// <summary>
    /// Interaction logic for Help.xaml
    /// </summary>
    public partial class Help : Window
    {
        public Help()
        {
            InitializeComponent();
            MD.Markdown = QuickRead("Help/Int.md");
            ScrollToTop();
        }

        private void Int_Click(object sender, MouseButtonEventArgs e)
        {
            MD.Markdown = QuickRead("Help/Int.md");
            ScrollToTop();
        }

        private void Add_Click(object sender, MouseButtonEventArgs e)
        {
            MD.Markdown = QuickRead("Help/Add.md");
            ScrollToTop();
        }

        private void Usr_Click(object sender, MouseButtonEventArgs e)
        {
            MD.Markdown = QuickRead("Help/Usr.md");
            ScrollToTop();
        }

        private void Tkt_Click(object sender, MouseButtonEventArgs e)
        {
            MD.Markdown = QuickRead("Help/Tkt.md");
            ScrollToTop();
        }

        private void Lnk_Click(object sender, MouseButtonEventArgs e)
        {
            MD.Markdown = QuickRead("Help/Lnk.md");
            ScrollToTop();
        }

        private void For_Click(object sender, MouseButtonEventArgs e)
        {
            MD.Markdown = QuickRead("Help/For.md");
            ScrollToTop();
        }

        private void Pak_Click(object sender, MouseButtonEventArgs e)
        {
            MD.Markdown = QuickRead("Help/Pak.md");
            ScrollToTop();
        }

        private void Mod_Click(object sender, MouseButtonEventArgs e)
        {
            MD.Markdown = QuickRead("Help/Mod.md");
            ScrollToTop();
        }

        private void Qrk_Click(object sender, MouseButtonEventArgs e)
        {
            MD.Markdown = QuickRead("Help/Qrk.md");
            ScrollToTop();
        }

        private void Faq_Click(object sender, MouseButtonEventArgs e)
        {
            MD.Markdown = QuickRead("Help/Faq.md");
            ScrollToTop();
        }

        private string QuickRead(string sender)
        {
            var uri = new Uri($"pack://application:,,,/{sender}");
            var streamInfo = Application.GetResourceStream(uri);
            using (var reader = new StreamReader(streamInfo.Stream))
            {
                return reader.ReadToEnd();
            }
        }

        private void ScrollToTop()
        {
            var scrollViewer = FindDescendant<ScrollViewer>(MD);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToTop();
            }
        }
        private T? FindDescendant<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);

                if (child is T target)
                    return target;

                var descendant = FindDescendant<T>(child);
                if (descendant != null)
                    return descendant;
            }
            return null;
        }
    }
}
