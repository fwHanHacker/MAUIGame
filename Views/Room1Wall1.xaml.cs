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

        // ���붯��������ȫ͸�����䵽��ȫ��͸��
        await this.FadeTo(1, 1000); // 1000�����ڵ��뵽��ȫ��͸��
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
}