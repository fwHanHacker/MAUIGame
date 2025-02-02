namespace Games.Views;
using Games.Models;

public partial class CabinettRightRiddle : ContentPage
{
	private readonly string[] imageSequence = { "one.png", "two.png", "three.png", "four.png", 
		"five.png", "six.png", "seven.png", "eight.png", "nine.png" };
	private int[] currentImageIndices = { 0, 0, 0, 0 };  // 跟踪每个图片当前显示的索引
	private StackLayout _inventoryLayout;
	private BoxView _currentSelectedBackground;
	private Image _currentSelectedItem;
	private bool _hasBalls = false;

	public CabinettRightRiddle()
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
		
		// 检查是否已经获得过球
		var balls = await App.InventoryRepo.GetInventoryItemAsync("blue_and_yellow_ball");
		_hasBalls = balls != null && balls.IsCollected;

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

		// 重置所有图片
		Image1.Source = "one.png";
		Image2.Source = "one.png";
		Image3.Source = "one.png";
		Image4.Source = "one.png";
		currentImageIndices = new int[] { 0, 0, 0, 0 };

		await this.FadeTo(1, 500);
	}

	private void UpdateImage(Image image, ref int index)
	{
		index = (index + 1) % imageSequence.Length;  // 循环切换索引
		image.Source = imageSequence[index];
		CheckPassword();
	}

	private async void CheckPassword()
	{
		// 如果已经获得过球，直接返回
		if (_hasBalls) return;

		// 检查密码是否正确 (4931)
		if (currentImageIndices[0] == 3 && // 4 (索引3对应four.png)
			currentImageIndices[1] == 8 && // 9 (索引8对应nine.png)
			currentImageIndices[2] == 2 && // 3 (索引2对应three.png)
			currentImageIndices[3] == 0)   // 1 (索引0对应one.png)
		{
			try
			{
				// 保存状态到两个数据库
				await App.GameStateRepo.SaveStateAsync("HasBlueAndYellowBalls", true);
				
				// 保存到 InventoryRepo，但不显示在物品栏中
				var newItem = new InventoryItem
				{
					ItemName = "blue_and_yellow_ball",
					SlotIndex = -1,  // 使用-1表示不显示在物品栏
					IsCollected = true
				};
				await App.InventoryRepo.SaveInventoryItemAsync(newItem);
				
				_hasBalls = true;

				// 显示获得物品的提示
				FragmentHintLabel.Text = "获得了一个蓝色的球和一个黄色的球";
				FragmentHintLabel.Opacity = 0;
				FragmentHintLabel.IsVisible = true;

				// 显示动画效果
				await FragmentHintLabel.FadeTo(1, 500);
				await Task.Delay(1800);
				await FragmentHintLabel.FadeTo(0, 500);
				FragmentHintLabel.IsVisible = false;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in CheckPassword: {ex.Message}");
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
            "dishcloth" => "衣的一角",
            "battery" => "一节电池",
			// 添加其他物品的描述...
			_ => "未知物品"
		};
	}

	public void OnImage1Clicked(object sender, EventArgs e)
	{
		UpdateImage(Image1, ref currentImageIndices[0]);
	}

	public void OnImage2Clicked(object sender, EventArgs e)
	{
		UpdateImage(Image2, ref currentImageIndices[1]);

	}

	public void OnImage3Clicked(object sender, EventArgs e)
	{
		UpdateImage(Image3, ref currentImageIndices[2]);
	}

	public void OnImage4Clicked(object sender, EventArgs e)
	{
		UpdateImage(Image4, ref currentImageIndices[3]);
	}

	public async void OnBackClicked(object sender, EventArgs e)
	{
		await this.FadeTo(0, 500);
		await Shell.Current.GoToAsync("//Room1Wall1");
	}

	public async void OnSettingClicked(object sender, EventArgs e)
	{
		await App.SettingRepo.UpdateBackPage(1, "CabinettRightRiddle");
		await Shell.Current.GoToAsync("//SettingPage");
	}
}