# NetEase


<!-- ========================== -->

    `<!-- 右侧主视图 (Grid.Column="1") -->`

    `<!-- ========================== -->`

    <GridShowGridLines="True"Grid.Column="1"

    Background="#2C2C2C">

    <Grid.RowDefinitions>

    `<!-- 歌单头部信息 -->`

    <RowDefinitionHeight="Auto"/>

    `<!-- 歌曲/评论/收藏者 Tab切换 -->`

    <RowDefinitionHeight="Auto"/>

    `<!-- 歌曲列表 -->`

    <RowDefinitionHeight="*"/>

    </Grid.RowDefinitions>

    `<!-- 歌单头部信息 -->`

    <GridShowGridLines="True"Grid.Row="0"

    Margin="30">

    <Grid.ColumnDefinitions>

    <ColumnDefinitionWidth="Auto"/>

    <ColumnDefinitionWidth="*"/>

    </Grid.ColumnDefinitions>

    `<!-- 歌单封面 -->`

    <BorderWidth="180"

    Height="180"

    CornerRadius="8"

    Background="#555"

    Margin="0,0,20,0">

    <TextBlockText="封面图"

    Foreground="White"

    HorizontalAlignment="Center"

    VerticalAlignment="Center"/>

    `</Border>`

    `<!-- 歌单详情 -->`

    <StackPanelGrid.Column="1"

    VerticalAlignment="Center">

    <TextBlockText="我喜欢的音乐"

    FontSize="24"

    FontWeight="Bold"

    Foreground="White"/>

    <TextBlockText="Brokenheart100 2017-02-18 创建"

    Foreground="Gray"

    Margin="0,10,0,20"/>

    `<!-- 使用 local 前缀来引用 Carousel 控件 -->`

    [local:CarouselHeight=&#34;200&#34;Margin=&#34;30,0&#34;/](local:CarouselHeight=%22200%22Margin=%2230,0%22/)

    <StackPanelOrientation="Horizontal">

    <ButtonContent="▶ 播放全部"

    Background="#D32F2F"

    Foreground="White"

    BorderThickness="0"

    Padding="15,8"

    Margin="0,0,10,0"/>

    <ButtonContent="↓ 下载"

    Background="#444"

    Foreground="White"

    BorderThickness="0"

    Padding="15,8"/>

    `</StackPanel>`

    `</StackPanel>`

    `</Grid>`

    `<!-- Tab切换区域 -->`

    <TabControlGrid.Row="1"

    Margin="30,0"

    BorderThickness="0">

    <TabItemHeader="歌曲">

    `<!-- Tab内容区域为空，因为列表在TabControl的外面，这样布局更简单 -->`

    `<!-- 在实际应用中，列表会放在这里 -->`

    `</TabItem>`

    <TabItemHeader="评论"/>

    <TabItemHeader="收藏者"/>

    `</TabControl>`

    `<!-- 歌曲列表 (使用ListView和GridView) -->`

    <ListViewGrid.Row="2"

    x:Name="SongListView"

    Margin="30,10"

    Background="Transparent"

    BorderThickness="0"

    Foreground="White">

    <ListView.View>

    `<GridView>`

    <GridView.ColumnHeaderContainerStyle>

    <StyleTargetType="{x:Type GridViewColumnHeader}">

    <SetterProperty="Foreground"

    Value="Gray"/>

    <SetterProperty="Background"

    Value="Transparent"/>

    <SetterProperty="BorderThickness"

    Value="0"/>

    `</Style>`

    </GridView.ColumnHeaderContainerStyle>

    <GridViewColumnHeader="#"

    DisplayMemberBinding="{Binding Index}"

    Width="40"/>

    <GridViewColumnHeader="标题"

    Width="350">

    <GridViewColumn.CellTemplate>

    `<DataTemplate>`

    <StackPanelOrientation="Horizontal">

    <TextBlockText="{Binding Title}"

    VerticalAlignment="Center"/>

    <TextBlockText="{Binding Artist}"

    Foreground="Gray"

    Margin="10,0,0,0"

    VerticalAlignment="Center"/>

    `</StackPanel>`

    `</DataTemplate>`

    </GridViewColumn.CellTemplate>

    `</GridViewColumn>`

    <GridViewColumnHeader="专辑"

    DisplayMemberBinding="{Binding Album}"

    Width="200"/>

    <GridViewColumnHeader="喜欢"

    Width="60">

    <GridViewColumn.CellTemplate>

    `<DataTemplate>`

    <TextBlockText="❤"

    FontSize="16"

    HorizontalAlignment="Center"/>

    `</DataTemplate>`

    </GridViewColumn.CellTemplate>

    `</GridViewColumn>`

    <GridViewColumnHeader="时长"

    DisplayMemberBinding="{Binding Duration}"

    Width="80"/>

    `</GridView>`

    </ListView.View>

    `</ListView>`

    `</Grid>`
