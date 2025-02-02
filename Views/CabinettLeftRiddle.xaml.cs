namespace Games.Views;
using Games.Models;

public partial class CabinettLeftRiddle : ContentPage
{
	private bool isNavigating = false;
	private int crackClickCount = 0;
	private StackLayout _inventoryLayout;
	private BoxView _currentSelectedBackground;
	private Image _currentSelectedItem;
	private bool _hasPictureFragment = false;

	public CabinettLeftRiddle()
	{
		InitializeComponent();
		
		// 添加页面背景的点击事件
		var backgroundTapGesture = new TapGestureRecognizer();
		backgroundTapGesture.Tapped += OnBackgroundTapped;
		RoomOneWallImage.GestureRecognizers.Add(backgroundTapGesture);
	}

	private void OnBackgroundTapped(object sender, EventArgs e)
	{
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
		this.Opacity = 0;
		isNavigating = false;
		
		// 检查柜子是否已经被打开过
		bool isCabinetBroken = await App.GameStateRepo.GetStateAsync("IsCabinetBroken");
		if (isCabinetBroken)
		{
			crackClickCount = 5;
			CabinetImage.Source = "cabinettleft.jpg";
		}
		else
		{
			crackClickCount = 0;
			CabinetImage.Source = "crack1.jpeg";
		}

		_hasPictureFragment = false;

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
		
		// 检查当前房间的画碎片是否已被收集
		var cabinetPictureFragment = await App.InventoryRepo.GetInventoryItemAsync("cabinet_picture_fragment");
		if (cabinetPictureFragment != null && cabinetPictureFragment.IsCollected)
		{
			_hasPictureFragment = true;
		}

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

	public async void OnBackClicked(object sender, EventArgs e)
	{
		if (!isNavigating && this.Opacity > 0)
		{
			isNavigating = true;
			await this.FadeTo(0, 500);
			await Shell.Current.GoToAsync("//Room1Wall1");
		}
	}

	public async void OnSettingClicked(object sender, EventArgs e)
	{
		if (!isNavigating && this.Opacity > 0)
		{
			isNavigating = true;
			await App.SettingRepo.UpdateBackPage(1, "CabinettLeftRiddle");
			await Shell.Current.GoToAsync("//SettingPage");
		}
	}

	public async void OnCrackClicked(object sender, EventArgs e)
	{
		if (!isNavigating && this.Opacity > 0)
		{
			// 检查柜子是否已经被打开过
			bool isCabinetBroken = await App.GameStateRepo.GetStateAsync("IsCabinetBroken");
			if (isCabinetBroken)
			{
				return; // 如果已经打开过，直接返回
			}

			crackClickCount++;
			
			switch (crackClickCount)
			{
				case 1:
					CabinetImage.Source = "crack2.jpeg";
					break;
				case 2:
					CabinetImage.Source = "crack3.jpeg";
					break;
				case 3:
					CabinetImage.Source = "crack4.jpeg";
					break;
				case 4:
					CabinetImage.Source = "crack5.jpeg";
					break;
				case 5:
					if (!_hasPictureFragment && _inventoryLayout != null)
					{
						// 将柜子图片改回原样
						CabinetImage.Source = "cabinettleft.jpg";
						
						// 记录柜子已被打开的状态
						await App.GameStateRepo.SaveStateAsync("IsCabinetBroken", true);

						int slotIndex;
						// 先检查是否有任何画碎片
						var anyPictureFragment = (await App.InventoryRepo.GetInventoryItemsAsync())
							.FirstOrDefault(i => i.ItemName.Contains("picture_fragment"));
							
						if (anyPictureFragment != null)
						{
							slotIndex = anyPictureFragment.SlotIndex;
						}
						else
						{
							// 找到第一个空的物品栏位置
							slotIndex = -1;
							for (int i = 0; i < _inventoryLayout.Children.Count; i++)
							{
								if (_inventoryLayout.Children[i] is Image slot && 
									slot.Source.ToString().EndsWith("inventory.png"))
								{
									slotIndex = i;
									break;
								}
							}
						}

						if (slotIndex != -1)
						{
							// 保存到数据库
							var newItem = new InventoryItem
							{
								ItemName = "cabinet_picture_fragment",
								SlotIndex = slotIndex,
								IsCollected = true
							};
							await App.InventoryRepo.SaveInventoryItemAsync(newItem);

							// 更新UI和状态
							UpdateInventorySlot(slotIndex, "sofa_picture_fragment"); // 使用相同的图片
							_hasPictureFragment = true;

							// 显示提示文本和动画效果
							var items = await App.InventoryRepo.GetInventoryItemsAsync();
							int fragmentCount = items.Count(i => i.ItemName.Contains("picture_fragment"));
							FragmentHintLabel.Text = $"画碎片 {fragmentCount}/5";
							FragmentHintLabel.Opacity = 0;
							FragmentHintLabel.IsVisible = true;

							// 同时执行文本和物品栏动画
							if (_inventoryLayout.Children[slotIndex] is Grid grid && 
								grid.Children.LastOrDefault() is Image inventoryItem)
							{
								// 启动字幕淡入动画但不等待它完成
								FragmentHintLabel.FadeTo(1, 500);
								
								// 执行画碎片的放大缩小动画
								await inventoryItem.ScaleTo(1.2, 100);
								await inventoryItem.ScaleTo(1.0, 100);
								
								// 等待显示时间后淡出字幕
								await Task.Delay(1800);
								await FragmentHintLabel.FadeTo(0, 500);
							}

							FragmentHintLabel.IsVisible = false;
						}
					}
					break;
			}
		}
	}
}