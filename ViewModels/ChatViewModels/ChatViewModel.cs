using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using NetEase.Dtos;
using NetEase.Models;
using NetEase.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
// WPF UI相关类（窗口、图像渲染）
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static NetEase.Converters.RandomNumber;

// 命名空间：聊天相关视图模型（遵循MVVM分层，ViewModel对应View）
namespace NetEase.ViewModels.ChatViewModels
{
    // 【会话模型】：表示一个聊天会话（如单聊/群聊的入口信息）
    public partial class ChatSession : ObservableObject
    {
        // 会话唯一ID（关联后端用户/群组ID）
        public int Id { get; set; }
        // 会话名称（如好友昵称、群组名称）
        public string Name { get; set; }
        // 会话头像URL（好友/群组头像地址）
        public string AvatarUrl { get; set; }
        // 【可观察属性】未读消息数（用[ObservableProperty]自动生成INotifyPropertyChanged，UI实时更新）
        [ObservableProperty]
        private int _unreadMessageCount;
    }

    // 【群成员模型】：表示群聊中的成员信息（仅存储UI展示所需的名称和头像）
    public class GroupMember
    {
        // 成员名称
        public string Name { get; set; }
        // 成员头像URL
        public string AvatarUrl { get; set; }
    }

    // 【消息类型枚举】：区分消息是文本还是图片
    public enum MessageType
    {
        Text,   // 文本消息
        Image   // 图片消息
    }

    // 【聊天消息实体】：用record类型（不可变对象，适合存储只读消息数据），包含UI展示和数据传输所需的所有属性
    public partial class ChatMessage : ObservableObject
    {
        // 消息唯一ID（后端生成）
        public long Id { get; init; }
        // 发送者ID（关联用户ID）
        public int SenderId { get; init; }
        // 消息内容（文本消息存文字，图片消息存URL/Base64标识）
        public string Content { get; init; }
        // 消息发送时间
        public DateTime SentAt { get; init; }
        // 消息类型（文本/图片）
        public MessageType Type { get; init; }
        // 图片URL（仅图片消息有效，指向远程/本地图片地址）
        public string ImageUrl { get; init; }
        // 是否是当前登录用户发送的消息（用于UI区分左/右布局）
        public bool IsSentByMe { get; init; }
        // 发送者名称（UI显示用，如“我”或好友昵称）
        public string SenderName { get; init; }
        // 发送者头像URL（UI显示用）
        public string AvatarUrl { get; init; }
        // 媒体类型（MIME类型，如image/jpeg，仅图片消息有效）
        public string MimeType { get; init; }
        // 图片源（WPF UI直接渲染的ImageSource，从Base64/URL转换而来）
        public ImageSource ImageData { get; init; }
        // 图片Base64字符串（从后端接收的原始图片数据，用于转换为ImageSource）
        public string ImageDataBase64 { get; set; }
        [ObservableProperty]
        private bool _isRead;
    }

    // 【聊天视图模型】：核心ViewModel，关联聊天界面（View），处理会话、消息、成员的业务逻辑
    // 继承BaseViewModel（自定义基类，可能包含公共属性如加载状态、错误提示等）
    public partial class ChatViewModel : BaseViewModel, IDisposable
    {
        // 依赖注入的服务（通过构造函数注入，解耦业务逻辑）
        private readonly ChatService _chatService;    // 聊天服务：处理消息发送、历史记录获取
        private readonly FileService _fileService;    // 文件服务：处理图片上传
        private readonly AuthService _authService;    // 授权服务：获取当前登录用户信息
        private readonly SignalRService _signalRService;  // SignalR服务：处理实时消息接收
        private readonly UserProfileService _profileService;
        private readonly string _apiBaseUrl;          // API基础地址（从HttpClient获取，拼接图片URL用）

        // 【会话列表】：绑定UI的会话列表控件（ObservableCollection支持UI自动更新）
        public ObservableCollection<ChatSession> Sessions { get; }

