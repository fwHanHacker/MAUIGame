namespace Games.Views;

public partial class Room1Wall1 : ContentPage
{
	public Room1Wall1()
	{
		InitializeComponent();
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await this.FadeTo(1, 500); 
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
}