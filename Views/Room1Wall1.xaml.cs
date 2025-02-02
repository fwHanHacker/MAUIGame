namespace Games.Views;
using Games.Models;

public partial class Room1Wall1 : ContentPage
{
	private StackLayout _inventoryLayout;
	private BoxView _currentSelectedBackground;
	private Image _currentSelectedItem;

	public Room1Wall1()
	{
		InitializeComponent();
		
		// 添加页面背景的点击事件
		var backgroundTapGesture = new TapGestureRecognizer();
		backgroundTapGesture.Tapped += OnBackgroundTapped;
		RoomOneWallImage.GestureRecognizers.Add(backgroundTapGesture);
	}

	private void OnBackgroundTapped(object sender, EventArgs e)
	{
		// 点击背景时取消物品选中状态
		if (_currentSelectedBackground != null && _currentSelectedItem != null)
		{
			_currentSelectedBackground.IsVisible = false;
			_currentSelectedItem.Scale = 1.0;
			_currentSelectedBackground = null;
			_currentSelectedItem = null;
		}
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		
		// 设置初始透明度为0
		this.Opacity = 0;

		// 先检查血迹状态并设置显示状态
		bool isBloodOneCleaned = await App.GameStateRepo.GetStateAsync("IsBloodOneCleaned");
		BloodOne.IsVisible = !isBloodOneCleaned;
		NineSymbol.IsVisible = isBloodOneCleaned;

		bool isBloodThreeCleaned = await App.GameStateRepo.GetStateAsync("IsBloodThreeCleaned");
		BloodThree.IsVisible = !isBloodThreeCleaned;
		ThreeSymbol.IsVisible = isBloodThreeCleaned;

		bool isBloodFourCleaned = await App.GameStateRepo.GetStateAsync("IsBloodFourCleaned");
		BloodFour.IsVisible = !isBloodFourCleaned;
		OneSymbol.IsVisible = isBloodFourCleaned;

		bool isBloodHiddenCleaned = await App.GameStateRepo.GetStateAsync("IsBloodHiddenCleaned");
		BloodHidden.IsVisible = !isBloodHiddenCleaned;
		FourSymbol.IsVisible = isBloodHiddenCleaned;

		// 初始化物品栏
		if (_inventoryLayout == null)
		{
			_inventoryLayout = this.FindByName<StackLayout>("InventoryLayout");
		}

		// 清空物品栏显示
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

		// 从数据库加载物品
		var items = await App.InventoryRepo.GetInventoryItemsAsync();
		
		// 如果有任何画碎片，在物品栏显示
		var anyPictureFragment = items.FirstOrDefault(i => i.ItemName.Contains("picture_fragment"));
		if (anyPictureFragment != null)
		{
			UpdateInventorySlot(anyPictureFragment.SlotIndex, "sofa_picture_fragment");
		}

		// 加载其他非画碎片物品
		foreach (var item in items.Where(i => !i.ItemName.Contains("picture_fragment")))
		{
			if (item.IsCollected)
			{
				UpdateInventorySlot(item.SlotIndex, item.ItemName);
			}
		}

		// 最后才显示页面
		await this.FadeTo(1, 500);
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
			Source = itemName,
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
		if (string.IsNullOrEmpty(itemName) || itemName == "inventory.png")
			return;

		if (_currentSelectedBackground != null && _currentSelectedItem != null)
		{
			_currentSelectedBackground.IsVisible = false;
			_currentSelectedItem.Scale = 1.0;
		}

		whiteBackground.IsVisible = true;
		await inventoryItem.ScaleTo(1.2, 100);

		_currentSelectedBackground = whiteBackground;
		_currentSelectedItem = inventoryItem;

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
		}

		FragmentHintLabel.Text = itemInfo;
		FragmentHintLabel.Opacity = 0;
		FragmentHintLabel.IsVisible = true;