        // 【当前会话消息列表】：绑定UI的消息展示区域（实时更新当前会话的消息）
        public ObservableCollection<ChatMessage> Messages { get; }

        // 【群成员列表】：绑定UI的群成员展示区域（仅群聊时有效）
        public ObservableCollection<GroupMember> Members { get; }

        // 【可观察属性】当前聊天的好友（可能用于单聊场景的额外信息）
        [ObservableProperty]
        private Friend _currentChatFriend;

        // 【可观察属性】当前选中的会话（切换会话时触发UI更新，加载对应消息）
        [ObservableProperty]
        private ChatSession _selectedSession;

        // 【可观察属性】新消息输入框内容（绑定UI输入框，且通知SendMessageCommand更新可用状态）
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
        private string _newMessageText;

        // 【构造函数】：通过依赖注入初始化服务，初始化集合，订阅实时消息事件
        public ChatViewModel(SignalRService signalRService, ChatService chatService,
                            FileService fileService, AuthService authService, HttpClient httpClient, UserProfileService profileService)
        {
            // 赋值注入的服务
            _chatService = chatService;
            _fileService = fileService;
            _authService = authService;
            _signalRService = signalRService;
            _profileService = profileService;
            // 从HttpClient获取API基础地址（用于拼接图片的完整URL）
            _apiBaseUrl = httpClient.BaseAddress.ToString();

            // 初始化可观察集合（避免UI绑定时空引用）
            Sessions = new ObservableCollection<ChatSession>();
            Messages = new ObservableCollection<ChatMessage>();
            Members = new ObservableCollection<GroupMember>();

            // 订阅SignalR的实时消息接收事件（收到新消息时触发OnMessageReceived）
            _signalRService.OnMessageReceived += OnMessageReceived;
            _signalRService.OnMessagesRead += OnMessagesRead;

        }

