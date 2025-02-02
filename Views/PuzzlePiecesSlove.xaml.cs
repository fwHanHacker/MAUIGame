using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Shapes;
using Games.Models;

namespace Games.Views;

public partial class PuzzlePiecesSlove : ContentPage
{
	private StackLayout _inventoryLayout;
	private BoxView _currentSelectedBackground;
	private Image _currentSelectedItem;
	private int _availableFragments = 0;
	private int _usedPieces = 0;
	private Image _backgroundImage;
	private bool _isFragmentSelected = false;

	public PuzzlePiecesSlove()
	{
		InitializeComponent();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		this.Opacity = 0;

		try
		{
			_usedPieces = 0;
			_isFragmentSelected = false;
			_currentSelectedBackground = null;
			_currentSelectedItem = null;

			ResetAllPieces();

			if (_backgroundImage != null)
			{
				_backgroundImage.Source = "riddlebackground.png";
			}

			if (_inventoryLayout == null)
			{
				_inventoryLayout = this.FindByName<StackLayout>("InventoryLayout");
			}

			if (_inventoryLayout == null)
			{
				Console.WriteLine("Error: InventoryLayout not found");
				return;
			}

			_backgroundImage = this.FindByName<Image>("BackgroundImage");

			for (int i = 0; i < _inventoryLayout.Children.Count; i++)
			{
				_inventoryLayout.Children[i] = new Image
				{
					Source = "inventory.png",
					Aspect = Aspect.AspectFill,
					WidthRequest = 50,
					HeightRequest = 50
				};
			}

			var items = await App.InventoryRepo.GetInventoryItemsAsync();
			
			_availableFragments = items.Count(i => i.ItemName.Contains("picture_fragment"));
			
			var anyPictureFragment = items.FirstOrDefault(i => i.ItemName.Contains("picture_fragment"));
			if (anyPictureFragment != null)
			{
				UpdateInventorySlot(anyPictureFragment.SlotIndex, "sofa_picture_fragment");
			}

			foreach (var item in items.Where(i => !i.ItemName.Contains("picture_fragment")))
			{
				if (item.IsCollected)
				{
					UpdateInventorySlot(item.SlotIndex, item.ItemName);
				}
			}

			await this.FadeTo(1, 500);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error in OnAppearing: {ex.Message}");
		}
	}

	private void ResetAllPieces()
	{
		var pieces = new[] { "Piece1", "Piece2", "Piece3", "Piece5", "Piece6" };
		foreach (var pieceName in pieces)
		{
			var piece = this.FindByName<Rectangle>(pieceName);
			if (piece != null)
			{
				piece.Fill = new SolidColorBrush(Colors.Black);
			}
		}
	}

	private void UpdateInventorySlot(int slotIndex, string itemName)
	{
		if (slotIndex < 0 || slotIndex >= _inventoryLayout.Children.Count)
			return;

		var grid = new Grid
		{
			WidthRequest = 50,
			HeightRequest = 50
		};

		var originalInventory = new Image
		{
			Source = "inventory.png",
			Aspect = Aspect.AspectFill,
			WidthRequest = 50,
			HeightRequest = 50
		};

		var inventoryItem = new Image
		{
			Source = itemName + ".png",
			WidthRequest = 40,
			HeightRequest = 40,
			Aspect = Aspect.AspectFill,
			Margin = new Thickness(5)
		};

		var whiteBackground = new BoxView
		{
			Color = Colors.White,
			IsVisible = false,
			WidthRequest = 50,
			HeightRequest = 50
		};

		grid.Children.Add(originalInventory);
		grid.Children.Add(whiteBackground);
		grid.Children.Add(inventoryItem);

		var tapGestureRecognizer = new TapGestureRecognizer();
		tapGestureRecognizer.Tapped += async (s, e) => 
		{
			await OnInventoryItemClicked(grid, itemName, whiteBackground, inventoryItem);
		};
		grid.GestureRecognizers.Add(tapGestureRecognizer);

		_inventoryLayout.Children[slotIndex] = grid;
	}

