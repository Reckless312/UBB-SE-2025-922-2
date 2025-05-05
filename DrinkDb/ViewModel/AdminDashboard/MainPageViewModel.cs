// <copyright file="MainPageViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DrinkDb_Auth.ViewModel.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using DrinkDb_Auth.AutoChecker;
    using DataAccess.Model.AdminDashboard;
    using DataAccess.Model.Authentication;
    using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
    using DrinkDb_Auth.ViewModel.AdminDashboard.Components;
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
            requestsService = upgradeRequestsService ?? throw new ArgumentNullException(nameof(upgradeRequestsService));
            this.checkersService = checkersService ?? throw new ArgumentNullException(nameof(checkersService));
            this.autoCheck = autoCheck ?? throw new ArgumentNullException(nameof(autoCheck));

            InitializeCommands();

            LoadAllData();
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
            get => flaggedReviews;
            set
            {
                flaggedReviews = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the collection of users with active appeals.
        /// </summary>
        public ObservableCollection<User> AppealsUsers
        {
            get => appealsUsers;
            set
            {
                appealsUsers = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the collection of upgrade requests.
        /// </summary>
        public ObservableCollection<UpgradeRequest> UpgradeRequests
        {
            get => upgradeRequests;
            set
            {
                upgradeRequests = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the collection of offensive words.
        /// </summary>
        public ObservableCollection<string> OffensiveWords
        {
            get => offensiveWords;
            set
            {
                offensiveWords = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the pie chart series for user role statistics.
        /// </summary>
        public ISeries[] PieChartSeries
        {
            get => pieChartSeries;
            set
            {
                pieChartSeries = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the bar chart series for review statistics.
        /// </summary>
        public ISeries[] BarChartSeries
        {
            get => barChartSeries;
            set
            {
                barChartSeries = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the X-axes for the bar chart.
        /// </summary>
        public IEnumerable<ICartesianAxis> BarChartXAxes
        {
            get => barChartXAxes;
            set
            {
                barChartXAxes = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the Y-axes for the bar chart.
        /// </summary>
        public IEnumerable<ICartesianAxis> BarChartYAxes
        {
            get => barChartYAxes;
            set
            {
                barChartYAxes = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the currently selected user for appeal review.
        /// </summary>
        public User SelectedAppealUser
        {
            get => selectedAppealUser;
            set
            {
                selectedAppealUser = value;
                if (value != null)
                {
                    LoadUserAppealDetails(value);
                }

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the currently selected upgrade request.
        /// </summary>
        public UpgradeRequest SelectedUpgradeRequest
        {
            get => selectedUpgradeRequest;
            set
            {
                selectedUpgradeRequest = value;
                if (value != null)
                {
                    LoadUpgradeRequestDetails(value);
                }

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the formatted reviews for the selected user.
        /// </summary>
        public ObservableCollection<string> UserReviewsFormatted
        {
            get => userReviewsFormatted;
            set
            {
                userReviewsFormatted = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the reviews with flag information for the selected user.
        /// </summary>
        public ObservableCollection<string> UserReviewsWithFlags
        {
            get => userReviewsWithFlags;
            set
            {
                userReviewsWithFlags = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the display text for user status information.
        /// </summary>
        public string UserStatusDisplay
        {
            get => userStatusDisplay;
            set
            {
                userStatusDisplay = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the display text for user upgrade information.
        /// </summary>
        public string UserUpgradeInfo
        {
            get => userUpgradeInfo;
            set
            {
                userUpgradeInfo = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the selected appeal user is banned.
        /// </summary>
        public bool IsAppealUserBanned
        {
            get => isAppealUserBanned;
            set
            {
                isAppealUserBanned = value;
                UserStatusDisplay = GetUserStatusDisplay(SelectedAppealUser, value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the offensive word list popup is visible.
        /// </summary>
        public bool IsWordListVisible
        {
            get => isWordListVisible;
            set
            {
                isWordListVisible = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Loads all data required by the view model.
        /// </summary>
        public void LoadAllData()
        {
            LoadFlaggedReviews();
            LoadAppeals();
            LoadRoleRequests();
            LoadStatistics();
            LoadOffensiveWords();
        }

        /// <summary>
        /// Loads the flagged reviews from the reviews service.
        /// </summary>
        public void LoadFlaggedReviews()
        {
            FlaggedReviews = new ObservableCollection<Review>(reviewsService.GetFlaggedReviews());
        }

        /// <summary>
        /// Loads the banned users who have submitted appeals.
        /// </summary>
        public void LoadAppeals()
        {
            AppealsUsers = new ObservableCollection<User>(userService.GetBannedUsersWhoHaveSubmittedAppeals().Result);
        }

        /// <summary>
        /// Loads the role upgrade requests.
        /// </summary>
        public void LoadRoleRequests()
        {
            UpgradeRequests = new ObservableCollection<UpgradeRequest>(requestsService.RetrieveAllUpgradeRequests());
        }

        /// <summary>
        /// Loads the list of offensive words.
        /// </summary>
        public void LoadOffensiveWords()
        {
            OffensiveWords = new ObservableCollection<string>(checkersService.GetOffensiveWordsList());
        }

        /// <summary>
        /// Loads the statistical data for charts.
        /// </summary>
        public void LoadStatistics()
        {
            LoadPieChart();
            LoadBarChart();
        }

        /// <summary>
        /// Filters reviews based on content.
        /// </summary>
        /// <param name="filter">The filter string to apply.</param>
        public void FilterReviews(string filter)
        {
            FlaggedReviews = new ObservableCollection<Review>(
                reviewsService.FilterReviewsByContent(filter));
        }

        /// <summary>
        /// Filters appeals based on user information.
        /// </summary>
        /// <param name="filter">The filter string to apply.</param>
        public void FilterAppeals(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                LoadAppeals();
                return;
            }

            filter = filter.ToLower();
            AppealsUsers = new ObservableCollection<User>(
                userService.GetBannedUsersWhoHaveSubmittedAppeals().Result
                    .Where(user =>
                        user.EmailAddress.ToLower().Contains(filter) ||
                        user.Username.ToLower().Contains(filter) ||
                        user.UserId.ToString().Contains(filter))
                    .ToList());
        }

        /// <summary>
        /// Resets the flags on a review.
        /// </summary>
        /// <param name="reviewId">The ID of the review to reset.</param>
        public void ResetReviewFlags(int reviewId)
        {
            reviewsService.ResetReviewFlags(reviewId);
            LoadFlaggedReviews();
            LoadStatistics();
        }

        /// <summary>
        /// Hides a review and resets its flags.
        /// </summary>
        /// <param name="userId">The ID of the user who owns the review.</param>
        /// <param name="reviewId">The ID of the review to hide.</param>
        public void HideReview(int reviewId)
        {
            reviewsService.HideReview(reviewId);
            reviewsService.ResetReviewFlags(reviewId);
            LoadFlaggedReviews();
            LoadStatistics();
        }

        /// <summary>
        /// Runs an AI check on a single review.
        /// </summary>
        /// <param name="review">The review to check.</param>
        public void RunAICheck(Review review)
        {
            checkersService.RunAICheckForOneReview(review);
            LoadFlaggedReviews();
        }

        /// <summary>
        /// Runs an automatic check on all flagged reviews.
        /// </summary>
        /// <returns>A list of messages from the checker.</returns>
        public List<string> RunAutoCheck()
        {
            List<Review> reviews = reviewsService.GetFlaggedReviews();
            List<string> messages = checkersService.RunAutoCheck(reviews);
            LoadFlaggedReviews();
            LoadStatistics();
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
                checkersService.AddOffensiveWord(word);
                LoadOffensiveWords();
            }
        }

        /// <summary>
        /// Deletes a word from the offensive words list.
        /// </summary>
        /// <param name="word">The word to delete.</param>
        public void DeleteOffensiveWord(string word)
        {
            checkersService.DeleteOffensiveWord(word);
            LoadOffensiveWords();
        }

        /// <summary>
        /// Handles a role upgrade request.
        /// </summary>
        /// <param name="approve">Whether to approve the request.</param>
        /// <param name="requestId">The ID of the request.</param>
        public void HandleUpgradeRequest(bool approve, int requestId)
        {
            requestsService.ProcessUpgradeRequest(approve, requestId);
            LoadRoleRequests();
            LoadStatistics();
        }

        /// <summary>
        /// Closes an appeal case for a user.
        /// </summary>
        /// <param name="user">The user whose appeal case should be closed.</param>
        public void CloseAppealCase(User user)
        {
            user.HasSubmittedAppeal = false;
            LoadAppeals();
        }

        /// <summary>
        /// Gets reviews by a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of reviews by the user.</returns>
        public List<Review> GetUserReviews(Guid userId)
        {
            return reviewsService.GetReviewsByUser(userId);
        }

        /// <summary>
        /// Gets a user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The user object.</returns>
        public User GetUserById(Guid userId)
        {
            return userService.GetUserById(userId).Result;
        }

        /// <summary>
        /// Gets the highest role type for a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The highest role type.</returns>
        public RoleType GetHighestRoleTypeForUser(Guid userId)
        {
            return userService.GetHighestRoleTypeForUser(userId).Result;
        }

        /// <summary>
        /// Gets the name of a role based on its ID.
        /// </summary>
        /// <param name="roleType">The role type.</param>
        /// <returns>The role name.</returns>
        public string GetRoleNameBasedOnID(RoleType roleType)
        {
            return requestsService.GetRoleNameBasedOnIdentifier(roleType);
        }

        /// <summary>
        /// Loads appeal details for a user.
        /// </summary>
        /// <param name="user">The user to load details for.</param>
        public void LoadUserAppealDetails(User user)
        {
            IsAppealUserBanned = true;
            UserStatusDisplay = GetUserStatusDisplay(user, true);

            List<Review> reviews = this.GetUserReviews(user.UserId);
            UserReviewsFormatted = new ObservableCollection<string>(
                reviews.Select(r => FormatReviewContent(r)).ToList());
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

            UpdateUserRole(user, RoleType.Banned);
            IsAppealUserBanned = true;
            UserStatusDisplay = GetUserStatusDisplay(user, true);
            LoadStatistics();
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

            UpdateUserRole(user, RoleType.User);
            IsAppealUserBanned = false;
            UserStatusDisplay = GetUserStatusDisplay(user, false);
            LoadStatistics();
        }

        /// <summary>
        /// Loads upgrade request details.
        /// </summary>
        /// <param name="request">The upgrade request to load details for.</param>
        public void LoadUpgradeRequestDetails(UpgradeRequest request)
        {
            SelectedUpgradeRequest = request;

            Guid userId = request.RequestingUserIdentifier;
            User selectedUser = GetUserById(userId);
            RoleType currentRoleID = this.GetHighestRoleTypeForUser(selectedUser.UserId);
            string currentRoleName = GetRoleNameBasedOnID(currentRoleID);
            string requiredRoleName = GetRoleNameBasedOnID(currentRoleID + 1);

            UserUpgradeInfo = FormatUserUpgradeInfo(selectedUser, currentRoleName, requiredRoleName);

            List<Review> reviews = this.GetUserReviews(selectedUser.UserId);
            UserReviewsWithFlags = new ObservableCollection<string>(
                reviews.Select(r => FormatReviewWithFlags(r)).ToList());
        }

        /// <summary>
        /// Formats the user status display tex
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
            IsWordListVisible = true;
        }

        /// <summary>
        /// Hides the offensive word list popup.
        /// </summary>
        public void HideWordListPopup()
        {
            IsWordListVisible = false;
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Loads the bar chart data.
        /// </summary>
        private void LoadBarChart()
        {
            int rejectedCount = reviewsService.GetHiddenReviews().Count;
            int pendingCount = reviewsService.GetFlaggedReviews().Count;
            int totalCount = reviewsService.GetAllReviews().Count;

            BarChartSeries = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = new double[] { rejectedCount, pendingCount, totalCount },
                },
            };

            BarChartXAxes = new[]
            {
                new Axis { Labels = new List<string> { "rejected", "pending", "total" } },
            };

            BarChartYAxes = new[]
            {
                new Axis { Name = "Total", MinLimit = 0 },
            };
        }

        /// <summary>
        /// Initializes the commands used by the view model.
        /// </summary>
        private void InitializeCommands()
        {
            KeepBanCommand = new RelayCommand(() => KeepBanForUser(SelectedAppealUser));
            AcceptAppealCommand = new RelayCommand(() => AcceptAppealForUser(SelectedAppealUser));
            CloseAppealCaseCommand = new RelayCommand(() => CloseAppealCase(SelectedAppealUser));

            HandleUpgradeRequestCommand = new RelayCommand<Tuple<bool, int>>(param =>
                HandleUpgradeRequest(param.Item1, param.Item2));

            ResetReviewFlagsCommand = new RelayCommand<int>(reviewId =>
                ResetReviewFlags(reviewId));

            HideReviewCommand = new RelayCommand<int>(param =>
                HideReview(param));

            RunAICheckCommand = new RelayCommand<Review>(review =>
                RunAICheck(review));

            RunAutoCheckCommand = new RelayCommand(() => RunAutoCheck());

            AddOffensiveWordCommand = new RelayCommand<string>(word =>
                AddOffensiveWord(word));

            DeleteOffensiveWordCommand = new RelayCommand<string>(word =>
                DeleteOffensiveWord(word));

            ShowWordListPopupCommand = new RelayCommand(() => ShowWordListPopup());
            HideWordListPopupCommand = new RelayCommand(() => HideWordListPopup());
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

            List<User> users = userService.GetAllUsers().Result;
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

            PieChartSeries = new ISeries[]
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
            userService.UpdateUserRole(user.UserId, roleType);
        }
    }
}