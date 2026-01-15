using Contracts.GameData;
using Lottery.Properties.Langs;
using Lottery.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            set => SetProperty(ref _isSelected, value);
        }

        private bool _isOccupied;
        public bool IsOccupied
        {
            get => _isOccupied;
            set => SetProperty(ref _isOccupied, value);
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
                if (value == null || value.IsOccupied)
                    return;
                
                foreach (var board in AvailableBoards)
                    board.IsSelected = false;
                
                if (SetProperty(ref _selectedBoard, value))
                {
                    _selectedBoard.IsSelected = true;
                }
            }
        }

        public ICommand ConfirmSelectionCommand { get; set; }
        public Action<int> OnBoardSelected;

        public SelectBoardViewModel(int currentBoardId, List<int> occupiedBoards = null)
        {
            AvailableBoards = new ObservableCollection<BoardItemViewModel>();
            var configurations = BoardConfigurations.GetAllBoards();

            foreach (var kvp in configurations)
            {
                var isOccupied = occupiedBoards != null && occupiedBoards.Contains(kvp.Key);

                var boardItem = new BoardItemViewModel
                {
                    BoardId = kvp.Key,
                    BoardName = string.Format(Lang.SelectBoardLabelBoardIndividual, kvp.Key),
                    CardIds = kvp.Value,
                    IsSelected = kvp.Key == currentBoardId,
                    IsOccupied = isOccupied
                };

                AvailableBoards.Add(boardItem);
            }

            SelectedBoard = AvailableBoards.FirstOrDefault(b => b.BoardId == currentBoardId);

            ConfirmSelectionCommand = new RelayCommand(ConfirmSelection);
        }

        private void ConfirmSelection()
        {
            if (SelectedBoard != null && !SelectedBoard.IsOccupied)
            {
                OnBoardSelected?.Invoke(SelectedBoard.BoardId);
            }
            else
            {
                MessageBox.Show(
                    Lang.SelectBoardMessageSelectAvailable ?? "Por favor selecciona un tablero disponible.",
                    Lang.SelectBoardTitleError ?? "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }
        
        public void UpdateOccupiedBoards(List<int> occupiedBoards, int currentBoardId)
        {
            foreach (var board in AvailableBoards)
            {               
                board.IsOccupied = occupiedBoards.Contains(board.BoardId);
            }
            
            if (SelectedBoard != null && SelectedBoard.IsOccupied)
            {
                SelectedBoard.IsSelected = false;
                SelectedBoard = null;
            }
        }
    }
}