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
    using System.Threading.Tasks;

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
                    _ = LoadUserAppealDetails(value);
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
                    _ = LoadUpgradeRequestDetails(value);
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

        public async Task LoadAllData()
        {
            await Task.WhenAll(
                LoadFlaggedReviews(),
                LoadAppeals(),
                LoadRoleRequests(),
                LoadOffensiveWords()
            );
        }

        public async Task LoadFlaggedReviews()
        {
            var reviews = await this.reviewsService.GetFlaggedReviews();
            this.FlaggedReviews = new ObservableCollection<Review>(reviews);
        }

        public async Task LoadAppeals()
        {
            var appeals = await this.userService.GetBannedUsersWhoHaveSubmittedAppeals();
            this.AppealsUsers = new ObservableCollection<User>(appeals);
        }

        public async Task LoadRoleRequests()
        {
            var requests = await Task.Run(() => this.requestsService.RetrieveAllUpgradeRequests());
            this.UpgradeRequests = new ObservableCollection<UpgradeRequest>(requests);
        }

        public async Task LoadOffensiveWords()
        {
            var words = await Task.Run(() => this.checkersService.GetOffensiveWordsList());
            this.OffensiveWords = new ObservableCollection<string>(words);
        }

        public async Task FilterReviews(string filter)
        {
            var reviews = await this.reviewsService.FilterReviewsByContent(filter);
            this.FlaggedReviews = new ObservableCollection<Review>(reviews);
        }

        public async Task FilterAppeals(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                await LoadAppeals();
                return;
            }

            filter = filter.ToLower();
            var appeals = await this.userService.GetBannedUsersWhoHaveSubmittedAppeals();
            this.AppealsUsers = new ObservableCollection<User>(
                appeals.Where(user =>
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

        public async Task<List<string>> RunAutoCheck()
        {
            try
            {
                var reviews = await this.reviewsService.GetFlaggedReviews();
                var messages = this.checkersService.RunAutoCheck(reviews);
                await this.LoadFlaggedReviews();
                return messages;
            }
            catch (Exception ex)
            {
                throw;
            }
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

        public async Task HandleUpgradeRequest(bool isAccepted, int requestId)
        {
            try
            {
                // Call the async method on the service
                await requestsService.ProcessUpgradeRequest(isAccepted, requestId);

                // Refresh the UI on the UI thread
                Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()?.TryEnqueue(async () =>
                {
                    // Refresh your data - reload upgrade requests using the async method
                    var requests = await requestsService.RetrieveAllUpgradeRequests();

                    // Update your property that holds the requests
                    UpgradeRequests = new System.Collections.ObjectModel.ObservableCollection<UpgradeRequest>(requests);

                    // Notify UI of changes
                    OnPropertyChanged(nameof(UpgradeRequests));
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void CloseAppealCase(User user)
        {
            user.HasSubmittedAppeal = false;
            this.LoadAppeals();
        }

        public List<Review> GetUserReviews(Guid userId)
        {
            return this.reviewsService.GetReviewsByUser(userId).Result;
        }

        public User GetUserById(Guid userId)
        {
            return this.userService.GetUserById(userId).Result;
        }

        public RoleType GetHighestRoleTypeForUser(Guid userId)
        {
            return this.userService.GetHighestRoleTypeForUser(userId).Result;
        }

        public async Task<string> GetRoleNameBasedOnID(RoleType roleType)
        {
            return await this.requestsService.GetRoleNameBasedOnIdentifier(roleType);
        }

        public async Task LoadUserAppealDetails(User user)
        {
            try
            {
                this.IsAppealUserBanned = true;
                this.UserStatusDisplay = this.GetUserStatusDisplay(user, true);

                var reviews = await this.reviewsService.GetReviewsByUser(user.UserId);
                this.UserReviewsFormatted = new ObservableCollection<string>(
                    reviews.Select(r => this.FormatReviewContent(r)).ToList());
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task UpdateUserHasAppealed(User user, bool newValue)
        {
            try
            {
                await this.userService.UpdateUserAppleaed(user, newValue);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task KeepBanForUser(User user)
        {
            try
            {
                if (user == null)
                {
                    return;
                }

                await this.userService.UpdateUserRole(user.UserId, RoleType.Banned);
                await this.UpdateUserHasAppealed(user, false);
                
                this.IsAppealUserBanned = true;
                this.UserStatusDisplay = this.GetUserStatusDisplay(user, true);
                
                await this.LoadAppeals();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task AcceptAppealForUser(User user)
        {
            try
            {
                if (user == null)
                {
                    return;
                }

                await this.userService.UpdateUserRole(user.UserId, RoleType.User);
                User updatedUser = await this.userService.GetUserById(user.UserId);
                await this.UpdateUserHasAppealed(updatedUser, false);
                
                this.IsAppealUserBanned = false;
                this.UserStatusDisplay = this.GetUserStatusDisplay(user, false);
                
                await this.LoadAppeals();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task LoadUpgradeRequestDetails(UpgradeRequest request)
        {
            if (request == null)
            {
                return;
            }

            User requestingUser = this.GetUserById(request.RequestingUserIdentifier);
            RoleType currentRoleID = this.GetHighestRoleTypeForUser(request.RequestingUserIdentifier);

            string currentRoleName = await this.GetRoleNameBasedOnID(currentRoleID);
            string requiredRoleName = await this.GetRoleNameBasedOnID(currentRoleID + 1);

            this.UserUpgradeInfo = this.FormatUserUpgradeInfo(requestingUser, currentRoleName, requiredRoleName);
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
            this.KeepBanCommand = new RelayCommand(async () => 
            {
                try
                {
                    await this.KeepBanForUser(this.SelectedAppealUser);
                }
                catch (Exception ex)
                {
                    // err
                }
            });
            this.AcceptAppealCommand = new RelayCommand(async () => 
            {
                try
                {
                    await this.AcceptAppealForUser(this.SelectedAppealUser);
                }
                catch (Exception ex)
                {
                    // err
                }
            });
            this.CloseAppealCaseCommand = new RelayCommand(() => this.CloseAppealCase(this.SelectedAppealUser));

            this.HandleUpgradeRequestCommand = new RelayCommand<Tuple<bool, int>>(async param =>
                await this.HandleUpgradeRequest(param.Item1, param.Item2));

            this.ResetReviewFlagsCommand = new RelayCommand<int>(reviewId =>
                this.ResetReviewFlags(reviewId));

            this.HideReviewCommand = new RelayCommand<int>(param =>
                this.HideReview(param));

            this.RunAICheckCommand = new RelayCommand<Review>(review =>
                this.RunAICheck(review));

            this.RunAutoCheckCommand = new RelayCommand(async () => 
            {
                try 
                {
                    var messages = await this.RunAutoCheck();
                }
                catch (Exception ex)
                {
                    // err
                }
            });

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