using System.Net;
using System.Windows.Input;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Audio;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Audio.Managers;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Network;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Network.DCS;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Singletons;
using Ciribob.DCS.SimpleRadio.Standalone.Client.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.ViewModels;

public interface IMainViewModel
{
	public AudioManager AudioManager { get; }
	public AudioPreview AudioPreview { get; set; }
	public AudioInputSingleton AudioInput { get; }
	public AudioOutputSingleton AudioOutput { get; }

	public ClientStateSingleton ClientState { get; }
	public ConnectedClientsSingleton Clients { get; set; }
	public SRSClientSyncHandler Client { get; }
	public DCSAutoConnectHandler DcsAutoConnectListener { get; set; }
	public ServerAddress ServerAddress { get; set; }
	public IPAddress ResolvedIp { get;  }
	public string Guid { get;  }
	public int Port { get;  }
	
	public SyncedServerSettings ServerSettings { get; }

	ISrsSettings SrsSettings { get; set; }
	
	IRelayCommand<bool> StopCommand { get; }
	IRelayCommand ConnectCommand { get; }

}