using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace audiopleer
{
    public partial class MainWindow : Window
    {
        private List<string> music = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            Thread thread = new Thread(ChangeSeconds);
            thread.Start();

            media.MediaEnded += media_MediaEnded;
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (media.NaturalDuration.HasTimeSpan)
            {
                TimerLeft.Text = FormatTime(media.NaturalDuration.TimeSpan - media.Position);
            }
        }

        private string FormatTime(TimeSpan timeSpan)
        {
            return string.Format("{0:D2}:{1:D2}", (int)timeSpan.TotalMinutes, timeSpan.Seconds);
        }

        private void media_MediaEnded(object sender, RoutedEventArgs e)
        {
            PlayNextSong();
        }

        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                string[] mus = Directory.GetFiles(dialog.FileName);
                foreach (string x in mus)
                {
                    string mp = x[x.Length - 3].ToString() + x[x.Length - 2].ToString() + x[x.Length - 1].ToString();
                    if (mp == "mp3" || mp == "mp4" || mp == "wav")
                    {
                        music.Add(x);
                    }
                }
                MediaList.Items.Clear();
                MediaList.ItemsSource = music;
                media.Source = new Uri(music[0]);
                media.Play();
            }
        }

        private void PlayNextSong()
        {
            var currentIndex = music.IndexOf(media.Source.LocalPath);
            var newIndex = currentIndex == music.Count - 1 ? 0 : currentIndex + 1;

            if (MediaList.SelectedIndex == MediaList.Items.Count - 1)
            {
                MediaList.SelectedIndex = 0;
            }
            else
            {
                MediaList.SelectedIndex += 1;
            }

            media.Source = new Uri(music[MediaList.SelectedIndex]);
            media.Play();
            Timer.Text = Path.GetFileNameWithoutExtension(music[MediaList.SelectedIndex]);
        }

        private void ChangeSeconds()
        {
            while (true)
            {
                Thread.Sleep(1000);
                Dispatcher.Invoke(() =>
                {
                    if (media.Source != null && media.NaturalDuration.HasTimeSpan && media.Position >= media.NaturalDuration.TimeSpan)
                    {
                        PlayNextSong();
                    }
                    else
                    {
                        TimerLeft.Text = media.Position.Minutes + ":" + media.Position.Seconds;
                        var currentPosition = media.Position.Ticks;
                        SliderMusic.Value = currentPosition;
                    }
                });

                Thread.Sleep(100);
            }
        }
        private void SliderMusic_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            media.Position = new TimeSpan(Convert.ToInt64(SliderMusic.Value));
            Timer.Text = media.Position.Minutes + ":" + media.Position.Seconds;
        }
        private void media_MediaOpened(object sender, RoutedEventArgs e)
        {
            SliderMusic.Maximum = media.NaturalDuration.TimeSpan.Ticks;
            Timer.Text = media.NaturalDuration.TimeSpan.Minutes + ":" + media.NaturalDuration.TimeSpan.Seconds;

            if (media.NaturalDuration.HasTimeSpan)
            {
                SliderMusic.Maximum = media.NaturalDuration.TimeSpan.Ticks;
                Timer.Text = media.NaturalDuration.TimeSpan.Minutes + ":" + media.NaturalDuration.TimeSpan.Seconds;
            }
        }
        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            media.Volume = 0.2;
            media.Volume = SliderVolume.Value;
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            media.Play();
        }
        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            media.Stop();
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            PlayNextSong();
        }


        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            var currentIndex = music.IndexOf(media.Source.LocalPath);
            var newIndex = currentIndex == 0 ? music.Count - 1 : currentIndex - 1;

            if (MediaList.SelectedIndex == 0)
            {
                MediaList.SelectedIndex = MediaList.Items.Count - 1;
            }
            else
            {
                MediaList.SelectedIndex -= 1;
            }

            media.Source = new Uri(music[MediaList.SelectedIndex]);
            media.Play();
            Timer.Text = Path.GetFileNameWithoutExtension(music[MediaList.SelectedIndex]);
        }
    }
}
