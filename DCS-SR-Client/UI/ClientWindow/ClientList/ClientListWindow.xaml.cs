﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Singletons;
using Ciribob.DCS.SimpleRadio.Standalone.Common.Network;
using MahApps.Metro.Controls;
using NLog;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.UI.ClientWindow.ClientList;

/// <summary>
///     Interaction logic for ClientListWindow.xaml
/// </summary>
public partial class ClientListWindow : MetroWindow
{
	private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

	private readonly ObservableCollection<SRClient> _clientList = new();
	private readonly DispatcherTimer _updateTimer;


	public ClientListWindow()
	{
		InitializeComponent();
		ClientList.ItemsSource = _clientList;
		UpdateList();

		_updateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
		_updateTimer.Tick += UpdateTimer_Tick;
		_updateTimer.Start();
	}

	private void UpdateList()
	{
		_clientList.Clear();

		//first create temporary list to sort
		var tempList = new List<SRClient>();


		foreach (var srClient in ConnectedClientsSingleton.Instance.Values) tempList.Add(srClient);

		foreach (var clientListModel in tempList.OrderByDescending(model => model.Coalition)
			         .ThenBy(model => model.Name.ToLower()).ToList())
			_clientList.Add(clientListModel);
	}


	private void UpdateTimer_Tick(object sender, EventArgs e)
	{
		try
		{
			UpdateList();
		}
		catch (Exception)
		{
		}
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		base.OnClosing(e);

		_updateTimer?.Stop();
	}
}