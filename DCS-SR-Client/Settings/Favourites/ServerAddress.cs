using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.UI
{
    public partial class ServerAddress : ObservableObject
    {
        public ServerAddress(string name, string address, string eamCoalitionPassword, bool isDefault)
        {
            // Set private values directly so we don't trigger useless re-saving of favourites list when being loaded for the first time
            _name = name;
            _address = address;
            _eamCoalitionPassword = eamCoalitionPassword;
            IsDefault = isDefault; // Explicitly use property setter here since IsDefault change includes additional logic
        }

        [ObservableProperty] private string _name;

        [ObservableProperty] private string _address;

        [ObservableProperty] private string _eamCoalitionPassword;

        [ObservableProperty] private bool _isDefault;

        public string HostName
        {
            get
            {
                var addr = Address.Trim();
                if (addr.Contains(":")) { return addr.Split(':')[0]; }
                return addr;
            }
        }

        public int Port
        {
            get
            {
                var addr = Address.Trim();

                if (addr.Contains(":"))
                {
                    int port;
                    if (int.TryParse(addr.Split(':')[1], out port))
                    {
                        return port;
                    }
                    throw new ArgumentException("specified port is not valid");
                }

                return 5002;
            }
        }
    }
}