	private async Task OnInventoryItemClicked(Grid grid, string itemName, BoxView whiteBackground, Image inventoryItem)
	{
		try
		{
			if (_currentSelectedBackground != null && _currentSelectedItem != null)
			{
				_currentSelectedBackground.IsVisible = false;
				_currentSelectedItem.Scale = 1.0;
			}

			whiteBackground.IsVisible = true;
			await inventoryItem.ScaleTo(1.2, 100);

			_currentSelectedBackground = whiteBackground;
			_currentSelectedItem = inventoryItem;

			_isFragmentSelected = itemName.Contains("picture_fragment");

			string itemInfo;
			if (itemName.Contains("picture_fragment"))
			{
				var items = await App.InventoryRepo.GetInventoryItemsAsync();
				int fragmentCount = items.Count(i => i.ItemName.Contains("picture_fragment"));
				itemInfo = $"画碎片 {fragmentCount}/5";
			}
			else
			{
				itemInfo = GetItemInfo(itemName);
				_isFragmentSelected = false;
			}

			FragmentHintLabel.Text = itemInfo;
			FragmentHintLabel.Opacity = 0;
			FragmentHintLabel.IsVisible = true;

			await FragmentHintLabel.FadeTo(1, 500);
			await Task.Delay(1800);
			await FragmentHintLabel.FadeTo(0, 500);
			FragmentHintLabel.IsVisible = false;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error in OnInventoryItemClicked: {ex.Message}");
		}
	}

	private string GetItemInfo(string itemName)
	{
		return itemName switch
		{
            "torch" => "一个紫外线手电筒",
            "knife" => "一把小刀",
            "dishcloth" => "衣服的一角",
            "battery" => "一节电池",
            "blue_and_yellow_ball" => "一个蓝色的球和一个黄色的球",
			_ => "未知物品"
		};
	}

	private void OnBackgroundTapped(object sender, EventArgs e)
	{
		if (_currentSelectedBackground != null && _currentSelectedItem != null)
		{
			_currentSelectedBackground.IsVisible = false;
			_currentSelectedItem.Scale = 1.0;
			_currentSelectedBackground = null;
			_currentSelectedItem = null;
			_isFragmentSelected = false;
		}
	}

	private async void HandlePieceClick(Rectangle rect)
	{
		if (_isFragmentSelected && _availableFragments > 0 && rect.Fill is SolidColorBrush brush && brush.Color == Colors.Black)
		{
			rect.Fill = Brush.Transparent;
			_availableFragments--;
			_usedPieces++;

			if (_currentSelectedBackground != null)
			{
				_currentSelectedBackground.IsVisible = false;
			}
			if (_currentSelectedItem != null)
			{
				_currentSelectedItem.Scale = 1.0;
			}
			_currentSelectedBackground = null;
			_currentSelectedItem = null;
			_isFragmentSelected = false;

			if (_usedPieces == 5)
			{
				await Task.Delay(500);
				var images = this.GetVisualTreeDescendants().OfType<Image>();
				var brokenPaintingsImage = images.FirstOrDefault(img => 
					img.Source is FileImageSource source && 
					source.File == "broken_paintings.png");
				
				if (brokenPaintingsImage != null)
				{
					var hintImage = new Image
					{
						Source = "book_shelf_move_hint.png",
						Aspect = brokenPaintingsImage.Aspect,
						HorizontalOptions = brokenPaintingsImage.HorizontalOptions,
						VerticalOptions = brokenPaintingsImage.VerticalOptions,
						Opacity = 0
					};

					var parent = brokenPaintingsImage.Parent as Layout;
					if (parent != null)
					{
						parent.Add(hintImage);
						if (parent is Grid)
						{
							Grid.SetRow(hintImage, Grid.GetRow(brokenPaintingsImage));
							Grid.SetColumn(hintImage, Grid.GetColumn(brokenPaintingsImage));
						}

						await Task.WhenAll(
							brokenPaintingsImage.FadeTo(0, 500),
							hintImage.FadeTo(1, 500)
						);

						parent.Remove(brokenPaintingsImage);
					}
				}
			}
		}
	}

	public void OnPiece1Clicked(object sender, EventArgs e)
	{
		HandlePieceClick((Rectangle)sender);
	}

	public void OnPiece2Clicked(object sender, EventArgs e)
	{
		HandlePieceClick((Rectangle)sender);
	}

	public void OnPiece3Clicked(object sender, EventArgs e)
	{
		HandlePieceClick((Rectangle)sender);
	}

	public void OnPiece5Clicked(object sender, EventArgs e)
	{
		HandlePieceClick((Rectangle)sender);
	}

	public void OnPiece6Clicked(object sender, EventArgs e)
	{
		HandlePieceClick((Rectangle)sender);
	}

	public async void OnBackClicked(object sender, EventArgs e)
	{
		await this.FadeTo(0, 500);
		await Shell.Current.GoToAsync("//Room1Wall1");
	}

	public async void OnSettingClicked(object sender, EventArgs e)
	{
		await App.SettingRepo.UpdateBackPage(1, "PuzzlePiecesSlove");
		await Shell.Current.GoToAsync("//SettingPage");
	}
}