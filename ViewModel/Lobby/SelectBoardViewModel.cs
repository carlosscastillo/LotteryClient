using Contracts.GameData;
using Lottery.Properties.Langs;
using Lottery.ViewModel.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Lottery.ViewModel.Lobby
{
    public class BoardItemViewModel : BaseViewModel
    {
        public int BoardId { get; set; }
        public string BoardName { get; set; }
        public List<int> CardIds { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }
    }

    public class SelectBoardViewModel : BaseViewModel
    {
        public ObservableCollection<BoardItemViewModel> AvailableBoards { get; set; }

        private BoardItemViewModel _selectedBoard;
        public BoardItemViewModel SelectedBoard
        {
            get => _selectedBoard;
            set
            {
                _selectedBoard = value;

                if (AvailableBoards != null)
                {
                    foreach (var board in AvailableBoards)
                    {
                        board.IsSelected = false;
                    }

                    if (_selectedBoard != null)
                    {
                        _selectedBoard.IsSelected = true;
                    }
                }

                OnPropertyChanged();
            }
        }

        public ICommand ConfirmSelectionCommand { get; set; }
        public System.Action<int> OnBoardSelected;

        public SelectBoardViewModel(int currentBoardId = 1)
        {
            AvailableBoards = new ObservableCollection<BoardItemViewModel>();
            LoadBoards();

            foreach (var board in AvailableBoards)
            {
                if (board.BoardId == currentBoardId)
                {
                    SelectedBoard = board;
                    break;
                }
            }

            ConfirmSelectionCommand = new RelayCommand(ConfirmSelection);
        }

        private void LoadBoards()
        {
            var configurations = BoardConfigurations.FixedBoards;

            foreach (var kvp in configurations)
            {
                AvailableBoards.Add(new BoardItemViewModel
                {
                    BoardId = kvp.Key,
                    BoardName = string.Format(Lang.SelectBoardLabelBoardIndividual, kvp.Key),
                    CardIds = kvp.Value,
                    IsSelected = false
                });
            }
        }

        private void ConfirmSelection(object obj)
        {
            if (SelectedBoard != null)
            {
                OnBoardSelected?.Invoke(SelectedBoard.BoardId);
            }
            else
            {
                MessageBox.Show("Por favor selecciona un tablero.");
            }
        }
    }
}