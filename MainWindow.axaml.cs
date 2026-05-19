using Avalonia.Controls;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Text.Json;
using System.IO;

namespace TaskApp;

public partial class MainWindow : Window
{

    private const string FileName = "tasks.json";

    private List<TaskItem> _tasks = new();

    public MainWindow()
    {
        InitializeComponent();
        LoadTasks();
        RefreshTaskList();
        UpdateStatus();
    }

    private void AddButton_Click(object? sender, RoutedEventArgs e)
    {
        AddTask();
    }

    private void RefreshTaskList()
    {

        var filteredItems = FilterBox.SelectedIndex == 0 ? _tasks : FilterBox.SelectedIndex == 1 ? _tasks.Where(task => !task.IsDone) : _tasks.Where(task => task.IsDone);

        var sortedItems = filteredItems.OrderByDescending(task => task.Priority).ToList();

        TaskList.SelectedItem = null;
        TaskList.ItemsSource = null;
        TaskList.ItemsSource = sortedItems;
        EmptyText.IsVisible = sortedItems.Count == 0;
    }

    private void UpdateStatus()
    {
        var totalCount = _tasks.Count;
        var doneCount = _tasks.Count(task => task.IsDone);
        var notDoneCount = totalCount - doneCount;

        StatusText.Text = $"Celkem: {totalCount}, Hotovo: {doneCount}, Zbyva: {notDoneCount}";
    }

    private void DeleteButton_Click(object? sender, RoutedEventArgs e)
    {
        var selectedTask = TaskList.SelectedItem as TaskItem;

        if (selectedTask == null)
        {
            return;
        }

        _tasks = _tasks.Where(task => task != selectedTask).ToList();
        SaveTasks();
        RefreshTaskList();
        UpdateStatus();
    }

    private void UpdateButton_Click(object? sender, RoutedEventArgs e)
    {

        var selectedTask = TaskList.SelectedItem as TaskItem;

        if (selectedTask is null)
        {
            return;
        }

        selectedTask.IsDone = !selectedTask.IsDone;
        SaveTasks();
        RefreshTaskList();
        UpdateStatus();
    }

    private void DeleteAllIsDoneButton_Click(object? sender, RoutedEventArgs e)
    {
        _tasks = _tasks.Where(task => !task.IsDone).ToList();

        SaveTasks();
        RefreshTaskList();
        UpdateStatus();
    }

    private void TaskInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AddTask();
        }
    }

    private void AddTask()
    {
        var text = TaskInput.Text?.Trim();
        var priority = (int)(PriorityInput.Value ?? 3);

        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        _tasks.Add(new TaskItem { Text = text, Priority = priority });
        SaveTasks();

        TaskInput.Text = string.Empty;
        PriorityInput.Value = 3;
        TaskInput.Focus();
        FilterBox.SelectedIndex = 0;
        RefreshTaskList();
        UpdateStatus();
    }

    private void SaveTasks()
    {
        var json = JsonSerializer.Serialize(_tasks);
        File.WriteAllText(FileName, json);
    }

    private void LoadTasks()
    {

        try
        {
            if (!File.Exists(FileName))
            {
                return;
            }

            var json = File.ReadAllText(FileName);

            var loadedTasks = JsonSerializer.Deserialize<List<TaskItem>>(json);

            if (loadedTasks != null)
            {
                _tasks = loadedTasks;
            }
        }
        catch
        {
            _tasks = new List<TaskItem>();
        }
    }

    private void DeleteAllButton_Click(object? sender, RoutedEventArgs e)
    {
        _tasks = new List<TaskItem>();
        SaveTasks();
        RefreshTaskList();
        UpdateStatus();
    }

    private void FilterBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TaskList == null || EmptyText == null)
        {
            return;
        }

        RefreshTaskList();
    }
}

public class TaskItem
{
    public string Text { get; set; } = "";
    public bool IsDone { get; set; } = false;
    public int Priority { get; set; } = 3;

    public override string ToString()
    {
        return IsDone ? $"[x] ({Priority}) {Text}" : $"[ ] ({Priority}) {Text}";
    }
}