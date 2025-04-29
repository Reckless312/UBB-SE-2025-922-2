// <copyright file="MainPageViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace App1.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using App1.AutoChecker;
    using App1.Models;
    using App1.Services;
    using LiveChartsCore;
    using LiveChartsCore.Kernel.Sketches;
    using LiveChartsCore.SkiaSharpView;

    /// <summary>
    /// View model for the main page that handles review moderation and user management.
    /// </summary>
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly IReviewService reviewsService;
        private readonly IUserService userService;
        private readonly ICheckersService checkersService;
        private readonly IUpgradeRequestsService requestsService;
        private readonly IAutoCheck autoCheck;

        private ObservableCollection<Review> flaggedReviews;
        private ObservableCollection<User> appealsUsers;
        private ObservableCollection<UpgradeRequest> upgradeRequests;
        private ObservableCollection<string> offensiveWords;
        private ISeries[] pieChartSeries;
        private ISeries[] barChartSeries;
        private IEnumerable<ICartesianAxis> barChartXAxes;
        private IEnumerable<ICartesianAxis> barChartYAxes;
        private User selectedAppealUser;
        private UpgradeRequest selectedUpgradeRequest;
        private ObservableCollection<string> userReviewsFormatted;
        private ObservableCollection<string> userReviewsWithFlags;
        private string userStatusDisplay;
        private string userUpgradeInfo;
        private bool isAppealUserBanned = true;
        private bool isWordListVisible = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPageViewModel"/> class.
        /// </summary>
        /// <param name="reviewsService">The service for handling review data.</param>
        /// <param name="userService">The service for managing user data.</param>
        /// <param name="upgradeRequestsService">The service for handling role upgrade requests.</param>
        /// <param name="checkersService">The service for content checking operations.</param>
        /// <param name="autoCheck">The automated content checking service.</param>
        public MainPageViewModel(
            IReviewService reviewsService,
            IUserService userService,
            IUpgradeRequestsService upgradeRequestsService,
            ICheckersService checkersService,
            IAutoCheck autoCheck)
        {
            this.reviewsService = reviewsService ?? throw new ArgumentNullException(nameof(reviewsService));
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.requestsService = upgradeRequestsService ?? throw new ArgumentNullException(nameof(upgradeRequestsService));
            this.checkersService = checkersService ?? throw new ArgumentNullException(nameof(checkersService));
            this.autoCheck = autoCheck ?? throw new ArgumentNullException(nameof(autoCheck));

            this.InitializeCommands();

            this.LoadAllData();
        }

        /// <summary>
        /// Event that fires when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the command to maintain a user's ban status.
        /// </summary>
        public ICommand KeepBanCommand { get; private set; }

        /// <summary>
        /// Gets the command to accept a user's appeal.
        /// </summary>
        public ICommand AcceptAppealCommand { get; private set; }

        /// <summary>
        /// Gets the command to close an appeal case.
        /// </summary>
        public ICommand CloseAppealCaseCommand { get; private set; }

        /// <summary>
        /// Gets the command to handle an upgrade request.
        /// </summary>
        public ICommand HandleUpgradeRequestCommand { get; private set; }

        /// <summary>
        /// Gets the command to reset flags on a review.
        /// </summary>
        public ICommand ResetReviewFlagsCommand { get; private set; }

        /// <summary>
        /// Gets the command to hide a review.
        /// </summary>
        public ICommand HideReviewCommand { get; private set; }

        /// <summary>
        /// Gets the command to run an AI check on a review.
        /// </summary>
        public ICommand RunAICheckCommand { get; private set; }

        /// <summary>
        /// Gets the command to run an automatic check on all flagged reviews.
        /// </summary>
        public ICommand RunAutoCheckCommand { get; private set; }

        /// <summary>
        /// Gets the command to add a word to the offensive word list.
        /// </summary>
        public ICommand AddOffensiveWordCommand { get; private set; }

        /// <summary>
        /// Gets the command to delete a word from the offensive word list.
        /// </summary>
        public ICommand DeleteOffensiveWordCommand { get; private set; }

        /// <summary>
        /// Gets the command to show the offensive word list popup.
        /// </summary>
        public ICommand ShowWordListPopupCommand { get; private set; }

        /// <summary>
        /// Gets the command to hide the offensive word list popup.
        /// </summary>
        public ICommand HideWordListPopupCommand { get; private set; }

        /// <summary>
        /// Gets or sets the collection of flagged reviews.
        /// </summary>
        public ObservableCollection<Review> FlaggedReviews
        {
            get => this.flaggedReviews;
            set
            {
                this.flaggedReviews = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the collection of users with active appeals.
        /// </summary>
        public ObservableCollection<User> AppealsUsers
        {
            get => this.appealsUsers;
            set
            {
                this.appealsUsers = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the collection of upgrade requests.
        /// </summary>
        public ObservableCollection<UpgradeRequest> UpgradeRequests
        {
            get => this.upgradeRequests;
            set
            {
                this.upgradeRequests = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the collection of offensive words.
        /// </summary>
        public ObservableCollection<string> OffensiveWords
        {
            get => this.offensiveWords;
            set
            {
                this.offensiveWords = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the pie chart series for user role statistics.
        /// </summary>
        public ISeries[] PieChartSeries
        {
            get => this.pieChartSeries;
            set
            {
                this.pieChartSeries = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the bar chart series for review statistics.
        /// </summary>
        public ISeries[] BarChartSeries
        {
            get => this.barChartSeries;
            set
            {
                this.barChartSeries = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the X-axes for the bar chart.
        /// </summary>
        public IEnumerable<ICartesianAxis> BarChartXAxes
        {
            get => this.barChartXAxes;
            set
            {
                this.barChartXAxes = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the Y-axes for the bar chart.
        /// </summary>
        public IEnumerable<ICartesianAxis> BarChartYAxes
        {
            get => this.barChartYAxes;
            set
            {
                this.barChartYAxes = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the currently selected user for appeal review.
        /// </summary>
        public User SelectedAppealUser
        {
            get => this.selectedAppealUser;
            set
            {
                this.selectedAppealUser = value;
                if (value != null)
                {
                    this.LoadUserAppealDetails(value);
                }

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the currently selected upgrade request.
        /// </summary>
        public UpgradeRequest SelectedUpgradeRequest
        {
            get => this.selectedUpgradeRequest;
            set
            {
                this.selectedUpgradeRequest = value;
                if (value != null)
                {
                    this.LoadUpgradeRequestDetails(value);
                }

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the formatted reviews for the selected user.
        /// </summary>
        public ObservableCollection<string> UserReviewsFormatted
        {
            get => this.userReviewsFormatted;
            set
            {
                this.userReviewsFormatted = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the reviews with flag information for the selected user.
        /// </summary>
        public ObservableCollection<string> UserReviewsWithFlags
        {
            get => this.userReviewsWithFlags;
            set
            {
                this.userReviewsWithFlags = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the display text for user status information.
        /// </summary>
        public string UserStatusDisplay
        {
            get => this.userStatusDisplay;
            set
            {
                this.userStatusDisplay = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the display text for user upgrade information.
        /// </summary>
        public string UserUpgradeInfo
        {
            get => this.userUpgradeInfo;
            set
            {
                this.userUpgradeInfo = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the selected appeal user is banned.
        /// </summary>
        public bool IsAppealUserBanned
        {
            get => this.isAppealUserBanned;
            set
            {
                this.isAppealUserBanned = value;
                this.UserStatusDisplay = this.GetUserStatusDisplay(this.SelectedAppealUser, value);
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the offensive word list popup is visible.
        /// </summary>
        public bool IsWordListVisible
        {
            get => this.isWordListVisible;
            set
            {
                this.isWordListVisible = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Loads all data required by the view model.
        /// </summary>
        public void LoadAllData()
        {
            this.LoadFlaggedReviews();
            this.LoadAppeals();
            this.LoadRoleRequests();
            this.LoadStatistics();
            this.LoadOffensiveWords();
        }

        /// <summary>
        /// Loads the flagged reviews from the reviews service.
        /// </summary>
        public void LoadFlaggedReviews()
        {
            this.FlaggedReviews = new ObservableCollection<Review>(this.reviewsService.GetFlaggedReviews());
        }

        /// <summary>
        /// Loads the banned users who have submitted appeals.
        /// </summary>
        public void LoadAppeals()
        {
            this.AppealsUsers = new ObservableCollection<User>(this.userService.GetBannedUsersWhoHaveSubmittedAppeals());
        }

        /// <summary>
        /// Loads the role upgrade requests.
        /// </summary>
        public void LoadRoleRequests()
        {
            this.UpgradeRequests = new ObservableCollection<UpgradeRequest>(this.requestsService.RetrieveAllUpgradeRequests());
        }

        /// <summary>
        /// Loads the list of offensive words.
        /// </summary>
        public void LoadOffensiveWords()
        {
            this.OffensiveWords = new ObservableCollection<string>(this.checkersService.GetOffensiveWordsList());
        }

        /// <summary>
        /// Loads the statistical data for charts.
        /// </summary>
        public void LoadStatistics()
        {
            this.LoadPieChart();
            this.LoadBarChart();
        }

        /// <summary>
        /// Filters reviews based on content.
        /// </summary>
        /// <param name="filter">The filter string to apply.</param>
        public void FilterReviews(string filter)
        {
            this.FlaggedReviews = new ObservableCollection<Review>(
                this.reviewsService.FilterReviewsByContent(filter));
        }

        /// <summary>
        /// Filters appeals based on user information.
        /// </summary>
        /// <param name="filter">The filter string to apply.</param>
        public void FilterAppeals(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                this.LoadAppeals();
                return;
            }

            filter = filter.ToLower();
            this.AppealsUsers = new ObservableCollection<User>(
                this.userService.GetBannedUsersWhoHaveSubmittedAppeals()
                    .Where(user =>
                        user.EmailAddress.ToLower().Contains(filter) ||
                        user.FullName.ToLower().Contains(filter) ||
                        user.UserId.ToString().Contains(filter))
                    .ToList());
        }

        /// <summary>
        /// Resets the flags on a review.
        /// </summary>
        /// <param name="reviewId">The ID of the review to reset.</param>
        public void ResetReviewFlags(int reviewId)
        {
            this.reviewsService.ResetReviewFlags(reviewId);
            this.LoadFlaggedReviews();
            this.LoadStatistics();
        }

        /// <summary>
        /// Hides a review and resets its flags.
        /// </summary>
        /// <param name="userId">The ID of the user who owns the review.</param>
        /// <param name="reviewId">The ID of the review to hide.</param>
        public void HideReview(int userId, int reviewId)
        {
            this.reviewsService.HideReview(userId);
            this.reviewsService.ResetReviewFlags(reviewId);
            this.LoadFlaggedReviews();
            this.LoadStatistics();
        }

        /// <summary>
        /// Runs an AI check on a single review.
        /// </summary>
        /// <param name="review">The review to check.</param>
        public void RunAICheck(Review review)
        {
            this.checkersService.RunAICheckForOneReview(review);
            this.LoadFlaggedReviews();
        }

        /// <summary>
        /// Runs an automatic check on all flagged reviews.
        /// </summary>
        /// <returns>A list of messages from the checker.</returns>
        public List<string> RunAutoCheck()
        {
            List<Review> reviews = this.reviewsService.GetFlaggedReviews();
            List<string> messages = this.checkersService.RunAutoCheck(reviews);
            this.LoadFlaggedReviews();
            this.LoadStatistics();
            return messages;
        }

        /// <summary>
        /// Adds a word to the offensive words list.
        /// </summary>
        /// <param name="word">The word to add.</param>
        public void AddOffensiveWord(string word)
        {
            if (!string.IsNullOrWhiteSpace(word))
            {
                this.checkersService.AddOffensiveWord(word);
                this.LoadOffensiveWords();
            }
        }

        /// <summary>
        /// Deletes a word from the offensive words list.
        /// </summary>
        /// <param name="word">The word to delete.</param>
        public void DeleteOffensiveWord(string word)
        {
            this.checkersService.DeleteOffensiveWord(word);
            this.LoadOffensiveWords();
        }

        /// <summary>
        /// Handles a role upgrade request.
        /// </summary>
        /// <param name="approve">Whether to approve the request.</param>
        /// <param name="requestId">The ID of the request.</param>
        public void HandleUpgradeRequest(bool approve, int requestId)
        {
            this.requestsService.ProcessUpgradeRequest(approve, requestId);
            this.LoadRoleRequests();
            this.LoadStatistics();
        }

        /// <summary>
        /// Closes an appeal case for a user.
        /// </summary>
        /// <param name="user">The user whose appeal case should be closed.</param>
        public void CloseAppealCase(User user)
        {
            user.HasSubmittedAppeal = false;
            this.LoadAppeals();
        }

        /// <summary>
        /// Gets reviews by a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of reviews by the user.</returns>
        public List<Review> GetUserReviews(int userId)
        {
            return this.reviewsService.GetReviewsByUser(userId);
        }

        /// <summary>
        /// Gets a user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The user object.</returns>
        public User GetUserById(int userId)
        {
            return this.userService.GetUserById(userId);
        }

        /// <summary>
        /// Gets the highest role type for a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The highest role type.</returns>
        public RoleType GetHighestRoleTypeForUser(int userId)
        {
            return this.userService.GetHighestRoleTypeForUser(userId);
        }

        /// <summary>
        /// Gets the name of a role based on its ID.
        /// </summary>
        /// <param name="roleType">The role type.</param>
        /// <returns>The role name.</returns>
        public string GetRoleNameBasedOnID(RoleType roleType)
        {
            return this.requestsService.GetRoleNameBasedOnIdentifier(roleType);
        }

        /// <summary>
        /// Loads appeal details for a user.
        /// </summary>
        /// <param name="user">The user to load details for.</param>
        public void LoadUserAppealDetails(User user)
        {
            this.SelectedAppealUser = user;
            this.IsAppealUserBanned = true;
            this.UserStatusDisplay = this.GetUserStatusDisplay(user, true);

            List<Review> reviews = this.GetUserReviews(user.UserId);
            this.UserReviewsFormatted = new ObservableCollection<string>(
                reviews.Select(r => this.FormatReviewContent(r)).ToList());
        }

        /// <summary>
        /// Maintains a ban on a user.
        /// </summary>
        /// <param name="user">The user to keep banned.</param>
        public void KeepBanForUser(User user)
        {
            if (user == null)
            {
                return;
            }

            this.UpdateUserRole(user, RoleType.Banned);
            this.IsAppealUserBanned = true;
            this.UserStatusDisplay = this.GetUserStatusDisplay(user, true);
            this.LoadStatistics();
        }

        /// <summary>
        /// Accepts an appeal from a user, removing their ban.
        /// </summary>
        /// <param name="user">The user to unban.</param>
        public void AcceptAppealForUser(User user)
        {
            if (user == null)
            {
                return;
            }

            this.UpdateUserRole(user, RoleType.User);
            this.IsAppealUserBanned = false;
            this.UserStatusDisplay = this.GetUserStatusDisplay(user, false);
            this.LoadStatistics();
        }

        /// <summary>
        /// Loads upgrade request details.
        /// </summary>
        /// <param name="request">The upgrade request to load details for.</param>
        public void LoadUpgradeRequestDetails(UpgradeRequest request)
        {
            this.SelectedUpgradeRequest = request;

            int userId = request.RequestingUserIdentifier;
            User selectedUser = this.GetUserById(userId);
            RoleType currentRoleID = this.GetHighestRoleTypeForUser(selectedUser.UserId);
            string currentRoleName = this.GetRoleNameBasedOnID(currentRoleID);
            string requiredRoleName = this.GetRoleNameBasedOnID(currentRoleID + 1);

            this.UserUpgradeInfo = this.FormatUserUpgradeInfo(selectedUser, currentRoleName, requiredRoleName);

            List<Review> reviews = this.GetUserReviews(selectedUser.UserId);
            this.UserReviewsWithFlags = new ObservableCollection<string>(
                reviews.Select(r => this.FormatReviewWithFlags(r)).ToList());
        }

        /// <summary>
        /// Formats the user status display text.
        /// </summary>
        /// <param name="user">The user to display status for.</param>
        /// <param name="isBanned">Whether the user is banned.</param>
        /// <returns>The formatted status display text.</returns>
        public string GetUserStatusDisplay(User user, bool isBanned)
        {
            if (user == null)
            {
                return string.Empty;
            }

            return $"User ID: {user.UserId}\nEmail: {user.EmailAddress}\nStatus: {(isBanned ? "Banned" : "Active")}";
        }

        /// <summary>
        /// Formats the user upgrade information text.
        /// </summary>
        /// <param name="user">The user to display upgrade info for.</param>
        /// <param name="currentRoleName">The user's current role name.</param>
        /// <param name="requiredRoleName">The role name the user is requesting.</param>
        /// <returns>The formatted upgrade info text.</returns>
        public string FormatUserUpgradeInfo(User user, string currentRoleName, string requiredRoleName)
        {
            if (user == null)
            {
                return string.Empty;
            }

            return $"User ID: {user.UserId}\nEmail: {user.EmailAddress}\n{currentRoleName} -> {requiredRoleName}";
        }

        /// <summary>
        /// Formats a review's content for display.
        /// </summary>
        /// <param name="review">The review to format.</param>
        /// <returns>The formatted review content.</returns>
        public string FormatReviewContent(Review review)
        {
            if (review == null)
            {
                return string.Empty;
            }

            return $"{review.Content}";
        }

        /// <summary>
        /// Formats a review with flag information for display.
        /// </summary>
        /// <param name="review">The review to format.</param>
        /// <returns>The formatted review with flag information.</returns>
        public string FormatReviewWithFlags(Review review)
        {
            if (review == null)
            {
                return string.Empty;
            }

            return $"{review.Content}\nFlags: {review.NumberOfFlags}";
        }

        /// <summary>
        /// Shows the offensive word list popup.
        /// </summary>
        public void ShowWordListPopup()
        {
            this.IsWordListVisible = true;
        }

        /// <summary>
        /// Hides the offensive word list popup.
        /// </summary>
        public void HideWordListPopup()
        {
            this.IsWordListVisible = false;
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Loads the bar chart data.
        /// </summary>
        private void LoadBarChart()
        {
            int rejectedCount = this.reviewsService.GetHiddenReviews().Count;
            int pendingCount = this.reviewsService.GetFlaggedReviews().Count;
            int totalCount = this.reviewsService.GetAllReviews().Count;

            this.BarChartSeries = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = new double[] { rejectedCount, pendingCount, totalCount },
                },
            };

            this.BarChartXAxes = new[]
            {
                new Axis { Labels = new List<string> { "rejected", "pending", "total" } },
            };

            this.BarChartYAxes = new[]
            {
                new Axis { Name = "Total", MinLimit = 0 },
            };
        }

        /// <summary>
        /// Initializes the commands used by the view model.
        /// </summary>
        private void InitializeCommands()
        {
            this.KeepBanCommand = new RelayCommand(() => this.KeepBanForUser(this.SelectedAppealUser));
            this.AcceptAppealCommand = new RelayCommand(() => this.AcceptAppealForUser(this.SelectedAppealUser));
            this.CloseAppealCaseCommand = new RelayCommand(() => this.CloseAppealCase(this.SelectedAppealUser));

            this.HandleUpgradeRequestCommand = new RelayCommand<Tuple<bool, int>>(param =>
                this.HandleUpgradeRequest(param.Item1, param.Item2));

            this.ResetReviewFlagsCommand = new RelayCommand<int>(reviewId =>
                this.ResetReviewFlags(reviewId));

            this.HideReviewCommand = new RelayCommand<Tuple<int, int>>(param =>
                this.HideReview(param.Item1, param.Item2));

            this.RunAICheckCommand = new RelayCommand<Review>(review =>
                this.RunAICheck(review));

            this.RunAutoCheckCommand = new RelayCommand(() => this.RunAutoCheck());

            this.AddOffensiveWordCommand = new RelayCommand<string>(word =>
                this.AddOffensiveWord(word));

            this.DeleteOffensiveWordCommand = new RelayCommand<string>(word =>
                this.DeleteOffensiveWord(word));

            this.ShowWordListPopupCommand = new RelayCommand(() => this.ShowWordListPopup());
            this.HideWordListPopupCommand = new RelayCommand(() => this.HideWordListPopup());
        }

        /// <summary>
        /// Loads the pie chart data.
        /// </summary>
        private void LoadPieChart()
        {
            int bannedCount = 0;
            int usersCount = 0;
            int adminsCount = 0;
            int managerCount = 0;

            List<User> users = this.userService.GetAllUsers();
            foreach (User user in users)
            {
                int count = user.AssignedRoles.Count;
                switch (count)
                {
                    case 0:
                        bannedCount++;
                        break;
                    case 1:
                        usersCount++;
                        break;
                    case 2:
                        adminsCount++;
                        break;
                    case 3:
                        managerCount++;
                        break;
                }
            }

            this.PieChartSeries = new ISeries[]
            {
                new PieSeries<double> { Values = new double[] { bannedCount }, Name = "Banned" },
                new PieSeries<double> { Values = new double[] { usersCount }, Name = "Users" },
                new PieSeries<double> { Values = new double[] { adminsCount }, Name = "Admins" },
                new PieSeries<double> { Values = new double[] { managerCount }, Name = "Managers" },
            };
        }

        /// <summary>
        /// Updates a user's role.
        /// </summary>
        /// <param name="user">The user to update.</param>
        /// <param name="roleType">The role type to assign.</param>
        private void UpdateUserRole(User user, RoleType roleType)
        {
            this.userService.UpdateUserRole(user.UserId, roleType);
        }
    }
}