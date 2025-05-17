namespace DrinkDb_Auth.ViewModel.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using DataAccess.Model.AdminDashboard;
    using DataAccess.Model.Authentication;
    using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
    using DrinkDb_Auth.ViewModel.AdminDashboard.Components;
    using DrinkDb_Auth.Service;
    using DataAccess.Service.AdminDashboard.Interfaces;
    using DataAccess.AutoChecker;

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
        private User selectedAppealUser;
        private UpgradeRequest selectedUpgradeRequest;
        private ObservableCollection<string> userReviewsFormatted;
        private ObservableCollection<string> userReviewsWithFlags;
        private string userStatusDisplay;
        private string userUpgradeInfo;
        private bool isAppealUserBanned = true;
        private bool isWordListVisible = false;

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

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand KeepBanCommand { get; private set; }

        public ICommand AcceptAppealCommand { get; private set; }

        public ICommand CloseAppealCaseCommand { get; private set; }

        public ICommand HandleUpgradeRequestCommand { get; private set; }

        public ICommand ResetReviewFlagsCommand { get; private set; }

        public ICommand HideReviewCommand { get; private set; }

        public ICommand RunAICheckCommand { get; private set; }

        public ICommand RunAutoCheckCommand { get; private set; }

        public ICommand AddOffensiveWordCommand { get; private set; }

        public ICommand DeleteOffensiveWordCommand { get; private set; }

        public ICommand ShowWordListPopupCommand { get; private set; }

        public ICommand HideWordListPopupCommand { get; private set; }

        public ObservableCollection<Review> FlaggedReviews
        {
            get => this.flaggedReviews;
            set
            {
                this.flaggedReviews = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<User> AppealsUsers
        {
            get => this.appealsUsers;
            set
            {
                this.appealsUsers = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<UpgradeRequest> UpgradeRequests
        {
            get => this.upgradeRequests;
            set
            {
                this.upgradeRequests = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<string> OffensiveWords
        {
            get => this.offensiveWords;
            set
            {
                this.offensiveWords = value;
                this.OnPropertyChanged();
            }
        }

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

        public ObservableCollection<string> UserReviewsFormatted
        {
            get => this.userReviewsFormatted;
            set
            {
                this.userReviewsFormatted = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<string> UserReviewsWithFlags
        {
            get => this.userReviewsWithFlags;
            set
            {
                this.userReviewsWithFlags = value;
                this.OnPropertyChanged();
            }
        }

        public string UserStatusDisplay
        {
            get => this.userStatusDisplay;
            set
            {
                this.userStatusDisplay = value;
                this.OnPropertyChanged();
            }
        }

        public string UserUpgradeInfo
        {
            get => this.userUpgradeInfo;
            set
            {
                this.userUpgradeInfo = value;
                this.OnPropertyChanged();
            }
        }

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

        public bool IsWordListVisible
        {
            get => this.isWordListVisible;
            set
            {
                this.isWordListVisible = value;
                this.OnPropertyChanged();
            }
        }

        public void LoadAllData()
        {
            this.LoadFlaggedReviews();
            this.LoadAppeals();
            this.LoadRoleRequests();
            this.LoadOffensiveWords();
        }

        public void LoadFlaggedReviews()
        {
            this.FlaggedReviews = new ObservableCollection<Review>(this.reviewsService.GetFlaggedReviews());
        }

        public void LoadAppeals()
        {
            this.AppealsUsers = new ObservableCollection<User>(this.userService.GetBannedUsersWhoHaveSubmittedAppeals().Result);
        }

        public void LoadRoleRequests()
        {
            this.UpgradeRequests = new ObservableCollection<UpgradeRequest>(this.requestsService.RetrieveAllUpgradeRequests());
        }

        public void LoadOffensiveWords()
        {
            this.OffensiveWords = new ObservableCollection<string>(this.checkersService.GetOffensiveWordsList());
        }

        public void FilterReviews(string filter)
        {
            this.FlaggedReviews = new ObservableCollection<Review>(
                this.reviewsService.FilterReviewsByContent(filter));
        }

        public void FilterAppeals(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                this.LoadAppeals();
                return;
            }

            filter = filter.ToLower();
            this.AppealsUsers = new ObservableCollection<User>(
                this.userService.GetBannedUsersWhoHaveSubmittedAppeals().Result
                    .Where(user =>
                        user.EmailAddress.ToLower().Contains(filter) ||
                        user.Username.ToLower().Contains(filter) ||
                        user.UserId.ToString().Contains(filter))
                    .ToList());
        }

        public void ResetReviewFlags(int reviewId)
        {
            this.reviewsService.ResetReviewFlags(reviewId);
            this.LoadFlaggedReviews();
        }

        public void HideReview(int reviewId)
        {
            this.reviewsService.HideReview(reviewId);
            this.reviewsService.ResetReviewFlags(reviewId);
            this.LoadFlaggedReviews();
        }

        public void RunAICheck(Review review)
        {
            this.checkersService.RunAICheckForOneReview(review);
            this.LoadFlaggedReviews();
        }

        public List<string> RunAutoCheck()
        {
            List<Review> reviews = this.reviewsService.GetFlaggedReviews();
            List<string> messages = this.checkersService.RunAutoCheck(reviews);
            this.LoadFlaggedReviews();
            return messages;
        }

        public void AddOffensiveWord(string word)
        {
            if (!string.IsNullOrWhiteSpace(word))
            {
                this.checkersService.AddOffensiveWord(word);
                this.LoadOffensiveWords();
            }
        }

        public void DeleteOffensiveWord(string word)
        {
            this.checkersService.DeleteOffensiveWord(word);
            this.LoadOffensiveWords();
        }

        public void HandleUpgradeRequest(bool isAccepted, int requestId)
        {
            try
            {
                // Call the synchronous method on the service
                requestsService.ProcessUpgradeRequest(isAccepted, requestId);

                // Refresh the UI on the UI thread
                // This approach uses the dispatcher to update UI after processing
                Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
                {
                    // Refresh your data - reload upgrade requests using the synchronous method
                    var requests = requestsService.RetrieveAllUpgradeRequests();

                    // Update your property that holds the requests
                  
                    UpgradeRequests = new System.Collections.ObjectModel.ObservableCollection<UpgradeRequest>(requests);

                    // Notify UI of changes
                    OnPropertyChanged(nameof(UpgradeRequests));
                });
            }
            catch (Exception ex)
            {
                
                System.Diagnostics.Debug.WriteLine($"Error processing upgrade request: {ex.Message}");
               
            }
        }

        public void CloseAppealCase(User user)
        {
            user.HasSubmittedAppeal = false;
            this.LoadAppeals();
        }

        public List<Review> GetUserReviews(Guid userId)
        {
            return this.reviewsService.GetReviewsByUser(userId);
        }

        public User GetUserById(Guid userId)
        {
            return this.userService.GetUserById(userId).Result;
        }

        public RoleType GetHighestRoleTypeForUser(Guid userId)
        {
            return this.userService.GetHighestRoleTypeForUser(userId).Result;
        }

        public string GetRoleNameBasedOnID(RoleType roleType)
        {
            return this.requestsService.GetRoleNameBasedOnIdentifier(roleType);
        }

        public void LoadUserAppealDetails(User user)
        {
            this.IsAppealUserBanned = true;
            this.UserStatusDisplay = this.GetUserStatusDisplay(user, true);

            List<Review> reviews = this.GetUserReviews(user.UserId);
            this.UserReviewsFormatted = new ObservableCollection<string>(
                reviews.Select(r => this.FormatReviewContent(r)).ToList());
        }

        public void KeepBanForUser(User user)
        {
            if (user == null)
            {
                return;
            }

            this.UpdateUserRole(user, RoleType.Banned);
            this.UpdateUserHasAppealed(user, false);
            this.IsAppealUserBanned = true;
            this.UserStatusDisplay = this.GetUserStatusDisplay(user, true);
        }

        public void AcceptAppealForUser(User user)
        {
            if (user == null)
            {
                return;
            }
            this.UpdateUserRole(user, RoleType.User);
            User updatedUser = GetUserById(user.UserId);
            this.UpdateUserHasAppealed(updatedUser, false);
            this.IsAppealUserBanned = false;
            this.UserStatusDisplay = this.GetUserStatusDisplay(user, false);
        }

        private void UpdateUserHasAppealed(User user, bool newValue)
        {
            this.userService.UpdateUserAppleaed(user, newValue);
        }

        public void LoadUpgradeRequestDetails(UpgradeRequest request)
        {
            this.SelectedUpgradeRequest = request;

            Guid userId = request.RequestingUserIdentifier;
            User selectedUser = this.GetUserById(userId);
            RoleType currentRoleID = this.GetHighestRoleTypeForUser(selectedUser.UserId);
            string currentRoleName = this.GetRoleNameBasedOnID(currentRoleID);
            string requiredRoleName = this.GetRoleNameBasedOnID(currentRoleID + 1);

            this.UserUpgradeInfo = this.FormatUserUpgradeInfo(selectedUser, currentRoleName, requiredRoleName);

            List<Review> reviews = this.GetUserReviews(selectedUser.UserId);
            this.UserReviewsWithFlags = new ObservableCollection<string>(
                reviews.Select(r => this.FormatReviewWithFlags(r)).ToList());
        }

        public string GetUserStatusDisplay(User user, bool isBanned)
        {
            if (user == null)
            {
                return string.Empty;
            }

            return $"User ID: {user.UserId}\nEmail: {user.EmailAddress}\nStatus: {(isBanned ? "Banned" : "Active")}";
        }

        public string FormatUserUpgradeInfo(User user, string currentRoleName, string requiredRoleName)
        {
            if (user == null)
            {
                return string.Empty;
            }

            return $"User ID: {user.UserId}\nEmail: {user.EmailAddress}\n{currentRoleName} -> {requiredRoleName}";
        }

        public string FormatReviewContent(Review review)
        {
            if (review == null)
            {
                return string.Empty;
            }

            return $"{review.Content}";
        }

        public string FormatReviewWithFlags(Review review)
        {
            if (review == null)
            {
                return string.Empty;
            }

            return $"{review.Content}\nFlags: {review.NumberOfFlags}";
        }

        public void ShowWordListPopup()
        {
            this.IsWordListVisible = true;
        }

        public void HideWordListPopup()
        {
            this.IsWordListVisible = false;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void InitializeCommands()
        {
            this.KeepBanCommand = new RelayCommand(() => this.KeepBanForUser(this.SelectedAppealUser));
            this.AcceptAppealCommand = new RelayCommand(() => this.AcceptAppealForUser(this.SelectedAppealUser));
            this.CloseAppealCaseCommand = new RelayCommand(() => this.CloseAppealCase(this.SelectedAppealUser));

            this.HandleUpgradeRequestCommand = new RelayCommand<Tuple<bool, int>>(param =>
                this.HandleUpgradeRequest(param.Item1, param.Item2));

            this.ResetReviewFlagsCommand = new RelayCommand<int>(reviewId =>
                this.ResetReviewFlags(reviewId));

            this.HideReviewCommand = new RelayCommand<int>(param =>
                this.HideReview(param));

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

        private void UpdateUserRole(User user, RoleType roleType)
        {
            this.userService.UpdateUserRole(user.UserId, roleType);
        }
    }
}