namespace Games.Views;
using Games.Models;

public partial class SafeDepositBox : ContentPage
{
	// 添加字段来存储当前的数字值
	private int number1 = 0;
	private int number2 = 0;
	private int number3 = 0;
	private int number4 = 0;
	private StackLayout _inventoryLayout;
	private BoxView _currentSelectedBackground;
	private Image _currentSelectedItem;

	public SafeDepositBox()
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
		this.Opacity = 0;

		try
		{
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

			await this.FadeTo(1, 500);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error in OnAppearing: {ex.Message}");
			this.Opacity = 1;
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
			// 添加其他物品的描述...
			_ => "未知物品"
		};
	}

	public async void OnBackClicked(object sender, EventArgs e)
	{
		await this.FadeTo(0, 500);
		await Shell.Current.GoToAsync("//Room1Wall2");
	}

	public async void OnSettingClicked(object sender, EventArgs e)
	{
		await App.SettingRepo.UpdateBackPage(1, "SafeDepositBox");
		await Shell.Current.GoToAsync("//SettingPage");
	}

	public void OnRedCircleClicked(object sender, EventArgs e)
	{
		// 处理红色圆圈点击事件
	}

	public void OnGreenCircleClicked(object sender, EventArgs e)
	{
		// 处理绿色圆圈点击事件
	}

	public void OnYellowCircleClicked(object sender, EventArgs e)
	{
		// 处理黄色圆圈点击事件
	}

	public void OnBlueCircleClicked(object sender, EventArgs e)
	{
		// 处理蓝色圆圈点击事件
	}

	// 点击数字1的处理方法
	private void OnNumber1Clicked(object sender, EventArgs e)
	{
		number1 = (number1 + 1) % 10;
		Number1.Text = number1.ToString();
		CheckPassword();
	}

	private void OnNumber2Clicked(object sender, EventArgs e)
	{
		number2 = (number2 + 1) % 10;
		Number2.Text = number2.ToString();
		CheckPassword();
	}

	private void OnNumber3Clicked(object sender, EventArgs e)
	{
		number3 = (number3 + 1) % 10;
		Number3.Text = number3.ToString();
		CheckPassword();
	}

	private void OnNumber4Clicked(object sender, EventArgs e)
	{
		number4 = (number4 + 1) % 10;
		Number4.Text = number4.ToString();
		CheckPassword();
	}

	// 检查密码是否正确
	private async void CheckPassword()
	{
		if (number1 == 5 && number2 == 1 && number3 == 8 && number4 == 7)
		{
			// 检查是否已经获得过手电筒
			bool hasTorch = await App.GameStateRepo.GetStateAsync("HasTorch");
			if (hasTorch)
			{
				// 如果已经获得过手电筒，显示提示信息
				FragmentHintLabel.Text = "保险箱是空的";
				FragmentHintLabel.Opacity = 0;
				FragmentHintLabel.IsVisible = true;
				await FragmentHintLabel.FadeTo(1, 500);
				await Task.Delay(1800);
				await FragmentHintLabel.FadeTo(0, 500);
				FragmentHintLabel.IsVisible = false;
				return;
			}

			// 找到第一个空的物品栏位置
			int slotIndex = -1;
			for (int i = 0; i < _inventoryLayout.Children.Count; i++)
			{
				if (_inventoryLayout.Children[i] is Image slot && 
					slot.Source.ToString().EndsWith("inventory.png"))
				{
					slotIndex = i;
					break;
				}
			}

			if (slotIndex != -1)
			{
				// 记��已获得手电筒
				await App.GameStateRepo.SaveStateAsync("HasTorch", true);

				// 保存手电筒到数据库
				var newItem = new InventoryItem
				{
					ItemName = "torch",
					SlotIndex = slotIndex,
					IsCollected = true
				};
				await App.InventoryRepo.SaveInventoryItemAsync(newItem);

				// 更新UI显示
				UpdateInventorySlot(slotIndex, "torch");

				// 显示获得物品的提示
				FragmentHintLabel.Text = "获得了一个紫外线手电筒";
				FragmentHintLabel.Opacity = 0;
				FragmentHintLabel.IsVisible = true;

				// 显示动画效果
				if (_inventoryLayout.Children[slotIndex] is Grid grid && 
						grid.Children.LastOrDefault() is Image inventoryItem)
				{
					// 启动字幕淡入动画
					FragmentHintLabel.FadeTo(1, 500);
					
					// 执行物品放大缩小动画
					await inventoryItem.ScaleTo(1.2, 100);
					await inventoryItem.ScaleTo(1.0, 100);
					
					// 等待显示时间后淡出字幕
					await Task.Delay(1800);
					await FragmentHintLabel.FadeTo(0, 500);
				}

				FragmentHintLabel.IsVisible = false;
			}
		}
	}
}