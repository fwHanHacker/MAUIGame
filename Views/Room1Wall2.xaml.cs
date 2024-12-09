namespace Games.Views;

public partial class Room1Wall2 : ContentPage
{
	public Room1Wall2()
	{
        InitializeComponent();
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await this.FadeTo(1, 1000);
    }

    public async void OnLeftButtonClicked(object sender, EventArgs e)
    {
        await this.FadeTo(0, 500);
        await Shell.Current.GoToAsync("//Room1Wall4");
    }

    public async void OnRightButtonClicked(object sender, EventArgs e)
    {
        await this.FadeTo(0, 500);
        await Shell.Current.GoToAsync("//Room1Wall1");
    }
}