using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Audio.Managers;

public interface IAudioHandler
{
	AudioPreview AudioPreview { get; }
	AudioManager AudioManager { get; }
	
	public bool IsPreviewing { get; }
	public bool IsActive { get; }
	public bool IsMicAvailable { get; }

	public List<string> InputDevices { get; }
	public string CurrentInputDevice { get; }
	public List<string> OutputDevices { get; }
	public string CurrentOutputDevice { get; }
	public List<string> SideToneDevices { get; }
	public string CurrentSideToneDevice { get; }
	
	public float InputUV { get; }
	public float OutputUV { get; }
	
	IRelayCommand StartPreviewCommand { get; }
	IRelayCommand StopPreviewCommand { get; }
	IRelayCommand StartEncodingCommand { get; }
	IRelayCommand StopEncodingCommand { get; }
}