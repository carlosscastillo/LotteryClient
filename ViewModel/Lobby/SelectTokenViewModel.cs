using Lottery.Properties.Langs;
using Lottery.ViewModel.Base;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.Lobby
{
    public class TokenItemViewModel : BaseViewModel
    {
        public string Name 
        { 
            get; 
            set; 
        }

        public string Key 
        { 
            get; 
            set; 
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                SetProperty(ref _isSelected, value);
            }
        }
    }

    public class SelectTokenViewModel : BaseViewModel
    {
        public ObservableCollection<TokenItemViewModel> AvailableTokens 
        { 
            get; 
            set; 
        }

        private TokenItemViewModel _selectedTokenItem;
        public TokenItemViewModel SelectedTokenItem
        {
            get
            {
                return _selectedTokenItem;
            }
            set
            {
                _selectedTokenItem = value;

                if (AvailableTokens != null)
                {
                    foreach (TokenItemViewModel item in AvailableTokens)
                    {
                        item.IsSelected = false;
                    }

                    if (value != null)
                    {
                        value.IsSelected = true;
                    }
                }

                OnPropertyChanged();
            }
        }

        public ICommand ConfirmSelectionCommand 
        { 
            get; 
            set; 
        }

        public System.Action<string> OnTokenSelected;

        public SelectTokenViewModel(string currentTokenKey)
        {
            AvailableTokens = new ObservableCollection<TokenItemViewModel>
            {
                new TokenItemViewModel 
                { 
                    Name = Lang.SelectTokenLabelMarkersBeans, 
                    Key = "beans" 
                },
                new TokenItemViewModel 
                { 
                    Name = Lang.SelectTokenLabelMarkersBottleCaps, 
                    Key = "bottle_caps" 
                },
                new TokenItemViewModel 
                { 
                    Name = Lang.SelectTokenLabelMarkersPous, 
                    Key = "pou" 
                },
                new TokenItemViewModel 
                { 
                    Name = Lang.SelectTokenLabelMarkersCorn, 
                    Key = "corn" 
                },
                new TokenItemViewModel 
                { 
                    Name = Lang.SelectTokenLabelMarkersCoins, 
                    Key = "coins" 
                }
            };

            foreach (TokenItemViewModel token in AvailableTokens)
            {
                if (token.Key == currentTokenKey)
                {
                    token.IsSelected = true;
                    SelectedTokenItem = token;
                    break;
                }
            }

            ConfirmSelectionCommand = new RelayCommand(ConfirmSelection);
        }

        private void ConfirmSelection(object obj)
        {
            if (SelectedTokenItem != null)
            {
                OnTokenSelected?.Invoke(SelectedTokenItem.Key);
            }
            else
            {
                MessageBox.Show("Por favor selecciona una ficha.");
            }
        }
    }
}