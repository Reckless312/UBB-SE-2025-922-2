using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;
using DataAccess.Service.AdminDashboard.Interfaces;
using IRepository;
using MimeKit;

namespace DataAccess.Service
{
    public class UpgradeRequestsService : IUpgradeRequestsService
    {
        private IUpgradeRequestsRepository upgradeRequestsRepository;
        private IRolesRepository rolesRepository;
        private IUserRepository userRepository;

        public UpgradeRequestsService(IUpgradeRequestsRepository upgradeRequestsRepository, IRolesRepository rolesRepository, IUserRepository userRepository)
        {
            this.upgradeRequestsRepository = upgradeRequestsRepository;
            this.rolesRepository = rolesRepository;
            this.userRepository = userRepository;
        }

        public async Task<List<UpgradeRequest>> RetrieveAllUpgradeRequests()
        {
            try
            {
                await this.RemoveUpgradeRequestsFromBannedUsersAsync();

                return await this.upgradeRequestsRepository.RetrieveAllUpgradeRequests();
            }
            catch
            {
                return new List<UpgradeRequest>();
            }
        }

        public async Task RemoveUpgradeRequestsFromBannedUsersAsync()
        {
            try
            {
                List<UpgradeRequest> pendingUpgradeRequests = await this.upgradeRequestsRepository.RetrieveAllUpgradeRequests();

                foreach (UpgradeRequest request in pendingUpgradeRequests)
                {
                    RoleType? roleType = await this.userRepository.GetRoleTypeForUser(request.RequestingUserIdentifier);

                    if (roleType == RoleType.Banned)
                    {
                        await this.upgradeRequestsRepository.RemoveUpgradeRequestByIdentifier(request.UpgradeRequestId);
                    }
                }
            }
            catch
            {
            }
        }

        public async Task<string> GetRoleNameBasedOnIdentifier(RoleType roleType)
        {
            try
            {
                List<Role> availableRoles = await this.rolesRepository.GetAllRoles();
                Role matchingRole = availableRoles.First(role => role.RoleType == roleType);
                return matchingRole.RoleName;
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task ProcessUpgradeRequest(bool isRequestAccepted, int upgradeRequestIdentifier)
        {
            try
            {
                if (isRequestAccepted)
                {
                    await this.SendEmail(upgradeRequestIdentifier);
                }

                await this.upgradeRequestsRepository.RemoveUpgradeRequestByIdentifier(upgradeRequestIdentifier);
            }
            catch
            {
            }
        }

        public async Task RemoveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
            try
            {
                await this.upgradeRequestsRepository.RemoveUpgradeRequestByIdentifier(upgradeRequestIdentifier);
            }
            catch
            {
            }
        }

        public async Task<UpgradeRequest?> RetrieveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
            try
            {
                return await this.upgradeRequestsRepository.RetrieveUpgradeRequestByIdentifier(upgradeRequestIdentifier);
            }
            catch
            {
                return null;
            }
        }

        private async Task SendEmail(int upgradeRequestIdentifier)
        {
            UpgradeRequest? currentUpgradeRequest = await this.upgradeRequestsRepository.RetrieveUpgradeRequestByIdentifier(upgradeRequestIdentifier);

            if (currentUpgradeRequest == null)
            {
                return;
            }

            Guid requestingUserIdentifier = currentUpgradeRequest.RequestingUserIdentifier;

            RoleType? currentHighestRoleType = await this.userRepository.GetRoleTypeForUser(requestingUserIdentifier);

            if (currentHighestRoleType == null)
            {
                return;
            }

            Role? nextRoleLevel = await this.rolesRepository.GetNextRoleInHierarchy((RoleType)currentHighestRoleType);

            if (nextRoleLevel == null)
            {
                return;
            }

            await this.userRepository.ChangeRoleToUser(requestingUserIdentifier, nextRoleLevel);

            User? user = await this.userRepository.GetUserById(requestingUserIdentifier);

            if (user != null && !string.IsNullOrEmpty(user.EmailAddress))
            {
                string smtpEmail = Environment.GetEnvironmentVariable("SMTP_MODERATOR_EMAIL") ?? "ionutcora66@gmail.com";
                string smtpPassword = Environment.GetEnvironmentVariable("SMTP_MODERATOR_PASSWORD") ?? "qvzl vtbe kkzm rlur";

                // Used to be a check here for environment variables, but if they are not setup, I made it default to me, the desktop tech lead
                string htmlBody = $@"
                    <html>
                    <body>
                        <h2>Congratulations!</h2>
                        <p>Your account has been upgraded.</p>
                        <p>
                        <b>Username:</b> {user.Username}<br>
                        <b>New Role:</b> {nextRoleLevel.RoleName}<br>
                        <b>Date:</b> {DateTime.Now:yyyy-MM-dd HH:mm}
                        </p>
                        <p>Thank you for being part of our community!</p>
                    </body>
                    </html>";

                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress("System Admin", "your-admin-email@example.com"));
                message.To.Add(new MailboxAddress(user.Username, user.EmailAddress));
                message.Subject = "Your Account Has Been Upgraded!";
                BodyBuilder bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = bodyBuilder.ToMessageBody();

                AdminDashboard.Components.SmtpEmailSender emailSender = new AdminDashboard.Components.SmtpEmailSender();
                await emailSender.SendEmailAsync(message, smtpEmail, smtpPassword);
            }
        }
    }
}