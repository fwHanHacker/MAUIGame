using System;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.Maui.Graphics;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Games.Models;

namespace Games.Views;

public partial class BoxRiddle : ContentPage
{
    private Image currentPiece;
    private Point startPosition;
    private const double PIECE_SIZE = 60;
    private const double SLOT_MARGIN_LEFT = 23;
    private const double SLOT_MARGIN_RIGHT = 243;
    private const double SLOT_TOP = 14;
    private const double SLOT_UPPER = 74;
    private const double SLOT_LOWER = 134;
    private const double SLOT_BOTTOM = 189;
    private const double SNAP_DISTANCE = 30;           // 增加吸附距离，使更容易对齐
    private const double SLOT_CENTER = 133;  // 中间位置的X坐标
    private const double HORIZONTAL_SNAP_DISTANCE = 40; // 增加横向吸附距离

    // 记录每个位置的方块
    private Dictionary<Point, Image> positionOccupancy = new Dictionary<Point, Image>();
    
    // 所有可能的位置
    private readonly List<Point> validPositions = new List<Point>
    {
        // 左侧位置
        new Point(SLOT_MARGIN_LEFT, SLOT_TOP),    // 左上
        new Point(SLOT_MARGIN_LEFT, SLOT_UPPER),  // 左上中
        new Point(SLOT_MARGIN_LEFT, SLOT_LOWER),  // 左下中
        new Point(SLOT_MARGIN_LEFT, SLOT_BOTTOM), // 左下

        // 中间位置
        new Point(SLOT_CENTER, SLOT_UPPER),       // 中上
        new Point(SLOT_CENTER, SLOT_LOWER),       // 中

        // 右侧位置
        new Point(SLOT_MARGIN_RIGHT, SLOT_TOP),    // 右上
        new Point(SLOT_MARGIN_RIGHT, SLOT_UPPER),  // 右上中
        new Point(SLOT_MARGIN_RIGHT, SLOT_LOWER),  // 右下中
        new Point(SLOT_MARGIN_RIGHT, SLOT_BOTTOM)  // 右下
    };

    // 修改正确位置的定义
    private readonly Dictionary<string, Point> correctPositions = new Dictionary<string, Point>
    {
        {"p3.png", new Point(SLOT_MARGIN_LEFT, SLOT_TOP)},     // 左上（竖线符号）
        {"p4.png", new Point(SLOT_MARGIN_LEFT, SLOT_UPPER)},   // 左中上（圆点符号）
        {"p5.png", new Point(SLOT_MARGIN_LEFT, SLOT_BOTTOM)},  // 左下（方框符号）
        {"p1.png", new Point(SLOT_MARGIN_RIGHT, SLOT_TOP)},    // 右上（弧线符号）
        {"p2.png", new Point(SLOT_MARGIN_RIGHT, SLOT_UPPER)},  // 右中上（三角形符号）
        {"p6.png", new Point(SLOT_MARGIN_RIGHT, SLOT_BOTTOM)}, // 右下T���符号）
    };

    private StackLayout _inventoryLayout;
    private BoxView _currentSelectedBackground;
    private Image _currentSelectedItem;
    private bool _hasKnife = false;

    public BoxRiddle()
    {
        InitializeComponent();
        
        // 添加页面背景的点击事件
        var backgroundTapGesture = new TapGestureRecognizer();
        backgroundTapGesture.Tapped += OnBackgroundTapped;
        
        // 将事件绑定到背景图片上
        var backgroundImage = this.FindByName<Image>("riddlebackground");
        if (backgroundImage != null)
        {
            backgroundImage.GestureRecognizers.Add(backgroundTapGesture);
        }
    }

