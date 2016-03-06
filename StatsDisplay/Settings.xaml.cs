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
using StatsFetcher;
using System.IO;
using System.Reflection;

namespace StatsDisplay
{
	/// <summary>
	/// Interaction logic for Settings.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		public Properties.Settings Settings { get { return Properties.Settings.Default; } }
		private Window currentWindow;
		private HotKey hotKey;


		public SettingsWindow()
		{
			InitializeComponent();
			var v = Assembly.GetExecutingAssembly().GetName().Version;
			Title = $"HotsStats v{v.Major}.{v.Minor}";
			if (Settings.SettingsWindowTop <= 0)
				WindowStartupLocation = WindowStartupLocation.CenterScreen;

			var mon = new FileMonitor();
			mon.BattleLobbyCreated += (o, e) => Dispatcher.BeginInvoke(new Action(() => { ProcessLobbyFile(e.Data); }));
			mon.RejoinFileCreated += (o,e) => Dispatcher.BeginInvoke(new Action(() => { ProcessRejoinFile(e.Data); }));
			mon.ReplayFileCreated += (o, e) => Dispatcher.BeginInvoke(new Action(() => { ProcessReplayFile(e.Data); }));
			mon.StartMonitoring();

			hotKey = new HotKey(Key.Tab, KeyModifier.Shift | KeyModifier.NoRepeat);
			hotKey.Pressed += (o, e) => {
				if (currentWindow != null)
					currentWindow.Visibility = currentWindow.IsVisible ? Visibility.Collapsed : Visibility.Visible;
			};

			Closing += (o, e) => {
				Settings.Save();
				Application.Current.Shutdown();
			};
		}

		private async Task ProcessLobbyFile(string path)
		{
			if (!Settings.Enabled)
				return;

			App.game = FileProcessor.ProcessLobbyFile(path);
			App.game.Me = App.game.Players.Where(p => p.BattleTag == Settings.BattleTag || p.Name == Settings.BattleTag).FirstOrDefault();

			currentWindow = new ShortStatsWindow();
			if (Settings.AutoShow)
				currentWindow.Show();
		}

		private void ProcessRejoinFile(string path)
		{
			FileProcessor.ProcessRejoin(path, App.game);
			currentWindow?.Close();
			currentWindow = new FullStatsWindow();
		}

		private void ProcessReplayFile(string path)
		{
			
		}

		private async void Test1_Click(object sender, RoutedEventArgs e)
		{
			await ProcessLobbyFile(@"replay.server.battlelobby");
		}

		private async void Test2_Click(object sender, RoutedEventArgs e)
		{
			ProcessRejoinFile(@"save.StormSave");
			currentWindow.Show();
		}
	}
}
