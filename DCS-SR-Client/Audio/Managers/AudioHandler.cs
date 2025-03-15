using System;
using System.Collections.Generic;
using System.Linq;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Settings;
using Ciribob.DCS.SimpleRadio.Standalone.Client.Singletons;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NLog;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Audio.Managers;

public partial class AudioHandler : ObservableObject, IAudioHandler
{
	private ISrsSettings SrsSettings { get; }
	public AudioInputSingleton AudioInput { get; }
	public AudioOutputSingleton AudioOutput { get; }

	private readonly Logger _logger = LogManager.GetCurrentClassLogger();
	
	[ObservableProperty] private AudioManager _audioManager;
	[ObservableProperty] private AudioPreview _audioPreview = new AudioPreview();

	public bool IsPreviewing { get; private set; } = false; // True when Preview is source.
	public bool IsActive { get; private set; } = false; // True when AudioManager is source.
	public bool IsMicAvailable { get; private set; } = false;
	
	public List<string> InputDevices
	{
		get
		{
			return AudioInput.InputAudioDevices.Select(x => x.Text).ToList();
		}
	}
	[ObservableProperty] private string _currentInputDevice;
	partial void OnCurrentInputDeviceChanged(string value)
	{
		SrsSettings.GlobalSettings.AudioInputDeviceId = value;
		WeakReferenceMessenger.Default.Send(new SettingChangingMessage());
	}

	public List<string> OutputDevices
	{
		get
		{
			return AudioOutput.OutputAudioDevices.Select(x => x.Text).ToList();
		}
	}
	[ObservableProperty] private string _currentOutputDevice;
	partial void OnCurrentOutputDeviceChanged(string value)
	{
		SrsSettings.GlobalSettings.AudioOutputDeviceId = value;
		WeakReferenceMessenger.Default.Send(new SettingChangingMessage());
	}

	public List<string> SideToneDevices
	{
		get
		{
			return AudioOutput.MicOutputAudioDevices.Select(x => x.Text).ToList();
		}
	}
	[ObservableProperty] private string _currentSideToneDevice;
	partial void OnCurrentSideToneDeviceChanged(string value)
	{
		SrsSettings.GlobalSettings.SideToneDeviceId = value;
		WeakReferenceMessenger.Default.Send(new SettingChangingMessage());
	}

	public float InputUV
	{
		get
		{
			if (AudioInput.MicrophoneAvailable)
			{
				if (IsActive) return AudioManager.MicMax;
				if (IsPreviewing) return AudioPreview.MicMax;
			}
			return 0;
		}
	}
	public float OutputUV
	{
		get
		{
			if (IsActive) return AudioManager.SpeakerMax;
			if (IsPreviewing) return AudioPreview.SpeakerMax;
			return 0;
		}
	}

	public AudioHandler()
	{
		SrsSettings = Ioc.Default.GetRequiredService<ISrsSettings>();
		AudioInput = Ioc.Default.GetRequiredService<AudioInputSingleton>();
		AudioOutput = Ioc.Default.GetRequiredService<AudioOutputSingleton>();
		
		CurrentInputDevice = SrsSettings.GlobalSettings.AudioInputDeviceId;
		CurrentOutputDevice = SrsSettings.GlobalSettings.AudioOutputDeviceId;
		CurrentSideToneDevice = SrsSettings.GlobalSettings.SideToneDeviceId;
		
		AudioManager = new AudioManager(AudioOutput.WindowsN);
		AudioManager.SpeakerBoost = (float)SrsSettings.GlobalSettings.SpeakerBoost;
	}

	[RelayCommand]
	private void StartPreview()
	{

		if (!AudioInput.MicrophoneAvailable) { return; }
		try
		{
			AudioPreview = new AudioPreview();
			AudioPreview.StartPreview(AudioOutput.WindowsN);
		}
		catch (Exception ex)
		{
			_logger.Error(ex, "Unable to preview audio - likely output device error - Pick another. Error:" + ex.Message);
		}
	}

	[RelayCommand]
	private void StopPreview()
	{
		AudioPreview.StopEncoding();
		AudioPreview = null;
	}

	[RelayCommand]
	private void StartEncoding()
	{
		
	}

	[RelayCommand]
	private void StopEncoding()
	{
		
	}
	
}