		await FragmentHintLabel.FadeTo(1, 500);
		await Task.Delay(1800);
		await FragmentHintLabel.FadeTo(0, 500);
		FragmentHintLabel.IsVisible = false;
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
            // 添加其他物品的描述...
            _ => "未知物品"
		};
	}

	public async void OnLeftButtonClicked(object sender, EventArgs e)
	{
		await this.FadeTo(0, 500);
		await Shell.Current.GoToAsync("//Room1Wall2");
	}

	public async void OnRightButtonClicked(object sender, EventArgs e)
	{
		await this.FadeTo(0, 500);
		await Shell.Current.GoToAsync("//Room1Wall3");
	}

	public async void OnSettingClicked(object sender, EventArgs e)
	{
		await App.SettingRepo.UpdateBackPage(1, "Room1Wall1");
		await Shell.Current.GoToAsync("//SettingPage");
	}

	public async void OnPaperImageClicked(object sender, EventArgs e)
	{
		await this.FadeTo(0, 500);
		await Shell.Current.GoToAsync("//PaperDisplayPage");
	}

	public async void OnPictureCornerClicked(object sender, EventArgs e)
	{
		await this.FadeTo(0, 500);
		await Shell.Current.GoToAsync("//PictureDisplayPage");
	}

	public async void OnBoxClicked(object sender, EventArgs e)
	{
		await this.FadeTo(0, 500);
		await Shell.Current.GoToAsync("//BoxRiddle");
	}
	public async void OnFrameClicked(object sender, EventArgs e)
	{
		await this.FadeTo(0, 500);
		await Shell.Current.GoToAsync("//PuzzlePiecesSlove");
	}

	public async void OnCabinettLeftClicked(object sender, EventArgs e)
	{
		await this.FadeTo(0, 500);
		await Shell.Current.GoToAsync("//CabinettLeftRiddle");
	}

	public async void OnCabinettRightClicked(object sender, EventArgs e)
	{
		await this.FadeTo(0, 500);
		await Shell.Current.GoToAsync("//CabinettRightRiddle");
	}

	private async void OnBloodOneClicked(object sender, EventArgs e)
	{
		// 检查是否选中了衣服的一角且血迹未被擦过
		if (_currentSelectedItem?.Source?.ToString()?.Contains("dishcloth") == true)
		{
			bool isBloodOneCleaned = await App.GameStateRepo.GetStateAsync("IsBloodOneCleaned");
			if (!isBloodOneCleaned)
			{
				BloodOne.IsVisible = false;
				NineSymbol.IsVisible = true;
				await App.GameStateRepo.SaveStateAsync("IsBloodOneCleaned", true);

				// 取消衣服的一角的选中状态
				if (_currentSelectedBackground != null && _currentSelectedItem != null)
				{
					_currentSelectedBackground.IsVisible = false;
					_currentSelectedItem.Scale = 1.0;
					_currentSelectedBackground = null;
					_currentSelectedItem = null;
				}
			}
		}
	}

	private async void OnBloodThreeClicked(object sender, EventArgs e)
	{
		if (_currentSelectedItem?.Source?.ToString()?.Contains("dishcloth") == true)
		{
			bool isBloodThreeCleaned = await App.GameStateRepo.GetStateAsync("IsBloodThreeCleaned");
			if (!isBloodThreeCleaned)
			{
				BloodThree.IsVisible = false;
				ThreeSymbol.IsVisible = true;
				await App.GameStateRepo.SaveStateAsync("IsBloodThreeCleaned", true);

				if (_currentSelectedBackground != null && _currentSelectedItem != null)
				{
					_currentSelectedBackground.IsVisible = false;
					_currentSelectedItem.Scale = 1.0;
					_currentSelectedBackground = null;
					_currentSelectedItem = null;
				}
			}
		}
	}

	private async void OnBloodFourClicked(object sender, EventArgs e)
	{
		if (_currentSelectedItem?.Source?.ToString()?.Contains("dishcloth") == true)
		{
			bool isBloodFourCleaned = await App.GameStateRepo.GetStateAsync("IsBloodFourCleaned");
			if (!isBloodFourCleaned)
			{
				BloodFour.IsVisible = false;
				OneSymbol.IsVisible = true;
				await App.GameStateRepo.SaveStateAsync("IsBloodFourCleaned", true);

				if (_currentSelectedBackground != null && _currentSelectedItem != null)
				{
					_currentSelectedBackground.IsVisible = false;
					_currentSelectedItem.Scale = 1.0;
					_currentSelectedBackground = null;
					_currentSelectedItem = null;
				}
			}
		}
	}

	private async void OnBloodHiddenClicked(object sender, EventArgs e)
	{
		if (_currentSelectedItem?.Source?.ToString()?.Contains("dishcloth") == true)
		{
			bool isBloodHiddenCleaned = await App.GameStateRepo.GetStateAsync("IsBloodHiddenCleaned");
			if (!isBloodHiddenCleaned)
			{
				BloodHidden.IsVisible = false;
				FourSymbol.IsVisible = true;
				await App.GameStateRepo.SaveStateAsync("IsBloodHiddenCleaned", true);

				if (_currentSelectedBackground != null && _currentSelectedItem != null)
				{
					_currentSelectedBackground.IsVisible = false;
					_currentSelectedItem.Scale = 1.0;
					_currentSelectedBackground = null;
					_currentSelectedItem = null;
				}
			}
		}
	}
}