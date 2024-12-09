
using Games.SQL;

namespace Games.Views;

public partial class StartGamePage : ContentPage
{
    public StartGamePage()
	{
		InitializeComponent();

        InitialBtn();//��ʼ����ť
    }

    public async void InitialBtn()
    {
        if (!await App.SettingRepo.IsSettingExists(1))//��������ڴ浵���򴴽��浵
        {
            await App.SettingRepo.AddNewSetting(1, 0,"StartGamePage");
        }

        if (await App.SettingRepo.GetArchiveById(1) == 0)
        {
            StartGameBtn1();//����浵Ϊ0����Ϊ��һ�δ���Ϸ��ֻ��ʾ��ʼ��Ϸ��ť
        }
        else if (await App.SettingRepo.GetArchiveById(1) == 1)
        {
            StartGameBtn2();
        }
    }

    public async void OnSettingClicked(object sender, EventArgs e)
    {
        await App.SettingRepo.UpdateBackPage(1,"StartGamePage");
        await Shell.Current.GoToAsync("//SettingPage");
    }

    public async void OnButtonClicked(object sender, EventArgs e)//�л�����
    {
        if (await App.SettingRepo.GetArchiveById(1) == 0)
        {
            await App.SettingRepo.UpdateArchive(1, 1);
        }
        await this.FadeTo(0, 1000);
        await Shell.Current.GoToAsync("//Room1Wall1");
    }

    public void StartGameBtn1()
    {
        Button startgameBtn = new Button
        {
            Text = "新游戏",
            HorizontalOptions = LayoutOptions.Center,
        };
        startgameBtn.Clicked += OnButtonClicked;

        Microsoft.Maui.Controls.StackLayout stackLayout = new Microsoft.Maui.Controls.StackLayout
        {
            Orientation = StackOrientation.Vertical,
            VerticalOptions = LayoutOptions.End,
            HorizontalOptions = LayoutOptions.Center,
            Spacing = 10,
            Margin = new Thickness(0, 0, 0, 30)
        };
        stackLayout.Children.Add(startgameBtn);
        Grid grid = this.Content as Grid;
        if (grid != null)
        {
            grid.Children.Add(stackLayout);
        }
    }

    public void StartGameBtn2()
    {
        Button startgameBtn = new Button
        {
            Text = "继续游戏",
            HorizontalOptions = LayoutOptions.Center,
            
        };
        startgameBtn.Clicked += OnButtonClicked;

        Button newgameBtn = new Button
        {
            Text = "新游戏",
            HorizontalOptions = LayoutOptions.Center,
            
        };
        newgameBtn.Clicked += OnButtonClicked;

        Microsoft.Maui.Controls.StackLayout stackLayout = new Microsoft.Maui.Controls.StackLayout
        {
            Orientation = StackOrientation.Vertical,
            VerticalOptions = LayoutOptions.End,
            HorizontalOptions = LayoutOptions.Center,
            Spacing = 10,
            Margin = new Thickness(0, 0, 0, 20)
        };
        stackLayout.Children.Add(startgameBtn);
        stackLayout.Children.Add(newgameBtn);
        Grid grid = this.Content as Grid;
        if (grid != null)
        {
            grid.Children.Add(stackLayout);
        }

    }

}