using System.Windows;
using System.Windows.Media;
using RadicleWindows.Core.Interfaces;
using RadicleWindows.Core.Services;

namespace RadicleWindows.Wpf;

public partial class MainWindow : Window
{
    private readonly IRadicleService _radicle;

    public MainWindow()
    {
        InitializeComponent();
        IProcessExecutor executor = new ProcessExecutor();
        _radicle = new RadicleService(executor);
        Loaded += async (_, _) => await RefreshDashboardAsync();
    }

    // ── Navigation ────────────────────────────────────────────

    private void ShowPanel(string panel)
    {
        DashboardPanel.Visibility = panel == "dashboard" ? Visibility.Visible : Visibility.Collapsed;
        RepositoriesPanel.Visibility = panel == "repos" ? Visibility.Visible : Visibility.Collapsed;
        IssuesPanel.Visibility = panel == "issues" ? Visibility.Visible : Visibility.Collapsed;
        PatchesPanel.Visibility = panel == "patches" ? Visibility.Visible : Visibility.Collapsed;
        NodePanel.Visibility = panel == "node" ? Visibility.Visible : Visibility.Collapsed;
        IdentityPanel.Visibility = panel == "identity" ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void OnDashboardClick(object sender, RoutedEventArgs e)
    {
        ShowPanel("dashboard");
        await RefreshDashboardAsync();
    }

    private async void OnRepositoriesClick(object sender, RoutedEventArgs e)
    {
        ShowPanel("repos");
        await RefreshReposAsync();
    }

    private async void OnIssuesClick(object sender, RoutedEventArgs e)
    {
        ShowPanel("issues");
        await RefreshIssuesAsync();
    }

    private async void OnPatchesClick(object sender, RoutedEventArgs e)
    {
        ShowPanel("patches");
        await RefreshPatchesAsync();
    }

    private async void OnNodeClick(object sender, RoutedEventArgs e)
    {
        ShowPanel("node");
        await RefreshNodeStatusAsync();
    }

    private async void OnIdentityClick(object sender, RoutedEventArgs e)
    {
        ShowPanel("identity");
        await RefreshIdentityAsync();
    }

    // ── Refresh handlers ──────────────────────────────────────

    private async void OnRefreshReposClick(object sender, RoutedEventArgs e) => await RefreshReposAsync();
    private async void OnRefreshIssuesClick(object sender, RoutedEventArgs e) => await RefreshIssuesAsync();
    private async void OnRefreshPatchesClick(object sender, RoutedEventArgs e) => await RefreshPatchesAsync();
    private async void OnRefreshNodeClick(object sender, RoutedEventArgs e) => await RefreshNodeStatusAsync();
    private async void OnRefreshIdentityClick(object sender, RoutedEventArgs e) => await RefreshIdentityAsync();

    private async void OnStartNodeClick(object sender, RoutedEventArgs e)
    {
        SetStatus("Starting node…");
        await _radicle.StartNodeAsync();
        await RefreshNodeStatusAsync();
    }

    private async void OnStopNodeClick(object sender, RoutedEventArgs e)
    {
        SetStatus("Stopping node…");
        await _radicle.StopNodeAsync();
        await RefreshNodeStatusAsync();
    }

    // ── Data loading ──────────────────────────────────────────

    private async Task RefreshDashboardAsync()
    {
        SetStatus("Loading dashboard…");
        try
        {
            var repos = await _radicle.ListRepositoriesAsync();
            RepoCountText.Text = repos.Count.ToString();

            var issues = await _radicle.ListIssuesAsync();
            IssueCountText.Text = issues.Count.ToString();

            var patches = await _radicle.ListPatchesAsync();
            PatchCountText.Text = patches.Count.ToString();

            await RefreshNodeStatusAsync();
            SetStatus("Ready");
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}");
        }
    }

    private async Task RefreshReposAsync()
    {
        SetStatus("Loading repositories…");
        try
        {
            var repos = await _radicle.ListRepositoriesAsync();
            ReposListBox.Items.Clear();
            foreach (var repo in repos)
                ReposListBox.Items.Add($"{repo.Rid}  {repo.Name}  {repo.Description}");
            if (repos.Count == 0)
                ReposListBox.Items.Add("No repositories found. Use 'rad init' or 'rad clone' to get started.");
            SetStatus($"Loaded {repos.Count} repositories");
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}");
        }
    }

    private async Task RefreshIssuesAsync()
    {
        SetStatus("Loading issues…");
        try
        {
            var issues = await _radicle.ListIssuesAsync();
            IssuesListBox.Items.Clear();
            foreach (var issue in issues)
                IssuesListBox.Items.Add($"[{issue.State}] {issue.Id}  {issue.Title}  (by {issue.Author})");
            if (issues.Count == 0)
                IssuesListBox.Items.Add("No issues found.");
            SetStatus($"Loaded {issues.Count} issues");
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}");
        }
    }

    private async Task RefreshPatchesAsync()
    {
        SetStatus("Loading patches…");
        try
        {
            var patches = await _radicle.ListPatchesAsync();
            PatchesListBox.Items.Clear();
            foreach (var patch in patches)
                PatchesListBox.Items.Add($"[{patch.State}] {patch.Id}  {patch.Title}  (by {patch.Author})");
            if (patches.Count == 0)
                PatchesListBox.Items.Add("No patches found.");
            SetStatus($"Loaded {patches.Count} patches");
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}");
        }
    }

    private async Task RefreshNodeStatusAsync()
    {
        try
        {
            var status = await _radicle.GetNodeStatusAsync();
            NodeStatusText.Text = status.IsRunning ? "Node: Running" : "Node: Stopped";
            NodeIndicator.Fill = status.IsRunning
                ? (Brush)FindResource("RadicleSuccess")
                : (Brush)FindResource("RadicleError");
            NodeDetailText.Text = status.StatusMessage;
        }
        catch (Exception ex)
        {
            NodeStatusText.Text = "Node: Error";
            NodeDetailText.Text = ex.Message;
        }
    }

    private async Task RefreshIdentityAsync()
    {
        SetStatus("Loading identity…");
        try
        {
            var profile = await _radicle.GetProfileAsync();
            if (profile is not null)
            {
                IdentityText.Text = $"""
                    Alias      : {profile.Alias}
                    DID        : {profile.Did}
                    Node ID    : {profile.NodeId}
                    Home       : {profile.Home}
                    SSH Agent  : {profile.SshAgent}
                    """;
            }
            else
            {
                IdentityText.Text = "No identity found. Run 'rad auth' to create one.";
            }
            SetStatus("Ready");
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}");
        }
    }

    private void SetStatus(string message)
    {
        StatusBarText.Text = message;
    }
}
