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


		public SettingsWindow()
		{
			InitializeComponent();
			var v = Assembly.GetExecutingAssembly().GetName().Version;
			Title = $"HotsStats v{v.Major}.{v.Minor}";
			if (Settings.SettingsWindowTop <= 0)
				WindowStartupLocation = WindowStartupLocation.CenterScreen;
			var mon = new FileMonitor();
			mon.BattleLobbyCreated += (o, e) => Dispatcher.BeginInvoke(new Action(() => { ProcessLobbyFile(e.Data); }));
			mon.StartWatchingForLobby();
			Closing += (o, e) => Settings.Save();
		}

		private async void ProcessLobbyFile(string path)
		{
			if (!Settings.Enabled)
				return;

			App.game = await FileProcessor.ProcessLobbyFile(path);
			App.game.Me = App.game.Players.Where(p => p.BattleTag == Settings.BattleTag || p.Name == Settings.BattleTag).FirstOrDefault();

			new ShortStatsWindow().Show();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			ProcessLobbyFile(@"replay.server.battlelobby");
		}
	}
}
