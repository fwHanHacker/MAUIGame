using Games.SQL;

namespace Games.Views;

public partial class SettingPage : ContentPage
{
	private readonly SettingRepository _settingRepo;
	
	public SettingPage()
	{
		InitializeComponent();
		_settingRepo = App.SettingRepo;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		
		// 初始化设置
		await InitializeSettings();
		
		// 淡入显示页面
		await this.FadeTo(1, 500);
	}

	private async Task InitializeSettings()
	{
		try
		{
			// 从设置中读取音乐状态
			var setting = await _settingRepo.GetSettingById(1);
			if (setting != null)
			{
				MusicSwitch.IsToggled = setting.IsMusicEnabled;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"初始化设置时出错: {ex.Message}");
			// 可以在这里添加错误处理逻辑
		}
	}

	public async void OnBackClicked(object sender, EventArgs e)
	{
		string backPage = await _settingRepo.GetBackPageById(1);
		await Shell.Current.GoToAsync($"//{backPage}");
	}

	public async void OnMusicToggled(object sender, ToggledEventArgs e)
	{
		await _settingRepo.UpdateMusicSetting(1, e.Value);
		// 这里添加音乐开关的实际逻辑
		if (e.Value)
		{
			// 播放背景音乐
		}
		else
		{
			// 停止背景音乐
		}
	}

	public async void OnResetClicked(object sender, EventArgs e)
	{
		bool answer = await DisplayAlert("警告", "确定要重置所有游戏进度吗？此操作不可撤销。", "确定", "取消");
		if (answer)
		{
			await _settingRepo.ResetGame(1);
			await DisplayAlert("提示", "游戏已重置", "确定");
			await Shell.Current.GoToAsync("//StartGamePage");
		}
	}

	public async void OnReturnToMainClicked(object sender, EventArgs e)
	{
			// 更新返回页面为StartGamePage
			await App.SettingRepo.UpdateBackPage(1, "StartGamePage");
			// 先淡出当前页面
			await this.FadeTo(0, 500);
			// 然后导航到主菜单
			await Shell.Current.GoToAsync("//StartGamePage");
	}
}