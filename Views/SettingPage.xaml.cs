using Games.SQL;

namespace Games.Views;

public partial class SettingPage : ContentPage
{
	private readonly SettingRepository _settingRepo;
	
	public SettingPage()
	{
		InitializeComponent();
		_settingRepo = App.SettingRepo;
		InitializeSettings();
	}

	private async void InitializeSettings()
	{
		// 从设置中读取音乐状态
		var setting = await _settingRepo.GetSettingById(1);
		if (setting != null)
		{
			MusicSwitch.IsToggled = setting.IsMusicEnabled;
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
		bool answer = await DisplayAlert("提示", "确定要返回主菜单吗？", "确定", "取消");
		if (answer)
		{
			await Shell.Current.GoToAsync("//StartGamePage");
		}
	}
}