using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using App_LernPlaner.Models;
using App_LernPlaner.Services;

namespace App_LernPlaner.Views.Chat
{
    public class ContactRowVm
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int UnreadCount { get; set; }
        public Visibility HasUnread => UnreadCount > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public class MessageRowVm
    {
        public int MessageId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string TimeText { get; set; } = string.Empty;
        public bool IsOwn { get; set; }
        public bool IsEdited { get; set; }
        public HorizontalAlignment Alignment => IsOwn ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        public Visibility OwnMessageVisibility => IsOwn ? Visibility.Visible : Visibility.Collapsed;
        public Visibility EditedVisibility => IsEdited ? Visibility.Visible : Visibility.Collapsed;
    }

    // Chat zwischen allen Benutzern - rein DB-basiert, Aktualisierung per Timer (kein WebSocket).
    public partial class ChatPage : Page
    {
        private readonly MessageService _messages = new();
        private readonly DispatcherTimer _pollTimer;
        private int? _selectedContactId;
        private int? _editingMessageId;

        public ChatPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await LoadContactsAsync();

            _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _pollTimer.Tick += async (_, _) =>
            {
                if (_selectedContactId.HasValue) await LoadConversationAsync(_selectedContactId.Value);
            };
            _pollTimer.Start();
            Unloaded += (_, _) => _pollTimer.Stop();
        }

        private async System.Threading.Tasks.Task LoadContactsAsync()
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            var contacts = await _messages.GetContactsAsync(user.UserId);
            ContactsList.ItemsSource = contacts.Select(c => new ContactRowVm
            {
                UserId = c.UserId,
                DisplayName = c.DisplayName,
                Role = c.Role,
                UnreadCount = c.UnreadCount
            }).ToList();
        }

        private async void ContactsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContactsList.SelectedItem is not ContactRowVm contact) return;

            _selectedContactId = contact.UserId;
            LblConversationWith.Text = $"Konversation mit {contact.DisplayName}";
            ResetEditState();
            await LoadConversationAsync(contact.UserId);
        }

        private async System.Threading.Tasks.Task LoadConversationAsync(int otherUserId)
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            var conversation = await _messages.GetConversationAsync(user.UserId, otherUserId);
            MessagesList.ItemsSource = conversation.Select(m => new MessageRowVm
            {
                MessageId = m.MessageId,
                SenderName = m.SenderId == user.UserId ? "Du" : m.SenderName,
                Body = m.Body,
                TimeText = m.TimeText,
                IsOwn = m.SenderId == user.UserId && !m.IsDeleted,
                IsEdited = m.EditedAt.HasValue
            }).ToList();

            await _messages.MarkConversationReadAsync(user.UserId, otherUserId);
            MessagesScroll.ScrollToBottom();

            await LoadContactsAsync();
        }

        private async void BtnSend_Click(object sender, RoutedEventArgs e) => await SendOrUpdateAsync();

        private async void TxtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) await SendOrUpdateAsync();
        }

        private async System.Threading.Tasks.Task SendOrUpdateAsync()
        {
            var user = sessionManager.CurrentUser;
            if (user == null || _selectedContactId == null) return;

            string text = TxtMessage.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            if (_editingMessageId.HasValue)
            {
                await _messages.EditAsync(_editingMessageId.Value, user.UserId, text);
                ResetEditState();
            }
            else
            {
                await _messages.SendAsync(user.UserId, _selectedContactId.Value, text);
            }

            TxtMessage.Text = string.Empty;
            await LoadConversationAsync(_selectedContactId.Value);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int messageId) return;
            if (MessagesList.ItemsSource is not IEnumerable<MessageRowVm> rows) return;

            var msg = rows.FirstOrDefault(m => m.MessageId == messageId);
            if (msg == null) return;

            _editingMessageId = messageId;
            TxtMessage.Text = msg.Body;
            TxtMessage.Focus();
        }

        private void ResetEditState()
        {
            _editingMessageId = null;
            TxtMessage.Text = string.Empty;
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var user = sessionManager.CurrentUser;
            if (user == null || sender is not Button btn || btn.Tag is not int messageId || _selectedContactId == null) return;

            var result = MessageBox.Show("Diese Nachricht wirklich löschen?", "Bestätigen", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            await _messages.DeleteAsync(messageId, user.UserId);
            await LoadConversationAsync(_selectedContactId.Value);
        }
    }
}
