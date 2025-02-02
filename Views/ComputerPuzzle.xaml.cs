namespace Games.Views;

public partial class ComputerPuzzle : ContentPage
{
	private bool isNavigating = false;

	public ComputerPuzzle()
	{
		InitializeComponent();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		this.Opacity = 0;
		isNavigating = false;
		await this.FadeTo(1, 500);
	}

	public async void OnBackClicked(object sender, EventArgs e)
	{
		if (!isNavigating && this.Opacity > 0)
		{
			isNavigating = true;
			await this.FadeTo(0, 500);
			await Shell.Current.GoToAsync("//Room1Wall4");
		}
	}

	public async void OnSettingClicked(object sender, EventArgs e)
	{
		if (!isNavigating && this.Opacity > 0)
		{
			isNavigating = true;
			await App.SettingRepo.UpdateBackPage(1, "ComputerPuzzle");
			await Shell.Current.GoToAsync("//SettingPage");
		}
	}

	private async void OnConfirmClicked(object sender, EventArgs e)
	{
		string password = PasswordEntry.Text;
		
		if (password == "lome")
		{
			// 创建一个白色背景
			var whiteBackground = new Grid
			{
				BackgroundColor = Colors.White,
				Opacity = 0
			};

			// 创建结束文字
			var endText = new Label
			{
				Text = "你成功逃脱了这个密室",
				TextColor = Colors.Black,
				FontSize = 28,
				FontAttributes = FontAttributes.Bold,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Opacity = 0
			};

			// 将新元素添加到页面
			var mainGrid = this.Content as Grid;
			mainGrid?.Children.Add(whiteBackground);
			mainGrid?.Children.Add(endText);

			// 执行动画
			await Task.WhenAll(
				whiteBackground.FadeTo(1, 1000),
				endText.FadeTo(1, 1500)
			);

			// 等待3秒
			await Task.Delay(3000);

			try
			{
				// 重置所有游戏数据
				await App.SettingRepo.ResetGame(1);
				await App.InventoryRepo.ResetInventoryAsync();
				await App.GameStateRepo.ResetGameStatesAsync();
				
				// 设置存档状态为1（表示有存档）
				await App.SettingRepo.UpdateArchive(1, 1);

				// 返回开始界面
				await Shell.Current.GoToAsync("//StartGamePage");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"重置游戏数据时出错: {ex.Message}");
			}
		}
		else
		{
			await DisplayAlert("提示", "密码错误，请重试", "确定");
		}
		
		// 清空密码输入框
		PasswordEntry.Text = "";
	}
}