    // 添加背景点击事件处理方法
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
        try
        {
            base.OnAppearing();

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

            // 检查是否已经获得过刀
            bool hasKnife = await App.GameStateRepo.GetStateAsync("HasKnife");
            _hasKnife = hasKnife;

            // 从数据库加载物品
            var items = await App.InventoryRepo.GetInventoryItemsAsync();
            foreach (var item in items)
            {
                if (item.IsCollected)
                {
                    UpdateInventorySlot(item.SlotIndex, item.ItemName);
                }
            }

            InitializePuzzle();
            await this.FadeTo(1, 500);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OnAppearing error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private void InitializePuzzle()
    {
        positionOccupancy.Clear();
        foreach (var child in PuzzleGrid.Children)
        {
            if (child is Image image)
            {
                var position = new Point(image.Margin.Left, image.Margin.Top);
                positionOccupancy[position] = image;
            }
        }
    }

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        var piece = (Image)sender;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                currentPiece = piece;
                startPosition = new Point(piece.Margin.Left, piece.Margin.Top);
                piece.ZIndex = 100;
                break;

            case GestureStatus.Running:
                if (currentPiece != null)   

                {
                    double currentX = currentPiece.Margin.Left;
                    double currentY = currentPiece.Margin.Top;
                    double newX = currentX;
                    double newY = currentY;

                    // 判断是否在第三个凹槽位或附近
                    bool isInLowerRow = Math.Abs(currentY - SLOT_LOWER) < SNAP_DISTANCE * 1.5;
                    bool isInUpperRow = Math.Abs(currentY - SLOT_UPPER) < SNAP_DISTANCE * 1.5;

                    // 在第三个凹槽位置或中间位置时允许自由移动
                    if (isInLowerRow || (isInUpperRow && Math.Abs(currentX - SLOT_CENTER) < HORIZONTAL_SNAP_DISTANCE))
                    {
                        // 处理横向移动
                        newX = currentX + e.TotalX * 1.2;
                        newX = Math.Clamp(newX, SLOT_MARGIN_LEFT, SLOT_MARGIN_RIGHT);
                        
                        // 横向吸附
                        if (Math.Abs(newX - SLOT_MARGIN_LEFT) < HORIZONTAL_SNAP_DISTANCE)
                            newX = SLOT_MARGIN_LEFT;
                        else if (Math.Abs(newX - SLOT_CENTER) < HORIZONTAL_SNAP_DISTANCE)
                            newX = SLOT_CENTER;
                        else if (Math.Abs(newX - SLOT_MARGIN_RIGHT) < HORIZONTAL_SNAP_DISTANCE)
                            newX = SLOT_MARGIN_RIGHT;

                        // 处理竖直移动
                        newY = currentY + e.TotalY * 1.2;
                        
                        // 根据当前X位置限制Y的移动范围
                        if (Math.Abs(newX - SLOT_CENTER) < HORIZONTAL_SNAP_DISTANCE)
                            newY = Math.Clamp(newY, SLOT_UPPER, SLOT_LOWER);
                        else
                            newY = Math.Clamp(newY, SLOT_TOP, SLOT_BOTTOM);
                        
                        // 竖直吸附
                        if (Math.Abs(newY - SLOT_TOP) < SNAP_DISTANCE && 
                            Math.Abs(newX - SLOT_CENTER) >= HORIZONTAL_SNAP_DISTANCE)
                            newY = SLOT_TOP;
                        else if (Math.Abs(newY - SLOT_UPPER) < SNAP_DISTANCE)
                            newY = SLOT_UPPER;
                        else if (Math.Abs(newY - SLOT_LOWER) < SNAP_DISTANCE)
                            newY = SLOT_LOWER;
                        else if (Math.Abs(newY - SLOT_BOTTOM) < SNAP_DISTANCE && 
                                 Math.Abs(newX - SLOT_CENTER) >= HORIZONTAL_SNAP_DISTANCE)
                            newY = SLOT_BOTTOM;
                    }
                    // 在左右两列的其他位置时
                    else if (Math.Abs(currentX - SLOT_MARGIN_LEFT) < HORIZONTAL_SNAP_DISTANCE || 
                             Math.Abs(currentX - SLOT_MARGIN_RIGHT) < HORIZONTAL_SNAP_DISTANCE)
                    {
                        // 只允许竖直移动
                        newY = currentY + e.TotalY * 1.2;
                        newY = Math.Clamp(newY, SLOT_TOP, SLOT_BOTTOM);
                        
                        // 竖直吸附
                        if (Math.Abs(newY - SLOT_TOP) < SNAP_DISTANCE)
                            newY = SLOT_TOP;
                        else if (Math.Abs(newY - SLOT_UPPER) < SNAP_DISTANCE)
                            newY = SLOT_UPPER;
                        else if (Math.Abs(newY - SLOT_LOWER) < SNAP_DISTANCE)
                            newY = SLOT_LOWER;
                        else if (Math.Abs(newY - SLOT_BOTTOM) < SNAP_DISTANCE)
                            newY = SLOT_BOTTOM;

                        // 保持在最近的列
                        if (Math.Abs(currentX - SLOT_MARGIN_LEFT) < HORIZONTAL_SNAP_DISTANCE)
                            newX = SLOT_MARGIN_LEFT;
                        else
                            newX = SLOT_MARGIN_RIGHT;
                    }

                    // 检查新位置是否被占用
                    var targetPosition = new Point(newX, newY);
                    if (!positionOccupancy.ContainsKey(targetPosition) || 
                        positionOccupancy[targetPosition] == currentPiece)
                    {
                        currentPiece.Margin = new Thickness(newX, newY, 0, 0);
                    }
                }
                break;

            case GestureStatus.Completed:
                if (currentPiece != null)
                {
                    var oldPosition = startPosition;
                    var currentPosition = new Point(currentPiece.Margin.Left, currentPiece.Margin.Top);

                    // 如果当前位置是有效位置且未被占用
                    if (validPositions.Contains(currentPosition) && 
                        (!positionOccupancy.ContainsKey(currentPosition) || currentPosition == oldPosition))
                    {
                        positionOccupancy.Remove(oldPosition);
                        positionOccupancy[currentPosition] = currentPiece;
                    }
                    else
                    {
                        // 返回原位
                        currentPiece.Margin = new Thickness(oldPosition.X, oldPosition.Y, 0, 0);
                    }

                    currentPiece.ZIndex = 1;
                    currentPiece = null;
                    CheckPuzzleComplete();
                }
                break;
        }
    }

    private Point FindNearestValidPosition(Point currentPosition)
    {
        // 找到最近的有效位置
        return validPositions
            .OrderBy(p => Math.Pow(p.X - currentPosition.X, 2) + Math.Pow(p.Y - currentPosition.Y, 2))
            .First();
    }

    private async void CheckPuzzleComplete()
    {
        try
        {
            // 如果已经获得过刀，直接返回
            if (_hasKnife)
            {
                return;
            }

            bool isComplete = true;
            foreach (var kvp in correctPositions)
            {
                // 找到对应的片
                var pieceImage = PuzzleGrid.Children.FirstOrDefault(c => 
                {
                    if (c is Image img)
                    {
                        var source = img.Source?.ToString() ?? "";
                        return source.EndsWith(kvp.Key);
                    }
                    return false;
                }) as Image;

                if (pieceImage == null)
                {
                    isComplete = false;
                    break;
                }

                var currentPosition = new Point(pieceImage.Margin.Left, pieceImage.Margin.Top);
                
                // 检查位置是否正确
                if (Math.Abs(currentPosition.X - kvp.Value.X) > 1 || 
                    Math.Abs(currentPosition.Y - kvp.Value.Y) > 1)
                {
                    isComplete = false;
                    break;
                }
            }

            if (isComplete)
            {
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
                    // 记录已获得刀
                    await App.GameStateRepo.SaveStateAsync("HasKnife", true);
                    _hasKnife = true;

                    // 保存刀到数据库
                    var newItem = new InventoryItem
                    {
                        ItemName = "knife",
                        SlotIndex = slotIndex,
                        IsCollected = true
                    };
                    await App.InventoryRepo.SaveInventoryItemAsync(newItem);

                    // 更新UI显示
                    UpdateInventorySlot(slotIndex, "knife");

                    // 显示获得物品的提示
                    FragmentHintLabel.Text = "获得了一把小刀";
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
        catch (Exception ex)
        {
            Console.WriteLine($"CheckPuzzleComplete error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
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

        string imageSource = itemName;
        if (itemName.Contains("picture_fragment"))
        {
            imageSource = "sofa_picture_fragment";
        }

        var inventoryItem = new Image
        {
            Source = imageSource + ".png",
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
        if (itemName.Contains("picture_fragment"))  // 添加对画碎片的处理
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
            _ => "未知物品"
        };
    }

    public async void OnBackClicked(object sender, EventArgs e)
    {
        await this.FadeTo(0, 500);
        await Shell.Current.GoToAsync("//Room1Wall1");
    }

    public async void OnSettingClicked(object sender, EventArgs e)
    {
        await App.SettingRepo.UpdateBackPage(1, "BoxRiddle");
        await Shell.Current.GoToAsync("//SettingPage");
    }
}