        private void OnMessagesRead(int readerId, List<long> readMessageIds)
        {
            // 检查这个回执是否与当前打开的会话有关
            if (SelectedSession != null && readerId == SelectedSession.Id)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var msgId in readMessageIds)
                    {
                        var messageToUpdate = Messages.FirstOrDefault(m => m.Id == msgId);
                        if (messageToUpdate != null)
                        {
                            // 更新消息的已读状态
                            messageToUpdate.IsRead = true;
                        }
                    }
                });
            }
        }
        public async Task InitializeAsync()
        {
            Debug.WriteLine($"Enter InitializeAsync()");
            // 1. 加载历史会话列表
            await LoadRecentSessionsAsync();

            // 2. 加载并恢复上次的应用状态
            var appState = _profileService.LoadAppState();
            if (appState?.LastActiveSessionId != null)
            {
                // 查找上次活动的会话
                var lastSession = Sessions.FirstOrDefault(s => s.Id == appState.LastActiveSessionId.Value);
                if (lastSession != null)
                {
                    // 如果找到了，就自动将其设置为当前选中会话
                    // 这会自动触发 OnSelectedSessionChanged，从而加载聊天记录
                    SelectedSession = lastSession;
                }
            }
        }
        private async Task LoadRecentSessionsAsync()
        {
            var sessionDtos = await _chatService.GetSessionsAsync();
            Debug.WriteLine($"Enter LoadRecentSessionsAsync() _chatService.GetSessionsAsync() : {sessionDtos}");
            //Sessions.Clear();
            //if (sessionDtos != null)
            //{
            //    foreach (var dto in sessionDtos)
            //    {
            //        // 将 DTO 转换为前端的 ChatSession 模型
            //        Sessions.Add(new ChatSession
            //        {
            //            Id = dto.ContactId,
            //            Name = dto.Name,
            //            AvatarUrl = dto.AvatarUrl,
            //            UnreadMessageCount = dto.UnreadCount
            //            // 还可以添加一个 LastMessage 属性用于UI显示
            //        });
            //    }
            //}
            // 在UI线程上执行所有集合操作
            Application.Current.Dispatcher.Invoke(async () =>
            {
                Sessions.Clear();
                if (sessionDtos != null)
                {
                    foreach (var dto in sessionDtos)
                    {
                        Sessions.Add(new ChatSession
                        {
                            Id = dto.ContactId,
                            Name = dto.Name,
                            AvatarUrl = dto.AvatarUrl,
                            UnreadMessageCount = dto.UnreadCount
                        });
                    }
                    Debug.WriteLine($"Sessions collection updated. Count: {Sessions.Count}");
                }
            });
        }
        // 【SignalR实时消息接收回调】：收到后端推送的新消息时执行
        private void OnMessageReceived(ChatMessageDto message)
        {
            // 检查：1. 是否有选中的会话 2. 消息是否属于当前会话（发送者/接收者ID匹配会话ID）

            Application.Current.Dispatcher.Invoke(async () => // 标记为 async
            {
                Debug.WriteLine($"Enter OnMessageReceived({message}) ");
                var targetSession = Sessions.FirstOrDefault(s => s.Id == message.SenderId);

                // 【核心修正】如果会话不存在，就创建一个
                if (targetSession == null)
                {
                    // TODO: 这里需要一个服务来根据用户ID(message.SenderId)获取用户名和头像
                    // 我们先用一个临时的方法
                    targetSession = await CreateNewSessionFromId(message.SenderId);
                    if (targetSession != null)
                    {
                        Sessions.Insert(0, targetSession);
                    }
                }

                if (targetSession == null) return; // 如果创建失败，则退出

                if (SelectedSession != null && targetSession.Id == SelectedSession.Id)
                {
                    // 是当前会话
                    Debug.WriteLine($"Enter OnMessageReceived if (SelectedSession != null && targetSession.Id == SelectedSession.Id){targetSession} ");

                    var uiMessage = ConvertDtoToChatMessage(message, targetSession);
                    Messages.Add(uiMessage);
                }
                else
                {
                    // 不是当前会话，增加未读数
                    targetSession.UnreadMessageCount++;
                }
            });
        }
        public void Dispose()
        {
            _signalRService.OnMessageReceived -= OnMessageReceived;
            _signalRService.OnMessagesRead -= OnMessagesRead; // <-- 【新增】取消订阅
            GC.SuppressFinalize(this);
        }
        private async Task<ChatSession> CreateNewSessionFromId(int userId)
        {
            // 在真实应用中:
            // var userDto = await _userService.GetUserProfileAsync(userId);
            // return new ChatSession { Id = userDto.Id, Name = userDto.Name, ... };

            // 暂时返回一个占位会话
            return new ChatSession { Id = userId, Name = $"新消息来自用户 {userId}", AvatarUrl = null };
        }
        // 【DTO转UI消息对象】：将后端传递的ChatMessageDto转换为UI可渲染的ChatMessage
        private bool IsUrlBasedImage(string content)
        {
            return !string.IsNullOrEmpty(content) && content.StartsWith("/images/");
        }

        private ChatMessage ConvertDtoToChatMessage(ChatMessageDto dto, ChatSession session)
        {
            // 获取当前登录用户ID，判断消息是否是自己发送的
            var currentUserId = _authService.GetCurrentUserId();
            if (currentUserId == null) return null; // 安全检查
            bool isMe = dto.SenderId == currentUserId.Value;
            bool isImage = !string.IsNullOrEmpty(dto.MimeType) && dto.MimeType.StartsWith("image");
            if (!isImage && IsUrlBasedImage(dto.Content))
            {
                isImage = true;
            }
            // 构建UI消息对象
            return new ChatMessage
            {
                Id = dto.Id,                      // 消息ID（后端传递）
                SenderId = dto.SenderId,          // 发送者ID
                Content = dto.Content,            // 消息内容
                SentAt = dto.SentAt,              // 发送时间
                // 消息类型：判断内容是否是图片URL（/images/开头），是则为Image，否则为Text
                Type = IsImageUrl(dto.Content) ? MessageType.Image : MessageType.Text,
                // 图片URL：如果是图片消息，拼接API基础地址为完整URL
                ImageUrl = IsImageUrl(dto.Content) ? $"{_apiBaseUrl}{dto.Content}" : null,
                IsSentByMe = isMe,                // 是否自己发送
                SenderName = isMe ? "我" : session?.Name ?? $"用户 {dto.SenderId}",
                AvatarUrl = GetRandomAvatarUrl(), // 临时用随机头像（后续可替换为真实头像URL）
                IsRead = dto.IsRead
            };
        }

        // 【选中会话变化时触发】：当UI选中的会话改变时，自动执行（由[ObservableProperty]生成的OnXXXChanged方法）
        partial void OnSelectedSessionChanged(ChatSession value)
        {
            _profileService.SaveLastActiveSession(value?.Id);
            if (value != null)
            {
                // 关键：用户点开会话后，将未读消息数清零（UI小红点消失）
                if (value.UnreadMessageCount > 0)
                {
                    value.UnreadMessageCount = 0;
                    // TODO：调用API通知后端“该会话消息已读”，避免下次加载仍显示未读
                }
                // 加载选中会话的详情（历史消息、群成员）
                LoadChatDetails(value);
                if (value.UnreadMessageCount > 0)
                {
                    // 乐观更新UI
                    value.UnreadMessageCount = 0;
                    // 异步发送已读通知
                    Task.Run(() => _chatService.MarkMessagesAsReadAsync(value.Id));
                }
            }
            else
            {
                // 未选中任何会话：清空消息列表和成员列表
                Messages.Clear();
                Members.Clear();
            }
        }
        private async void UploadAndSendMessage(string localImagePath)
        {
            if (SelectedSession == null) return;

            // 1. 乐观更新UI
            var pendingMessage = new ChatMessage
            {
                SenderName = "我",
                IsSentByMe = true,
                Type = MessageType.Image,
                ImageUrl = localImagePath,
                Content = "发送中...",
                AvatarUrl = GetRandomAvatarUrl()
            };
            Messages.Add(pendingMessage);

            // 2. 调用文件服务上传图片
            var remoteImageUrl = await _fileService.UploadFileAsync(localImagePath);

            Messages.Remove(pendingMessage); // 移除"发送中"状态

            if (string.IsNullOrEmpty(remoteImageUrl))
            {
                //var failedMessage = pendingMessage with
                //{
                //    Content = "[图片发送失败]"
                //    // ImageUrl 保持不变，仍然是本地路径，以便显示预览
                //};

                //Messages.Add(failedMessage);
                return;
            }

            // 3. 上传成功后，调用聊天服务发送消息（内容为URL）
            var savedMessageDto = await _chatService.SendMessageAsync(new SendMessageDto
            {
                ReceiverId = SelectedSession.Id,
                Content = remoteImageUrl,
                MimeType = "image/url" // 标记为URL图片
            });

            if (savedMessageDto != null)
            {
                // 发送成功
                Messages.Add(ConvertDtoToChatMessage(savedMessageDto, SelectedSession));
            }
            else
            {
                // 消息保存失败
                //var failedMessage = pendingMessage with { Content = "[图片发送失败]", ImageUrl = localImagePath };
                //var failedMessage = pendingMessage with { Content = "发送失败" };
                //Messages.Add(failedMessage);
            }
        }

        // 【发送图片命令】：绑定UI的“选择图片”按钮，触发文件选择和图片发送
        [RelayCommand]
        private async void SendImage()
        {
            Debug.WriteLine("进入SendImage()");
            // 1. 创建文件选择对话框，配置仅显示图片格式
            var openFileDialog = new OpenFileDialog
            {
                Title = "选择要发送的图片",  // 对话框标题
                // 筛选文件类型：常见图片格式 + 所有文件（兜底）
                Filter = "图片文件 (*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|所有文件 (*.*)|*.*"
            };

            // 2. 显示对话框，判断用户是否选择了文件（点击“确定”）
            if (openFileDialog.ShowDialog() == true)
            {
                // 3. 读取选中的图片文件，转换为Base64字符串（用于前端预览和后端传输）
                byte[] imageBytes = File.ReadAllBytes(openFileDialog.FileName);  // 读取文件为字节数组
                string base64String = Convert.ToBase64String(imageBytes);       // 字节数组转Base64
                // 获取图片MIME类型（如image/jpg，根据文件扩展名判断）
                string mimeType = "image/" + Path.GetExtension(openFileDialog.FileName).TrimStart('.');
                // 构建DataURL（格式：data:MIME类型;base64,Base64字符串，用于WPF Image控件直接渲染）
                string dataUrl = $"data:{mimeType};base64,{base64String}";
                UploadAndSendMessage(openFileDialog.FileName);
                // 4. 调用统一发送方法，发送图片消息（复用文本消息的乐观更新逻辑）
                await SendMessageInternal(dataUrl, MessageType.Image, mimeType);
            }
        }

        // 【Base64转ImageSource】：将Base64格式的图片数据转换为WPF可渲染的ImageSource
        private ImageSource CreateImageFromBase64(string dataUrl)
        {
            try
            {
                // 提取Base64字符串（DataURL格式：data:xxx;base64,xxx，从逗号后开始截取）
                var base64Data = dataUrl.Substring(dataUrl.IndexOf(',') + 1);
                // Base64字符串转字节数组
                byte[] imageBytes = Convert.FromBase64String(base64Data);
                // 用内存流读取字节数组（WPF BitmapImage需要流作为数据源）
                using (var ms = new MemoryStream(imageBytes))
                {
                    var bitmapImage = new BitmapImage();
                    // 初始化BitmapImage（BeginInit/EndInit是WPF的标准流程）
                    bitmapImage.BeginInit();
                    // 缓存选项：加载后立即缓存，避免流关闭后图片失效
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    // 设置流数据源
                    bitmapImage.StreamSource = ms;
                    bitmapImage.EndInit();
                    // 冻结对象：使ImageSource可跨线程访问，提升性能（WPF自由线程对象）
                    bitmapImage.Freeze();
                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                // 捕获转换异常（如Base64格式错误、图片损坏），输出日志
                Debug.WriteLine($"Base64转图片失败：{ex.Message}");
                return null;  // 转换失败返回null，UI可显示默认图片
            }
        }

        // 【发送文本消息命令】：绑定UI的“发送”按钮，触发文本消息发送
        [RelayCommand(CanExecute = nameof(CanSendMessage))]  // CanExecute控制命令是否可用（依赖CanSendMessage方法）
        private async Task SendMessage()
        {
            if (SelectedSession == null) return;

            string messageContent = NewMessageText;
            NewMessageText = string.Empty;

            // 1. 乐观更新UI
            var pendingMessage = new ChatMessage
            {
                SenderName = "我",
                IsSentByMe = true,
                Type = MessageType.Text,
                Content = messageContent,
                AvatarUrl = GetRandomAvatarUrl(),
                SentAt = DateTime.UtcNow
            };
            Messages.Add(pendingMessage);

            // 2. 调用HTTP服务发送
            var savedMessageDto = await _chatService.SendMessageAsync(new SendMessageDto
            {
                ReceiverId = SelectedSession.Id,
                Content = messageContent
            });

            // 3. 状态同步
            Messages.Remove(pendingMessage);
            if (savedMessageDto != null)
            {
                // 发送成功，用后端返回的权威数据更新UI
                Messages.Add(ConvertDtoToChatMessage(savedMessageDto, SelectedSession));
            }
            else
            {
                // 发送失败
                //var failedMessage = pendingMessage with { Content = $"{pendingMessage.Content}\n(发送失败)" };
                //Messages.Add(failedMessage);
            }
        }

        // 【发送命令可用状态判断】：控制“发送”按钮是否可点击（输入框非空则可用）
        private bool CanSendMessage()
        {
            // 检查输入框内容：非null、非空字符串、非纯空格
            return !string.IsNullOrWhiteSpace(NewMessageText);
        }

        // 【外部调用方法】：供父ViewModel（如好友列表ViewModel）调用，启动与指定好友的聊天
        public void StartChatWith(Friend friend)
        {
            // 检查好友对象是否为空（避免空引用）
            if (friend == null) return;

            // 检查会话列表中是否已存在该好友的会话（根据好友ID匹配）
            var session = Sessions.FirstOrDefault(s => s.Id == friend.Id);
            if (session == null)
            {
                // 不存在：创建新会话，插入到会话列表顶部（最新会话优先显示）
                session = new ChatSession
                {
                    Id = friend.Id,          // 会话ID = 好友ID
                    Name = friend.Name,      // 会话名称 = 好友昵称
                    AvatarUrl = friend.AvatarUrl  // 会话头像 = 好友头像
                };
                Sessions.Insert(0, session);
            }

            // 关键：将新会话/已有会话设为当前选中会话（触发OnSelectedSessionChanged，加载消息）
            SelectedSession = session;
        }

        // 【统一消息发送方法】：整合文本/图片消息的发送逻辑，支持乐观更新和失败处理
        private async Task SendMessageInternal(string content, MessageType type, string mimeType = null)
        {
            // 检查是否有选中的会话（无会话则不发送）
            if (SelectedSession == null) return;

            // 1. 乐观更新UI：先添加“发送中”的临时消息（提升用户体验，无需等待后端响应）
            var pendingMessage = new ChatMessage
            {
                Type = type,                          // 消息类型（文本/图片）
                // 内容：文本消息显示输入内容，图片消息显示“[图片]”占位
                Content = type == MessageType.Text ? content : "[图片]",
                // 图片源：图片消息则从DataURL转换为ImageSource（前端预览），文本消息为null
                ImageData = type == MessageType.Image ? CreateImageFromBase64(content) : null,
                SenderName = "我",                    // 发送者名称
                IsSentByMe = true,                   // 自己发送的消息
                AvatarUrl = GetRandomAvatarUrl(),    // 临时随机 头像
                SentAt = DateTime.UtcNow             // 临时发送时间（后续用后端时间替换）
            };
            Messages.Add(pendingMessage);

            // 2. 异步调用ChatService发送消息到后端（传递会话ID、内容、MIME类型）
            var savedMessageDto = await _chatService.SendMessageAsync(SelectedSession.Id, content, mimeType);

            // 3. 处理后端响应结果
            Messages.Remove(pendingMessage);  // 先移除临时消息
            if (savedMessageDto != null)
            {
                // 发送成功：添加后端返回的正式消息（用后端数据覆盖临时数据）
                var finalMessage = new ChatMessage
                {
                    Id = savedMessageDto.Id,                  // 后端生成的消息ID
                    SenderId = savedMessageDto.SenderId,      // 发送者ID（当前用户）
                    Content = savedMessageDto.Content,        // 后端存储的内容（文本/图片标识）
                    SentAt = savedMessageDto.SentAt,          // 后端记录的发送时间（更准确）
                    // 消息类型：根据后端返回的MIME类型判断（有则为图片，无则为文本）
                    Type = string.IsNullOrEmpty(savedMessageDto.MimeType) ? MessageType.Text : MessageType.Image,
                    // 图片源：如果后端返回Base64，转换为ImageSource（用于渲染图片）
                    ImageData = !string.IsNullOrEmpty(savedMessageDto.ImageDataBase64) ?
                        CreateImageFromBase64($"data:{savedMessageDto.MimeType};base64,{savedMessageDto.ImageDataBase64}") : null,
                    IsSentByMe = true,                       // 自己发送
                    SenderName = "我",                        // 发送者名称
                    AvatarUrl = pendingMessage.AvatarUrl      // 复用临时消息的头像
                };
                Messages.Add(finalMessage);
            }
            else
            {
                // 发送失败：添加“发送失败”的消息（保留原内容，追加失败提示）
                //var failedMessage = pendingMessage with { Content = $"{pendingMessage.Content}\n(发送失败)" };
                //Messages.Add(failedMessage);
            }
        }

        // 【加载会话详情】：选中会话后，加载该会话的历史消息和群成员（单聊则成员列表为空）
        private async void LoadChatDetails(ChatSession session)
        {
            Debug.WriteLine($"进入LoadChatDetails()，会话：{session}");
            // 清空现有数据（避免残留上一个会话的消息/成员）
            Messages.Clear();
            Members.Clear();

            // 获取当前登录用户信息（未登录则不加载）
            var currentUserId = _authService.GetCurrentUserId();
            var currentUserName = _authService.GetCurrentUserName();
            Debug.WriteLine($"当前登录用户：ID={currentUserId}，Name={currentUserName}");
            if (currentUserId == null)
            {
                // 用户未登录：终止加载（避免API调用失败）
                return;
            }

            // 2. 异步调用ChatService获取会话历史消息（传递会话ID）
            var historyDtos = await _chatService.GetHistoryAsync(session.Id);

            if (historyDtos != null)
            {
                // 3. 转换历史消息DTO为UI消息对象（批量转换）
                var uiMessages = historyDtos.Select(dto => new ChatMessage
                {
                    // 从DTO复制核心数据（后端传递的不可变数据）
                    Id = dto.Id,
                    SenderId = dto.SenderId,
                    Content = dto.Content,
                    SentAt = dto.SentAt,
                    // 图片URL：临时逻辑（后续可替换为后端返回的ImageUrl）
                    ImageUrl = IsImageUrl(dto.Content) ? dto.Content : GetRandomAvatarUrl(),

                    // 计算UI所需的动态属性
                    IsSentByMe = dto.SenderId == currentUserId.Value,  // 是否自己发送
                    // 发送者名称：自己显示“我”，他人显示会话名称（群聊/单聊通用）
                    SenderName = (dto.SenderId == currentUserId.Value) ? "我" : session.Name,
                    AvatarUrl = GetRandomAvatarUrl(),  // 临时随机头像
                    // 消息类型：根据MIME类型判断（有则为图片，无则为文本）
                    Type = string.IsNullOrEmpty(dto.MimeType) ? MessageType.Text : MessageType.Image,
                    // 图片源：如果后端返回Base64，转换为ImageSource（用于渲染历史图片消息）
                    ImageData = !string.IsNullOrEmpty(dto.ImageDataBase64) ?
                        CreateImageFromBase64($"data:{dto.MimeType};base64,{dto.ImageDataBase64}") : null,
                });

                // 4. 批量添加历史消息到UI列表（避免多次触发UI更新，提升性能）
                foreach (var msg in uiMessages)
                {
                    Messages.Add(msg);
                }
            }
            // TODO：群聊场景需添加“加载群成员”的逻辑（调用MembersService获取成员列表）
        }

        // 【图片URL判断】：简单判断消息内容是否为图片（根据后端约定的URL前缀“/images/”）
        private bool IsImageUrl(string content)
        {
            // 检查内容：非空 + 以“/images/”开头（后端图片存储的URL前缀）
            if (string.IsNullOrEmpty(content)) return false;
            return content.StartsWith("/images/");
        }
    }
}